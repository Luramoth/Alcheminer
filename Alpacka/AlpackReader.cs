using System.Text;

using Alpacka.Compression;

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
    
    /// <summary>
    /// Path to the .alpack file
    /// </summary>
    public string FilePath { get; }
    /// <summary>
    /// Number of files in the archive
    /// </summary>
    public int EntryCount => (int)_header.EntryCount;
    
    /// <summary>
    /// creates a new AlpackReader for the .alpack archive
    /// </summary>
    /// <param name="path">Path to the .alpack file</param>
    /// <exception cref="InvalidDataException">Thrown if the .alpack file is not a valid .alpack file</exception>
    /// <exception cref="NotSupportedException">Thrown if the version is too old or new</exception>
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
                CompressedSize = _reader.ReadUInt32(),
                OriginalSize = _reader.ReadUInt32(),
                CompressionType = _reader.ReadUInt16(),
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
            (ushort)AlpackFormat.CompressionType.Zstd => Zstd.Decompress(compressed, entry.OriginalSize),
            (ushort)AlpackFormat.CompressionType.Deflate => Deflate.Decompress(compressed, entry.OriginalSize),
            (ushort)AlpackFormat.CompressionType.Lz4 => LZ4.Decompress(compressed, entry.OriginalSize),
            _ => throw new NotSupportedException($"Compression type: {entry.CompressionType}")
        };
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

    /// <summary>
    /// Gives you a list of all the files in the archive
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> ListFiles()
    {
        foreach (var entry in _index.Values)
        {
            yield return GetPath(entry.PathHash) ?? $"[hash:{entry.PathHash:X}]";
        }
    }
    
    /// <summary>
    /// Releases all resources from the AlpackReader class
    /// </summary>
    public void Dispose()
    {
        _reader.Dispose();
        _stream.Dispose();
    }
}