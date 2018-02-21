using IP2Country.Entities;
using System;
using System.Text;

namespace IP2Country.DataSources.CSVFile
{
    public abstract class BaseCSVRecordParser<T> : ICSVRecordParser<T>
        where T : IIPRangeCountry
    {
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public Encoding Encoding { get; protected set; }
        public bool IgnoreErrors { get; protected set; }

        public BaseCSVRecordParser()
            : this(false) { }

        public BaseCSVRecordParser(bool ignoreErrors)
            : this(ignoreErrors, Encoding.UTF8) { }

        public BaseCSVRecordParser(bool ignoreErrors, Encoding encoding)
        {
            IgnoreErrors = ignoreErrors;
            Encoding = encoding;
        }

        protected DateTime ParseTimeStamp(string value)
        {
            return EPOCH.AddSeconds(long.Parse(value));
        }

        protected string StripQuotes(string value)
        {
            return value.TrimStart('"').TrimEnd('"');
        }

        public abstract T ParseRecord(string record);
    }
}
