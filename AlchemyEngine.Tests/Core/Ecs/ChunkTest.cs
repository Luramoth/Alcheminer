using System.Numerics;
using AlchemyEngine.Core.Components;
using AlchemyEngine.Core.Ecs;
using JetBrains.Annotations;

namespace AlchemyEngine.Tests.Core.Ecs;

[TestSubject(typeof(Chunk<>))]
public class ChunkTest
{

    [Fact]
    public void AddEntity_IncreasesCount()
    {
        var chunk = new Chunk<Transform>();
        var entity = new Entity(0);
        
        chunk.AddEntity(entity, new Transform{Position = new(1,2,3)});
        
        Assert.Equal(1,chunk.Count);
    }

    [Fact]
    public void GetComponent_ReturnsCorrectData()
    {
        var chunk = new Chunk<Transform>();
        var entity = new Entity(0);
        var transform = new Transform{Position = new Vector3(1,2,3)};
        
        chunk.AddEntity(entity, transform);

        ref var retrieved = ref chunk.GetComponent(0);
        Assert.Equal(new Vector3(1,2,3), retrieved.Position);
    }

    [Fact]
    public void RemoveEntity_DecreasesCount()
    {
        var chunk = new Chunk<Transform>();
        var e1 = new Entity(0);
        var e2 = new Entity(1);
        
        chunk.AddEntity(e1, new Transform());
        chunk.AddEntity(e2, new Transform());

        bool removed = chunk.RemoveEntity(e1);
        
        Assert.True(removed);
        Assert.Equal(1, chunk.Count);
    }

    [Fact]
    public void RemoveEntity_SwapsWithLast()
    {
        var chunk = new Chunk<Transform>();
        var e1 = new Entity(0);
        var e2 = new Entity(1);
        
        chunk.AddEntity(e1, new Transform{Position = new(1,1,1)});
        chunk.AddEntity(e2, new Transform{Position = new(2,2,2)});

        chunk.RemoveEntity(e1);
        
        Assert.Equal(e2, chunk.GetEntity(0));
    }

    [Fact]
    public void AddEntity_ThrowsWhenFull()
    {
        var chunk = new Chunk<Transform>();
        
        for (int i = 0; i < 128; i++)
        {
            chunk.AddEntity(new Entity(i), new Transform());
        }

        Assert.Throws<InvalidOperationException>(() => chunk.AddEntity(new Entity(129), new Transform()));
    }

    [Fact]
    public void RemoveEntity_ReturnsFalseIfEntityIsNotThere()
    {
        var chunk = new Chunk<Transform>();
        
        Assert.False(chunk.RemoveEntity(new Entity(0)));
    }
}