using IP2Country.Datasources;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace IP2Country
{
    public class IP2CountryResolver : IIP2CountryResolver
    {
        private readonly IDictionary<AddressFamily, IIPRangeCountry[]> _ipinfo;
        private readonly IPAddressComparer _comparer;

        public IP2CountryResolver(IIP2CountryDataSource ipDataSource)
            : this(new[] { ipDataSource }) { }

        public IP2CountryResolver(IIP2CountryDataSource ipDataSource, IPAddressComparer ipAddressComparer)
            : this(new[] { ipDataSource }, ipAddressComparer) { }

        public IP2CountryResolver(IEnumerable<IIP2CountryDataSource> ipDataSources)
            : this(ipDataSources, IPAddressComparer.Default) { }

        public IP2CountryResolver(IEnumerable<IIP2CountryDataSource> ipDataSources, IPAddressComparer ipAddressComparer)
        {
            if (ipDataSources.Any(i => i == null))
            {
                throw new ArgumentNullException(nameof(ipDataSources));
            }

            _comparer = ipAddressComparer ?? throw new ArgumentNullException(nameof(ipAddressComparer));

            _ipinfo = ipDataSources
                .SelectMany(d => d.Read())
                .GroupBy(i => i.Start.AddressFamily)
                .ToDictionary(g => g.Key, g => g.OrderBy(i => i.Start, _comparer).ToArray());
        }

        public IIPRangeCountry? Resolve(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                throw new ArgumentNullException(nameof(ip));
            }

            return Resolve(IPAddress.Parse(ip));
        }

        public IIPRangeCountry? Resolve(IPAddress ip)
        {
            if (ip == null)
            {
                throw new ArgumentNullException(nameof(ip));
            }

            if (_ipinfo.TryGetValue(ip.AddressFamily, out var data))
            {
                return Resolve(data, ip);
            }

            return null;
        }

        private IIPRangeCountry? Resolve(IIPRangeCountry[] data, IPAddress ip)
        {
            var lower = 0;
            var upper = data.Length - 1;

            while (lower <= upper)
            {
                var middle = (lower + upper) / 2;
                var cs = _comparer.Compare(ip, data[middle].Start);
                var ce = _comparer.Compare(ip, data[middle].End);
                var comparisonResult = (cs >= 0 && ce <= 0) ? 0 : cs;
                if (comparisonResult == 0)
                {
                    return data[middle];
                }
                else if (comparisonResult < 0)
                {
                    upper = middle - 1;
                }
                else
                {
                    lower = middle + 1;
                }
            }

            return null;
        }
    }
}