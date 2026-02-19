using Optional;
using Optional.Unsafe;

namespace AlchemyEngine.Core;

public class Ecs
{
    // took me a while to wrap my head around this but how this ECS is meant to work is that the component types themselves
    // make a list of all the entities that use those component types. this method seems to avoid boxing and looks ideal for performance.
    // all subject to change as much of this comes from AI generated assistance, it took me a while to figure out as this is
    // very unintuitive. but it makes it so the component storage system is lazy initialized
    private int _nextId = 0;
    private HashSet<int> _entities = new();
    private Dictionary<Type, object> _componentStores = new();

    public int CreateEntity()
    {
        int id = _nextId++;
        _entities.Add(id);
        return id;
    }

    public void DestroyEntity(int id)
    {
        if (!_entities.Contains(id))
        {
            Runtime.Logger.ErrorFormat("Cannot delete entity {0}. Entity already destoryed.");
            return;
        }

        _entities.Remove(id);
    }
    
    public bool AddComponent<T>(int entityId, T component) where T : struct
    {
        if (!_entities.Contains(entityId))
        {
            Runtime.Logger.ErrorFormat("Cannot add component to entity {0}, entity does not exist.", entityId);
            return false;
        }
        
        var store = _getOrCreateStore<T>();
        store[entityId] = component;
        return true;
    }

    public Option<T> GetComponant<T>(int entityId) where T : struct
    {
        if (!_entities.Contains(entityId))
        {
            Runtime.Logger.ErrorFormat("Cannot get component to entity {0}, entity does not exist.", entityId);
            return Option.None<T>();
        }

        var store = _getOrCreateStore<T>();
        return store.TryGetValue(entityId, out var componant)
            ? Option.Some(componant)
            : Option.None<T>();
    }

    private Dictionary<int, T> _getOrCreateStore<T>() where T : struct
    {
        if (!_componentStores.TryGetValue(typeof(T), out var store))
        {
            store = new Dictionary<int, T>();
            _componentStores[typeof(T)] = store;
        }
        return (Dictionary<int, T>)store;
    }
}