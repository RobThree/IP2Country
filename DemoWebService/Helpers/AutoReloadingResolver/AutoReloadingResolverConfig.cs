using System;
using System.IO;

namespace DemoWebService.Helpers
{
    public class AutoReloadingResolverConfig
    {
        public string CacheDirectory { get; set; } = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "delegationcache")).FullName;
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromHours(24);
    }
}