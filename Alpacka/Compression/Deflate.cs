using System.IO.Compression;

namespace Alpacka.Compression;

public class Deflate : ICompressionSystem
{
    public static (byte[] data, AlpackFormat.CompressionType) Compress(byte[] input)
    {
        // only use compression if beneficial
        if (input.Length < 1024)
            return (input, AlpackFormat.CompressionType.None);

        using var output = new MemoryStream();
        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal))
        {
            deflate.Write(input, 0, input.Length);
        }

        var compressed = output.ToArray();

        // only use compression if it actually helped at all
        if (compressed.Length < input.Length * 0.9)
            return (compressed, AlpackFormat.CompressionType.Deflate);

        return (input, AlpackFormat.CompressionType.None);
    }

    public static byte[] Decompress(byte[] compressed, uint originalSize)
    {
        using var input = new MemoryStream(compressed);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream((int)originalSize);
        
        deflate.CopyTo(output);
        return output.ToArray();
    }
}