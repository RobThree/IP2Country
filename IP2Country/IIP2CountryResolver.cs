using IP2Country.Entities;
using System.Collections.Generic;
using System.Net;

namespace IP2Country
{
    public interface IIP2CountryResolver
    {
        IIPRangeCountry Resolve(string ip);
        IIPRangeCountry Resolve(IPAddress ip);
        IIPRangeCountry[] Resolve(string[] ips);
        IIPRangeCountry[] Resolve(IPAddress[] ips);
        IIPRangeCountry[] Resolve(IEnumerable<string> ips);
        IIPRangeCountry[] Resolve(IEnumerable<IPAddress> ips);
        IDictionary<string, IIPRangeCountry> ResolveAsDictionary(IEnumerable<string> ips);
        IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IEnumerable<IPAddress> ips);
        IDictionary<string, IIPRangeCountry> ResolveAsDictionary(string[] ips);
        IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IPAddress[] ips);
    }
}
