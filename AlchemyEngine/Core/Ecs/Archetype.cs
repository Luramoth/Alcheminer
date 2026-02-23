using System.Diagnostics;

namespace AlchemyEngine.Core.Ecs;

/// <summary>
/// Stores all entities with the same component combination
/// has one <see cref="Chunk{T}"/> per component type
/// </summary>
public sealed class Archetype
{
    // what components define the archetype?
    private readonly Type[] _componentTypes;

    // ComponentType -> Chunk<T>
    // object due to using different T types
    private readonly Dictionary<Type, object> _chunks = new();

    /// <summary>
    /// Creates an archetype that can store the given component types
    /// </summary>
    /// <param name="componentTypes">Types of components that amke up this archetype</param>
    public Archetype(params Type[] componentTypes)
    {
        _componentTypes = componentTypes.OrderBy(t => t.Name).ToArray();

        foreach (var type in _componentTypes)
        {
            var chunkType = typeof(Chunk<>).MakeGenericType(type);
            var chunk = Activator.CreateInstance(chunkType);
            Debug.Assert(chunk != null, nameof(chunk) + " != null");
            _chunks[type] = chunk;
        }
    }

    /// <summary>
    /// Does the archetype have a specific component?
    /// </summary>
    /// <typeparam name="T">Type of component</typeparam>
    /// <returns>True if component has been found</returns>
    public bool Has<T>() where T : struct => _chunks.ContainsKey((typeof(T)));

    /// <summary>
    /// Get the chunk for a specific component type
    /// </summary>
    /// <typeparam name="T">Type of Component</typeparam>
    /// <returns><see cref="Chunk{T}"/> that contains this component</returns>
    public Chunk<T> GetChunk<T>() where T : struct => (Chunk<T>)_chunks[typeof(T)];
}
