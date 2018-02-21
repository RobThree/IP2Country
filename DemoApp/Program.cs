using IP2Country;
using IP2Country.Datasources;
using IP2Country.DbIp;
using IP2Country.Entities;
using IP2Country.IP2IQ;
using IP2Country.MaxMind;
using IP2Country.Ludost;
using IP2Country.WebNet77;
using IP2Country.MarkusGo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IP2Country.Registries;
using System.IO;
using IP2Country.IpToAsn;
using System.Collections.Concurrent;

namespace DemoApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var d = new CachingWebClient();

                var ls = Stopwatch.StartNew();
                var resolvers = new[] {
                    new IP2CountryResolver(new IIP2CountryDataSource[] {
                        new IpToAsnCSVFileSource(await d.DownloadAsync("https://iptoasn.com/data/ip2country-v4.tsv.gz", @"D:\test\IP2Country\ip2asnipv4.dat")),
                        new IpToAsnCSVFileSource(await d.DownloadAsync("https://iptoasn.com/data/ip2country-v6.tsv.gz", @"D:\test\IP2Country\ip2asnipv6.dat")),
                    }),

                    new IP2CountryResolver(new IIP2CountryDataSource[] {
                        new WebNet77IPv4CSVFileSource(await d.DownloadAsync("http://software77.net/geo-ip/?DL=1", @"D:\test\IP2Country\webnet77ipv4.dat")),
                        new WebNet77IPv6CSVFileSource(await d.DownloadAsync("http://software77.net/geo-ip/?DL=7", @"D:\test\IP2Country\webnet77ipv6.dat"))
                    }),

                    new IP2CountryResolver(
                        new DbIpCSVFileSource(await d.DownloadAsync("http://download.db-ip.com/free/dbip-country-2018-02.csv.gz", @"D:\test\IP2Country\dbipv46.dat"))
                    ),

                    new IP2CountryResolver(new IIP2CountryDataSource[] {
                        new MaxMindGeoLiteIPCSVFileSource(await d.DownloadAsync("http://geolite.maxmind.com/download/geoip/database/GeoIPCountryCSV.zip", @"D:\test\IP2Country\maxmindipv4.dat")),
                        new MaxMindGeoLiteIPCSVFileSource(await d.DownloadAsync("http://geolite.maxmind.com/download/geoip/database/GeoIPv6.csv.gz", @"D:\test\IP2Country\maxmindipv6.dat"))
                    }),

                    new IP2CountryResolver(
                        new IP2IQCSVFileSource(await d.DownloadAsync("http://www.ip2iq.com/get?ip2-cc.csv.gz", @"D:\test\IP2Country\ip2iqipv4.dat"))
                    ),

                    new IP2CountryResolver(
                        new LudostCSVFileSource(await d.DownloadAsync("https://ip.ludost.net/raw/country.db.gz", @"D:\test\IP2Country\ludostipv4.dat"))
                    ),

                    new IP2CountryResolver(
                        new MarkusGoCSVFileSource(await d.DownloadAsync("https://github.com/Markus-Go/ip-countryside/blob/downloads/ip2country.zip?raw=true", @"D:\test\IP2Country\markusgoipv4.dat"))
                    ),

                    new IP2CountryResolver(new IIP2CountryDataSource[] {
                        new RegistryCSVFileSource(await d.DownloadAsync("http://ftp.ripe.net/ripe/stats/delegated-ripencc-extended-latest", @"D:\test\IP2Country\registries\ripe.dat")),
                        new RegistryCSVFileSource(await d.DownloadAsync("http://ftp.apnic.net/pub/stats/apnic/delegated-apnic-extended-latest", @"D:\test\IP2Country\registries\apnic.dat")),
                        new RegistryCSVFileSource(await d.DownloadAsync("http://ftp.arin.net/pub/stats/arin/delegated-arin-extended-latest", @"D:\test\IP2Country\registries\arin.dat")),
                        new RegistryCSVFileSource(await d.DownloadAsync("http://ftp.lacnic.net/pub/stats/lacnic/delegated-lacnic-extended-latest", @"D:\test\IP2Country\registries\lacnic.dat")),
                        new RegistryCSVFileSource(await d.DownloadAsync("http://ftp.afrinic.net/pub/stats/afrinic/delegated-afrinic-extended-latest", @"D:\test\IP2Country\registries\afrinic.dat"))
                    }),

                };
                Console.WriteLine($"Load: {ls.Elapsed}");

                // Warm up
                foreach (var r in resolvers)
                {
                    var resultv4 = r.Resolve("196.76.0.35");
                    var resultv6 = r.Resolve("2a00:1450:400e:807::200e");

                    //Console.WriteLine($"{resultv4?.Country}\t{resultv6?.Country}");
                }

                var ips = File.ReadAllLines(@"D:\test\ips.txt").Select(l => IPAddress.Parse(l)).ToArray();
                for (int ri = 0; ri < resolvers.Length; ri++)
                {
                    var r = resolvers[ri];

                    var res = new IIPRangeCountry[ips.Length];
                    var s = Stopwatch.StartNew();

                    //for (int i = 0; i < ips.Length; i++)
                    //    res[i] = r.Resolve(ips[i]);

                    Parallel.ForEach(Partitioner.Create(0, ips.Length), range =>
                    {
                        for (var index = range.Item1; index < range.Item2; index++)
                            res[index] = r.Resolve(ips[index]);
                    });


                    Console.WriteLine($"Time: {s.Elapsed.ToString("G")}\tNot found: {res.Count(i => i == null)}\tCountries: {res.Where(i => i != null).Select(i => i.Country).Distinct().Count()}\tResolves: {(int)(ips.Length / s.Elapsed.TotalSeconds),8:N0}/s");

                    //var cntcount = results[ri].Where(v => v != null).GroupBy(v => v.Country).Select(g => new { g.Key, Count = g.Count() });
                    //foreach (var ct in cntcount.OrderByDescending(v => v.Count))
                    //{
                    //    Console.WriteLine($"{ct.Key}\t{ct.Count,8}\t{ct.Count / (double)ips.Length:P2}");
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
