using System.Numerics;
using AlchemyEngine.Core.Components;
using AlchemyEngine.Core.Ecs;
using JetBrains.Annotations;
using Optional.Unsafe;

namespace AlchemyEngine.Tests.Core.Ecs;

[TestSubject(typeof(World))]
public class WorldTest
{

    [Fact]
    public void CreateEntity_CreatesCorrectEntity()
    {
        Runtime.Init();
        
        var world = new World();

        Entity entity = world.CreateEntity()
            .Add(new Transform { Position = new(1, 2, 3) })
            .Add(new Rigidbody())
            .Build();

        Archetype archetype = world.GetEntityArchetype(entity).ValueOrFailure();
        
        Assert.True(archetype.Has<Transform>());
        Assert.True(archetype.Has<Rigidbody>());

        var chunk = archetype.GetChunk<Transform>();
        Assert.Equal(1, chunk.Count);
        Assert.Equal(new Vector3(1,2,3), chunk.GetComponent(0).Position);
    }
}