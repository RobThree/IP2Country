using IP2Country.Entities;
using System.Text;

namespace IP2Country.DataSources.CSVFile
{
    public interface ICSVRecordParser<T>
       where T : IIPRangeCountry
    {
        bool IgnoreErrors { get; }
        Encoding Encoding { get; }
        T ParseRecord(string record);
    }
}
