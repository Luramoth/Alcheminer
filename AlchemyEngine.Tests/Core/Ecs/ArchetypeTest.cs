using AlchemyEngine.Core.Components;
using AlchemyEngine.Core.Ecs;
using JetBrains.Annotations;

namespace AlchemyEngine.Tests.Core.Ecs;

[TestSubject(typeof(Archetype))]
public class ArchetypeTest
{

    [Fact]
    public void Archetype_ContainsProperComponents()
    {
        var archetype = new Archetype(typeof(Transform), typeof(KinematicBody));
        
        Assert.True(archetype.Has<Transform>());
        Assert.True(archetype.Has<KinematicBody>());
        Assert.False(archetype.Has<Rigidbody>());
    }
}