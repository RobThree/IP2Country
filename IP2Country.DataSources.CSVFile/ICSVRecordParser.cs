using System.Text;

namespace IP2Country.DataSources.CSVFile
{
    public interface ICSVRecordParser<T>
    {
        bool IgnoreErrors { get; }
        Encoding Encoding { get; }
        T ParseRecord(string record);
    }
}
