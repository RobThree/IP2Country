# ![Logo](https://raw.githubusercontent.com/RobThree/IP2Country/master/icons/icon.png) IP2Country
Library to map IP addresses (both IPv4 and IPv6) to a country. The accuracy depends on the data provider; a lot of providers are supported 'out of the box':

* All registries ([RIPE](https://www.ripe.net/), [APNIC](https://www.apnic.net/), [ARIN](https://www.arin.net/), [LACNIC](http://www.lacnic.net/) and [AFRINIC](https://www.afrinic.net/))
* [MaxMind](https://www.maxmind.com)
* [DB-IP](https://db-ip.com/)
* [IP2IQ](http://www.ip2iq.com/)
* [IPToASN](https://iptoasn.com/)
* [Ludost](https://ip.ludost.net/)
* [Markus Go's ip-countryside](https://github.com/Markus-Go/ip-countryside/)
* [WebNet77](https://webnet77.net/geo-ip/)

This library provides an easy to implement interface (`IIP2CountryDataSource`) to provide your own data to the `IP2CountryResolver`. This library only aims for 'country level resolution'; city, ISP etc. information _can_ be returned (when provided) but is not the goal of this library. All data is stored in entities / models derived from `IIPRangeCountry` which, at the very minimum, must provide a start- and end IP address (IPv4 and/or IPv6, supported completely transparently) and country (_usually_ an [ISO 3166-1 alpha-2](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2) country code, but may contain other data depending on the data source).

## Usage

In general, you'll want to use one of the above mentioned datasources. You can combine them but it's recommended to use a single one. Install the desired NuGet package (i.e. `IP-2-Country.MaxMind`) which will also pull in the base package (`IP-2-Country`) and the generic CSV parser (`IP-2-Country.DataSources.CSVFile`). Download the datafiles (example code is provided in the DemoApp in this repository) and create an IP2CountryResolver object:

```c#
var resolver = new IP2CountryResolver(new IIP2CountryDataSource[] {
    new MaxMindGeoLiteIPCSVFileSource(@"D:\files\maxmindipv4.dat"),
    new MaxMindGeoLiteIPCSVFileSource(@"D:\files\maxmindipv6.dat"))
});
```

And you're done! Now you can resolve IP addresses:

```c#
var result = resolver.Resolve("172.217.17.110");
Console.WriteLine("Country: " + result?.Country);
```

Depending on the datasource some more information _may_ be returned (for example: the `IP-2-Country.Registries` package returns the `Registry`, `Date`, `Status` and even some `Extensions` when available). You may need to cast to the returned type in case you need more than the` IIPRangeCountry` interface provides:

```c#
var result = (RegistryIPRangeCountry)resolver.Resolve("172.217.17.110");
```

## Datasources

Besides the provided datasources it's very simple to implement your own datasource; all you need to do is implement a single method (`IEnumerable<IIPRangeCountry> Read()`) from a single interface (`IIP2CountryDataSource`). It's up to you wether you want to use a CVS file, binary file, database, Excel file or whatever other method of storage you can imagine. As long as you can provide IP ranges (as start/end pairs) and country data (as string) as an IEnumerable source you're done. This repository has lots of examples of CSV datasources but it's perfectly fine to build and provide your own datasource.

### A word on the actual data

The provided datasources are nothing but simple "CSV file readers". It's up to you to download the actual data files (and cache them for later use). The DemoApp contains an example of downloading files into a temp directory and caching them. Ofcourse you're free to implement your own download/cache mechanism. As long as you can provide a datasource that implements the `IIP2CountryDataSource` interface this library doesn't care about the rest.

More on actual, compatible, databases can be found [in the wiki](../../wiki/IP-to-country-databases)

## Acuracy

The acuracy depends entirely on the acuracy of the datasource. This library has no built-in data whatsoever. You _may_ also want to ensure your datasource doesn't provide ranges to the `IP2CountryResolver` that overlap. It's up to you / the datasource to ensure the data is as acurate as possible (and as up-to-date as possible).

## Benchmarks / performance:

On an [Intel Xeon E3-1225 v3](https://ark.intel.com/products/75461/Intel-Xeon-Processor-E3-1225-v3-8M-Cache-3_20-GHz) CPU it's perfectly possible to achieve over **2,000,000 lookups per second** (yes, that is over 2 million!). Ofcourse results will vary depending on the size of the dataset, the usage, the CPU and other factors.

Not that when an instance of an `IP2CountryResolver` is created it will load **all** the data from the datasource into memory. Naturally this is a costly, though one-time, operation. As long as the `IP2CountryResolver` object is kept alive you can (re)use it to resolve IP adresses. We recommend creating an instance once and keeping it around as long as possible. Create a new instance and swap it out with the old one if there is new data available from the datasource.

## Icon
Source: [ShareIcon.Net](https://www.shareicon.net/internet-marketing-geo-geo-location-geomarketing-ip-address-isp-address-target-888208)
Author: [Ayesha Nasir](https://www.shareicon.net/author/ayesha-nasir)
License: [Creative Commons (Attribution 3.0 Unported)](https://creativecommons.org/licenses/by/3.0/)
<hr>
[![Build status](https://ci.appveyor.com/api/projects/status/bs1l4mjdnlusv4n5)](https://ci.appveyor.com/project/RobIII/ip2country) <a href="https://www.nuget.org/packages/IP-2-Country/"><img src="http://img.shields.io/nuget/v/IP-2-Country.svg?style=flat-square" alt="NuGet version" height="18"></a>
