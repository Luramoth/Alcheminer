using System.Runtime.CompilerServices;

namespace AlchemyEngine.Core.Ecs;

public sealed class QueryBuilder
{
    private readonly World _world;
    private readonly List<int> _with = new();
    private readonly List<int> _without = new();

    internal QueryBuilder(World world) => _world = world;

    public QueryBuilder With<T>() where T : struct
    {
        _with.Add(ComponentType<T>.Id);
        return this;
    }

    public QueryBuilder Without<T>() where T : struct
    {
        _without.Add(ComponentType<T>.Id);
        return this;
    }

    public Query Build() => new(_world, _with.ToArray(), _without.ToArray());
}

public sealed class Query
{
    private readonly World _world;
    internal readonly int[] With;
    internal readonly int[] Without;
    private List<Archetype>? _cachedArchetypes;
    private int _archetypeVersion;

    internal Query(World world, int[] with, int[] without)
    {
        _world = world;
        With = with;
        Without = without;
    }

    internal List<Archetype> GetMatchingArchetypes()
    {
        if (_cachedArchetypes != null && _archetypeVersion == _world.ArchetypeVersion)
            return _cachedArchetypes;

        _cachedArchetypes = _world.FindMatchingArchetypes(With, Without);
        _archetypeVersion = _world.ArchetypeVersion;
        return _cachedArchetypes;
    }

    public void ForEach(Action<Entity> action)
    {
        foreach (var archetype in GetMatchingArchetypes())
        {
            archetype.IterateChunks((chunk, _) =>
            {
                int count = chunk.Count;
                for (int i = 0; i < count; i++)
                {
                    int entityId = chunk.Entities[i];
                    ref var meta = ref _world.EntityManager.GetMeta(entityId);
                    action(new Entity(entityId, meta.Generation));
                }
            });
        }
    }

    public void ForEach<T1>(ComponentAction<T1> action) where T1 : struct
    {
        foreach (var archetype in GetMatchingArchetypes())
        {
            if (!archetype.TryGetSlot(ComponentType<T1>.Id, out int slot1)) continue;
            archetype.IterateChunks((chunk, _) =>
            {
                int count = chunk.Count;
                for (int i = 0; i < count; i++)
                    action(ref chunk.GetComponent<T1>(slot1, i));
            });
        }
    }

    public void ForEach<T1, T2>(ComponentAction<T1, T2> action) where T1 : struct where T2 : struct
    {
        foreach (var archetype in GetMatchingArchetypes())
        {
            if (!archetype.TryGetSlot(ComponentType<T1>.Id, out int slot1)) continue;
            if (!archetype.TryGetSlot(ComponentType<T2>.Id, out int slot2)) continue;
            archetype.IterateChunks((chunk, _) =>
            {
                int count = chunk.Count;
                for (int i = 0; i < count; i++)
                    action(ref chunk.GetComponent<T1>(slot1, i), ref chunk.GetComponent<T2>(slot2, i));
            });
        }
    }

    public void ForEach<T1, T2, T3>(ComponentAction<T1, T2, T3> action)
        where T1 : struct where T2 : struct where T3 : struct
    {
        foreach (var archetype in GetMatchingArchetypes())
        {
            if (!archetype.TryGetSlot(ComponentType<T1>.Id, out int slot1)) continue;
            if (!archetype.TryGetSlot(ComponentType<T2>.Id, out int slot2)) continue;
            if (!archetype.TryGetSlot(ComponentType<T3>.Id, out int slot3)) continue;
            archetype.IterateChunks((chunk, _) =>
            {
                int count = chunk.Count;
                for (int i = 0; i < count; i++)
                    action(
                        ref chunk.GetComponent<T1>(slot1, i),
                        ref chunk.GetComponent<T2>(slot2, i),
                        ref chunk.GetComponent<T3>(slot3, i));
            });
        }
    }

    public void ForEachParallel<T1>(ComponentAction<T1> action) where T1 : struct
    {
        var archetypes = GetMatchingArchetypes();
        Parallel.ForEach(archetypes, archetype =>
        {
            if (!archetype.TryGetSlot(ComponentType<T1>.Id, out int slot1)) return;
            archetype.IterateChunks((chunk, _) =>
            {
                int count = chunk.Count;
                for (int i = 0; i < count; i++)
                    action(ref chunk.GetComponent<T1>(slot1, i));
            });
        });
    }

    public void ForEachParallel<T1, T2>(ComponentAction<T1, T2> action) where T1 : struct where T2 : struct
    {
        var archetypes = GetMatchingArchetypes();
        Parallel.ForEach(archetypes, archetype =>
        {
            if (!archetype.TryGetSlot(ComponentType<T1>.Id, out int slot1)) return;
            if (!archetype.TryGetSlot(ComponentType<T2>.Id, out int slot2)) return;
            archetype.IterateChunks((chunk, _) =>
            {
                int count = chunk.Count;
                for (int i = 0; i < count; i++)
                    action(ref chunk.GetComponent<T1>(slot1, i), ref chunk.GetComponent<T2>(slot2, i));
            });
        });
    }
}

public delegate void ComponentAction<T1>(ref T1 c1) where T1 : struct;
public delegate void ComponentAction<T1, T2>(ref T1 c1, ref T2 c2) where T1 : struct where T2 : struct;
public delegate void ComponentAction<T1, T2, T3>(ref T1 c1, ref T2 c2, ref T3 c3)
    where T1 : struct where T2 : struct where T3 : struct;
