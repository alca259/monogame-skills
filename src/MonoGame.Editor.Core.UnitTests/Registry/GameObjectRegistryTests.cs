using System.Reflection;

namespace MonoGame.Editor.Core.UnitTests.Registry;

// Fake base class — simulates Alca.MonoGame.Kernel.GameBehaviour without a real dependency.
internal class GameBehaviour { }
internal sealed class ConcreteTestBehaviour : GameBehaviour { }
internal abstract class AbstractTestBehaviour : GameBehaviour { }
internal class IntermediateBehaviour : GameBehaviour { }
internal sealed class DeeplyDerivedBehaviour : IntermediateBehaviour { }

public sealed class GameObjectRegistryTests
{
    [Fact]
    public void Scan_FindsConcreteSubclassesOfGameBehaviour()
    {
        GameObjectRegistry registry = new();
        registry.Scan(new Assembly[] { typeof(ConcreteTestBehaviour).Assembly });

        Assert.True(registry.RegisteredTypes.Values.Any(t => t == typeof(ConcreteTestBehaviour)));
    }

    [Fact]
    public void Scan_ExcludesAbstractSubclasses()
    {
        GameObjectRegistry registry = new();
        registry.Scan(new Assembly[] { typeof(AbstractTestBehaviour).Assembly });

        Assert.False(registry.RegisteredTypes.Values.Any(t => t == typeof(AbstractTestBehaviour)));
    }

    [Fact]
    public void Scan_FindsDeeplyDerivedConcreteTypes()
    {
        GameObjectRegistry registry = new();
        registry.Scan(new Assembly[] { typeof(DeeplyDerivedBehaviour).Assembly });

        Assert.True(registry.RegisteredTypes.Values.Any(t => t == typeof(DeeplyDerivedBehaviour)));
    }

    [Fact]
    public void Scan_DoesNotContainGameBehaviourBaseClass()
    {
        GameObjectRegistry registry = new();
        registry.Scan(new Assembly[] { typeof(GameBehaviour).Assembly });

        // GameBehaviour itself has no base class named GameBehaviour, so it should NOT be registered.
        Assert.False(registry.RegisteredTypes.Values.Any(t => t == typeof(GameBehaviour)));
    }

    [Fact]
    public void RegisteredTypes_IsEmptyBeforeScan()
    {
        GameObjectRegistry registry = new();
        Assert.Empty(registry.RegisteredTypes);
    }

    [Fact]
    public void Scan_ClearsAndRebuildsOnSubsequentCall()
    {
        GameObjectRegistry registry = new();
        registry.Scan(new Assembly[] { typeof(ConcreteTestBehaviour).Assembly });
        int firstCount = registry.RegisteredTypes.Count;

        registry.Scan(new Assembly[] { typeof(ConcreteTestBehaviour).Assembly });
        Assert.Equal(firstCount, registry.RegisteredTypes.Count);
    }

    [Fact]
    public void Scan_UsesFullTypeNameAsKey()
    {
        GameObjectRegistry registry = new();
        registry.Scan(new Assembly[] { typeof(ConcreteTestBehaviour).Assembly });

        string fullName = typeof(ConcreteTestBehaviour).FullName!;
        Assert.True(registry.RegisteredTypes.ContainsKey(fullName));
        Assert.Equal(typeof(ConcreteTestBehaviour), registry.RegisteredTypes[fullName]);
    }

    [Fact]
    public void Scan_EmptyAssemblyArray_ProducesNoResults()
    {
        GameObjectRegistry registry = new();
        registry.Scan(System.Array.Empty<Assembly>());
        Assert.Empty(registry.RegisteredTypes);
    }
}
