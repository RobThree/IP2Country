using IP2Country.Entities;
using System.Net;

namespace IP2Country
{
    public interface IIP2CountryResolver
    {
        IIPRangeCountry Resolve(string ip);
        IIPRangeCountry Resolve(IPAddress ip);
        IIPRangeCountry[] Resolve(string[] ips);
        IIPRangeCountry[] Resolve(IPAddress[] ips);
    }
}
