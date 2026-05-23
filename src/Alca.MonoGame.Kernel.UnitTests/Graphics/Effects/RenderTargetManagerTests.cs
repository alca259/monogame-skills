using Alca.MonoGame.Kernel.Graphics.Effects;

namespace Alca.MonoGame.Kernel.UnitTests.Graphics.Effects;

// RenderTargetManager requires a live GraphicsDevice and SpriteBatch (GPU dependency).
// GPU tests must run in an integration test project with a headless MonoGame device.
//
// The tests here verify pure-logic behaviour that does not touch the GPU:
// the IDisposable contract via reflection, and that the public API surface
// matches the specification (constructor + BeginCapture/EndCapture/Apply/ApplyChain).

public sealed class RenderTargetManagerApiSurfaceTests
{
    [Fact]
    public void RenderTargetManager_ImplementsIDisposable()
    {
        bool implements = typeof(RenderTargetManager)
            .GetInterfaces()
            .Any(i => i == typeof(IDisposable));

        Assert.True(implements);
    }

    [Fact]
    public void RenderTargetManager_HasExpectedPublicMethods()
    {
        Type t = typeof(RenderTargetManager);

        Assert.NotNull(t.GetMethod("BeginCapture"));
        Assert.NotNull(t.GetMethod("EndCapture"));
        Assert.NotNull(t.GetMethod("Apply"));
        Assert.NotNull(t.GetMethod("ApplyChain"));
        Assert.NotNull(t.GetMethod("Dispose"));
    }
}
