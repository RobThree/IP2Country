using IP2Country.Datasources;
using IP2Country.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IP2Country
{
    public class IP2CountryResolver : IIP2CountryResolver
    {
        private readonly Dictionary<AddressFamily, IIPRangeCountry[]> _ipinfo;
        private static readonly IPAddressComparer _comparer = IPAddressComparer.Default;

        public IP2CountryResolver(IIP2CountryDataSource ipDataSource)
            : this(new[] { ipDataSource }) { }

        public IP2CountryResolver(IEnumerable<IIP2CountryDataSource> ipDataSources)
        {
            if (ipDataSources.Any(i => i == null))
                throw new ArgumentNullException(nameof(ipDataSources));

            _ipinfo = ipDataSources
                .SelectMany(d => d.Read())
                .GroupBy(i => i.Start.AddressFamily)
                .ToDictionary(g => g.Key, g => g.AsParallel().OrderBy(i => i.Start, _comparer).ToArray());
        }

        public IIPRangeCountry Resolve(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                throw new ArgumentNullException(nameof(ip));

            return Resolve(IPAddress.Parse(ip));
        }

        public IIPRangeCountry Resolve(IPAddress ip)
        {

            if (_ipinfo.TryGetValue(ip.AddressFamily, out IIPRangeCountry[] data))
                return Resolve(data, ip);
            return null;
        }

        public IIPRangeCountry[] Resolve(string[] ips)
        {
            if (ips.Any(i => string.IsNullOrEmpty(i)))
                throw new ArgumentNullException(nameof(ips));

            return Resolve(ips.Select(i => IPAddress.Parse(i)).ToArray());
        }

        public IIPRangeCountry[] Resolve(IPAddress[] ips)
        {
            var results = new IIPRangeCountry[ips.Length];
            Parallel.ForEach(Partitioner.Create(0, ips.Length), range =>
            {
                for (var index = range.Item1; index < range.Item2; index++)
                    results[index] = Resolve(ips[index]);
            });
            return results;
        }

        public IIPRangeCountry[] Resolve(IEnumerable<string> ips)
        {
            return Resolve(ips.ToArray());
        }

        public IIPRangeCountry[] Resolve(IEnumerable<IPAddress> ips)
        {
            return Resolve(ips.ToArray());
        }

        public IDictionary<string, IIPRangeCountry> ResolveAsDictionary(string[] ips)
        {
            return ResolveAsDictionary(ips.AsEnumerable());
        }

        public IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IPAddress[] ips)
        {
            return ResolveAsDictionary(ips.AsEnumerable());
        }

        public IDictionary<string, IIPRangeCountry> ResolveAsDictionary(IEnumerable<string> ips)
        {
            var resolveips = ips.Distinct().ToArray();
            return ToDict(resolveips, Resolve(resolveips));
        }

        public IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IEnumerable<IPAddress> ips)
        {
            var resolveips = ips.Distinct().ToArray();
            return ToDict(resolveips, Resolve(resolveips));
        }

        private IDictionary<T, IIPRangeCountry> ToDict<T>(T[] ips, IIPRangeCountry[] results)
        {
            var r = new Dictionary<T, IIPRangeCountry>(results.Length);
            for (int i = 0; i < results.Length; i++)
                r[ips[i]] = results[i];
            return r;
        }

        private static IIPRangeCountry Resolve(IIPRangeCountry[] data, IPAddress ip)
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
                    return data[middle];
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return null;
        }
    }
}
