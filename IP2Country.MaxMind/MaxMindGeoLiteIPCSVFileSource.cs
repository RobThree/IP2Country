using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IP2Country.MaxMind
{
    public class MaxMindGeoLiteIPCSVFileSource : IP2CountryCSVFileSource<MaxMindGeoLiteIPIPRangeCountry>
    {
        public MaxMindGeoLiteIPCSVFileSource(string file)
            : base(file, new MaxMindGeoLiteIPCSVRecordParser()) { }

        public override IEnumerable<IIPRangeCountry> Read()
        {
            return ReadFile(Path, Parser);
        }
    }

    public class MaxMindGeoLiteIPCSVRecordParser : BaseCSVRecordParser<MaxMindGeoLiteIPIPRangeCountry>
    {
        private static IPAddressComparer _comparer = IPAddressComparer.Default;

        public override MaxMindGeoLiteIPIPRangeCountry ParseRecord(string record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var data = record.Split(new[] { ',' }, 6).Select(v => StripQuotes(v.Trim())).ToArray();
            if (data.Length != 6)
            {
                if (IgnoreErrors)
                    return null;
                throw new UnexpectedNumberOfFieldsException(data.Length, 6);
            }

            return new MaxMindGeoLiteIPIPRangeCountry
            {
                Start = IPAddress.Parse(data[0]),
                End = IPAddress.Parse(data[1]),
                Country = data[4],
                CountryName = data[5]
            };
        }
    }
}
