using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Net;

namespace IP2Country.Ludost
{
    public class LudostCSVFileSource : IP2CountryCSVFileSource<LudostIPRangeCountry>
    {
        public LudostCSVFileSource(string file)
            : base(file, new LudostCSVRecordParser()) { }

        public override IEnumerable<IIPRangeCountry> Read() => ReadFile(Path, Parser);
    }

    public class LudostCSVRecordParser : BaseCSVRecordParser<LudostIPRangeCountry>
    {
        public override LudostIPRangeCountry ParseRecord(string record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var data = record.Split(' ');
            if (data.Length != 3)
            {
                if (IgnoreErrors)
                {
                    return null;
                }

                throw new UnexpectedNumberOfFieldsException(data.Length, 3);
            }

            return new LudostIPRangeCountry
            {
                Start = IPAddress.Parse(data[0]),
                End = IPAddress.Parse(data[1]),
                Country = data[2]
            };
        }
    }
}
