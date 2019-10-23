using IP2Country.Entities;
using System;
using System.Collections.ObjectModel;

namespace IP2Country.Registries
{
    public class RegistryIPRangeCountry : IPRangeCountry
    {
        public string Registry { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public ReadOnlyCollection<string> Extensions { get; internal set; }
    }
}