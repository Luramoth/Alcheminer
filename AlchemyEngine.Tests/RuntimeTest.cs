using AlchemyEngine;
using JetBrains.Annotations;

namespace AlchemyEngine.Tests;

[TestSubject(typeof(Runtime))]
public class RuntimeTest
{

    [Fact]
    public void WasCalledFromEnmgineAssembly_ReturnsFalseWhenCalledFromTest()
    {
        Assert.False(Runtime.WasCalledFromEngineAssembly());
    }

    [Fact]
    public void Init_RunsNominally()
    {
        Runtime.Init();
    }
}