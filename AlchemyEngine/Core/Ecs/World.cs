using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Optional;

namespace AlchemyEngine.Core.Ecs;

public sealed class World : IDisposable
{
    private readonly ConcurrentDictionary<ArchetypeId, Archetype> _archetypes = new();
    private readonly Archetype _emptyArchetype;
    private readonly ReaderWriterLockSlim _structuralLock = new(LockRecursionPolicy.NoRecursion);
    private readonly ConcurrentDictionary<int, ComponentType> _componentTypeRegistry = new();

    internal readonly EntityManager EntityManager;
    internal int ArchetypeVersion;

    private bool _disposed;

    public World(int initialEntityCapacity = 1024)
    {
        EntityManager = new EntityManager(initialEntityCapacity);
        _emptyArchetype = GetOrCreateArchetype(ArchetypeId.Empty);
        Runtime.Logger.Info("ECS World created.");
    }

    public Entity CreateEntity()
    {
        _structuralLock.EnterWriteLock();
        try
        {
            var (chunkIndex, indexInChunk) = _emptyArchetype.AddEntity(0);
            var entity = EntityManager.Create(_emptyArchetype, chunkIndex, indexInChunk);
            _emptyArchetype.GetChunk(chunkIndex).Entities[indexInChunk] = entity.Id;
            return entity;
        }
        finally
        {
            _structuralLock.ExitWriteLock();
        }
    }

    public bool DestroyEntity(Entity entity)
    {
        _structuralLock.EnterWriteLock();
        try
        {
            if (!EntityManager.IsAlive(entity))
            {
                Runtime.Logger.ErrorFormat("Cannot destroy entity {0}: not alive.", entity);
                return false;
            }

            ref var meta = ref EntityManager.GetMeta(entity.Id);
            var archetype = meta.Archetype!;
            archetype.RemoveEntity(meta.ChunkIndex, meta.IndexInChunk, OnEntityMoved);
            return EntityManager.Destroy(entity);
        }
        finally
        {
            _structuralLock.ExitWriteLock();
        }
    }

    public bool IsAlive(Entity entity) => EntityManager.IsAlive(entity);

    public bool AddComponent<T>(Entity entity, T component) where T : struct
    {
        _structuralLock.EnterWriteLock();
        try
        {
            if (!EntityManager.IsAlive(entity))
            {
                Runtime.Logger.ErrorFormat("Cannot add component to entity {0}: not alive.", entity);
                return false;
            }

            int ctId = ComponentType<T>.Id;
            ref var meta = ref EntityManager.GetMeta(entity.Id);
            var srcArchetype = meta.Archetype!;

            if (srcArchetype.HasComponent(ctId)) return false;

            var destArchetype = GetAddEdgeArchetype(srcArchetype, ctId, ComponentType<T>.Value);
            var (destChunk, destIndex) = srcArchetype.MoveEntityTo(
                destArchetype, meta.ChunkIndex, meta.IndexInChunk, entity.Id, OnEntityMoved);

            EntityManager.UpdateLocation(entity.Id, destArchetype, destChunk, destIndex);

            ref T componentRef = ref destArchetype.GetComponent<T>(destChunk, destIndex);
            componentRef = component;
            return true;
        }
        finally
        {
            _structuralLock.ExitWriteLock();
        }
    }

    public bool RemoveComponent<T>(Entity entity) where T : struct
    {
        _structuralLock.EnterWriteLock();
        try
        {
            if (!EntityManager.IsAlive(entity))
            {
                Runtime.Logger.ErrorFormat("Cannot remove component from entity {0}: not alive.", entity);
                return false;
            }

            int ctId = ComponentType<T>.Id;
            ref var meta = ref EntityManager.GetMeta(entity.Id);
            var srcArchetype = meta.Archetype!;

            if (!srcArchetype.HasComponent(ctId)) return false;

            var destArchetype = GetRemoveEdgeArchetype(srcArchetype, ctId);
            var (destChunk, destIndex) = srcArchetype.MoveEntityTo(
                destArchetype, meta.ChunkIndex, meta.IndexInChunk, entity.Id, OnEntityMoved);

            EntityManager.UpdateLocation(entity.Id, destArchetype, destChunk, destIndex);
            return true;
        }
        finally
        {
            _structuralLock.ExitWriteLock();
        }
    }

    public Option<T> GetComponent<T>(Entity entity) where T : struct
    {
        _structuralLock.EnterReadLock();
        try
        {
            if (!EntityManager.IsAlive(entity))
            {
                Runtime.Logger.ErrorFormat("Cannot get component from entity {0}: not alive.", entity);
                return Option.None<T>();
            }

            ref var meta = ref EntityManager.GetMeta(entity.Id);
            var archetype = meta.Archetype!;

            if (!archetype.TryGetSlot(ComponentType<T>.Id, out int slot))
                return Option.None<T>();

            return Option.Some(archetype.GetChunk(meta.ChunkIndex).GetComponent<T>(slot, meta.IndexInChunk));
        }
        finally
        {
            _structuralLock.ExitReadLock();
        }
    }

    public bool SetComponent<T>(Entity entity, T component) where T : struct
    {
        _structuralLock.EnterReadLock();
        try
        {
            if (!EntityManager.IsAlive(entity))
            {
                Runtime.Logger.ErrorFormat("Cannot set component on entity {0}: not alive.", entity);
                return false;
            }

            ref var meta = ref EntityManager.GetMeta(entity.Id);
            var archetype = meta.Archetype!;

            if (!archetype.TryGetSlot(ComponentType<T>.Id, out int slot))
                return false;

            archetype.GetChunk(meta.ChunkIndex).GetComponent<T>(slot, meta.IndexInChunk) = component;
            return true;
        }
        finally
        {
            _structuralLock.ExitReadLock();
        }
    }

    public bool HasComponent<T>(Entity entity) where T : struct
    {
        if (!EntityManager.IsAlive(entity)) return false;
        ref var meta = ref EntityManager.GetMeta(entity.Id);
        return meta.Archetype!.HasComponent(ComponentType<T>.Id);
    }

    public QueryBuilder CreateQuery() => new(this);

    public SystemGroup CreateSystemGroup() => new(this);

    internal List<Archetype> FindMatchingArchetypes(int[] with, int[] without)
    {
        var result = new List<Archetype>();
        foreach (var (_, archetype) in _archetypes)
        {
            bool match = true;
            foreach (int ctId in with)
            {
                if (!archetype.HasComponent(ctId)) { match = false; break; }
            }
            if (!match) continue;
            foreach (int ctId in without)
            {
                if (archetype.HasComponent(ctId)) { match = false; break; }
            }
            if (match) result.Add(archetype);
        }
        return result;
    }

    internal void AddComponentRaw(Entity entity, int componentTypeId, byte[] data)
    {
        _structuralLock.EnterWriteLock();
        try
        {
            if (!EntityManager.IsAlive(entity)) return;
            if (!_componentTypeRegistry.TryGetValue(componentTypeId, out var ct)) return;

            ref var meta = ref EntityManager.GetMeta(entity.Id);
            var srcArchetype = meta.Archetype!;
            if (srcArchetype.HasComponent(componentTypeId)) return;

            var destArchetype = GetAddEdgeArchetype(srcArchetype, componentTypeId, ct);
            var (destChunk, destIndex) = srcArchetype.MoveEntityTo(
                destArchetype, meta.ChunkIndex, meta.IndexInChunk, entity.Id, OnEntityMoved);

            EntityManager.UpdateLocation(entity.Id, destArchetype, destChunk, destIndex);

            if (destArchetype.TryGetSlot(componentTypeId, out int slot))
                destArchetype.GetChunk(destChunk).SetRawBytes(slot, destIndex, data);
        }
        finally
        {
            _structuralLock.ExitWriteLock();
        }
    }

    internal void RemoveComponentRaw(Entity entity, int componentTypeId)
    {
        _structuralLock.EnterWriteLock();
        try
        {
            if (!EntityManager.IsAlive(entity)) return;
            if (!_componentTypeRegistry.TryGetValue(componentTypeId, out _)) return;

            ref var meta = ref EntityManager.GetMeta(entity.Id);
            var srcArchetype = meta.Archetype!;
            if (!srcArchetype.HasComponent(componentTypeId)) return;

            var destArchetype = GetRemoveEdgeArchetype(srcArchetype, componentTypeId);
            var (destChunk, destIndex) = srcArchetype.MoveEntityTo(
                destArchetype, meta.ChunkIndex, meta.IndexInChunk, entity.Id, OnEntityMoved);

            EntityManager.UpdateLocation(entity.Id, destArchetype, destChunk, destIndex);
        }
        finally
        {
            _structuralLock.ExitWriteLock();
        }
    }

    internal void SetComponentRaw(Entity entity, int componentTypeId, byte[] data)
    {
        _structuralLock.EnterReadLock();
        try
        {
            if (!EntityManager.IsAlive(entity)) return;
            ref var meta = ref EntityManager.GetMeta(entity.Id);
            if (!meta.Archetype!.TryGetSlot(componentTypeId, out int slot)) return;
            meta.Archetype.GetChunk(meta.ChunkIndex).SetRawBytes(slot, meta.IndexInChunk, data);
        }
        finally
        {
            _structuralLock.ExitReadLock();
        }
    }

    internal void RegisterComponentType(int id, ComponentType ct) =>
        _componentTypeRegistry.TryAdd(id, ct);

    private Archetype GetAddEdgeArchetype(Archetype src, int componentTypeId, ComponentType ct)
    {
        if (src.TryGetAddEdge(componentTypeId, out var cached) && cached != null)
            return cached;

        var newId = src.Id.With(componentTypeId);
        var dest = GetOrCreateArchetypeWithType(newId, ct);
        src.SetAddEdge(componentTypeId, dest);
        dest.SetRemoveEdge(componentTypeId, src);
        return dest;
    }

    private Archetype GetRemoveEdgeArchetype(Archetype src, int componentTypeId)
    {
        if (src.TryGetRemoveEdge(componentTypeId, out var cached) && cached != null)
            return cached;

        var newId = src.Id.Without(componentTypeId);
        var dest = GetOrCreateArchetype(newId);
        src.SetRemoveEdge(componentTypeId, dest);
        dest.SetAddEdge(componentTypeId, src);
        return dest;
    }

    private Archetype GetOrCreateArchetype(ArchetypeId id)
    {
        return _archetypes.GetOrAdd(id, archetypeId =>
        {
            var componentTypes = ResolveComponentTypes(archetypeId.ComponentIds);
            var archetype = new Archetype(archetypeId, componentTypes);
            Interlocked.Increment(ref ArchetypeVersion);
            Runtime.Logger.DebugFormat("Created archetype with {0} component types.", archetypeId.ComponentIds.Length);
            return archetype;
        });
    }

    private Archetype GetOrCreateArchetypeWithType(ArchetypeId id, ComponentType newType)
    {
        _componentTypeRegistry.TryAdd(newType.Id, newType);
        return GetOrCreateArchetype(id);
    }

    private ComponentType[] ResolveComponentTypes(int[] componentIds)
    {
        var types = new ComponentType[componentIds.Length];
        for (int i = 0; i < componentIds.Length; i++)
        {
            if (!_componentTypeRegistry.TryGetValue(componentIds[i], out var ct))
                throw new InvalidOperationException($"Component type id {componentIds[i]} not registered.");
            types[i] = ct;
        }
        return types;
    }

    private void OnEntityMoved(int entityId, int newChunkIndex, int newIndexInChunk)
    {
        ref var meta = ref EntityManager.GetMeta(entityId);
        meta.ChunkIndex = newChunkIndex;
        meta.IndexInChunk = newIndexInChunk;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _structuralLock.Dispose();
        Runtime.Logger.Info("ECS World disposed.");
    }
}
