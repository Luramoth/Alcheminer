using AlchemyEngine.Core.Ecs;

namespace AlchemyEngine.Core.Components;

/// <summary>
/// The core Component for all physics, does not on its own have any systems that use it, but all physics systems require it.
/// </summary>
public struct Rigidbody
{
    /// <summary>
    /// Weight of the <see cref="Entity"/>
    /// </summary>
    public float Mass;
}