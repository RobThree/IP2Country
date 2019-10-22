using IP2Country.Datasources;
using IP2Country.Entities;
using System.Collections.Generic;
using System.Linq;

namespace IP2Country.Tests.Mocks
{
    public class MockDataSource<T> : IIP2CountryDataSource
        where T : IIPRangeCountry
    {
        private IIPRangeCountry[] _data;

        public MockDataSource(IEnumerable<IIPRangeCountry> data)
        {
            _data = data.ToArray();
        }

        public IEnumerable<IIPRangeCountry> Read() => _data;
    }
}
