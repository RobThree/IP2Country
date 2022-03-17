using IP2Country.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IP2Country
{
    public class IP2CountryBatchResolver : IIP2CountryBatchResolver
    {
        private readonly IIP2CountryResolver _resolver;

        public IP2CountryBatchResolver(IIP2CountryResolver resolver)
            => _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        public IIPRangeCountry? Resolve(string ip) => _resolver.Resolve(ip);
        public IIPRangeCountry? Resolve(IPAddress ip) => _resolver.Resolve(ip);

        public IIPRangeCountry[] Resolve(string[] ips)
        {
            if (ips.Any(i => string.IsNullOrEmpty(i)))
            {
                throw new ArgumentNullException(nameof(ips));
            }

            return Resolve(ips.Select(i => IPAddress.Parse(i)).ToArray());
        }

        public IIPRangeCountry[] Resolve(IPAddress[] ips)
        {
            if (ips == null)
            {
                throw new ArgumentNullException(nameof(ips));
            }

            var results = new IIPRangeCountry[ips.Length];
            Parallel.ForEach(Partitioner.Create(0, ips.Length), range =>
            {
                for (var index = range.Item1; index < range.Item2; index++)
                {
                    results[index] = Resolve(ips[index]);
                }
            });
            return results;
        }

        public IIPRangeCountry[] Resolve(IEnumerable<string> ips)
            => Resolve(ips.ToArray());

        public IIPRangeCountry[] Resolve(IEnumerable<IPAddress> ips)
            => Resolve(ips.ToArray());

        public IDictionary<string, IIPRangeCountry> ResolveAsDictionary(string[] ips)
            => ResolveAsDictionary(ips.AsEnumerable());

        public IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IPAddress[] ips)
            => ResolveAsDictionary(ips.AsEnumerable());

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

        private static IDictionary<T, IIPRangeCountry> ToDict<T>(T[] ips, IIPRangeCountry[] results)
        {
            var r = new Dictionary<T, IIPRangeCountry>(results.Length);
            for (var i = 0; i < results.Length; i++)
            {
                r[ips[i]] = results[i];
            }

            return r;
        }
    }
}
