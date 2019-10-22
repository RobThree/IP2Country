using IP2Country;
using IP2Country.Registries;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Download all registry delegation latest files and store/"cache" them in a temp directory
            using (var downloader = new CachingWebClient())
            {
                var temppath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "delegationcache")).FullName;
                await Task.WhenAll(
                    downloader.DownloadAsync("http://ftp.ripe.net/ripe/stats/delegated-ripencc-extended-latest", Path.Combine(temppath, "ripe.dat")),
                    downloader.DownloadAsync("http://ftp.apnic.net/pub/stats/apnic/delegated-apnic-extended-latest", Path.Combine(temppath, "apnic.dat")),
                    downloader.DownloadAsync("http://ftp.arin.net/pub/stats/arin/delegated-arin-extended-latest", Path.Combine(temppath, "arin.dat")),
                    downloader.DownloadAsync("http://ftp.lacnic.net/pub/stats/lacnic/delegated-lacnic-extended-latest", Path.Combine(temppath, "lacnic.dat")),
                    downloader.DownloadAsync("http://ftp.afrinic.net/pub/stats/afrinic/delegated-afrinic-extended-latest", Path.Combine(temppath, "afrinic.dat"))
                ).ConfigureAwait(false);

                // Initialize resolver with all data files
                var resolver = new IP2CountryBatchResolver(
                    new IP2CountryResolver(
                        Directory.GetFiles(temppath, "*.dat").Select(f => new RegistryCSVFileSource(f))
                    )
                );

                // A bunch of semi-"random" IPv4/IPv6 IP's from random countries for demonstration purposes...
                var ips = new[] {
                    "172.217.17.110", "31.13.91.36",
                    "2607:f8b0:4005:80b:0:0:0:200e", "2a03:2880:f11b:83:face:b00c:0:25de"
                };

                var results = resolver.Resolve(ips);

                // Now show the IP -> Country results
                for (int i = 0; i < ips.Length; i++)
                    Console.WriteLine($"IP: {ips[i],40}\tCountry: {results[i]?.Country ?? "Unknown"}");
            }
        }
    }
}
