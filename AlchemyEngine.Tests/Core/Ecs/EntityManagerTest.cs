using AlchemyEngine.Core.Ecs;
using JetBrains.Annotations;

namespace AlchemyEngine.Tests.Core.Ecs;

[TestSubject(typeof(EntityManager))]
public class EntityManagerTest
{

    [Fact]
    public void EntityCreationIsEverSequential()
    {
        var entityManager = new EntityManager();

        var entity0 = entityManager.Create();
        var entity1 = entityManager.Create();
        
        Assert.Equal(0, entity0.Id);
        Assert.Equal(1, entity1.Id);
        
        entityManager.Destroy(entity1);
        
        Assert.False(entityManager.IsAlive(entity1));

        var entity2 = entityManager.Create();
        
        Assert.Equal(2, entity2.Id);
    }
}