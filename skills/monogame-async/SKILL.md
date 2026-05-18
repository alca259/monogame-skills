---
name: monogame-async
description: MonoGame asynchronous programming guide covering async/await with Task, the legacy Begin/End APM pattern, loading screen patterns with background content loading, GraphicsDevice thread safety rules, and safe cross-thread UI updates. Use this skill whenever the user asks about loading screens, background loading, async operations, freezing/stuttering during load, Task or async/await in MonoGame, non-blocking I/O, save game operations, or threading — even if they just say "my game freezes while loading" or "how do I load in the background".
---

# MonoGame Async Programming Guide

This skill covers asynchronous patterns in MonoGame: when and how to use them safely, the main thread constraints, and practical loading screen implementation. For ready-to-copy code, see `references/async.md`.

## The Core Rule: GraphicsDevice Owns the Main Thread

MonoGame's `GraphicsDevice`, `SpriteBatch`, and `ContentManager` are **not thread-safe**. All rendering calls (`Draw`) and content loading via `Content.Load<T>()` must happen on the main game thread.

Work you **can** safely do on background threads:
- File I/O (reading/writing save data, JSON, config)
- Network requests
- Parsing/deserialization of game data
- Heavy computation (pathfinding, world generation)
- Pre-reading raw bytes from disk

Work you **must not** do on background threads:
- `Content.Load<T>()` or any `ContentManager` call
- Calling `GraphicsDevice` methods
- Drawing to `RenderTarget2D`
- Creating `Texture2D`, `SoundEffect`, or other GPU resources

## Two Async Patterns in MonoGame

### Pattern 1 — Modern: async/await with Task (recommended)

Use `Task.Run` to push work off the main thread, then capture the result back. The game loop continues to tick during the background work.

```csharp
// Fire off background work and continue ticking:
_loadTask = Task.Run(() => LoadWorldDataFromDisk(savePath));

// In Update(), check completion without blocking:
if (_loadTask.IsCompleted)
{
    _worldData = _loadTask.Result; // safe: already completed
    _state = GameState.Playing;
}
```

### Pattern 2 — Legacy: Begin/End APM

Some older MonoGame APIs (e.g., `Guide.BeginShowMessageBox` on Xbox/Mobile, storage APIs) still use the APM pattern with `IAsyncResult`. The structure is always:

```csharp
// Begin: starts the operation, returns IAsyncResult
IAsyncResult result = SomeApi.BeginOperation(param, callback: null, state: null);

// In Update() — polling approach:
if (result.IsCompleted)
    var data = SomeApi.EndOperation(result); // always call End — prevents deadlocks
```

Or with a callback (fires on a thread pool thread — do not touch GraphicsDevice here):

```csharp
SomeApi.BeginOperation(param, ar =>
{
    var data = SomeApi.EndOperation(ar);
    // Queue the result; consume it in Update() on the main thread
    _pendingResult = data;
}, state: null);
```

**Always call the matching `End` method** — skipping it leaks resources and can deadlock.

## Loading Screen Pattern

The standard approach for a loading screen in MonoGame:

1. Switch to a `LoadingScene` that draws an animated spinner/progress bar.
2. Start a `Task` that loads game data (raw bytes, JSON, etc.) in the background.
3. When the task completes, call `Content.Load<T>()` **on the main thread** to push assets to the GPU.
4. Transition to the gameplay scene.

```
[LoadingScene.Enter()]
    └─ Task.Run(LoadRawDataAsync)     ← background thread: disk I/O, parsing
         └─ on completion → flag set
              └─ Update() detects flag
                   └─ Content.Load<T>() on main thread  ← GPU upload
                        └─ transition to GameScene
```

This split (background I/O + main-thread GPU upload) is the safest model and avoids any thread-safety issues.

## Progress Reporting

Use `IProgress<T>` to report progress back to the main thread safely (the implementation marshals the callback via the synchronization context):

```csharp
var progress = new Progress<float>(pct => _loadProgress = pct);
_loadTask = Task.Run(() => LoadWithProgress(savePath, progress));
```

In the loading screen `Draw()`:
```csharp
DrawProgressBar(_loadProgress); // 0.0f → 1.0f
```

## Save Game and File I/O

Save/load operations should always be async to avoid frame spikes:

```csharp
// Save — fire and forget is acceptable for auto-save; await for manual save:
await File.WriteAllTextAsync(savePath, JsonSerializer.Serialize(_saveData));

// Load — run in background, apply result on main thread:
_loadTask = Task.Run(async () =>
    JsonSerializer.Deserialize<SaveData>(await File.ReadAllTextAsync(savePath)));
```

Never block the main thread with `File.ReadAllText` (synchronous) for files larger than a few KB.

## Anti-Patterns to Avoid

- **Never `task.Wait()` or `task.Result` on the main game thread** unless the task is already completed (`IsCompleted == true`). Blocking the main thread stalls the game loop and freezes rendering.
- **Never call `Content.Load<T>()` from a `Task.Run` body** — content loading touches the GPU.
- **Never call `Begin`/`End` from different threads** — `End` must be called by the same logical operation that owns the `IAsyncResult`.
- **Never use `async void` for game logic** — use `async Task` so exceptions propagate and are observable. Exception: event handlers that must be `void` (e.g., button click events).
- **Never read `task.Result` without checking `IsCompleted` first** in `Update()` — it blocks if the task is still running.

## Reference

For `Task`-based loading screen scaffolding, progress bar helpers, APM wrapper utilities, and save/load patterns, see `references/async.md`.
