using IP2Country.Entities;
using System;

namespace IP2Country.MaxMind
{
    public class MaxMindGeoLiteIPIPRangeCountry : IPRangeCountry
    {
        public GeonameEntry CountryInfo { get; set; }
        public GeonameEntry RegisteredCountry { get; set; }
        public GeonameEntry RepresentedCountry { get; set; }
        [Obsolete]
        public bool IsAnonymousProxy { get; set; }
        [Obsolete]
        public bool IsSatelliteProvider { get; set; }
    }

    public class MaxMindGeoLiteIPIPRangeCicty : MaxMindGeoLiteIPIPRangeCountry
    {
        public string PostalCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? AccuracyRadius { get; set; }
    }

    public abstract class GeonameEntry
    {
        public int Id { get; set; }
        public string CountryName { get; set; }
        public string LocaleCode { get; set; }
        public string ContinentCode { get; set; }
        public string ContinentName { get; set; }
        public string ISOCode { get; set; }
        public bool IsInEU { get; set; }
    }

    public class GeonameCountry : GeonameEntry
    {
    }

    public class GeonameCity : GeonameEntry
    {
        public string SubDivision1ISOCode { get; set; }
        public string SubDivision1Name { get; set; }
        public string SubDivision2ISOCode { get; set; }
        public string SubDivision2Name { get; set; }
        public string CityName { get; set; }
        public int? MetroCode { get; set; }
        public string TimeZone { get; set; }
    }
}
