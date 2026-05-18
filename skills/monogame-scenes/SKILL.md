---
name: monogame-scenes
description: MonoGame scene and screen management implementation guide covering SceneManager patterns, scene lifecycle, ContentManager per-scene, transitions, and UI layering. Use this skill whenever the user asks about scenes, screens, screen management, game states, transitions between levels, pause menus, title screens, loading screens, or how to organize the top-level structure of a MonoGame game.
---

# MonoGame Scene Management Implementation Guide

This skill guides scene/screen architecture in MonoGame. For transition effect code, see `references/scenes.md`.

## Scene Interface

Every scene implements this lifecycle. Use an abstract base class or interface:

```csharp
public abstract class Scene
{
    protected Game  Game    { get; }
    protected SpriteBatch SpriteBatch { get; }
    protected ContentManager Content { get; }

    protected Scene(Game game, SpriteBatch spriteBatch)
    {
        Game        = game;
        SpriteBatch = spriteBatch;
        Content     = new ContentManager(game.Services, "Content");
    }

    public virtual  void Initialize()               { }
    public virtual  void LoadContent()              { }
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);
    public virtual  void UnloadContent()
    {
        Content.Unload();
        Content.Dispose();
    }
}
```

Each scene owns its own `ContentManager`. Call `UnloadContent()` when leaving the scene to release its assets.

## SceneManager (State Machine)

The `SceneManager` sits inside `Game1` and delegates `Update`/`Draw` to the active scene:

```csharp
public class SceneManager
{
    private Scene _currentScene;
    private Scene _nextScene;    // deferred switch — never switch mid-Update

    public void SwitchTo(Scene newScene)
    {
        _nextScene = newScene;   // queued, not immediate
    }

    public void Update(GameTime gameTime)
    {
        // Apply deferred switch at the start of a frame
        if (_nextScene != null)
        {
            _currentScene?.UnloadContent();
            _currentScene = _nextScene;
            _nextScene    = null;
            _currentScene.Initialize();
            _currentScene.LoadContent();
        }
        _currentScene?.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        _currentScene?.Draw(gameTime);
    }
}
```

**Never switch scenes mid-Update** — queue the switch with `_nextScene` and apply it at the top of the next frame. Switching mid-Update can invalidate iterators and cause null-reference errors in the current scene's Update logic.

## Scene Stack (for Pause / Overlays)

When you need the game scene to remain visible under a pause menu, use a stack:

```csharp
private readonly Stack<Scene> _stack = new();

public void Push(Scene scene)
{
    scene.Initialize();
    scene.LoadContent();
    _stack.Push(scene);
}

public void Pop()
{
    if (_stack.Count == 0) return;
    _stack.Peek().UnloadContent();
    _stack.Pop();
}

public void Update(GameTime gameTime)
{
    // Only the top scene updates:
    _stack.Peek()?.Update(gameTime);
}

public void Draw(GameTime gameTime)
{
    // All scenes draw, bottom to top:
    foreach (var scene in _stack.Reverse())
        scene.Draw(gameTime);
}
```

The game scene is paused (no Update) but remains visible behind the pause overlay. The pause scene draws on top.

## Scene Transitions

For fade/slide transitions between scenes:

1. Render the outgoing scene to a `RenderTarget2D` on the last frame before switching.
2. Render the incoming scene normally.
3. Use an alpha accumulator to blend between the two targets over N frames.

```csharp
private float _transitionAlpha = 0f;
private const float TransitionSpeed = 2.0f; // seconds to complete

// In Update during transition:
_transitionAlpha += (float)gameTime.ElapsedGameTime.TotalSeconds * TransitionSpeed;
if (_transitionAlpha >= 1f)
    CompleteTransition(); // switch to new scene

// In Draw:
DrawCurrentScene();
// Overlay outgoing scene fading out:
_spriteBatch.Begin();
_spriteBatch.Draw(_outgoingTarget, Vector2.Zero,
    new Color(1f, 1f, 1f, 1f - _transitionAlpha));
_spriteBatch.End();
```

## UI Layering

Draw UI in a separate `SpriteBatch.Begin`/`End` pair at the end of `Draw()`, after all world geometry. UI is always in screen space (no camera transform):

```csharp
protected override void Draw(GameTime gameTime)
{
    GraphicsDevice.Clear(Color.Black);

    // World — with camera matrix
    _spriteBatch.Begin(SpriteSortMode.BackToFront, transformMatrix: _cameraMatrix);
    DrawWorld();
    _spriteBatch.End();

    // UI — no camera, screen space only
    _spriteBatch.Begin(SpriteSortMode.Deferred);
    DrawHUD();
    DrawDialogue();
    _spriteBatch.End();
}
```

Never mix world sprites and UI sprites in the same `Begin`/`End` pair — UI positions would be affected by the camera transform.

## Asset Isolation Between Scenes

- Never hold a reference to a texture or sound loaded by Scene A from Scene B.
- If two scenes share assets (e.g., a common font), load them with the root `Content` manager in `Game1.LoadContent()` and pass them as constructor arguments to scenes.
- Scene-specific assets (level tileset, boss music) go in the scene's own `ContentManager`.

## Rules

- Never switch scenes mid-Update — queue with `_nextScene` and apply at the top of the next frame.
- Each scene owns a `ContentManager` — call `Unload()` + `Dispose()` in `UnloadContent()`.
- Never store cross-scene asset references — each scene is self-contained.
- UI draws in a separate `SpriteBatch.Begin/End` with no camera transform.
- On stack-based managers, only the top scene updates; all scenes draw bottom-to-top.

## Reference

For transition effect implementations (fade, slide, wipe) and loading screen patterns, see `references/scenes.md`.
