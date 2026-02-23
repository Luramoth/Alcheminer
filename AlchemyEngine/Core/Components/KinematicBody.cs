using AlchemyEngine.Core.Ecs;

namespace AlchemyEngine.Core.Components;

/// <summary>
/// Base component for all <see cref="Entity"/>'s meant to be moved by physics
/// </summary>
public struct KinematicBody
{
    /// <summary>
    /// The velocity in which an <see cref="Entity"/> is meant to move
    /// </summary>
    public System.Numerics.Vector3 Velocity;
}