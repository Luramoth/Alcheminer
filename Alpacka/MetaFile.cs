using Tomlyn;

namespace Alpacka;

public class MetaFile
{
    public PackSettings Pack { get; set; } = new();
    
    public class PackSettings
    {
        public string Compression { get; set; } = "zstd";
        public string ForceCompression { get; set; } = "false";
    }

    public static MetaFile LoadOrDefault(string metaPath)
    {
        if (!File.Exists(metaPath))
            return new MetaFile();

        var text = File.ReadAllText(metaPath);
        return TomlSerializer.Deserialize<MetaFile>(text)!;
    }

    public AlpackFormat.CompressionType GetCompressionType()
    {
        return Pack.Compression.ToLower() switch
        {
            "none" => AlpackFormat.CompressionType.None,
            "deflate" => AlpackFormat.CompressionType.Deflate,
            "lz4" => AlpackFormat.CompressionType.Lz4,
            "zstd" => AlpackFormat.CompressionType.Zstd,
            _ => AlpackFormat.CompressionType.Zstd
        };
    }

    public bool GetForceCompression()
    {
        return Pack.ForceCompression.ToLower().Equals("true");
    }
}