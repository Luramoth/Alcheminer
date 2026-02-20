namespace AlchemyEngine.Core.Ecs;

public class EntityManager
{
    private long _nextId = 0;
    private readonly HashSet<long> _alive = new();

    /// <summary>
    /// Creates an <see cref="Entity"/> and automatically gives it an ID and put that ID into storage sequentially
    /// </summary>
    /// <returns><see cref="Entity"/> with a unique ID</returns>
    public Entity Create()
    {
        long id = _nextId++;
        _alive.Add(id);
        return new Entity(id);
    }

    /// <summary>
    /// Destroy an <see cref="Entity"/> and remove it from the ECS.
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> to destroy.</param>
    public void Destroy(Entity entity)
    {
        _alive.Remove(entity.Id);
    }

    /// <summary>
    /// Will check to see if an <see cref="Entity"/> exists
    /// </summary>
    /// <param name="entity"><see cref="Entity"/> to check</param>
    /// <returns>True if the <see cref="Entity"/> exists within the ECS</returns>
    public bool IsAlive(Entity entity) => _alive.Contains(entity.Id);
}