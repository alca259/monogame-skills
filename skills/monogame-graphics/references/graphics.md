# MonoGame Graphics Reference

Extracted from the official MonoGame documentation. Read specific sections as needed.

## Table of Contents
1. [SpriteBatch.Draw overloads](#spritebatch-draw-overloads)
2. [SpriteBatch.Begin overloads](#spritebatch-begin-overloads)
3. [SpriteEffects](#spriteeffects)
4. [RenderTarget2D creation](#rendertarget2d-creation)
5. [BlendState configurations](#blendstate-configurations)
6. [GraphicsDeviceManager settings](#graphicsdevicemanager-settings)
7. [Viewport and display area](#viewport-and-display-area)
8. [Drawing text with SpriteFont](#drawing-text-with-spritefont)
9. [Sprite animation pattern](#sprite-animation-pattern)

---

## SpriteBatch.Draw overloads

### Basic draw (texture + destination + color)
```csharp
spriteBatch.Draw(
    texture,            // Texture2D
    position,           // Vector2 — top-left position in world/screen space
    Color.White         // Color tint (Color.White = no tint)
);
```

### Full overload
```csharp
spriteBatch.Draw(
    texture,            // Texture2D
    destinationRect,    // Rectangle — stretches texture to fill this rect
    sourceRect,         // Rectangle? — source region in texture atlas (null = full texture)
    color,              // Color
    rotation,           // float — radians, clockwise
    origin,             // Vector2 — pivot point in texture coordinates
    effects,            // SpriteEffects.None / FlipHorizontally / FlipVertically
    layerDepth          // float — 0.0f (front) to 1.0f (back)
);
```

### Rotation origin
`origin` is in texture-local coordinates (pixels from top-left). To rotate around center:
```csharp
var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
```

---

## SpriteBatch.Begin overloads

Full signature:
```csharp
spriteBatch.Begin(
    sortMode,           // SpriteSortMode — default: Deferred
    blendState,         // BlendState — default: AlphaBlend
    samplerState,       // SamplerState — default: LinearClamp
    depthStencilState,  // DepthStencilState — default: None
    rasterizerState,    // RasterizerState — default: CullCounterClockwise
    effect,             // Effect — custom shader (null = built-in)
    transformMatrix     // Matrix? — camera or scale transform (null = identity)
);
```

Most common call patterns:
```csharp
// Pixel art game with camera
spriteBatch.Begin(
    SpriteSortMode.BackToFront,
    BlendState.AlphaBlend,
    SamplerState.PointClamp,
    null, null, null,
    cameraMatrix
);

// Particles / additive effects
spriteBatch.Begin(
    SpriteSortMode.Deferred,
    BlendState.Additive
);

// Post-process pass through Effect
spriteBatch.Begin(
    SpriteSortMode.Immediate,
    BlendState.Opaque,
    SamplerState.LinearClamp,
    null, null,
    myPostProcessEffect
);
```

---

## SpriteEffects

```csharp
SpriteEffects.None             // Normal
SpriteEffects.FlipHorizontally // Mirror left-right
SpriteEffects.FlipVertically   // Mirror top-bottom
```

Combine with `|` operator:
```csharp
SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
```

---

## RenderTarget2D creation

### Basic creation
```csharp
var renderTarget = new RenderTarget2D(
    GraphicsDevice,
    width,   // int
    height   // int
);
```

### With depth buffer
```csharp
var renderTarget = new RenderTarget2D(
    GraphicsDevice,
    width, height,
    false,                          // mipMap
    SurfaceFormat.Color,            // preferredFormat
    DepthFormat.Depth24             // preferredDepthFormat
);
```

### Usage pattern
```csharp
// In Draw():
GraphicsDevice.SetRenderTarget(_renderTarget);
GraphicsDevice.Clear(Color.Transparent);

_spriteBatch.Begin();
// ... draw scene to target
_spriteBatch.End();

// Restore back buffer
GraphicsDevice.SetRenderTarget(null);

// Now use _renderTarget as a texture
_spriteBatch.Begin(effect: _postProcessEffect);
_spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
_spriteBatch.End();
```

---

## BlendState configurations

### Premultiplied alpha (default alpha blend)
```csharp
BlendState.AlphaBlend
// Result.rgb = Source.rgb + Dest.rgb * (1 - Source.a)
// Correct only for premultiplied textures (default from content pipeline)
```

### Non-premultiplied (raw RGBA textures)
```csharp
BlendState.NonPremultiplied
// Result.rgb = Source.rgb * Source.a + Dest.rgb * (1 - Source.a)
```

### Additive (particles, lights)
```csharp
BlendState.Additive
// Result.rgb = Source.rgb + Dest.rgb (ignores alpha for accumulation)
```

### Custom BlendState (create once in LoadContent)
```csharp
var customBlend = new BlendState
{
    AlphaBlendFunction = BlendFunction.Add,
    AlphaSourceBlend = Blend.One,
    AlphaDestinationBlend = Blend.InverseSourceAlpha
};
```

---

## GraphicsDeviceManager settings

Set in the `Game` constructor:
```csharp
_graphics = new GraphicsDeviceManager(this)
{
    PreferredBackBufferWidth  = 1920,
    PreferredBackBufferHeight = 1080,
    IsFullScreen              = false,
    SynchronizeWithVerticalRetrace = true  // vsync
};
_graphics.ApplyChanges();
```

Toggle fullscreen at runtime:
```csharp
_graphics.ToggleFullScreen();
_graphics.ApplyChanges();
```

---

## Viewport and display area

```csharp
Viewport vp = GraphicsDevice.Viewport;
int screenW = vp.Width;
int screenH = vp.Height;

// Safe display area (accounts for overscan on TV)
Rectangle safeArea = vp.TitleSafeArea;
```

Split-screen — set viewport before each player's SpriteBatch.Begin:
```csharp
GraphicsDevice.Viewport = new Viewport(0, 0, screenW / 2, screenH);       // Player 1 (left)
// ... draw player 1 view ...
GraphicsDevice.Viewport = new Viewport(screenW / 2, 0, screenW / 2, screenH); // Player 2 (right)
// ... draw player 2 view ...
GraphicsDevice.Viewport = new Viewport(0, 0, screenW, screenH);           // Restore full viewport
```

---

## Drawing text with SpriteFont

Load in `LoadContent()`:
```csharp
_font = Content.Load<SpriteFont>("Fonts/MyFont");
```

Draw:
```csharp
spriteBatch.DrawString(
    _font,
    "Hello World",
    new Vector2(100, 50),   // position
    Color.White
);
```

Measure text before drawing (e.g., for centering):
```csharp
Vector2 size = _font.MeasureString(text);
Vector2 centeredPos = new Vector2(
    screenWidth  / 2f - size.X / 2f,
    screenHeight / 2f - size.Y / 2f
);
```

---

## Sprite animation pattern

Store all animation frames as `Rectangle[]` pointing into a texture atlas:
```csharp
// Define frames (source rectangles in the atlas)
private Rectangle[] _frames;
private int _currentFrame;
private float _frameTimer;
private float _frameDuration = 0.1f; // seconds per frame

// In LoadContent:
_frames = new Rectangle[]
{
    new Rectangle(0, 0, 32, 32),
    new Rectangle(32, 0, 32, 32),
    new Rectangle(64, 0, 32, 32),
    new Rectangle(96, 0, 32, 32),
};

// In Update:
_frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
if (_frameTimer >= _frameDuration)
{
    _frameTimer -= _frameDuration;
    _currentFrame = (_currentFrame + 1) % _frames.Length;
}

// In Draw:
spriteBatch.Draw(_atlas, _position, _frames[_currentFrame], Color.White);
```

Do not allocate the `Rectangle[]` in `Update()` or `Draw()`.
