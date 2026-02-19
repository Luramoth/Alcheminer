using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AlchemyEngine.Core.Ecs;

internal sealed class Archetype
{
    internal readonly ArchetypeId Id;
    internal readonly ComponentType[] ComponentTypes;

    private readonly Dictionary<int, int> _componentTypeToSlot;
    private readonly List<Chunk> _chunks = new();
    private readonly ConcurrentDictionary<int, Archetype> _addEdges = new();
    private readonly ConcurrentDictionary<int, Archetype> _removeEdges = new();

    internal int EntityCount { get; private set; }

    internal Archetype(ArchetypeId id, ComponentType[] componentTypes)
    {
        Id = id;
        ComponentTypes = componentTypes;
        _componentTypeToSlot = new Dictionary<int, int>(componentTypes.Length);
        for (int i = 0; i < componentTypes.Length; i++)
            _componentTypeToSlot[componentTypes[i].Id] = i;

        _chunks.Add(new Chunk(componentTypes));
    }

    internal bool HasComponent(int componentTypeId) => _componentTypeToSlot.ContainsKey(componentTypeId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetSlot(int componentTypeId, out int slot) =>
        _componentTypeToSlot.TryGetValue(componentTypeId, out slot);

    internal (int chunkIndex, int indexInChunk) AddEntity(int entityId)
    {
        var chunk = GetOrCreateFreeChunk(out int chunkIndex);
        int indexInChunk = chunk.Add(entityId);
        EntityCount++;
        return (chunkIndex, indexInChunk);
    }

    internal void RemoveEntity(int chunkIndex, int indexInChunk, Action<int, int, int> onEntityMoved)
    {
        var chunk = _chunks[chunkIndex];
        int lastIndex = chunk.Count - 1;

        if (indexInChunk != lastIndex)
        {
            int movedEntityId = chunk.Entities[lastIndex];
            onEntityMoved(movedEntityId, chunkIndex, indexInChunk);
        }

        chunk.RemoveAt(indexInChunk, lastIndex);
        EntityCount--;
    }

    internal (int chunkIndex, int indexInChunk) MoveEntityTo(
        Archetype dest,
        int srcChunkIndex,
        int srcIndexInChunk,
        int entityId,
        Action<int, int, int> onEntityMoved)
    {
        var (destChunkIndex, destIndexInChunk) = dest.AddEntity(entityId);
        var srcChunk = _chunks[srcChunkIndex];
        var destChunk = dest._chunks[destChunkIndex];

        var srcSlots = new List<int>();
        var destSlots = new List<int>();

        foreach (var ct in ComponentTypes)
        {
            if (dest.TryGetSlot(ct.Id, out int destSlot) && TryGetSlot(ct.Id, out int srcSlot))
            {
                srcSlots.Add(srcSlot);
                destSlots.Add(destSlot);
            }
        }

        srcChunk.CopyComponentsTo(srcIndexInChunk, destChunk, destIndexInChunk, srcSlots.ToArray(), destSlots.ToArray());
        RemoveEntity(srcChunkIndex, srcIndexInChunk, onEntityMoved);

        return (destChunkIndex, destIndexInChunk);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Chunk GetChunk(int chunkIndex) => _chunks[chunkIndex];

    internal int ChunkCount => _chunks.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetComponent<T>(int chunkIndex, int indexInChunk) where T : struct
    {
        int slot = _componentTypeToSlot[ComponentType<T>.Id];
        return ref _chunks[chunkIndex].GetComponent<T>(slot, indexInChunk);
    }

    internal bool TryGetAddEdge(int componentTypeId, out Archetype? next) =>
        _addEdges.TryGetValue(componentTypeId, out next);

    internal void SetAddEdge(int componentTypeId, Archetype next) =>
        _addEdges[componentTypeId] = next;

    internal bool TryGetRemoveEdge(int componentTypeId, out Archetype? prev) =>
        _removeEdges.TryGetValue(componentTypeId, out prev);

    internal void SetRemoveEdge(int componentTypeId, Archetype prev) =>
        _removeEdges[componentTypeId] = prev;

    private Chunk GetOrCreateFreeChunk(out int chunkIndex)
    {
        for (int i = 0; i < _chunks.Count; i++)
        {
            if (!_chunks[i].IsFull)
            {
                chunkIndex = i;
                return _chunks[i];
            }
        }
        var newChunk = new Chunk(ComponentTypes);
        _chunks.Add(newChunk);
        chunkIndex = _chunks.Count - 1;
        return newChunk;
    }

    internal void IterateChunks(Action<Chunk, int> action)
    {
        for (int i = 0; i < _chunks.Count; i++)
        {
            if (_chunks[i].Count > 0)
                action(_chunks[i], i);
        }
    }
}
