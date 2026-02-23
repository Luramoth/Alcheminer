namespace AlchemyEngine.Core.Ecs;

/// <summary>
/// Container to turn a struct into an ID number
/// </summary>
/// <typeparam name="T">Struct to turn into an ID</typeparam>
public class ComponentType<T> where T : struct
{
    private static readonly int _id = TypeRegistry.Register(typeof(T));
    /// <summary>
    /// returns the ID of a struct
    /// </summary>
    public static int Id => _id;
}

/// <summary>
/// component numerical ID system per type
/// </summary>
internal static class TypeRegistry
{
    private static readonly Dictionary<Type, int> _ids = new();
    private static int _nextId = 0;

    /// <summary>
    /// Register a type to turn into an integer
    /// </summary>
    /// <returns>the id if it's already registered, or the id if it's newly made</returns>
    public static int Register(Type type)
    {
        if (!_ids.TryGetValue(type, out var id))
        {
            id = _nextId++;
            _ids[type] = id;
            Runtime.Logger.Debug($"Registered: {type.Name} = {id}");
        }

        return id;
    }
}