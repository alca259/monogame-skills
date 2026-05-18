---
name: monogame-graphics
description: MonoGame graphics implementation guide covering SpriteBatch patterns, render targets, camera systems, blend states, resolution independence, and GPU performance rules. Use this skill whenever the user asks about drawing sprites, SpriteBatch usage, rendering layers, camera 2D, post-processing, render targets, blend/sampler states, resolution scaling, or any visual rendering in MonoGame — even if they just say "how do I draw X" or "my sprites are wrong" without mentioning graphics explicitly.
---

# MonoGame Graphics Implementation Guide

This skill provides architecture rules and implementation patterns for MonoGame graphics. Apply the rules below directly when writing rendering code. For detailed API signatures and code samples, read `references/graphics.md`.

## SpriteBatch Fundamentals

`SpriteBatch` is the primary 2D rendering API. Every `Begin()`/`End()` pair flushes a draw batch to the GPU — minimize how many pairs you use per frame.

### Choosing a Sort Mode

| Mode | When to use |
|------|-------------|
| `Deferred` (default) | General sprite rendering; buffers all calls, sorts by texture at `End()` |
| `BackToFront` | Overlapping sprites with explicit depth; uses `layerDepth` parameter |
| `FrontToBack` | Same as above, reversed |
| `Texture` | Many sprites from multiple atlases; groups by texture to reduce state changes |
| `Immediate` | Per-sprite `Effect` parameter changes; sends each draw to GPU instantly — avoid unless needed |

### BlendState

Use the static presets — never recreate `BlendState` instances per frame:
- `BlendState.AlphaBlend` — standard transparency (default)
- `BlendState.Additive` — for particles, lights, and glow effects (colors stack and brighten)
- `BlendState.NonPremultiplied` — for textures loaded without premultiplied alpha
- `BlendState.Opaque` — for fully opaque geometry; disables blending

### SamplerState

- `SamplerState.PointClamp` — nearest-neighbor filtering; **required for pixel art** to prevent blurring
- `SamplerState.LinearClamp` — bilinear filtering for smooth high-res assets
- `SamplerState.AnisotropicClamp` — for 3D textured surfaces at oblique angles

## Sprite Layering

Use `layerDepth` (0.0f = front, 1.0f = back) with `SpriteSortMode.BackToFront` to control draw order without multiple `Begin`/`End` pairs. Define layers as named constants:

```csharp
// Define once, use everywhere — no magic numbers
private const float LayerBackground = 1.0f;
private const float LayerTerrain    = 0.8f;
private const float LayerEntities   = 0.5f;
private const float LayerEffects    = 0.3f;
private const float LayerUI         = 0.0f;
```

## RenderTarget2D

Render targets let you draw to an off-screen texture for post-processing, UI compositing, or multi-pass shader effects.

**Rules:**
- Create `RenderTarget2D` in `LoadContent()` — never in `Update()` or `Draw()`
- After drawing to a target, always restore with `GraphicsDevice.SetRenderTarget(null)` before presenting to screen
- Call `Dispose()` explicitly when unloading a scene — `RenderTarget2D` holds GPU memory
- When chaining passes (e.g., blur then color grade), use two targets and ping-pong between them

**Canonical frame pattern with render target:**
1. `SetRenderTarget(myTarget)` → clear → draw scene
2. `SetRenderTarget(null)` → draw `myTarget` through a post-process `Effect`

## Resolution Independence

Design at a fixed virtual resolution and scale to any screen size using a transform matrix — this keeps all game logic in virtual coordinates.

```csharp
// Compute once per frame (or when window resizes)
float scaleX = GraphicsDevice.Viewport.Width  / (float)VirtualWidth;
float scaleY = GraphicsDevice.Viewport.Height / (float)VirtualHeight;
Matrix scaleMatrix = Matrix.CreateScale(scaleX, scaleY, 1f);

// Use in every SpriteBatch.Begin call
_spriteBatch.Begin(transformMatrix: scaleMatrix);
```

Never scatter screen-size conditionals across gameplay code — all positions and sizes stay in virtual space.

## Camera 2D

A 2D camera is a transform matrix built from position, zoom, and rotation. Compute it once per `Update()` frame and cache it as a field.

```csharp
// Build camera matrix (origin at screen center)
_cameraMatrix =
    Matrix.CreateTranslation(-_cameraPos.X, -_cameraPos.Y, 0f) *
    Matrix.CreateRotationZ(_cameraRotation) *
    Matrix.CreateScale(_cameraZoom, _cameraZoom, 1f) *
    Matrix.CreateTranslation(Viewport.Width / 2f, Viewport.Height / 2f, 0f);

// Apply
_spriteBatch.Begin(transformMatrix: _cameraMatrix);
```

To convert screen coordinates to world coordinates (e.g., mouse position):
```csharp
Vector2 worldPos = Vector2.Transform(screenPos, Matrix.Invert(_cameraMatrix));
```

Cache `Matrix.Invert` if called every frame — matrix inversion is not free.

## Performance Rules

These prevent GC stalls and frame-rate stuttering:

- **No allocations in `Update()` / `Draw()`** — never write `new Texture2D(...)`, `new RenderTarget2D(...)`, `new SpriteBatch(...)`, or `new Vector2(...)` inside the game loop. Allocate in `LoadContent()` or `Initialize()`.
- **Mutate struct fields instead of reassigning** — write `_pos.X = x; _pos.Y = y;` not `_pos = new Vector2(x, y)`.
- **Minimize Begin/End pairs** — each pair is a GPU flush. Group sprites that share the same `Effect`, `BlendState`, and `SamplerState` into one batch.
- **Use static Color presets** — `Color.White`, `Color.Black`, etc. When you need a custom tint, store it as a class field.

## Anti-Patterns to Avoid

- Never call `Begin()` without a matching `End()` in the same frame path.
- Never share a `SpriteBatch` across threads — it is not thread-safe.
- Never sample from a `RenderTarget2D` while it is also set as the active render target — GPU undefined behavior.
- Never use `SpriteSortMode.Immediate` with a shared `Effect` across sprites without resetting parameters between draws.

## Reference

For detailed method overloads, `Draw()` parameter descriptions, and worked examples, read `references/graphics.md`.
