---
name: monogame-content-pipeline
description: MonoGame content pipeline implementation guide covering MGCB, ContentManager, asset loading patterns, custom processors, SpriteFont, and asset lifecycle. Use this skill whenever the user asks about loading textures, sounds, fonts, effects, or any assets; MGCB configuration; content build errors; custom importers/processors; or ContentManager usage in MonoGame — even if they just say "how do I load X" or "my content isn't loading".
---

# MonoGame Content Pipeline Implementation Guide

This skill guides asset loading and content pipeline usage in MonoGame. For custom processor code patterns, see `references/content-pipeline.md`.

## How the Pipeline Works

```
Source asset → Importer → Processor → Writer → .xnb file (compiled)
```

- The **.mgcb file** defines which assets to build, which importer/processor to use, and build settings.
- The **MGCB Editor** is a GUI tool to manage the `.mgcb` file — use it to add, configure, and remove assets.
- At runtime, `ContentManager.Load<T>()` reads the compiled `.xnb` files.

## Loading Assets

All asset loading belongs in `LoadContent()`. Never load assets in `Update()` or `Draw()`.

```csharp
// Path is relative to the Content/ root, without extension:
Texture2D   playerSprite = Content.Load<Texture2D>("Sprites/Player");
SoundEffect jumpSound    = Content.Load<SoundEffect>("Audio/Jump");
Song        bgMusic      = Content.Load<Song>("Audio/Theme");
SpriteFont  uiFont       = Content.Load<SpriteFont>("Fonts/UI");
Effect      vignetteEffect = Content.Load<Effect>("Shaders/Vignette");
```

Path rules:
- **Relative to Content/ folder** — do not include the Content/ prefix.
- **No file extension** — just the name as defined in the `.mgcb` file.
- **Case-sensitive on Linux/macOS** — use consistent casing in the `.mgcb` file.

## ContentManager Scoping

Use **separate `ContentManager` instances** per scene to make unloading clean:

```csharp
// Create in scene constructor or LoadContent:
_sceneContent = new ContentManager(Game.Services, "Content");

// Load scene assets:
_texture = _sceneContent.Load<Texture2D>("Level1/Tileset");

// Unload all assets for this scene only:
protected override void UnloadContent()
{
    _sceneContent.Unload();
    _sceneContent.Dispose();
}
```

`ContentManager.Unload()` disposes all assets loaded through that manager. Calling the root `Content.Unload()` disposes **every** asset including shared ones — avoid unless exiting.

## Asset Types and Built-in Processors

| C# type | Content type | Processor |
|---------|-------------|-----------|
| `Texture2D` | PNG, JPG, BMP, TGA | Texture - MonoGame |
| `SoundEffect` | WAV | Sound Effect - MonoGame |
| `Song` | MP3, OGG, WMA | Song - MonoGame |
| `SpriteFont` | .spritefont XML | Sprite Font Description - MonoGame |
| `Effect` | .fx (HLSL) | Effect - MonoGame |
| `Model` | FBX, OBJ | Model - MonoGame |

## SpriteFont

Define fonts using a `.spritefont` XML file added to the MGCB. Minimum definition:

```xml
<?xml version="1.0" encoding="utf-8"?>
<XnaContent>
  <Asset Type="Graphics:FontDescription">
    <FontName>Arial</FontName>
    <Size>16</Size>
    <Spacing>0</Spacing>
    <UseKerning>true</UseKerning>
    <Style>Regular</Style>
    <DefaultCharacter>*</DefaultCharacter>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>   <!-- space -->
        <End>&#126;</End>      <!-- tilde -->
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>
```

For non-Latin characters, add additional `CharacterRegion` entries covering the required Unicode ranges. Each language variant of the font should be a separate `.spritefont` file.

## Custom XML Data

For game data (level definitions, configuration, item databases), use `IntermediateSerializer`:

```csharp
// Define the data class:
public class LevelData
{
    public string Name;
    public int Width;
    public int Height;
    public List<TileData> Tiles;
}

// Add the .xml file to the MGCB with processor: "Xml Importer - MonoGame"
// Load at runtime:
LevelData level = Content.Load<LevelData>("Levels/Level01");
```

## Custom Processor

When the built-in processors don't cover your needs (e.g., a tile map format, a custom atlas packer), create a custom processor in a **separate class library project**:

1. Create a new project: `MyGame.Content.Pipeline` (class library).
2. Reference `MonoGame.Framework.Content.Pipeline` NuGet.
3. Inherit from `ContentProcessor<TInput, TOutput>`.
4. Reference the compiled dll from the `.mgcb` file: add the path under **References**.

```csharp
[ContentProcessor(DisplayName = "My Tilemap Processor")]
public class TilemapProcessor : ContentProcessor<string, TilemapData>
{
    public override TilemapData Process(string input, ContentProcessorContext context)
    {
        // parse input, return TilemapData
    }
}
```

## Rules

- All `Content.Load<T>()` calls go in `LoadContent()` — no exceptions.
- Use per-scene `ContentManager` instances so `Unload()` doesn't affect other scenes.
- Never load the same asset twice with different `ContentManager` instances — the second load creates a duplicate in memory.
- Add `.fx` shader files to the MGCB with the **Effect - MonoGame** processor to get the compiled `.mgfxo`.
- On Linux/macOS, asset paths are case-sensitive — keep naming consistent with the `.mgcb` file.

## Reference

For custom importer/processor scaffolding, `IntermediateSerializer` patterns, and Android texture compression, see `references/content-pipeline.md`.
