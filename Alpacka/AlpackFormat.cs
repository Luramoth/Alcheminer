namespace Alpacka;

/// <summary>
/// the overall template of the Alpack file format
/// </summary>
public static class AlpackFormat
{
    /// <summary>
    /// Magic number constant that will be found at the beginning of any .alpack file. 'ALPK'
    /// </summary>
    public const uint Magic = 0x4B504C41;   // 'ALPK' 
    /// <summary>
    /// .alpack file format version
    /// </summary>
    public const uint Version = 1;

    /// <summary>
    /// Denotes what kind of compression is used for each entry
    /// </summary>
    public enum CompressionType : ushort
    {
        /// <summary> No Compression </summary>
        None = 0,
        /// <summary> ZStandard compression (compromise between speed and compression ratio)</summary>
        Zstd = 1,
        /// <summary> Deflate compression (slowest speed, max compression)</summary>
        Deflate = 2,
        /// <summary> LZ4 compression (fastest speed, worst compression)</summary>
        Lz4 = 3,
    }

    /// <summary>
    /// header at the start of the file
    /// </summary>
    public struct Header
    {
        /// <summary> Number that identifies an Alpacka file 'ALPK' </summary>
        public uint Magic;
        /// <summary> Version of the format </summary>
        public uint Version;
        /// <summary> Amount of files stored in the archive </summary>
        public uint EntryCount;
        /// <summary> Location in which the file data begins within the archive </summary>
        public uint DataOffset;
        /// <summary> Where file string table is stored to store the file paths </summary>
        public uint StringTableOffset;
        /// <summary> The beginning of the file index where file archive metadata is stored </summary>
        public uint IndexOffset;
        /// <summary> Padding </summary>
        public uint Reserved;
    }
    
    /// <summary>
    /// metadata entry per file
    /// </summary>
    public struct Entry
    {
        /// <summary> the file path made into a has using FNV-1a </summary>
        public ulong PathHash;
        /// <summary> offset to the data inside the data section </summary>
        public uint DataOffset;
        /// <summary> size of the file when compressed </summary>
        public uint CompressedSize;
        /// <summary> the original size of the file before compression </summary>
        public uint OriginalSize;
        /// <summary> type of compression used on file (refer to <see cref="CompressionType"/>) </summary>
        public ushort CompressionType;
        /// <summary> offset of the file's name inside the string table </summary>
        public uint NameOffset;
        /// <summary> padding </summary>
        public uint Reserved;
    }

    /// <summary>
    /// Hash a path string (FNV-1a 64-bit)
    /// </summary>
    /// <param name="path">file path</param>
    /// <returns>hashed file path</returns>
    public static ulong HashPath(string path)
    {
        const ulong offset = 14695981039346656037;  // FNV offset basis for bit distribution
        const ulong prime = 1099511628211;          // FNV prime, large number to spread bits well

        ulong hash = offset;
        foreach (var c in path.ToLowerInvariant().Replace('\\', '/'))
        {
            hash ^= c;      // XOR with character
            hash *= prime;  // Multiply with prime
        }

        return hash;
    }
}