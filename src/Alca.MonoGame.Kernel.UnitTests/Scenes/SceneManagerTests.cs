using Alca.MonoGame.Kernel.Scenes;

namespace Alca.MonoGame.Kernel.UnitTests.Scenes;

public sealed class SceneManagerTests
{
    private static GameTime MakeGameTime(float elapsedSeconds) =>
        new GameTime(TimeSpan.FromSeconds(elapsedSeconds), TimeSpan.FromSeconds(elapsedSeconds));

    [Fact]
    public void CurrentScene_IsNull_WhenFirstCreated()
    {
        SceneManager sut = new();

        Assert.Null(sut.CurrentScene);
    }

    [Fact]
    public void FadeAlpha_IsZero_WhenNoTransitionIsActive()
    {
        SceneManager sut = new();

        Assert.Equal(0f, sut.FadeAlpha);
    }

    [Fact]
    public void RequestChange_WhenNoFadeActive_StartsFadingOut()
    {
        SceneManager sut = new();
        MockScene scene = new();

        sut.RequestChange(scene);
        sut.Update(MakeGameTime(0.001f));

        Assert.True(sut.FadeAlpha > 0f);
    }

    [Fact]
    public void Update_FadingOut_IncreasesAlpha_Proportionally()
    {
        SceneManager sut = new();
        MockScene scene = new();

        sut.RequestChange(scene);
        sut.Update(MakeGameTime(0.15f));

        Assert.True(Math.Abs(sut.FadeAlpha - 0.5f) < 0.01f);
    }

    [Fact]
    public void Update_FadingOut_DoesNotExceedAlphaOne()
    {
        SceneManager sut = new();
        MockScene scene = new();

        sut.RequestChange(scene);
        sut.Update(MakeGameTime(0.29f));

        Assert.True(sut.FadeAlpha <= 1.0f);
    }

    [Fact]
    public void RequestChange_WhenAlreadyFading_DoesNotResetFadeTimer()
    {
        SceneManager sut = new();
        MockScene sceneA = new();
        MockScene sceneB = new();

        sut.RequestChange(sceneA);
        sut.Update(MakeGameTime(0.1f));
        float alphaAfterFirstUpdate = sut.FadeAlpha;

        sut.RequestChange(sceneB);
        sut.Update(MakeGameTime(0.001f));

        Assert.True(sut.FadeAlpha > alphaAfterFirstUpdate);
    }

    private sealed class MockScene : Scene { }
}
