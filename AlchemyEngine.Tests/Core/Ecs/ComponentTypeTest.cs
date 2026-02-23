using AlchemyEngine.Core.Components;
using AlchemyEngine.Core.Ecs;
using JetBrains.Annotations;

namespace AlchemyEngine.Tests.Core.Ecs;

[TestSubject(typeof(ComponentType<>))]
public class ComponentTypeTest
{

    [Fact]
    public void ID_ReturnsSameIdForSameType()
    {
        Assert.Equal(ComponentType<Transform>.Id, ComponentType<Transform>.Id);
    }
}