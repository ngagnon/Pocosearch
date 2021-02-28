using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using Elasticsearch.Net;
using Pocosearch.Utils;

namespace Pocosearch
{
    public class EmbeddedSearchEngine : ConnectionConfiguration, IDisposable
    {
        private static readonly string currentVersion = "7.10.2";
        private static readonly string downloadUrl = $"https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-{currentVersion}-{{0}}";

        private readonly Process process;

        private EmbeddedSearchEngine(Process process) 
        {
            this.process = process;
        }

        public static EmbeddedSearchEngine Launch()
        {
            var enginePath = Path.Combine(
                Directory.GetCurrentDirectory(), ".embedded-engine");

            return Launch(enginePath);
        }

        public static EmbeddedSearchEngine Launch(string enginePath)
        {
            var exe = GetExecutablePath(enginePath);

            if (!File.Exists(exe))
                FetchSearchEngine(enginePath);

            var startInfo = new ProcessStartInfo(exe);
            var process = Process.Start(startInfo);

            return new EmbeddedSearchEngine(process);
        }

        private static void FetchSearchEngine(string enginePath)
        {
            var downloadFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(downloadFolder);

            var downloadUrl = GetDownloadUrl();
            var filename = Path.GetFileName(downloadUrl.LocalPath);
            var archivePath = Path.Combine(downloadFolder, filename);
            var webClient = new WebClient();
            webClient.DownloadFile(downloadUrl, archivePath);

            if (filename.EndsWith(".zip"))
                ZipFile.ExtractToDirectory(archivePath, enginePath);
            else
                TarGzFile.ExtractToDirectory(archivePath, enginePath);

            File.Delete(archivePath);
        }

        private static string GetExecutablePath(string enginePath)
        {
            var path = $"elasticsearch-{currentVersion}/bin/elasticsearch";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                path += ".exe";

            return Path.Combine(enginePath, path);
        }

        private static Uri GetDownloadUrl()
        {
            string os;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                os = "windows-x86_64.zip";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                os = "linux-x86_64.tar.gz";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                os = "darwin-x86_64.tar.gz";
            else
                throw new InvalidOperationException("Unknown operating system");

            return new Uri(string.Format(downloadUrl, os));
        }

        public void Dispose()
        {
            if (!process.HasExited)
            {
                process.Kill();
            }

            process.Dispose();
        }
    }
}