namespace AlchemyEngine.Core.Ecs;

public abstract class EcsSystem
{
    protected World World { get; private set; } = null!;
    protected internal CommandBuffer CommandBuffer { get; } = new();

    internal void Initialize(World world) => World = world;

    public virtual void OnCreate() { }
    public abstract void OnUpdate(float deltaTime);
    public virtual void OnDestroy() { }

    protected QueryBuilder Query() => World.CreateQuery();
}
