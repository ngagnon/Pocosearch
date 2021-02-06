using System;
using System.Runtime.InteropServices;

namespace Pocosearch
{
    /* @TODO: install right Java version automatically */
    /* @TODO: make version configurable */
    /* @TODO: check if current version matches desired version (bin/elasticsearch --version) */
    public class EmbeddedSearchEngine : ConnectionConfiguration, IDisposable
    {
        private static readonly string downloadUrl = "https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-7.10.2-{0}-x86_64.{1}";

        public EmbeddedSearchEngine()
        {
            /* @TODO: call path constructor with default path */
        }

        public EmbeddedSearchEngine(string path)
        {
            string os;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                os = "windows";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                os = "linux";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                os = "darwin";
            else
                throw new InvalidOperationException("Unknown operating system");

            var url = string.Format(downloadUrl, os, os == "windows" ? "zip" : "tar.gz");

            /* @TODO: download */
            /* @TODO: unzip */
            /* @TODO: launch subprocess */
        }

        public void Dispose()
        {
            /* @TODO */
        }
    }
}