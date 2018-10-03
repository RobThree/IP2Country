using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace DemoWebService.Helpers
{
    public class WebClient
    {
        public static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(30);

        private static readonly string _appname = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string _appversion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        private readonly HttpClient _httpclient;

        public IWebProxy Proxy { get; set; }
        public ICredentials Credentials { get; set; }

        public WebClient(TimeSpan? timeOut = null, ICredentials credentials = null, IWebProxy proxy = null)
        {
            Credentials = credentials;
            Proxy = proxy;

            _httpclient = new HttpClient(GetHttpClientHandler())
            {
                Timeout = timeOut ?? DEFAULT_TIMEOUT,
            };

            _httpclient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_appname, _appversion));
        }

        public Task<Stream> DownloadAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            return DownloadAsync(new Uri(url));
        }

        public async Task<Stream> DownloadAsync(Uri url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            return await _httpclient.GetStreamAsync(url);
        }

        private HttpClientHandler GetHttpClientHandler()
        {
            return new HttpClientHandler()
            {
                Proxy = Proxy,
                UseProxy = Proxy != null,
                PreAuthenticate = Credentials != null,
                UseDefaultCredentials = Credentials != null,
                Credentials = Credentials,
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                MaxAutomaticRedirections = 3,
                UseCookies = false
            };
        }
    }
}