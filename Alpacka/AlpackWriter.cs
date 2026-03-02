using System.Text;

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

    public AlpackWriter(string outputPath)
    {
        _stream = File.Create(outputPath);
        _writer = new BinaryWriter(_stream, Encoding.UTF8);
        
        // Reserve header space
        _writer.Write(new byte[24]);
        _dataOffset = 24;
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
            var entry = new AlpackFormat.Entry
            {
                PathHash = AlpackFormat.HashPath(file.RelativePath),
                DataOffset = _dataOffset,
                Size = (uint)file.Data.Length,
                NameOffset = 0
            };
            
            entryInfos.Add((entry, file.RelativePath));
            _writer.Write(file.Data);
            _dataOffset += (uint)file.Data.Length;
        }
        
        var dataSectionEnd = (uint)_stream.Position;
        
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
            _writer.Write(finalEntry.Size);
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
            DataOffset = 24,
            IndexOffset = indexOffset
        };
        
        _writer.Write(header.Magic);
        _writer.Write(header.Version);
        _writer.Write(header.EntryCount);
        _writer.Write(header.DataOffset);
        _writer.Write(header.IndexOffset);
        _writer.Write(header.Reserved);
    }

    public void Dispose()
    {
        Finalise();
        _writer.Dispose();
        _stream.Dispose();
    }
}