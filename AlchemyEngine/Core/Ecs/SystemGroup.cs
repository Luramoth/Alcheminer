namespace AlchemyEngine.Core.Ecs;

public sealed class SystemGroup
{
    private readonly List<EcsSystem> _systems = new();
    private readonly World _world;

    internal SystemGroup(World world) => _world = world;

    public SystemGroup Add<T>() where T : EcsSystem, new()
    {
        var system = new T();
        system.Initialize(_world);
        system.OnCreate();
        _systems.Add(system);
        Runtime.Logger.InfoFormat("System {0} registered.", typeof(T).Name);
        return this;
    }

    public SystemGroup Add(EcsSystem system)
    {
        system.Initialize(_world);
        system.OnCreate();
        _systems.Add(system);
        Runtime.Logger.InfoFormat("System {0} registered.", system.GetType().Name);
        return this;
    }

    public void Update(float deltaTime)
    {
        foreach (var system in _systems)
        {
            system.OnUpdate(deltaTime);
            system.CommandBuffer.Playback(_world);
        }
    }

    public void Destroy()
    {
        foreach (var system in _systems)
            system.OnDestroy();
        _systems.Clear();
    }
}
