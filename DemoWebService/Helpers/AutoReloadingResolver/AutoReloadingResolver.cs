﻿using IP2Country;
using IP2Country.Datasources;
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
    public class AutoReloadingResolver : IAutoReloadingResolver, IDisposable
    {
        private IIP2CountryBatchResolver _resolver;
        private readonly Func<IEnumerable<IIP2CountryDataSource>, IIP2CountryBatchResolver> _resolverfactory;
        private readonly AutoReloadingResolverConfig _config;
        private readonly Timer _reloadtimer;
        private bool _reloading = false;

        public AutoReloadingResolver(Func<IEnumerable<IIP2CountryDataSource>, IIP2CountryBatchResolver> resolverFactory, IOptions<AutoReloadingResolverConfig> config)
        {
            _resolverfactory = resolverFactory ?? throw new ArgumentNullException(nameof(resolverFactory));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _reloadtimer = new Timer(Reload);
        }

        public void Initialize()
            => _reloadtimer.Change(TimeSpan.Zero, _config.RefreshInterval);

        private void Reload(object state)
        {
            if (!_reloading)
            {
                _reloading = true;
                Trace.WriteLine("Reloading...");

                // Download latest files
                DownloadLatest(_config.CacheDirectory, _config.RefreshInterval.Add(TimeSpan.FromSeconds(-1)));

                // Make a new resolver
                var resolver = _resolverfactory(
                    Directory.GetFiles(_config.CacheDirectory, "*.dat").Select(f => new RegistryCSVFileSource(f))
                );

                // Swap out current resolver with new resolver
                _resolver = resolver ?? throw new ReloadException("Unable to create resolver");

                _reloading = false;
                Trace.WriteLine("Reload done...");
            }
        }

        private static void DownloadLatest(string path, TimeSpan ttl)
        {
            // Download all registry delegation latest files and store/"cache" them in a temp directory
            using var d = new CachingWebClient();
            Task.Run(async () =>
            {
                await Task.WhenAll(
                    d.DownloadAsync(new Uri("http://ftp.ripe.net/ripe/stats/delegated-ripencc-extended-latest"), Path.Combine(path, "ripe.dat"), ttl),
                    d.DownloadAsync(new Uri("http://ftp.apnic.net/pub/stats/apnic/delegated-apnic-extended-latest"), Path.Combine(path, "apnic.dat"), ttl),
                    d.DownloadAsync(new Uri("http://ftp.arin.net/pub/stats/arin/delegated-arin-extended-latest"), Path.Combine(path, "arin.dat"), ttl),
                    d.DownloadAsync(new Uri("http://ftp.lacnic.net/pub/stats/lacnic/delegated-lacnic-extended-latest"), Path.Combine(path, "lacnic.dat"), ttl),
                    d.DownloadAsync(new Uri("http://ftp.afrinic.net/pub/stats/afrinic/delegated-afrinic-extended-latest"), Path.Combine(path, "afrinic.dat"), ttl)
                ).ConfigureAwait(false);
            }).Wait();
        }

        public IIPRangeCountry? Resolve(string ip)
            => GetResolver().Resolve(ip);

        public IIPRangeCountry? Resolve(IPAddress ip)
            => GetResolver().Resolve(ip);

        public IIPRangeCountry[] Resolve(string[] ips)
            => GetResolver().Resolve(ips);

        public IIPRangeCountry[] Resolve(IPAddress[] ips)
            => GetResolver().Resolve(ips);

        public IIPRangeCountry[] Resolve(IEnumerable<string> ips)
            => GetResolver().Resolve(ips);

        public IIPRangeCountry[] Resolve(IEnumerable<IPAddress> ips)
            => GetResolver().Resolve(ips);

        public IDictionary<string, IIPRangeCountry> ResolveAsDictionary(IEnumerable<string> ips)
            => GetResolver().ResolveAsDictionary(ips);

        public IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IEnumerable<IPAddress> ips)
            => GetResolver().ResolveAsDictionary(ips);

        public IDictionary<string, IIPRangeCountry> ResolveAsDictionary(string[] ips)
            => GetResolver().ResolveAsDictionary(ips);

        public IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IPAddress[] ips)
            => GetResolver().ResolveAsDictionary(ips);

        private IIP2CountryBatchResolver GetResolver()
            => _resolver ?? throw GetException();

        private Exception GetException()
        {
            if (_reloading)
            {
                return new WarmingUpException("Resolver currently not available; please wait");
            }

            return new NullReferenceException("Resolver is not set");
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _reloadtimer?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
