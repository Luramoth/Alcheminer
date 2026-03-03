struct header {
    u32 MagicNumber;
    u32 Version;
    u32 EntryCount;
    u32 DataOffset;
    u32 IndexOffset;
    u32 Reserved [[color("f9f9f9")]];
};

struct entry {
    u64 PathHash;
    u32 DataOffset;
    u32 Size;
    u32 NameOffset;
    u32 Reserved [[color("f9f9f9")]];
    
    u8 data[Size] @ DataOffset;
    
    char name[] @ NameOffset;
};

struct alpack {
    header Header;
    
    entry entries[Header.EntryCount] @ Header.IndexOffset;
};

alpack Alpack @ 0x00;