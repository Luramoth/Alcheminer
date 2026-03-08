namespace Alpacka.Compression;

public interface ICompressionSystem
{
    public static abstract (byte[] data, AlpackFormat.CompressionType) Compress(byte[] input, bool forceCompression);
    public static abstract byte[] Decompress(byte[] compressed, uint originalSize);
}