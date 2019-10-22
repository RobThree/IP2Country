using IP2Country.Datasources;
using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using NetTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace IP2Country.MaxMind
{
    [Flags]
    public enum BlockOptions
    {
        IPv4 = 1,
        IPv6 = 2,
        All = 255
    }

    public class MaxMindGeoLiteFileSource : IIP2CountryDataSource
    {
        private readonly string _zipfile;
        private readonly string[] _preferredlanguages;
        private readonly BlockOptions _blockoptions;
        private const BlockOptions DEFAULTBLOCKOPTIONS = BlockOptions.All;
        private readonly Regex _blocksfilter = new Regex(@"-Blocks-IPv(\d+).csv", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public MaxMindGeoLiteFileSource(string zipFile)
            : this(zipFile, DEFAULTBLOCKOPTIONS) { }

        public MaxMindGeoLiteFileSource(string zipFile, BlockOptions blockOptions)
            : this(zipFile, new[] { "en" }, blockOptions) { }

        public MaxMindGeoLiteFileSource(string zipFile, string[] preferredLanguages)
            : this(zipFile, preferredLanguages, DEFAULTBLOCKOPTIONS) { }

        public MaxMindGeoLiteFileSource(string zipFile, string[] preferredLanguages, BlockOptions blockOptions)
        {
            _zipfile = zipFile ?? throw new ArgumentNullException(nameof(zipFile));
            if (preferredLanguages == null)
                throw new ArgumentNullException(nameof(preferredLanguages));
            if (preferredLanguages.Length == 0)
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                throw new ArgumentException("At least one preferred language must be specified", nameof(preferredLanguages));
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            _preferredlanguages = preferredLanguages;
            _blockoptions = Enum.IsDefined(typeof(BlockOptions), blockOptions) ? blockOptions : throw new ArgumentOutOfRangeException(nameof(blockOptions));
        }

        public IEnumerable<IIPRangeCountry> Read()
        {
            using (var zipArchive = new ZipArchive(File.OpenRead(_zipfile), ZipArchiveMode.Read))
            {
                // We're only interested in CSV files in the zip file
                var csvfiles = zipArchive.Entries.Where(e => e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)).ToArray();

                // First locate the GeoNames file(s) in order of preference (and make sure to always try english)
                var geofile = _preferredlanguages.Concat(new[] { "en" }).Distinct()
                    .Select(l => csvfiles.FirstOrDefault(f => f.Name.EndsWith($"locations-{l}.csv", StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault(e => e != null);

                // Next, read all locations for lookup later or use an empty dummy lookup
                var lookup = (geofile != null
                    ? ReadGeonames(geofile)
                    : Enumerable.Empty<MaxMindGeoLiteGeonameCountry>()
                ).ToDictionary(e => e.GeoNameId);

                // Now start reading the actual files
                var parser = new MaxMindGeoLiteIPCSVRecordParser(lookup);
                foreach (var e in csvfiles.Where(f => MatchesBlockOptionsFilter(f.Name, _blockoptions)))
                {
                    foreach (var line in ReadArchiveEntry(e))
                        yield return parser.ParseRecord(line);
                }
            }
        }

        private bool MatchesBlockOptionsFilter(string name, BlockOptions options)
        {
            var m = _blocksfilter.Match(name);
            if (m.Success)
            {
                switch (int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
                {
                    case 4:
                        return options.HasFlag(BlockOptions.IPv4);
                    case 6:
                        return options.HasFlag(BlockOptions.IPv6);
                }
            }
            return false;
        }

        private static IEnumerable<string> ReadArchiveEntry(ZipArchiveEntry entry, bool skipHeader = true)
        {
            using (var stream = entry.Open())
            using (var sr = new StreamReader(stream))
            {
                // Read past header?
                if (skipHeader)
                    sr.ReadLine();

                string line;
                while ((line = sr.ReadLine()) != null)
                    yield return line;
            }
        }

        private static IEnumerable<MaxMindGeoLiteGeonameEntry> ReadGeonames(ZipArchiveEntry geoEntry)
        {
            var geoparser = new MaxMindGeoLiteGeonameCSVRecordParser();
            foreach (var line in ReadArchiveEntry(geoEntry))
                yield return geoparser.ParseRecord(line);
        }
    }

    public class MaxMindGeoLiteIPCSVRecordParser : BaseCSVRecordParser<IPRangeCountry>
    {
        private readonly IDictionary<int, MaxMindGeoLiteGeonameEntry> _geolookup;

        public MaxMindGeoLiteIPCSVRecordParser(IDictionary<int, MaxMindGeoLiteGeonameEntry> geoNames)
        {
            _geolookup = geoNames ?? throw new ArgumentNullException(nameof(geoNames));
        }

        public override IPRangeCountry ParseRecord(string record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            var data = ReadQuotedValues(record).ToArray();

            // Minimum of 3 columns (ASN file)
            if (data.Length >= 3)
            {
                var ipnetwork = IPAddressRange.Parse(data[0]);

                MaxMindGeoLiteGeonameEntry country = null, regcountry = null, repcountry = null;

                // Only city/country files contain these columns; ASN file doesn't
                if (data.Length > 3)    
                {
                    country = int.TryParse(data[1], out var cntid) ? _geolookup.TryGetValue(cntid, out var cnt) ? cnt : null : null;
                    regcountry = int.TryParse(data[2], out var regid) ? _geolookup.TryGetValue(regid, out var reg) ? reg : null : null;
                    repcountry = int.TryParse(data[3], out var repid) ? _geolookup.TryGetValue(repid, out var rep) ? rep : null : null;
                }

#pragma warning disable CS0612 // Type or member is obsolete
                switch (data.Length)
                {
                    case 3:
                        return new MaxMindGeoLiteASN
                        {
                            Start = ipnetwork.Begin,
                            End = ipnetwork.End,
                            Country = null,
                            ASN = int.Parse(data[1], CultureInfo.InvariantCulture),
                            Organisation = data[2]
                        };
                    case 6:
                        return new MaxMindGeoLiteIPRangeCountry
                        {
                            Start = ipnetwork.Begin,
                            End = ipnetwork.End,
                            Country = country?.ISOCode,
                            Location = country,
                            RegisteredLocation = regcountry,
                            RepresentedLocation = repcountry,
                            IsAnonymousProxy = ParseBool(data[4]),
                            IsSatelliteProvider = ParseBool(data[5])
                        };
                    case 10:
                        return new MaxMindGeoLiteIPRangeCity
                        {
                            Start = ipnetwork.Begin,
                            End = ipnetwork.End,
                            Country = country?.ISOCode,
                            Location = country,
                            RegisteredLocation = regcountry,
                            RepresentedLocation = repcountry,
                            IsAnonymousProxy = ParseBool(data[4]),
                            IsSatelliteProvider = ParseBool(data[5]),
                            PostalCode = data[6],
                            Latitude = ParseLatLon(data[7]),
                            Longitude = ParseLatLon(data[8]),
                            AccuracyRadius = int.TryParse(data[9], out var ac) ? (int?)ac : null
                        };
                    default:
                        if (IgnoreErrors)
                            return null;
                        break;
                }
#pragma warning restore CS0612 // Type or member is obsolete
            }
            throw new UnexpectedNumberOfFieldsException(data.Length, new[] { 3, 6, 10 });
        }

        private static bool ParseBool(string value) => int.TryParse(value, out var r) ? r != 0 : false;

        private static double? ParseLatLon(string value) => double.TryParse(value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var lat) ? (double?)lat : null;
    }

    internal class MaxMindGeoLiteGeonameCSVRecordParser : BaseCSVRecordParser<MaxMindGeoLiteGeonameEntry>
    {
        public override MaxMindGeoLiteGeonameEntry ParseRecord(string record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var data = ReadQuotedValues(record).ToArray();
            switch (data.Length)
            {
                case 7:
                    return new MaxMindGeoLiteGeonameCountry
                    {
                        GeoNameId = int.Parse(data[0], CultureInfo.InvariantCulture),
                        LocaleCode = data[1],
                        ContinentCode = data[2],
                        ContinentName = data[3],
                        ISOCode = data[4],
                        CountryName = data[5],
                        IsInEU = data[6] != "0"
                    };
                case 14:
                    return new MaxMindGeoLiteGeonameCity
                    {
                        GeoNameId = int.Parse(data[0], CultureInfo.InvariantCulture),
                        LocaleCode = data[1],
                        ContinentCode = data[2],
                        ContinentName = data[3],
                        ISOCode = data[4],
                        CountryName = data[5],
                        SubDivision1ISOCode = data[6],
                        SubDivision1Name = data[7],
                        SubDivision2ISOCode = data[8],
                        SubDivision2Name = data[9],
                        CityName = data[10],
                        MetroCode = int.TryParse(data[11], out var mc) ? (int?)mc : null,
                        TimeZone = data[12],
                        IsInEU = data[13] != "0"
                    };
                default:
                    if (IgnoreErrors)
                        return null;
                    throw new UnexpectedNumberOfFieldsException(data.Length, new[] { 7, 14 });
            }
        }
    }
}