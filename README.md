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

## Icon
Source: [ShareIcon.Net](https://www.shareicon.net/internet-marketing-geo-geo-location-geomarketing-ip-address-isp-address-target-888208)
Author: [Ayesha Nasir](https://www.shareicon.net/author/ayesha-nasir)
License: [Creative Commons (Attribution 3.0 Unported)](https://creativecommons.org/licenses/by/3.0/)
<hr>
[![Build status](https://ci.appveyor.com/api/projects/status/bs1l4mjdnlusv4n5)](https://ci.appveyor.com/project/RobIII/ip2country) <a href="https://www.nuget.org/packages/IP-2-Country/"><img src="http://img.shields.io/nuget/v/IP-2-Country.svg?style=flat-square" alt="NuGet version" height="18"></a>
