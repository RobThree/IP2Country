using IP2Country;
using IP2Country.Entities;
using IP2Country.Registries;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DemoWebService.Helpers
{
    public class AutoReloadingResolverConfig
    {
        public string CacheDirectory { get; set; } = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "delegationcache")).FullName;
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromHours(24);
    }

    public interface IAutoReloadingResolver : IIP2CountryResolver
    {
        void Initialize();
    }

    public class AutoReloadingResolver : IAutoReloadingResolver
    {
        private IIP2CountryResolver _resolver;
        private readonly AutoReloadingResolverConfig _config;
        private readonly Timer _reloadtimer;
        private bool _reloading = false;

        public AutoReloadingResolver(IOptions<AutoReloadingResolverConfig> config)
        {
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _reloadtimer = new Timer(Reload);
        }

        public void Initialize()
        {
            _reloadtimer.Change(TimeSpan.Zero, _config.RefreshInterval);
        }

        private void Reload(object state)
        {
            if (!_reloading)
            {
                _reloading = true;
                Trace.WriteLine("Reloading...");

                // Download latest files
                DownloadLatest(_config.CacheDirectory, _config.RefreshInterval.Add(TimeSpan.FromSeconds(-1)));

                // Make a new resolver
                var resolver = new IP2CountryResolver(
                    Directory.GetFiles(_config.CacheDirectory, "*.dat").Select(f => new RegistryCSVFileSource(f))
                );

                // Swap out current resolver with new resolver
                _resolver = resolver;

                _reloading = false;
                Trace.WriteLine("Reload done...");
            }
        }

        private void DownloadLatest(string path, TimeSpan ttl)
        {
            // Download all registry delegation latest files and store/"cache" them in a temp directory
            var d = new CachingWebClient();
            Task.Run(async () =>
            {
                await Task.WhenAll(
                    d.DownloadAsync("http://ftp.ripe.net/ripe/stats/delegated-ripencc-extended-latest", Path.Combine(path, "ripe.dat"), ttl),
                    d.DownloadAsync("http://ftp.apnic.net/pub/stats/apnic/delegated-apnic-extended-latest", Path.Combine(path, "apnic.dat"), ttl),
                    d.DownloadAsync("http://ftp.arin.net/pub/stats/arin/delegated-arin-extended-latest", Path.Combine(path, "arin.dat"), ttl),
                    d.DownloadAsync("http://ftp.lacnic.net/pub/stats/lacnic/delegated-lacnic-extended-latest", Path.Combine(path, "lacnic.dat"), ttl),
                    d.DownloadAsync("http://ftp.afrinic.net/pub/stats/afrinic/delegated-afrinic-extended-latest", Path.Combine(path, "afrinic.dat"), ttl)
                ).ConfigureAwait(false);
            }).Wait();
        }

        public IIPRangeCountry Resolve(string ip)
        {
            return GetResolver().Resolve(ip);
        }

        public IIPRangeCountry Resolve(IPAddress ip)
        {
            return GetResolver().Resolve(ip);
        }

        public IIPRangeCountry[] Resolve(string[] ips)
        {
            return GetResolver().Resolve(ips);
        }

        public IIPRangeCountry[] Resolve(IPAddress[] ips)
        {
            return GetResolver().Resolve(ips);
        }

        public IIPRangeCountry[] Resolve(IEnumerable<string> ips)
        {
            return GetResolver().Resolve(ips);
        }

        public IIPRangeCountry[] Resolve(IEnumerable<IPAddress> ips)
        {
            return GetResolver().Resolve(ips);
        }

        public IDictionary<string, IIPRangeCountry> ResolveAsDictionary(IEnumerable<string> ips)
        {
            return GetResolver().ResolveAsDictionary(ips);
        }

        public IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IEnumerable<IPAddress> ips)
        {
            return GetResolver().ResolveAsDictionary(ips);
        }

        public IDictionary<string, IIPRangeCountry> ResolveAsDictionary(string[] ips)
        {
            return GetResolver().ResolveAsDictionary(ips);
        }

        public IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IPAddress[] ips)
        {
            return GetResolver().ResolveAsDictionary(ips);
        }

        private IIP2CountryResolver GetResolver()
        {
            return _resolver ?? throw GetException();
        }

        private Exception GetException()
        {
            if (_reloading)
                return new WarmingUpException("Resolver currently not available; please wait");
            return new NullReferenceException("Resolver is not set");
        }
    }

    public class WarmingUpException : Exception
    {
        public WarmingUpException(string message)
            : base(message) { }
    }
}
