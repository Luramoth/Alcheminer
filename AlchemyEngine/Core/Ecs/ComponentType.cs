using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AlchemyEngine.Core.Ecs;

public readonly struct ComponentType : IEquatable<ComponentType>
{
    public readonly int Id;
    public readonly int Size;
    public readonly string Name;

    private ComponentType(int id, int size, string name)
    {
        Id = id;
        Size = size;
        Name = name;
    }

    public bool Equals(ComponentType other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is ComponentType other && Equals(other);
    public override int GetHashCode() => Id;
    public static bool operator ==(ComponentType left, ComponentType right) => left.Id == right.Id;
    public static bool operator !=(ComponentType left, ComponentType right) => left.Id != right.Id;
    public override string ToString() => Name;

    internal static class Registry
    {
        private static int _nextId = 0;
        private static readonly ConcurrentDictionary<Type, ComponentType> _types = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ComponentType Get(Type type, int size, string name)
        {
            return _types.GetOrAdd(type, _ =>
            {
                int id = Interlocked.Increment(ref _nextId) - 1;
                return new ComponentType(id, size, name);
            });
        }
    }
}

public static class ComponentType<T> where T : struct
{
    public static readonly ComponentType Value = ComponentType.Registry.Get(
        typeof(T),
        Unsafe.SizeOf<T>(),
        typeof(T).Name
    );

    public static int Id
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value.Id;
    }

    public static int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value.Size;
    }
}
