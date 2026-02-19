using System.Runtime.CompilerServices;

namespace AlchemyEngine.Core.Ecs;

internal readonly struct ArchetypeId : IEquatable<ArchetypeId>
{
    internal readonly int[] ComponentIds;
    private readonly int _hashCode;

    internal ArchetypeId(int[] sortedComponentIds)
    {
        ComponentIds = sortedComponentIds;
        var hc = new HashCode();
        foreach (int id in sortedComponentIds)
            hc.Add(id);
        _hashCode = hc.ToHashCode();
    }

    internal static ArchetypeId Empty { get; } = new(Array.Empty<int>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool Has(int componentId)
    {
        return Array.BinarySearch(ComponentIds, componentId) >= 0;
    }

    internal ArchetypeId With(int componentId)
    {
        if (Has(componentId)) return this;
        var ids = new int[ComponentIds.Length + 1];
        ComponentIds.CopyTo(ids, 0);
        ids[ComponentIds.Length] = componentId;
        Array.Sort(ids);
        return new ArchetypeId(ids);
    }

    internal ArchetypeId Without(int componentId)
    {
        int idx = Array.BinarySearch(ComponentIds, componentId);
        if (idx < 0) return this;
        var ids = new int[ComponentIds.Length - 1];
        int write = 0;
        for (int i = 0; i < ComponentIds.Length; i++)
        {
            if (i != idx) ids[write++] = ComponentIds[i];
        }
        return new ArchetypeId(ids);
    }

    public bool Equals(ArchetypeId other)
    {
        if (_hashCode != other._hashCode) return false;
        if (ComponentIds.Length != other.ComponentIds.Length) return false;
        for (int i = 0; i < ComponentIds.Length; i++)
        {
            if (ComponentIds[i] != other.ComponentIds[i]) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is ArchetypeId other && Equals(other);
    public override int GetHashCode() => _hashCode;
    public static bool operator ==(ArchetypeId left, ArchetypeId right) => left.Equals(right);
    public static bool operator !=(ArchetypeId left, ArchetypeId right) => !left.Equals(right);
}
