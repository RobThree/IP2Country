using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DemoWebService.Helpers
{
    public class CachingWebClient : WebClient
    {
        public TimeSpan DefaultTTL { get; set; } = TimeSpan.FromDays(1);

        public CachingWebClient(TimeSpan? timeOut = null, ICredentials credentials = null, IWebProxy proxy = null)
            : base(timeOut, credentials, proxy) { }

        public Task<string> DownloadAsync(string url, string path, TimeSpan? ttl = null)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            return DownloadAsync(new Uri(url), path, ttl);
        }

        public async Task<string> DownloadAsync(Uri url, string path, TimeSpan? ttl = null)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (IsFileExpired(path, ttl ?? DefaultTTL))
            {

                using (var filestream = File.Create(path))
                using (var contentstream = DownloadAsync(url))
                {
                    await (await contentstream.ConfigureAwait(false))
                        .CopyToAsync(filestream).ConfigureAwait(false);
                }
            }
            return path;
        }

        private static bool IsFileExpired(string path, TimeSpan ttl) => (!File.Exists(path) || (DateTime.UtcNow - new FileInfo(path).LastWriteTimeUtc) > ttl);
    }
}