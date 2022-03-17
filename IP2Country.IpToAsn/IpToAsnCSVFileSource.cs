using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IP2Country.IpToAsn
{
    public class IpToAsnCSVFileSource : IP2CountryCSVFileSource<IpToAsnIPRangeCountry>
    {
        public IpToAsnCSVFileSource(string file)
            : base(file, new IpToAsnCSVRecordParser()) { }

        public override IEnumerable<IIPRangeCountry> Read() => ReadFile(Path, Parser);
    }

    public class IpToAsnCSVRecordParser : BaseCSVRecordParser<IpToAsnIPRangeCountry>
    {
        public override IpToAsnIPRangeCountry ParseRecord(string record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var data = record.Split('\t').ToArray();
            if (data.Length != 3)
            {
                if (IgnoreErrors)
                {
                    return null;
                }

                throw new UnexpectedNumberOfFieldsException(data.Length, 3);
            }

            return new IpToAsnIPRangeCountry
            {
                Start = IPAddress.Parse(data[0]),
                End = IPAddress.Parse(data[1]),
                Country = data[2]
            };
        }
    }
}
