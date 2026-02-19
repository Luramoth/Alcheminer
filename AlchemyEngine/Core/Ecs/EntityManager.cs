using System.Runtime.CompilerServices;

namespace AlchemyEngine.Core.Ecs;

internal sealed class EntityManager
{
    private EntityMeta[] _metas;
    private readonly Stack<int> _freeIds = new();
    private int _nextId = 1;

    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    internal EntityManager(int initialCapacity = 1024)
    {
        _metas = new EntityMeta[initialCapacity];
    }

    internal Entity Create(Archetype archetype, int chunkIndex, int indexInChunk)
    {
        _lock.EnterWriteLock();
        try
        {
            int id;
            if (_freeIds.Count > 0)
            {
                id = _freeIds.Pop();
            }
            else
            {
                id = _nextId++;
                EnsureCapacity(id);
            }

            int gen = _metas[id].Generation == 0 ? 1 : _metas[id].Generation;
            _metas[id] = new EntityMeta
            {
                Generation = gen,
                Archetype = archetype,
                ChunkIndex = chunkIndex,
                IndexInChunk = indexInChunk
            };
            return new Entity(id, gen);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    internal bool Destroy(Entity entity)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!IsAlive(entity)) return false;
            ref var meta = ref _metas[entity.Id];
            meta.Generation++;
            meta.Archetype = null;
            meta.ChunkIndex = 0;
            meta.IndexInChunk = 0;
            _freeIds.Push(entity.Id);
            return true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsAlive(Entity entity)
    {
        if (entity.Id <= 0 || entity.Id >= _metas.Length) return false;
        return _metas[entity.Id].Generation == entity.Generation;
    }

    internal ref EntityMeta GetMeta(int entityId)
    {
        return ref _metas[entityId];
    }

    internal void UpdateLocation(int entityId, Archetype archetype, int chunkIndex, int indexInChunk)
    {
        _lock.EnterWriteLock();
        try
        {
            ref var meta = ref _metas[entityId];
            meta.Archetype = archetype;
            meta.ChunkIndex = chunkIndex;
            meta.IndexInChunk = indexInChunk;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private void EnsureCapacity(int id)
    {
        if (id >= _metas.Length)
        {
            int newSize = Math.Max(_metas.Length * 2, id + 1);
            Array.Resize(ref _metas, newSize);
        }
    }
}
