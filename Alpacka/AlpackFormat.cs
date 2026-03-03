namespace Alpacka;

public static class AlpackFormat
{
    public const uint Magic = 0x414C504B;   // 'ALPK'
    public const uint Version = 1;

    /// <summary>
    /// 24-byte header at the start of the file
    /// </summary>
    public struct Header
    {
        public uint Magic;
        public uint Version;
        public uint EntryCount;
        public uint DataOffset;         // Where file data starts
        public uint StringTableOffset;  // Where String Table starts
        public uint IndexOffset;        // Where index starts
        public uint Reserved;           // Padding
    }
    
    /// <summary>
    /// 24-byte index entry per file
    /// </summary>
    public struct Entry
    {
        public ulong PathHash;  // FNV-1a hash
        public uint DataOffset;     // Offset in data section
        public uint Size;       // File size
        public uint NameOffset; // Offset in string table
        public uint Reserved;   // Padding
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