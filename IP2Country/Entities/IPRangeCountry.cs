using System.Net;

namespace IP2Country.Entities
{
    public class IPRangeCountry : IIPRangeCountry
    {
        public IPAddress Start { get; set; } = IPAddress.None;
        public IPAddress End { get; set; } = IPAddress.None;
        public string Country { get; set; }
    }
}
