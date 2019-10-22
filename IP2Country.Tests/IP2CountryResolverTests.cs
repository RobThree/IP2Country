using IP2Country.Entities;
using IP2Country.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace IP2Country.Tests
{
    [TestClass]
    public class IP2CountryResolverTests
    {
        [TestMethod]
        public void ResolveIPv4_WorksCorrectly()
        {
            var target = new IP2CountryResolver(
                new MockDataSource<IPRangeCountry>(new[]
                {
                    new IPRangeCountry { Country = "AA", Start = IPAddress.Parse("0.0.0.1"), End = IPAddress.Parse("0.0.0.9")},
                    new IPRangeCountry { Country = "BB", Start = IPAddress.Parse("0.0.0.10"), End = IPAddress.Parse("0.0.0.19")},
                    new IPRangeCountry { Country = "CC", Start = IPAddress.Parse("0.0.0.20"), End = IPAddress.Parse("0.0.0.29")}
                })
            );


            Assert.IsNull(target.Resolve("0.0.0.0"));
            Assert.AreEqual("AA", target.Resolve("0.0.0.1")?.Country);
            Assert.AreEqual("AA", target.Resolve("0.0.0.9")?.Country);
            Assert.AreEqual("BB", target.Resolve("0.0.0.10")?.Country);
            Assert.AreEqual("BB", target.Resolve("0.0.0.19")?.Country);
            Assert.AreEqual("CC", target.Resolve("0.0.0.20")?.Country);
            Assert.AreEqual("CC", target.Resolve("0.0.0.29")?.Country);
            Assert.IsNull(target.Resolve("0.0.0.30"));
        }

        [TestMethod]
        public void ResolveIPv6_WorksCorrectly()
        {
            var target = new IP2CountryResolver(
                new MockDataSource<IPRangeCountry>(new[]
                {
                    new IPRangeCountry { Country = "AA", Start = IPAddress.Parse("::1"), End = IPAddress.Parse("::F")},
                    new IPRangeCountry { Country = "BB", Start = IPAddress.Parse("::10"), End = IPAddress.Parse("::1F")},
                    new IPRangeCountry { Country = "CC", Start = IPAddress.Parse("::20"), End = IPAddress.Parse("::2F")}
                })
            );


            Assert.IsNull(target.Resolve("::0"));
            Assert.AreEqual("AA", target.Resolve("::1")?.Country);
            Assert.AreEqual("AA", target.Resolve("::F")?.Country);
            Assert.AreEqual("BB", target.Resolve("::10")?.Country);
            Assert.AreEqual("BB", target.Resolve("::1F")?.Country);
            Assert.AreEqual("CC", target.Resolve("::20")?.Country);
            Assert.AreEqual("CC", target.Resolve("::2F")?.Country);
            Assert.IsNull(target.Resolve("::30"));
        }

        [TestMethod]
        public void ResolveOnEmptyDataSet_WorksCorrectly()
        {
            var target = new IP2CountryResolver(
                new MockDataSource<IPRangeCountry>(Array.Empty<IPRangeCountry>())
            );


            Assert.IsNull(target.Resolve("1.2.3.4"));
        }

        [TestMethod]
        public void ResolveSingleRecord_WorksCorrectly()
        {
            var target = new IP2CountryResolver(
                new MockDataSource<IPRangeCountry>(new[]
                {
                    new IPRangeCountry { Country = "AA", Start = IPAddress.Parse("0.0.0.1"), End = IPAddress.Parse("0.0.0.1")},
                })
            );


            Assert.IsNull(target.Resolve("0.0.0.0"));
            Assert.AreEqual("AA", target.Resolve("0.0.0.1")?.Country);
            Assert.IsNull(target.Resolve("0.0.0.2"));
        }
    }
}
