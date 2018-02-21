using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Net;

namespace IP2Country.WebNet77
{
    public class WebNet77IPv6CSVFileSource : IP2CountryCSVFileSource<WebNet77IPv6IPRangeCountry>
    {
        public WebNet77IPv6CSVFileSource(string file)
            : base(file, new WebNet77IPv6CSVRecordParser()) { }

        public override IEnumerable<IIPRangeCountry> Read()
        {
            return ReadFile(Path, Parser);
        }
    }

    public class WebNet77IPv6CSVRecordParser : BaseCSVRecordParser<WebNet77IPv6IPRangeCountry>
    {
        public override WebNet77IPv6IPRangeCountry ParseRecord(string record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var data = record.Split(',');
            if (data.Length != 4)
            {
                if (IgnoreErrors)
                    return null;
                throw new Exception($"Unexpected number of fields: {data.Length}, expected: 4");
            }

            var ip = data[0].Split('-');
            return new WebNet77IPv6IPRangeCountry
            {
                Start = IPAddress.Parse(ip[0]),
                End = IPAddress.Parse(ip[1]),
                Registry = data[2],
                Assigned = ParseTimeStamp(data[3]),
                Country = data[1]
            };
        }
    }
}
