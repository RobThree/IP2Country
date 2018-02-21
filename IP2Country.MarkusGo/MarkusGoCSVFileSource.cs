using IP2Country.DataSources.CSVFile;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Net;

namespace IP2Country.MarkusGo
{
    public class MarkusGoCSVFileSource : IP2CountryCSVFileSource<MarkusGoIPRangeCountry>
    {
        public MarkusGoCSVFileSource(string file)
            : base(file, new MarkusGoCSVRecordParser()) { }

        public override IEnumerable<IIPRangeCountry> Read()
        {
            return ReadFile(Path, Parser);
        }
    }

    public class MarkusGoCSVRecordParser : BaseCSVRecordParser<MarkusGoIPRangeCountry>
    {
        private static IPAddressComparer _comparer = IPAddressComparer.Default;

        public override MarkusGoIPRangeCountry ParseRecord(string record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var data = record.Split(' ');
            if (data.Length != 3)
            {
                if (IgnoreErrors)
                    return null;
                throw new Exception($"Unexpected number of fields: {data.Length}, expected: 3");
            }

            return new MarkusGoIPRangeCountry
            {
                Start = IPAddress.Parse(data[0]),
                End = IPAddress.Parse(data[1]),
                Country = data[2]
            };
        }
    }
}
