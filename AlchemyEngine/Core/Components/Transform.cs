namespace AlchemyEngine.Core.Components;

/// <summary>
/// A component that makes an Entity exist within the game world itself. it specifies position, rotation, and scale of
/// any entity that uses it.
/// </summary>
public struct Transform
{
    /// <summary>
    /// The global position of an object. <see cref="System.Numerics.Vector3"/>
    /// </summary>
    public System.Numerics.Vector3 Position;
}