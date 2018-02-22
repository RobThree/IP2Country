using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IP2Country.DbIp
{
    public class DbIpCSVFileSource : IP2CountryCSVFileSource<DbIpIPRangeCountry>
    {
        public DbIpCSVFileSource(string file)
            : base(file, new DbIpCSVRecordParser()) { }

        public override IEnumerable<IIPRangeCountry> Read()
        {
            return ReadFile(Path, Parser);
        }
    }

    public class DbIpCSVRecordParser : BaseCSVRecordParser<DbIpIPRangeCountry>
    {
        public override DbIpIPRangeCountry ParseRecord(string record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var data = record.Split(',').Select(v => StripQuotes(v)).ToArray();
            if (data.Length != 3)
            {
                if (IgnoreErrors)
                    return null;
                throw new UnexpectedNumberOfFieldsException(data.Length, 3);
            }

            return new DbIpIPRangeCountry
            {
                Start = IPAddress.Parse(data[0]),
                End = IPAddress.Parse(data[1]),
                Country = data[2]
            };
        }
    }
}
