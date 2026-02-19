using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AlchemyEngine.Core.Ecs;

internal sealed class Chunk
{
    internal const int ChunkSizeBytes = 16384;

    private readonly byte[] _data;
    private readonly int[] _componentOffsets;
    private readonly int[] _componentSizes;
    private readonly int _entityCapacity;

    internal int Count;
    internal int[] Entities;

    internal Chunk(ComponentType[] componentTypes)
    {
        int stride = CalculateStride(componentTypes);
        _entityCapacity = stride > 0 ? Math.Max(1, ChunkSizeBytes / stride) : ChunkSizeBytes;
        _componentOffsets = new int[componentTypes.Length];
        _componentSizes = new int[componentTypes.Length];

        int offset = 0;
        for (int i = 0; i < componentTypes.Length; i++)
        {
            _componentSizes[i] = componentTypes[i].Size;
            _componentOffsets[i] = offset;
            offset += _componentSizes[i] * _entityCapacity;
        }

        _data = new byte[Math.Max(offset, 1)];
        Entities = new int[_entityCapacity];
        Count = 0;
    }

    internal int Capacity => _entityCapacity;
    internal bool IsFull => Count >= _entityCapacity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetComponent<T>(int componentSlot, int indexInChunk) where T : struct
    {
        int byteOffset = _componentOffsets[componentSlot] + indexInChunk * _componentSizes[componentSlot];
        return ref Unsafe.As<byte, T>(ref _data[byteOffset]);
    }

    internal int Add(int entityId)
    {
        int index = Count++;
        Entities[index] = entityId;
        return index;
    }

    internal void RemoveAt(int index, int lastIndex)
    {
        if (index != lastIndex)
        {
            Entities[index] = Entities[lastIndex];
            for (int slot = 0; slot < _componentOffsets.Length; slot++)
            {
                int size = _componentSizes[slot];
                int srcOffset = _componentOffsets[slot] + lastIndex * size;
                int dstOffset = _componentOffsets[slot] + index * size;
                Buffer.BlockCopy(_data, srcOffset, _data, dstOffset, size);
            }
        }
        Count--;
    }

    internal void CopyComponentsTo(int srcIndex, Chunk dest, int destIndex, int[] srcSlots, int[] destSlots)
    {
        for (int i = 0; i < srcSlots.Length; i++)
        {
            int size = _componentSizes[srcSlots[i]];
            int srcOffset = _componentOffsets[srcSlots[i]] + srcIndex * size;
            int dstOffset = dest._componentOffsets[destSlots[i]] + destIndex * size;
            Buffer.BlockCopy(_data, srcOffset, dest._data, dstOffset, size);
        }
    }

    internal void SetRawBytes(int componentSlot, int indexInChunk, ReadOnlySpan<byte> bytes)
    {
        int byteOffset = _componentOffsets[componentSlot] + indexInChunk * _componentSizes[componentSlot];
        bytes.CopyTo(_data.AsSpan(byteOffset));
    }

    internal ReadOnlySpan<byte> GetRawBytes(int componentSlot, int indexInChunk)
    {
        int size = _componentSizes[componentSlot];
        int byteOffset = _componentOffsets[componentSlot] + indexInChunk * size;
        return _data.AsSpan(byteOffset, size);
    }

    private static int CalculateStride(ComponentType[] types)
    {
        int total = 0;
        foreach (var t in types) total += t.Size;
        return total;
    }
}
