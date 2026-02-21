namespace AlchemyEngine.Core.Ecs;

/// <summary>
/// A struct to store and manage the ID of an entity inside the ECS for Alchemy. it does not refer to components and is to be used more like a 'pointer'
/// </summary>
/// <param name="Id">A 64-bit integer to refer to the Entity inside the ECS system.</param>
public readonly record struct Entity(long Id) : IEquatable<Entity>
{
    /// <summary>
    /// Will check to make sure an entity is actually valid based on if it has ID a value at or above 0
    /// <returns>Whether an entity is valid or not</returns>
    /// </summary>
    public bool IsValid => Id >= 0;
    
    /// <summary>
    /// Will make an invalid entity on purpose, this is meant to be a safer alternative to having a function return null.
    /// <returns>Entity with an ID of -1</returns>
    /// <example>return error == false ? new Entity(Id) : Entity.Invalid();</example>
    /// </summary>
    public static Entity Invalid => new(-1);
    
    /// <summary>
    /// Turns entity into a string
    /// </summary>
    /// <returns>"Entity({Id})"</returns>
    public override string ToString() => $"Entity({Id})";
}