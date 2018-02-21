using System.Net;

namespace IP2Country.Entities
{
    public interface IIPRangeCountry
    {
        IPAddress Start { get; set; }
        IPAddress End { get; set; }
        string Country { get; set; }
    }
}
