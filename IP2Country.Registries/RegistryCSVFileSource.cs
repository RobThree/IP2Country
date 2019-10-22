using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace IP2Country.Registries
{
    public class RegistryCSVFileSource : IP2CountryCSVFileSource<RegistryIPRangeCountry>
    {
        public RegistryCSVFileSource(string file)
            : base(file, new RegistryCSVRecordParser()) { }

        public override IEnumerable<IIPRangeCountry> Read() => ReadFile(Path, Parser);
    }

    public class RegistryCSVRecordParser : BaseCSVRecordParser<RegistryIPRangeCountry>
    {
        private static readonly string[] _emptyextensions = Array.Empty<string>();

        public override RegistryIPRangeCountry ParseRecord(string record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var data = record.Split('|');

            if (data.Length >= 7 && data[2].StartsWith("ipv", StringComparison.OrdinalIgnoreCase))
            {
                var start = IPAddress.Parse(data[3]);
                return new RegistryIPRangeCountry
                {
                    Registry = data[0],
                    Country = data[1],
                    Start = start,
                    End = ParseEndIP(start, int.Parse(data[4], CultureInfo.InvariantCulture)),
                    Date = ParseDate(data[5]),
                    Status = data[6],
                    Extensions = data.Length > 7 ? data.Skip(7).ToArray() : _emptyextensions
                };
            }
            return null;
        }

        private static DateTime? ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        private static IPAddress ParseEndIP(IPAddress start, int value)
        {
            switch (start.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    var ipv4bytes = start.GetAddressBytes();
                    var address = ((uint)ipv4bytes[0] << 24)
                        + ((uint)ipv4bytes[1] << 16)
                        + ((uint)ipv4bytes[2] << 8)
                        + ipv4bytes[3];

                    return new IPAddress(GetBigEndianBytes(address + (uint)value - 1));
                case AddressFamily.InterNetworkV6:
                    var ipv6bytes = start.GetAddressBytes();

                    var upper = ToUInt64(ipv6bytes, 0);
                    var lower = ToUInt64(ipv6bytes, 8);

                    // This needs to work with values between 0 and 128
                    // where 0's mask is 0, and 128's mask is 0xFFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF.
                    var upperMask = 0UL;
                    var lowerMask = 0UL;
                    if (value > 0 && value <= 64)
                    {
                        upperMask = unchecked(~((1UL << (64 - value)) - 1));
                        lowerMask = ulong.MinValue;
                    }
                    else
                    {
                        upperMask = ulong.MaxValue;
                        lowerMask = unchecked(~((1UL << (128 - value)) - 1));
                    }

                    var addressBytes = new byte[16];
                    GetBigEndianBytes((upper & upperMask) | ~upperMask).CopyTo(addressBytes, 0);
                    GetBigEndianBytes((lower & lowerMask) | ~lowerMask).CopyTo(addressBytes, 8);

                    return new IPAddress(addressBytes);
                default:
                    throw new NotImplementedException();
            }
        }

        private static byte[] GetBigEndianBytes(uint value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        private static byte[] GetBigEndianBytes(ulong value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        private static ulong ToUInt64(byte[] bigEndianBytes, int startIndex)
        {
            // We can't use BitConverter.ToUInt64 because it will use little-endian on x86/x64.
            var result = 0UL;
            for (var i = 0; i < 8; i++)
                result |= (ulong)bigEndianBytes[i + startIndex] << ((8 - i - 1) * 8);

            return result;
        }
    }
}