using System.Text;
using ZstdSharp;

namespace Alpacka;

/// <summary>
/// Reads Alpacka archive file (.alpack)
/// </summary>
public class AlpackReader : IDisposable
{
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly AlpackFormat.Header _header;
    private readonly Dictionary<ulong, AlpackFormat.Entry> _index = new();
    
    public string FilePath { get; }
    public int EntryCount => (int)_header.EntryCount;
    
    public AlpackReader(string path)
    {
        FilePath = path;
        _stream = File.OpenRead(path);
        _reader = new BinaryReader(_stream, Encoding.UTF8);
        
        // Read header
        _header = new AlpackFormat.Header
        {
            Magic = _reader.ReadUInt32(),
            Version = _reader.ReadUInt32(),
            EntryCount = _reader.ReadUInt32(),
            DataOffset = _reader.ReadUInt32(),
            StringTableOffset = _reader.ReadUInt32(),
            IndexOffset = _reader.ReadUInt32(),
            Reserved = _reader.ReadUInt32()
        };

        if (_header.Magic != AlpackFormat.Magic)
            throw new InvalidDataException("Invalid Alpacka file");
        if (_header.Version != AlpackFormat.Version)
            throw new NotSupportedException($"Version {_header.Version} is not supported.");
        
        // Read index
        _stream.Position = _header.IndexOffset;
        for (int i = 0; i < _header.EntryCount; i++)
        {
            var entry = new AlpackFormat.Entry
            {
                PathHash = _reader.ReadUInt64(),
                DataOffset = _reader.ReadUInt32(),
                OriginalSize = _reader.ReadUInt32(),
                NameOffset = _reader.ReadUInt32(),
                Reserved = _reader.ReadUInt32()
            };
            _index[entry.PathHash] = entry;
        }
    }

    /// <summary>
    /// Check if a file exists in the archive
    /// </summary>
    /// <param name="path">Path to the file inside the archive</param>
    /// <returns>True if the file exists</returns>
    public bool Contains(string path)
    {
        var hash = AlpackFormat.HashPath(path);
        return _index.ContainsKey(hash);
    }

    /// <summary>
    /// Reads file data from the archive
    /// </summary>
    /// <param name="path">Path to the file from within the archive</param>
    /// <returns>File data</returns>
    /// <exception cref="FileNotFoundException">Throw if file does not exist</exception>
    public byte[] ReadFile(String path)
    {
        var hash = AlpackFormat.HashPath(path);

        if (!_index.TryGetValue(hash, out var entry))
            throw new FileNotFoundException($"File not found in archive: {path}");

        _stream.Position = entry.DataOffset;
        var compressed = _reader.ReadBytes((int)entry.CompressedSize);

        return entry.CompressionType switch
        {
            (ushort)AlpackFormat.CompressionType.None => compressed,
            (ushort)AlpackFormat.CompressionType.Zstd => Decompress(compressed, entry.OriginalSize),
            _ => throw new NotSupportedException($"Compression type: {entry.CompressionType}")
        };
    }

    private static byte[] Decompress(byte[] compressed, uint originalSize)
    {
        using var decompressor = new Decompressor();
        var result = decompressor.Unwrap(compressed);
        return result.ToArray();
    }

    /// <summary>
    /// Get original file path from hash
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public string? GetPath(ulong hash)
    {
        if (!_index.TryGetValue(hash, out var entry))
            return null;

        _stream.Position = _header.StringTableOffset + entry.NameOffset;
        var bytes = new List<byte>();

        byte b;
        while ((b = _reader.ReadByte()) != 0)
            bytes.Add(b);

        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    public IEnumerable<string> ListFiles()
    {
        foreach (var entry in _index.Values)
        {
            yield return GetPath(entry.PathHash) ?? $"[hash:{entry.PathHash:X}]";
        }
    }
    
    public void Dispose()
    {
        _reader.Dispose();
        _stream.Dispose();
    }
}