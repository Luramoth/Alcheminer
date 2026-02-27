using Optional;

namespace AlchemyEngine.Core.Ecs;


/// <summary>
/// The main ECS container, manages <see cref="Entity"/>'s, Components, and Systems
/// </summary>
public sealed class World
{
    // All archetypes index by signature
    private readonly Dictionary<string, Archetype> _archetypes = new();

    // Track what archetype each entity lives
    private readonly Dictionary<long, Archetype> _entityLocations = new();

    /// <summary>
    /// Will return a created archetype weather it already exists or needs to be created
    /// </summary>
    /// <param name="componentTypes">All the components the archetype consists of</param>
    /// <returns>the requested archetype</returns>
    public Archetype GetOrCreateArchetype(params Type[] componentTypes)
    {
        var sorted = componentTypes.OrderBy(t => t.Name).ToArray();
        var key = string.Join("+", sorted.Select(t => t.Name));

        if (!_archetypes.TryGetValue(key, out var archetype))
        {
            archetype = new Archetype(sorted);
            _archetypes[key] = archetype;
            Runtime.Logger.Info($"Created Archetype: {key}");
        }

        return archetype;
    }

    /// <summary>
    /// Allows you to create a new <see cref="Entity"/> with specified components with the builder model
    /// </summary>
    /// <returns><see cref="EntityBuilder"/> loaded with a new <see cref="Entity"/></returns>
    public EntityBuilder CreateEntity()
    {
        var entity = Runtime.EntityManager.Create();
        return new EntityBuilder(this, entity);
    }

    // track where entity was placed
    internal void SetEntityArchetype(Entity entity, Archetype archetype)
    {
        _entityLocations[entity.Id] = archetype;
    }

    /// <summary>
    /// Returns the archetype your <see cref="Entity"/> exists in, if it does exist in an archetype.
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> you want to see is in an archetype</param>
    /// <returns>an <see cref="Option{T,TException}"/> that may or many not contain the archetype your <see cref="Entity"/> esists in, if it's in an archetype</returns>
    public Option<Archetype> GetEntityArchetype(Entity entity)
    {
        _entityLocations.TryGetValue(entity.Id, out var archetype);

        return (archetype is null) ? Option.None<Archetype>() : Option.Some(archetype);
    }
}

/// <summary>
/// A builder model class for the creation of <see cref="Entity"/>'s
/// </summary>
public class EntityBuilder(World world, Entity entity)
{
    private readonly Dictionary<Type, object> _components = new();

    /// <summary>
    /// Adds a component to the <see cref="Entity"/>
    /// </summary>
    /// <param name="component"> Component you want to add</param>
    /// <typeparam name="T"> type of the component (must be a struct)</typeparam>
    public EntityBuilder Add<T>(T component) where T : struct
    {
        _components[typeof(T)] = component;
        return this;
    }

    /// <summary>
    /// Finishes the creation of an <see cref="Entity"/>
    /// </summary>
    /// <returns>the created <see cref="Entity"/></returns>
    public Entity Build()
    {
        var archetype = world.GetOrCreateArchetype(_components.Keys.ToArray());

        foreach (var (type, component) in _components)
        {
            var chunk = archetype.GetType()
                .GetMethod("GetChunk")!
                .MakeGenericMethod(type)
                .Invoke(archetype, null)!;

            var addMethod = chunk.GetType().GetMethod("AddEntity")!;
            addMethod.Invoke(chunk, new[] { entity, component });
        }

        world.SetEntityArchetype(entity, archetype);
        return entity;
    }
}