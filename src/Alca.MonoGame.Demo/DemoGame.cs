using Alca.MonoGame.Demo.Scenes;

namespace Alca.MonoGame.Demo;

/// <summary>Entry point for the MonoGame demo application.</summary>
public sealed class DemoGame : Core
{
    public DemoGame() : base("Alca MonoGame Demo", 1280, 720, false) { }

    protected override void PostInitialize()
    {
        Core.SceneManager.RequestChange(new UIDemoScene());
    }
}
