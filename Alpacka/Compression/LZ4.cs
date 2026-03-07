using K4os.Compression.LZ4;

namespace Alpacka.Compression;

public class LZ4 : ICompressionSystem
{
    public static (byte[] data, AlpackFormat.CompressionType) Compress(byte[] input)
    {
        // only use compression if beneficial
        if (input.Length < 1024)
            return (input, AlpackFormat.CompressionType.None);

        int maxCompressedSize = LZ4Codec.MaximumOutputSize(input.Length);
        var compressed = new byte[maxCompressedSize];

        int compressedSize = LZ4Codec.Encode(
            input, 0, input.Length,
            compressed, 0, compressed.Length,
            LZ4Level.L12_MAX);
        
        if (compressedSize < compressed.Length)
            Array.Resize(ref compressed, compressedSize);

        if (compressed.Length < input.Length * 0.9)
            return (compressed, AlpackFormat.CompressionType.Lz4);

        return (input, AlpackFormat.CompressionType.None);
    }

    public static byte[] Decompress(byte[] compressed, uint originalSize)
    {
        var result = new byte[originalSize];

        int decoded = LZ4Codec.Decode(
            compressed, 0, compressed.Length,
            result, 0, result.Length);

        if (decoded != originalSize)
            throw new InvalidOperationException($"Size mismatch");

        return result;
    }
}