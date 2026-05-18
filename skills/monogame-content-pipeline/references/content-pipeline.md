# MonoGame Content Pipeline Reference

## Table of Contents
1. [ContentManager API](#contentmanager-api)
2. [Built-in asset types and load paths](#built-in-asset-types-and-load-paths)
3. [Multiple ContentManagers (per-scene scoping)](#multiple-contentmanagers-per-scene-scoping)
4. [SpriteFont .spritefont schema](#spritefont-spritefont-schema)
5. [Custom XML data (IntermediateSerializer)](#custom-xml-data-intermediateserializer)
6. [Custom Processor scaffold](#custom-processor-scaffold)
7. [Extending a built-in processor](#extending-a-built-in-processor)
8. [Loading content in a Game Library](#loading-content-in-a-game-library)
9. [Android texture compression](#android-texture-compression)

---

## ContentManager API

```csharp
// Root manager — available via Game.Content:
Content.RootDirectory = "Content"; // default

// Load an asset (path relative to RootDirectory, no extension):
Texture2D tex   = Content.Load<Texture2D>("Sprites/Player");
SoundEffect sfx = Content.Load<SoundEffect>("Audio/Jump");
Song music      = Content.Load<Song>("Audio/Theme");
SpriteFont font = Content.Load<SpriteFont>("Fonts/UI");
Effect shader   = Content.Load<Effect>("Shaders/Vignette");
Model model     = Content.Load<Model>("Models/Character");

// Load typed custom XML data:
MyData data = Content.Load<MyData>("Data/LevelConfig");

// Unload ALL assets loaded through this manager:
Content.Unload();

// Create a secondary manager (for scene-scoped loading):
var sceneContent = new ContentManager(Services, "Content");
// ... load scene assets ...
sceneContent.Unload();
sceneContent.Dispose();
```

Path rules:
- Relative to the `Content/` folder (the MGCB output root)
- Case-sensitive on Linux and macOS
- No file extension
- Subdirectory separator: `/` on all platforms

---

## Built-in asset types and load paths

| C# type | Typical MGCB processor | Source formats |
|---------|------------------------|----------------|
| `Texture2D` | Texture - MonoGame | PNG, JPG, BMP, TGA, GIF |
| `SoundEffect` | Sound Effect - MonoGame | WAV |
| `Song` | Song - MonoGame | MP3, OGG, WMA, M4A |
| `SpriteFont` | Sprite Font Description - MonoGame | `.spritefont` XML |
| `Effect` | Effect - MonoGame | `.fx` (HLSL) |
| `Model` | Model - MonoGame | FBX, OBJ |
| Custom type | Xml Importer - MonoGame | `.xml` (IntermediateSerializer) |

---

## Multiple ContentManagers (per-scene scoping)

```csharp
// Global/shared assets — load in Game.LoadContent(), never unload mid-game:
Texture2D _cursor = Content.Load<Texture2D>("UI/Cursor");
SpriteFont _uiFont = Content.Load<SpriteFont>("Fonts/UI");

// Per-scene assets — each scene creates and owns a ContentManager:
public class GameScene
{
    private ContentManager _content;

    public void LoadContent(IServiceProvider services)
    {
        _content = new ContentManager(services, "Content");
        _playerTexture = _content.Load<Texture2D>("Sprites/Player");
        _tilesheet     = _content.Load<Texture2D>("Tilesets/Dungeon");
        _music         = _content.Load<Song>("Audio/DungeonTheme");
    }

    public void UnloadContent()
    {
        MediaPlayer.Stop();          // stop streaming before unload
        _content.Unload();
        _content.Dispose();
        _content = null;
    }
}
```

**Never load the same asset through two different `ContentManager` instances** — it creates a duplicate in memory.

---

## SpriteFont .spritefont schema

```xml
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:Graphics="Microsoft.Xna.Framework.Content.Pipeline.Graphics">
  <Asset Type="Graphics:FontDescription">

    <!-- System font name (must be installed on the build machine) -->
    <FontName>Arial</FontName>

    <!-- Point size -->
    <Size>16</Size>

    <!-- Extra pixels between characters -->
    <Spacing>0</Spacing>

    <!-- Kerning pairs from the font -->
    <UseKerning>true</UseKerning>

    <!-- Regular, Bold, Italic, Bold Italic -->
    <Style>Regular</Style>

    <!-- Fallback character for unmapped glyphs -->
    <DefaultCharacter>*</DefaultCharacter>

    <!-- Unicode ranges to include -->
    <CharacterRegions>
      <!-- Basic Latin (space through tilde) -->
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
      <!-- Latin-1 Supplement (accented characters: é, ñ, ü, etc.) -->
      <CharacterRegion>
        <Start>&#160;</Start>
        <End>&#255;</End>
      </CharacterRegion>
    </CharacterRegions>

  </Asset>
</XnaContent>
```

Add this file to the MGCB project and set the processor to **Sprite Font Description - MonoGame**.

---

## Custom XML data (IntermediateSerializer)

### Step 1 — Define the data class (in a shared library or the game project)

```csharp
// Must be public, serializable fields/properties:
public class EnemyData
{
    public string Name;
    public int    Health;
    public float  Speed;
    public List<string> Drops = new();
}
```

### Step 2 — Generate the XML file (run once with IntermediateSerializer)

```csharp
// Console app using MonoGame.Framework.Content.Pipeline:
using var writer = XmlWriter.Create("EnemyConfig.xml", new XmlWriterSettings { Indent = true });
IntermediateSerializer.Serialize(writer, new EnemyData
{
    Name = "Goblin", Health = 30, Speed = 1.5f, Drops = { "Gold", "Key" }
}, null);
```

The resulting XML format is recognized by the Content Pipeline **Xml Importer - MonoGame** processor.

### Step 3 — Add to MGCB and load

```csharp
// In MGCB Editor: set Importer = "Xml Importer - MonoGame", Processor = "No Processing Required"
EnemyData enemy = Content.Load<EnemyData>("Data/EnemyConfig");
```

---

## Custom Processor scaffold

Create a **separate Class Library** project (e.g., `MyGame.Pipeline`):

```xml
<!-- MyGame.Pipeline.csproj -->
<ItemGroup>
  <PackageReference Include="MonoGame.Framework.Content.Pipeline" Version="3.8.*" />
</ItemGroup>
```

```csharp
using Microsoft.Xna.Framework.Content.Pipeline;

[ContentImporter(".tmx", DisplayName = "Tiled Map Importer", DefaultProcessor = "TiledMapProcessor")]
public class TiledMapImporter : ContentImporter<string>
{
    public override string Import(string filename, ContentImporterContext context)
    {
        return File.ReadAllText(filename);
    }
}

[ContentProcessor(DisplayName = "Tiled Map Processor")]
public class TiledMapProcessor : ContentProcessor<string, TilemapData>
{
    public override TilemapData Process(string input, ContentProcessorContext context)
    {
        // Parse input XML/JSON, return strongly-typed TilemapData
        return new TilemapData { /* ... */ };
    }
}

[ContentTypeWriter]
public class TilemapWriter : ContentTypeWriter<TilemapData>
{
    protected override void Write(ContentWriter output, TilemapData value)
    {
        output.Write(value.Width);
        output.Write(value.Height);
        // write all data
    }
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
        => "MyGame.TilemapReader, MyGame"; // namespace.ClassName, AssemblyName
}
```

Reference the compiled `.dll` in the MGCB file under **References**.

---

## Extending a built-in processor

```csharp
[ContentProcessor(DisplayName = "Model Processor - With Tangents")]
public class ModelWithTangentsProcessor : ModelProcessor
{
    // Override to add tangent/binormal data:
    protected override void ProcessGeometry(MeshContent mesh, ContentProcessorContext context)
    {
        MeshHelper.CalculateTangentFrames(mesh,
            VertexChannelNames.TextureCoordinate(0),
            VertexChannelNames.Tangent(0),
            VertexChannelNames.Binormal(0));
        base.ProcessGeometry(mesh, context);
    }
}
```

---

## Loading content in a Game Library

```csharp
// Option A — Load .xnb files from disk (same Content pipeline):
public class MyComponent : DrawableGameComponent
{
    private ContentManager _localContent;

    protected override void LoadContent()
    {
        _localContent = new ContentManager(Game.Services, "Content");
        _texture = _localContent.Load<Texture2D>("MyLib/Texture");
    }
}

// Option B — Embed assets as resources and use ResourceContentManager:
// Add files to the class library with Build Action = Embedded Resource
// Then:
var resourceContent = new ResourceContentManager(services, MyLibResources.ResourceManager);
_texture = resourceContent.Load<Texture2D>("EmbeddedTextureName");
```

---

## Android texture compression

Add textures to MGCB with the appropriate processor output format:

```
# In Content.mgcb — add a Reference to the compressor pipeline:
/processorParam:TextureFormat=Compressed

# Or per-texture in MGCB Editor:
# Processor: Texture - MonoGame
# Processor Parameters > Texture Format: Compressed
```

For distribution `.aab` files with multiple compression formats, add suffixed directories:

| Suffix | Format |
|--------|--------|
| `_etc2` | ETC2 (required, Android 4.3+) |
| `_dxt` | S3TC/DXT (Nvidia Tegra) |
| `_atc` | ATC (Qualcomm Adreno) |
| `_pvrtc` | PVRTC (older PowerVR) |
| `_astc` | ASTC (modern, best quality) |
