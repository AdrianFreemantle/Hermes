using System.IO;
using System.IO.Compression;

namespace Hermes.Compression
{
    public static class GzipCompressor
    {
        public static byte[] Compress(Stream input)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                input.CopyTo(zipStream);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static Stream Decompress(byte[] data)
        {
            var output = new MemoryStream();
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                zipStream.CopyTo(output);
                zipStream.Close();
                output.Position = 0;
                return output;
            }
        }
    }
}
