using IP2Country.Entities;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;

namespace IP2Country.Caching
{
    public class CachingIP2CountryResolver : IIP2CountryResolver
    {
        private readonly IIP2CountryResolver _resolver;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _ttl;

        public CachingIP2CountryResolver(IIP2CountryResolver resolver, IMemoryCache cache, TimeSpan ttl)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _ttl = ttl <= TimeSpan.Zero ? TimeSpan.Zero : ttl;
        }

        public IIPRangeCountry Resolve(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                throw new ArgumentNullException(nameof(ip));
            }

            return Resolve(IPAddress.Parse(ip));
        }

        public IIPRangeCountry Resolve(IPAddress ip) => _cache.GetOrCreate(ip ?? throw new ArgumentNullException(nameof(ip)), entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _ttl;
            return _resolver.Resolve(ip);
        });
    }
}
