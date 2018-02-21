using IP2Country.Entities;
using System.Collections.Generic;

namespace IP2Country.Datasources
{
    public interface IIP2CountryDataSource
    {
        IEnumerable<IIPRangeCountry> Read();
    }
}
