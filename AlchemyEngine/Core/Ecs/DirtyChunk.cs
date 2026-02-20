namespace AlchemyEngine.Core.Ecs;

/// <summary>
/// temporary rudimentary data chunk class.
/// </summary>
public sealed class DirtyChunk
{
    public const int Capacity = 128;
    public int Count { get; private set; }

    internal Transform[] Transforms = new Transform[Capacity];
    internal long[] EntityIds = new long[Capacity];

    public bool IsFull => Count >= Capacity;

    public int AddEntity(Entity entity, Transform transform)
    {
        int index = Count++;
        EntityIds[index] = entity.Id;
        Transforms[index] = transform;
        return index;
    }
}

public struct Transform
{
    public System.Numerics.Vector3 Position;
}