using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AlchemyEngine.Core.Ecs;

public sealed class CommandBuffer
{
    private enum CommandType : byte
    {
        CreateEntity,
        DestroyEntity,
        AddComponent,
        RemoveComponent,
        SetComponent
    }

    private readonly List<(CommandType type, int entityId, int gen, int ctId, byte[]? data)> _commands = new();

    public void CreateEntity() => _commands.Add((CommandType.CreateEntity, 0, 0, 0, null));

    public void DestroyEntity(Entity entity) =>
        _commands.Add((CommandType.DestroyEntity, entity.Id, entity.Generation, 0, null));

    public void AddComponent<T>(Entity entity, T component) where T : struct
    {
        var bytes = new byte[Unsafe.SizeOf<T>()];
        MemoryMarshal.Write(bytes, in component);
        _commands.Add((CommandType.AddComponent, entity.Id, entity.Generation, ComponentType<T>.Id, bytes));
    }

    public void RemoveComponent<T>(Entity entity) where T : struct =>
        _commands.Add((CommandType.RemoveComponent, entity.Id, entity.Generation, ComponentType<T>.Id, null));

    public void SetComponent<T>(Entity entity, T component) where T : struct
    {
        var bytes = new byte[Unsafe.SizeOf<T>()];
        MemoryMarshal.Write(bytes, in component);
        _commands.Add((CommandType.SetComponent, entity.Id, entity.Generation, ComponentType<T>.Id, bytes));
    }

    internal void Playback(World world)
    {
        foreach (var (type, entityId, gen, ctId, data) in _commands)
        {
            var entity = new Entity(entityId, gen);
            switch (type)
            {
                case CommandType.CreateEntity:
                    world.CreateEntity();
                    break;
                case CommandType.DestroyEntity:
                    world.DestroyEntity(entity);
                    break;
                case CommandType.AddComponent:
                    if (data != null)
                        world.AddComponentRaw(entity, ctId, data);
                    break;
                case CommandType.RemoveComponent:
                    world.RemoveComponentRaw(entity, ctId);
                    break;
                case CommandType.SetComponent:
                    if (data != null)
                        world.SetComponentRaw(entity, ctId, data);
                    break;
            }
        }
        _commands.Clear();
    }

    public void Clear() => _commands.Clear();
}
