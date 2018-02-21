using IP2Country.Entities;
using System;

namespace IP2Country.Registries
{
    public class RegistryIPRangeCountry : IPRangeCountry
    {
        public string Registry { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public string[] Extensions { get; set; }
    }
}