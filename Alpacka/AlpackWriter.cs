using System.Runtime.CompilerServices;
using System.Text;

using Alpacka.Compression;

namespace Alpacka;

/// <summary>
/// Creates Alpacka archive files (.alpack)
/// </summary>
public class AlpackWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;
    private readonly List<FileEntry> _entries = new();
    private uint _dataOffset;
    private bool _finalised;

    private class FileEntry
    {
        public required string RelativePath { get; init; }
        public required byte[] Data { get; init; }
    }

    /// <summary>
    /// creates a AlpackWriter for the file specified
    /// </summary>
    /// <param name="outputPath"></param>
    public AlpackWriter(string outputPath)
    {
        _stream = File.Create(outputPath);
        _writer = new BinaryWriter(_stream, Encoding.UTF8);
        
        // Reserve header space
        int headerSize = Unsafe.SizeOf<AlpackFormat.Header>();
        _writer.Write(new byte[headerSize]);
        _dataOffset = (uint)headerSize;
    }

    /// <summary>
    /// Add file to the archive
    /// </summary>
    /// <param name="relativePath">File path inside pack</param>
    /// <param name="data">File data</param>
    /// <exception cref="InvalidOperationException">Thrown if file is already finished and finalized</exception>
    public void AddFile(string relativePath, byte[] data)
    {
        if (_finalised) throw new InvalidOperationException("Already Finalised");

        _entries.Add(new FileEntry
        {
            RelativePath = relativePath.ToLowerInvariant().Replace('\\', '/'),
            Data = data
        });
    }

    /// <summary>
    /// Add file from disk
    /// </summary>
    /// <param name="relativePath">File path from inside pack</param>
    /// <param name="sourcePath">File path from the disk to the file</param>
    public void AddFileFromDisk(string relativePath, string sourcePath)
    {
        AddFile(relativePath, File.ReadAllBytes(sourcePath));
    }

    /// <summary>
    /// Write the file out
    /// </summary>
    public void Finalise()
    {
        if (_finalised) return;
        _finalised = true;
        
        // Write Data section
        var entryInfos = new List<(AlpackFormat.Entry entry, string path)>();

        // For each file added, write its data to the archive
        foreach (var file in _entries)
        {
            var(compressed, compressionType) = Zstd.Compress(file.Data);
            
            var entry = new AlpackFormat.Entry
            {
                PathHash = AlpackFormat.HashPath(file.RelativePath),
                DataOffset = _dataOffset,
                CompressedSize = (uint)compressed.Length,
                OriginalSize = (uint)file.Data.Length,
                CompressionType = (ushort)compressionType,
                NameOffset = 0
            };
            
            entryInfos.Add((entry, file.RelativePath));
            _writer.Write(compressed);
            _dataOffset += (uint)compressed.Length;
        }
        
        // Write string table
        var stringTableOffset = (uint)_stream.Position;
        var stringOffsets = new Dictionary<string, uint>();

        foreach (var (_, path) in entryInfos)
        {
            stringOffsets[path] = (uint)(_stream.Position - stringTableOffset);
            _writer.Write(Encoding.UTF8.GetBytes(path));
            _writer.Write((byte)0);
        }

        var indexOffset = (uint)_stream.Position;
        
        // write index
        foreach (var (entry, path) in entryInfos)
        {
            var finalEntry = entry with { NameOffset = stringOffsets[path] };
            
            _writer.Write(finalEntry.PathHash);
            _writer.Write(finalEntry.DataOffset);
            _writer.Write(finalEntry.CompressedSize);
            _writer.Write(finalEntry.OriginalSize);
            _writer.Write(finalEntry.CompressionType);
            _writer.Write(finalEntry.NameOffset);
            _writer.Write(finalEntry.Reserved);
        }
        
        // Write header
        _stream.Position = 0;

        var header = new AlpackFormat.Header()
        {
            Magic = AlpackFormat.Magic,
            Version = AlpackFormat.Version,
            EntryCount = (uint)_entries.Count,
            DataOffset = (uint)Unsafe.SizeOf<AlpackFormat.Header>(),
            StringTableOffset = stringTableOffset,
            IndexOffset = indexOffset
        };
        
        _writer.Write(header.Magic);
        _writer.Write(header.Version);
        _writer.Write(header.EntryCount);
        _writer.Write(header.DataOffset);
        _writer.Write(header.StringTableOffset);
        _writer.Write(header.IndexOffset);
        _writer.Write(header.Reserved);
    }

    /// <summary>
    /// Releases all resources from the AlpackWriter class
    /// </summary>
    public void Dispose()
    {
        Finalise();
        _writer.Dispose();
        _stream.Dispose();
    }
}