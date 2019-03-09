using System;
using System.Collections.Generic;
using System.Text;

namespace IP2Country.DataSources.CSVFile
{
    public abstract class BaseCSVRecordParser<T> : ICSVRecordParser<T>
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

        protected IEnumerable<string> ReadQuotedValues(string record)
        {
            var inquotes = false;
            var fieldvalue = new StringBuilder(4096);   //Let's assume 4k per line
            for (var i = 0; i < record.Length; i++)
            {
                if ('"' == record[i])
                {
                    inquotes = !inquotes;
                }
                else if (',' == record[i] && !inquotes)
                {
                    yield return fieldvalue.ToString();
                    fieldvalue.Clear();
                }
                else
                {
                    fieldvalue.Append(record[i]);
                }
            }
            yield return fieldvalue.ToString();
        }

        public abstract T ParseRecord(string record);
    }
}
