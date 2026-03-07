using ZstdNet;

namespace Alpacka.Compression;

public class Zstd : ICompressionSystem
{
    public static (byte[] data, AlpackFormat.CompressionType) Compress(byte[] input)
    {
        // only use compression if beneficial
        if (input.Length < 1024)
            return (input, AlpackFormat.CompressionType.None);
        
        using var compressor = new Compressor();
        var compressed = compressor.Wrap(input);
        
        if (compressed.Length < input.Length * 0.9)
            return (compressed, AlpackFormat.CompressionType.Zstd);

        return (input, AlpackFormat.CompressionType.None);
    }

    public static byte[] Decompress(byte[] compressed, uint originalSize)
    {
        using var decompressor = new Decompressor();
        var result = decompressor.Unwrap(compressed, (int)originalSize);

        return result;
    }
}