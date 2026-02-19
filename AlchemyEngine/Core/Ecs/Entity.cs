using System.Runtime.CompilerServices;

namespace AlchemyEngine.Core.Ecs;

public readonly struct Entity : IEquatable<Entity>
{
    public static readonly Entity Invalid = new(0, 0);

    public readonly int Id;
    public readonly int Generation;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Entity(int id, int generation)
    {
        Id = id;
        Generation = generation;
    }

    public bool IsValid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Generation > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Entity other) => Id == other.Id && Generation == other.Generation;
    public override bool Equals(object? obj) => obj is Entity other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Id, Generation);
    public static bool operator ==(Entity left, Entity right) => left.Equals(right);
    public static bool operator !=(Entity left, Entity right) => !left.Equals(right);
    public override string ToString() => $"Entity({Id}:{Generation})";
}

internal struct EntityMeta
{
    public int Generation;
    public Archetype? Archetype;
    public int IndexInChunk;
    public int ChunkIndex;
}
