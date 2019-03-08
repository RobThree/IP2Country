using IP2Country.Entities;
using System;

namespace IP2Country.IP2Location.Lite
{
    public abstract class IP2LocationRangeCountry : IPRangeCountry
    { }

    public class IP2LocationRangeCountryDB1 : IP2LocationRangeCountry
    {
        public string CountryName { get; set; }
    }

    public class IP2LocationRangeCountryDB3 : IP2LocationRangeCountryDB1
    {
        public string Region { get; set; }
        public string City { get; set; }
    }

    public class IP2LocationRangeCountryDB5 : IP2LocationRangeCountryDB3
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class IP2LocationRangeCountryDB9 : IP2LocationRangeCountryDB5
    {
        public string ZipCode { get; set; }
    }

    public class IP2LocationRangeCountryDB11 : IP2LocationRangeCountryDB9
    {
        public TimeSpan? TimeZoneOffset { get; set; }
    }
}
