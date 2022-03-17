using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace DemoApp
{
    public class WebClient : IDisposable
    {
        public static readonly TimeSpan DEFAULTTIMEOUT = TimeSpan.FromSeconds(30);

        private static readonly string _appname = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string _appversion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        private readonly HttpClient _httpclient;
        private readonly HttpClientHandler _httpclienthandler;

        public IWebProxy? Proxy { get; set; }
        public ICredentials? Credentials { get; set; }

        public WebClient(TimeSpan? timeOut = null, ICredentials? credentials = null, IWebProxy? proxy = null)
        {
            Credentials = credentials;
            Proxy = proxy;

            _httpclienthandler = GetHttpClientHandler();
            _httpclient = new HttpClient(_httpclienthandler)
            {
                Timeout = timeOut ?? DEFAULTTIMEOUT,
            };

            _httpclient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_appname, _appversion));
        }

        public Task<Stream> DownloadAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            return DownloadAsync(new Uri(url));
        }

        public Task<Stream> DownloadAsync(Uri url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            return _httpclient.GetStreamAsync(url);
        }

        private HttpClientHandler GetHttpClientHandler()
            => new()
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

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpclient?.Dispose();
                    _httpclienthandler.Dispose();
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
