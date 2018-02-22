using System;

namespace IP2Country.DataSources.CSVFile
{
    public abstract class CSVFileSourceException : Exception
    {
        public CSVFileSourceException(string message)
            : base(message) { }
    }

    public class UnexpectedNumberOfFieldsException : CSVFileSourceException
    {
        public UnexpectedNumberOfFieldsException(int actualFieldCount, int expectedFieldCount)
            : base($"Unexpected number of fields: {actualFieldCount}, expected: {expectedFieldCount}") { }
    }
}
