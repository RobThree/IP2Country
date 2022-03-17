using IP2Country.Datasources;
using IP2Country.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace IP2Country.DataSources.CSVFile
{
    public abstract class IP2CountryCSVFileSource<T> : IIP2CountryDataSource
        where T : IIPRangeCountry
    {
        public Encoding Encoding { get; private set; }
        public string Path { get; private set; }
        public ICSVRecordParser<T> Parser { get; private set; }

        public IP2CountryCSVFileSource(string path, ICSVRecordParser<T> parser)
            : this(path, parser, Encoding.UTF8) { }

        public IP2CountryCSVFileSource(string path, ICSVRecordParser<T> parser, Encoding encoding)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        private static bool CheckMagicNumber(string path, int offset, byte[] magicnumber)
        {
            var buffer = new byte[magicnumber.Length];
            using (var fs = File.OpenRead(path))
            {
                fs.Read(buffer, offset, buffer.Length);
                for (var i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] != magicnumber[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected static bool IsZIP(string path)
            => CheckMagicNumber(path, 0, new byte[] { 0x50, 0x4B, 0x03, 0x04 });


        protected static bool IsGZ(string path)
            => CheckMagicNumber(path, 0, new byte[] { 0x1F, 0x8B });

        protected IEnumerable<TRecord> ReadFile<TRecord>(string path, ICSVRecordParser<TRecord> parser)
            where TRecord : IIPRangeCountry
        {
            if (IsZIP(path))
            {
                using (var zipArchive = new ZipArchive(File.OpenRead(path), ZipArchiveMode.Read))
                {
                    foreach (var entry in zipArchive.Entries)
                    {
                        foreach (var r in Read(entry.Open(), parser))
                        {
                            yield return r;
                        }
                    }
                }
            }
            else if (IsGZ(path))
            {
                using (var s = File.OpenRead(path))
                using (var g = new GZipStream(s, CompressionMode.Decompress))
                {
                    foreach (var r in Read(g, parser))
                    {
                        yield return r;
                    }
                }
            }
            else
            {
                using (var s = File.OpenRead(path))
                {
                    foreach (var r in Read(s, parser))
                    {
                        yield return r;
                    }
                }
            }
        }

        public IEnumerable<TRecord> Read<TRecord>(Stream stream, ICSVRecordParser<TRecord> parser)
            where TRecord : IIPRangeCountry
            => ReadLines(stream)
                .Where(l => l.Length > 0 && l[0] != '#' && !char.IsWhiteSpace(l[0]))
                .Select(l => parser.ParseRecord(l))
                .Where(r => r != null);

        private IEnumerable<string> ReadLines(Stream stream)
        {
            using (var r = new StreamReader(stream, Encoding))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public abstract IEnumerable<IIPRangeCountry> Read();
    }
}
