using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IP2Country.IP2IQ
{
    public class IP2IQCSVFileSource : IP2CountryCSVFileSource<IP2IQIPRangeCountry>
    {
        public IP2IQCSVFileSource(string file)
            : base(file, new IP2IQCSVRecordParser()) { }

        public override IEnumerable<IIPRangeCountry> Read()
        {
            return ReadFile(Path, Parser);
        }
    }

    public class IP2IQCSVRecordParser : BaseCSVRecordParser<IP2IQIPRangeCountry>
    {
        private static IPAddressComparer _comparer = IPAddressComparer.Default;

        public override IP2IQIPRangeCountry ParseRecord(string record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var data = record.Split(new[] { ',' }, 5).Select(v => StripQuotes(v.Trim())).ToArray();
            if (data.Length != 5)
            {
                if (IgnoreErrors)
                    return null;
                throw new UnexpectedNumberOfFieldsException(data.Length, 5);
            }

            return new IP2IQIPRangeCountry
            {
                Start = IPAddress.Parse(data[0]),
                End = IPAddress.Parse(data[1]),
                Country = data[4]
            };
        }
    }
}
