using AlchemyEngine.Core.Ecs;
using JetBrains.Annotations;

namespace AlchemyEngine.Tests.Core.Ecs;

[TestSubject(typeof(Entity))]
public class EntityTest
{
    [Fact]
    public void EntityAt0IsValidAndInvalidAtNegative()
    {
        var entity = new Entity(0);

        Assert.True(entity.IsValid);

        var invalidEntity = new Entity(-1);

        Assert.False(invalidEntity.IsValid);
    }

    [Fact]
    public void Invalid_ProducesInvalidEntity()
    {
        var invalidEntity = Entity.Invalid;

        Assert.False(invalidEntity.IsValid);
    }

    [Fact]
    public void ToString_ProducesCorrectSTring()
    {
        var entity = new Entity();

        Assert.Equal("Entity(0)", entity.ToString());
    }
}