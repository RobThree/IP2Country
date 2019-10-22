using DemoWebService.Helpers;
using IP2Country.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DemoWebService.Controllers
{
    [Route("lookup")]
    [ApiController]
    public class IPLookupController : ControllerBase
    {
        private IAutoReloadingResolver _resolver;

        public IPLookupController(IAutoReloadingResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        [HttpGet]
        [Route("ip")]
        public IIPRangeCountry Get(string q) => _resolver.Resolve(q);

        [HttpGet]
        [Route("ips")]
        public IDictionary<string, IIPRangeCountry> Get([FromQuery(Name = "q")] string[] q) //https://github.com/aspnet/Mvc/issues/7712#issuecomment-397003420
=> _resolver.ResolveAsDictionary(q);

        [HttpPost]
        [Route("ips")]
        public IDictionary<string, IIPRangeCountry> Post([FromBody] string[] q) => _resolver.ResolveAsDictionary(q);
    }
}
