using IP2Country.Entities;
using System;

namespace IP2Country.WebNet77
{
    public class WebNet77IPv4IPRangeCountry : IPRangeCountry
    {
        public string Registry { get; set; }
        public DateTime Assigned { get; set; }
        public string ISO2 { get; set; }
        public string ISO3 { get; set; }
        public string CountryName { get; set; }
    }
}
