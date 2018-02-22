using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IP2Country.WebNet77
{
    public class WebNet77IPv4CSVFileSource : IP2CountryCSVFileSource<WebNet77IPv4IPRangeCountry>
    {
        public WebNet77IPv4CSVFileSource(string file)
            : base(file, new WebNet77IPv4CSVRecordParser()) { }

        public override IEnumerable<IIPRangeCountry> Read()
        {
            return ReadFile(Path, Parser);
        }
    }

    public class WebNet77IPv4CSVRecordParser : BaseCSVRecordParser<WebNet77IPv4IPRangeCountry>
    {
        public override WebNet77IPv4IPRangeCountry ParseRecord(string record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var data = record.Split(',').Select(v => StripQuotes(v)).ToArray();
            if (data.Length != 7)
            {
                if (IgnoreErrors)
                    return null;
                throw new UnexpectedNumberOfFieldsException(data.Length, 7);
            }

            return new WebNet77IPv4IPRangeCountry
            {
                Start = IPAddress.Parse(data[0]),
                End = IPAddress.Parse(data[1]),
                Registry = data[2],
                Assigned = ParseTimeStamp(data[3]),
                Country = data[4],
                ISO2 = data[4],
                ISO3 = data[5],
                CountryName = data[6]
            };
        }
    }
}
