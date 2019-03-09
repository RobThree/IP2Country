using IP2Country.Entities;
using System;

namespace IP2Country.MaxMind
{
    public class MaxMindGeoLiteIPRangeCountry : IPRangeCountry
    {
        public MaxMindGeoLiteGeonameEntry Location { get; set; }
        public MaxMindGeoLiteGeonameEntry RegisteredLocation { get; set; }
        public MaxMindGeoLiteGeonameEntry RepresentedLocation { get; set; }
        [Obsolete]
        public bool IsAnonymousProxy { get; set; }
        [Obsolete]
        public bool IsSatelliteProvider { get; set; }
    }

    public class MaxMindGeoLiteIPRangeCity : MaxMindGeoLiteIPRangeCountry
    {
        public string PostalCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? AccuracyRadius { get; set; }
    }

    public class MaxMindGeoLiteASN : IPRangeCountry
    {
        public int ASN { get; set; }
        public string Organisation { get; set; }
    }

    public abstract class MaxMindGeoLiteGeonameEntry
    {
        public int GeoNameId { get; set; }
        public string CountryName { get; set; }
        public string LocaleCode { get; set; }
        public string ContinentCode { get; set; }
        public string ContinentName { get; set; }
        public string ISOCode { get; set; }
        public bool IsInEU { get; set; }
    }

    public class MaxMindGeoLiteGeonameCountry : MaxMindGeoLiteGeonameEntry
    {
    }

    public class MaxMindGeoLiteGeonameCity : MaxMindGeoLiteGeonameEntry
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
