using System.Net;

namespace IP2Country.Entities
{
    public class IPRangeCountry : IIPRangeCountry
    {
        public IPAddress Start { get; set; }
        public IPAddress End { get; set; }
        public string Country { get; set; }
    }
}
