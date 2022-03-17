using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace IP2Country.IP2Location.Lite
{
    public class IP2LocationFileSource : IP2CountryCSVFileSource<IP2LocationRangeCountry>
    {
        public IP2LocationFileSource(string file)
            : base(file, new IP2LocationRecordParser((file ?? throw new ArgumentNullException(nameof(file))).IndexOf("ipv6", StringComparison.OrdinalIgnoreCase) > 0 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork)) { }

        public override IEnumerable<IIPRangeCountry> Read() => ReadFile(Path, Parser);
    }

    public class IP2LocationRecordParser : BaseCSVRecordParser<IP2LocationRangeCountry>
    {
        public AddressFamily AddressFamily { get; private set; }

        public IP2LocationRecordParser(AddressFamily addressFamily)
        {
            switch (addressFamily)
            {
                case AddressFamily.InterNetwork:
                case AddressFamily.InterNetworkV6:
                    AddressFamily = addressFamily;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(addressFamily));
            }
        }
        public override IP2LocationRangeCountry ParseRecord(string record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var data = ReadQuotedValues(record).ToArray();
            switch (data.Length)
            {
                case 4:     //DB1
                    return new IP2LocationRangeCountryDB1
                    {
                        Start = ParseIp(data[0]),
                        End = ParseIp(data[1]),
                        Country = FixEmpty(data[2]),
                        CountryName = FixEmpty(data[3]),
                    };
                case 6:     //DB3
                    return new IP2LocationRangeCountryDB3
                    {
                        Start = ParseIp(data[0]),
                        End = ParseIp(data[1]),
                        Country = FixEmpty(data[2]),
                        CountryName = FixEmpty(data[3]),
                        Region = FixEmpty(data[4]),
                        City = FixEmpty(data[5]),
                    };
                case 8:     //DB5
                    return new IP2LocationRangeCountryDB5
                    {
                        Start = ParseIp(data[0]),
                        End = ParseIp(data[1]),
                        Country = FixEmpty(data[2]),
                        CountryName = FixEmpty(data[3]),
                        Region = FixEmpty(data[4]),
                        City = FixEmpty(data[5]),
                        Latitude = ParseLatLon(data[6]),
                        Longitude = ParseLatLon(data[7])
                    };
                case 9:     //DB9
                    return new IP2LocationRangeCountryDB9
                    {
                        Start = ParseIp(data[0]),
                        End = ParseIp(data[1]),
                        Country = FixEmpty(data[2]),
                        CountryName = FixEmpty(data[3]),
                        Region = FixEmpty(data[4]),
                        City = FixEmpty(data[5]),
                        Latitude = ParseLatLon(data[6]),
                        Longitude = ParseLatLon(data[7]),
                        ZipCode = FixEmpty(data[8]),
                    };
                case 10:     //DB11
                    return new IP2LocationRangeCountryDB11
                    {
                        Start = ParseIp(data[0]),
                        End = ParseIp(data[1]),
                        Country = FixEmpty(data[2]),
                        CountryName = FixEmpty(data[3]),
                        Region = FixEmpty(data[4]),
                        City = FixEmpty(data[5]),
                        Latitude = ParseLatLon(data[6]),
                        Longitude = ParseLatLon(data[7]),
                        ZipCode = FixEmpty(data[8]),
                        TimeZoneOffset = ParseTZOffset(data[9])
                    };
                default:
                    if (IgnoreErrors)
                    {
                        return null;
                    }

                    throw new UnexpectedNumberOfFieldsException(data.Length, new[] { 4, 6, 8, 9, 10 });
            }
        }

        private IPAddress ParseIp(string value)
        {
            switch (AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return IPAddress.Parse(value);
                case AddressFamily.InterNetworkV6:
                    return ParseIPv6(value);
                default:
                    throw new InvalidOperationException();
            }
        }

        private static IPAddress ParseIPv6(string value)
        {
            if (BigInteger.TryParse(value, out var result))
            {
                var addrbytes = new byte[16];       // Prepare result array
                var bytes = result.ToByteArray();   // Get bytes

                // Ensure length is never larger than 16 bytes (see https://docs.microsoft.com/en-us/dotnet/api/system.numerics.biginteger.tobytearray)
                // Because two's complement representation always interprets the highest-order bit of the last byte in
                // the array (the byte at position Array.Length- 1) as the sign bit, the method returns a byte array
                // with an extra element whose value is zero to disambiguate positive values that could otherwise be
                // interpreted as having their sign bits set. For example, the value 120 or 0x78 is represented as a
                // single -byte array: 0x78. However, 128, or 0x80, is represented as a two-byte array: 0x80, 0x00.
                var l = bytes[bytes.Length - 1] == 0 ? bytes.Length - 1 : bytes.Length;

                var di = bytes.Length;              // Get index into result bytes (at end)
                for (var i = 0; i < 16; i++)        // Copy byte array, reversed, into result array
                {
                    addrbytes[i] = (l >= 16 - i) ? bytes[--di] : (byte)0;
                }

                return new IPAddress(addrbytes);
            }
            throw new FormatException($"The value '{value}' could not be converted to an IPv6 address");
        }

        private static string FixEmpty(string value)
            => "-".Equals(value, StringComparison.OrdinalIgnoreCase) ? null : value;

        private static double? ParseLatLon(string value)
            => double.TryParse(value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var lat) ? (double?)lat : null;

        private static TimeSpan? ParseTZOffset(string value)
            => TimeSpan.TryParse(value.Replace("+", ""), out var tz) ? (TimeSpan?)tz : null;
    }
}