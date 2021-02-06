using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Pocosearch.Utils
{
    public static class TarGzFile
    {
        public static void ExtractToDirectory(string filename, string outputDir)
        {
            using (var stream = File.OpenRead(filename))
                ExtractToDirectory(stream, outputDir);
        }

        public static void ExtractToDirectory(Stream stream, string outputDir)
        {
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            using (var mem = new MemoryStream())
            {
                gzip.CopyTo(mem);
                mem.Seek(0, SeekOrigin.Begin);
                ExtractTar(mem, outputDir);
            }
        }

        private static void ExtractTar(Stream stream, string outputDir)
        {
            var buffer = new byte[512];
            var eof = false;

            while (!eof)
            {
                int readBytes = stream.Read(buffer, 0, 512);

                if (readBytes < 512 || buffer.All(b => b == 0))
                    eof = true;
                else
                    ExtractTarRecord(buffer, stream, outputDir);
            }
        }

        private static void ExtractTarRecord(byte[] header, Stream stream, string outputDir)
        {
            var name = Encoding.ASCII.GetString(header.Slice(100)).Trim('\0');
            var namePrefix = Encoding.ASCII.GetString(header.Slice(345, 155)).Trim('\0');
            name = Path.Combine(namePrefix, name);

            var output = Path.Combine(outputDir, name);
            var type = header[156];

            if (type == '5')
                return;

            var directoryName = Path.GetDirectoryName(output);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var sizeBuffer = header.Slice(124, 11);
            var sizeString = Encoding.ASCII.GetString(sizeBuffer);
            var size = Convert.ToInt64(sizeString, 8);

            using (var fileStream = File.Open(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var buf = new byte[size];
                stream.Read(buf, 0, buf.Length);
                fileStream.Write(buf, 0, buf.Length);
            }

            var mode = Encoding.ASCII.GetString(header.Slice(100, 7)).TrimStart('0');
            ChangeFileMode(output, mode);

            var paddingBytes = stream.Position % 512;

            if (paddingBytes > 0)
            {
                var offset = 512 - paddingBytes;
                stream.Seek(offset, SeekOrigin.Current);
            }
        }

        private static void ChangeFileMode(string path, string mode)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = "chmod";
                process.StartInfo.Arguments = $"{mode} \"{path}\"";
                process.Start();
                process.WaitForExit();
            }
        }
    }
}