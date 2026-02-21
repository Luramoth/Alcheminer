namespace AlchemyEngine.Core.Ecs;

/// <summary>
/// A typed chunk of 16kb to store one component type contiguously meant to fit into core cache neatly to optimize against cache misses
/// </summary>
public sealed class Chunk<T> where T : struct
{
    /// <summary>
    /// A hardcoded cap of how many <see cref="Entity"/>'s can be in a chunk at any time
    /// </summary>
    public const int Capacity = 128;
    /// <summary>
    /// The amount of <see cref="Entity"/>'s that are currently inside the chunk
    /// </summary>
    public int Count { get; private set; }

    internal readonly T[] Components  = new T[Capacity];
    internal readonly long[] EntityIds = new long[Capacity];

    /// <summary>
    /// Will tell you if the chunk is fully occupied
    /// <returns>True if full</returns>
    /// </summary>
    public bool IsFull => Count >= Capacity;

    /// <summary>
    /// Grabs an <see cref="Entity"/> from the chunk at the specified index
    /// </summary>
    /// <param name="index">index in where the chunk will grab the <see cref="Entity"/></param>
    /// <returns>the <see cref="Entity"/> at the index</returns>
    public Entity GetEntity(int index) => new Entity(EntityIds[index]);
    /// <summary>
    /// Grabs a component from the chunk at the specified index
    /// </summary>
    /// <param name="index">Index in where the chunk will grab the component</param>
    /// <returns>the component at the index</returns>
    public ref T GetComponent(int index) => ref Components[index];
    

    /// <summary>
    /// Add's an <see cref="Entity"/> to the chunk
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="component">The component corresponding to this <see cref="Entity"/></param>
    /// <exception cref="InvalidOperationException">Throws if chunk is full</exception>
    public void AddEntity(Entity entity, T component)
    {
        if (IsFull)
            throw new InvalidOperationException("Chunk is full");

        int index = Count++;
        EntityIds[index] = entity.Id;
        Components[index] = component;
    }

    /// <summary>
    /// Remove <see cref="Entity"/> by searching for it (O(n) where n ≤ 128).
    /// Swaps with last element to keep chunks dense
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>True if found and removed, false if not</returns>
    public bool RemoveEntity(Entity entity)
    {
        for (int i = 0; i < Count; i++)
        {
            if (EntityIds[i] == entity.Id)
            {
                RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    private void RemoveAt(int index)
    {
        int lastIndex = --Count;

        // if not already at the end, swap with the last slot to keep things clumped together
        if (index != lastIndex)
        {
            EntityIds[index] = EntityIds[lastIndex];
            Components[index] = Components[lastIndex];
        }
        
        // Clear last spot
        EntityIds[lastIndex] = -1;
        Components[lastIndex] = default;
    }
}