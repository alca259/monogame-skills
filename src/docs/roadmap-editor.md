# MonoGame Editor — Roadmap técnico (v2)

**Stack**: .NET 10 · C# 14 · WinForms · MonoGame · MonoGame.Extended · MonoGame.Framework.WindowsDX · Alca.MonoGame.Kernel · System.Text.Json  
**Objetivo**: Editor de juegos 2D estilo Unity, integrado con Visual Studio, con viewport MonoGame embebido, jerarquía ECS, gizmos de transform, editores especializados y pipeline de build.
**Reglas transversales a todos los desarrollos:**
- Al terminar, actualizar este fichero marcando los TODOs completados.

---

## Estilo visual e interfaz

Modo oscuro nativo de WinForms (.NET 10). No hay modo claro ni selector de tema.

- `Application.SetColorMode(SystemColorMode.Dark)` en `Program.cs` antes de lanzar el formulario
- Controles estándar heredan colores del sistema (`SystemColors`) sin configuración adicional
- Controles personalizados usan `SystemColors` para coherencia con el tema
- Control de cualquier excepción no controlada en Program.cs con Log de Serilog a disco y un MessageBox.
- Tipografía: `Segoe UI` 9pt en todos los controles
- Iconos: Segoe Fluent Icons o PNG 16×16 / 24×24 con fondo transparente
- Superficies planas, sin gradientes ni sombras
- Botones Play/Pause/Stop con color de acento cuando están activos

---

## Estructura de la solución

```
MonoGame.Editor.sln
├── Alca.MonoGame.Kernel          # Librería existente (referencia, nunca modificar)
├── MonoGame.Editor.Core          # Lógica del editor, sin UI — referencia Kernel + System.Text.Json
├── MonoGame.Editor.WinForms      # Aplicación WinForms — referencia Editor.Core
└── MonoGame.Editor.Templates     # Plantillas dotnet new para proyectos de juego nuevos
```

- El nuget de Alca.MonoGame.Kernel está una carpeta local "F:\Dev\NugetLocal\Alca.MonoGame.Kernel.1.0.0.nupkg" en principio está registrada como fuente de nugets bajo el nombre "DevLocal"

**Reglas de arquitectura:**
- `MonoGame.Editor.Core` no contiene ninguna referencia a `System.Windows.Forms`
- `MonoGame.Editor.WinForms` solo referencia `MonoGame.Editor.Core`
- La comunicación entre paneles es exclusivamente a través de `IEditorEventBus` — los paneles nunca se llaman directamente entre sí
- `EditorContext` es la fuente de verdad única del estado del editor en tiempo de ejecución
- Los `.json` de escena son la fuente de verdad para edición, legibles y versionables con git
- Toda la aplicación del editor debe ser async/await en la medida de lo posible.

---

## Máquina de estados del editor

```
┌────────────┐  Play  ┌────────────┐  Pause  ┌────────────┐
│            │───────►│            │────────►│            │
│  Editing   │        │  Playing   │         │  Paused    │
│            │◄───────│            │◄────────│            │
└────────────┘  Stop  └────────────┘ Resume  └────────────┘
      ▲                                              │
      └──────────────────── Stop ───────────────────┘
```

- **Editing**: cámara independiente del juego, gizmos visibles, game loop parado (render del editor activo)
- **Playing**: snapshot de escena guardado en memoria, game loop real activo, cámara del juego
- **Paused**: render activo, `Update` no ejecuta lógica de juego, inspector editable en caliente
- **Stop**: restaura snapshot, vuelve a `Editing`

---

## Fase 0 — Fundamentos y contratos base

### Objetivo
Infraestructura transversal del editor: estado global, bus de eventos, preferencias persistidas.  
Todo lo demás se construye sobre esta fase.

### Proyecto: MonoGame.Editor.Core

**`EditorContext`** — singleton, fuente de verdad del estado del editor
```csharp
sealed class EditorContext
{
    EditorState State { get; }
    EditorScene? ActiveScene { get; }
    EditorGameObject? SelectedObject { get; }
    IReadOnlyList<EditorGameObject> MultiSelection { get; }
    EditorProject? ActiveProject { get; }

    void SetState(EditorState state);
    void SetSelection(EditorGameObject? obj);
    void SetMultiSelection(IEnumerable<EditorGameObject> objects);
}
```

**`IEditorEventBus`** — comunicación desacoplada entre paneles
```csharp
interface IEditorEventBus
{
    void Publish<TEvent>(TEvent e) where TEvent : IEditorEvent;
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEditorEvent;
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEditorEvent;
}
```

Eventos tipados:
- `GameObjectSelectedEvent` — objeto seleccionado en jerarquía o viewport
- `SceneLoadedEvent` — escena cargada o cambiada
- `ProjectOpenedEvent` — proyecto abierto o cerrado
- `AssetImportedEvent` — asset nuevo detectado por FileWatcher
- `BehaviourAddedEvent` — behaviour adjuntado a un game object
- `UndoPerformedEvent` / `RedoPerformedEvent` — actualiza menú Edit
- `EditorStateChangedEvent` — Play/Pause/Stop

**`EditorPreferences`** — persistencia del layout entre sesiones
```csharp
sealed class EditorPreferences
{
    int LeftPanelWidth { get; set; }
    int RightPanelWidth { get; set; }
    int ConsolePanelHeight { get; set; }
    bool HierarchyVisible { get; set; }
    bool InspectorVisible { get; set; }
    bool AssetBrowserVisible { get; set; }
    bool ConsoleVisible { get; set; }
    string LastProjectPath { get; set; }

    void Save();   // escribe a %APPDATA%/MonoGameEditor/preferences.json
    void Load();
}
```

**`EditorProject`** — representa el proyecto de juego abierto
```csharp
sealed class EditorProject
{
    string Name { get; }
    string RootPath { get; }         // raíz del proyecto (donde vive el .sln/.slnx)
    string EditorPath { get; }       // {RootPath}/Editor/   ← todos los ficheros del editor
    string ScenesPath { get; }       // {EditorPath}/Scenes/
    string PrefabsPath { get; }      // {EditorPath}/Prefabs/
    string ContentPath { get; }      // configurable; default {RootPath}/Content
    string LocalizationPath { get; } // configurable; default {RootPath}/Localization
}
```

**Fichero descriptor:** `{RootPath}/Editor/project.json`
```json
{
  "name": "MyGame",
  "version": "1.0",
  "contentPath": "Content",
  "localizationPath": "Localization"
}
```

**Apertura de proyectos existentes:** si la carpeta seleccionada no tiene `Editor/project.json` pero contiene un `.sln`/`.slnx`, el editor ofrece inicializarlo: crea `Editor/` con su descriptor y crea las carpetas estándar faltantes sin tocar nada existente.

Eventos tipados adicionales:
- `ProjectOpenedEvent` — proyecto abierto o cerrado

**Reusar del Kernel:** `EventBus` como implementación base de `IEditorEventBus`.

---

## Fase 1 — WinForms shell con viewport MonoGame embebido

### Objetivo
Ventana del editor funcional con viewport MonoGame, controles Play/Pause/Stop y layout de paneles.

### Proyecto: MonoGame.Editor.WinForms

**Framework y dependencias**
- `<TargetFramework>net10.0-windows</TargetFramework>`
- `<UseWindowsForms>true</UseWindowsForms>`
- NuGet: `MonoGame.Framework.WindowsDX`
- Referencia a `MonoGame.Editor.Core`

**Layout de `EditorForm`**
- `MenuStrip`: File · Edit · View · Project · Debug
- `ToolStrip`: Play/Pause/Stop + indicador de estado + modos de gizmo (Select/Move/Rotate/Scale)
- `SplitContainer` horizontal: panel izquierdo (`TabControl`: jerarquía + asset browser) + resto
- `SplitContainer` vertical en el resto: viewport (centro) + inspector (derecha)
- `Panel` inferior colapsable: consola

Tamaños mínimos aplicados con `SplitContainer.Panel1MinSize` / `Panel2MinSize`:
- Panel izquierdo: 180 px
- Viewport: 320 × 240 px
- Inspector: 220 px
- Consola: 80 px

Visibilidad de paneles via menú View con checkboxes; último tamaño persistido en `EditorPreferences`.

Debe haber una barra de menús superior y una barra de estado inferior docked para informar al usuario de tareas en segundo plano.

**`MonoGameControl`** — viewport embebido
```csharp
sealed class MonoGameControl : Control
{
    // SwapChainRenderTarget inicializado con Handle del control
    // Game loop en hilo separado; actualizaciones de UI via Control.Invoke
    // Al redimensionar, recrear SwapChainRenderTarget
}
```

**Cámara del editor** — reusar `Camera2D` del Kernel
- Pan: botón central del ratón o Alt + arrastrar
- Zoom: rueda del ratón
- En modo Playing: usar la cámara de la escena en lugar de la del editor

**Shortcuts del editor** — reusar `InputActionMap` + `InputSerializer` del Kernel
- Acciones predefinidas: `editor.play`, `editor.pause`, `editor.stop`, `editor.undo`, `editor.redo`, `editor.save`, `gizmo.select`, `gizmo.move`, `gizmo.rotate`, `gizmo.scale`, `grid.toggle`
- Serializado en `%APPDATA%/MonoGameEditor/shortcuts.json` con `InputSerializer`
- Editable desde menú Edit → Keyboard Shortcuts

**Conversión de coordenadas** — reusar `ResolutionManager` del Kernel
- `ResolutionManager.ScreenToVirtual(screenPos)` para convertir click en viewport a coordenadas mundo
- Necesario para ray picking 2D y colocación de gizmos

**Snapshot de escena en Play/Stop**
- Play: serializar `GameWorld` completo a JSON con `SceneSerializer` → guardar en memoria
- Stop: deserializar el snapshot y reconstruir `GameWorld` → volver a estado `Editing`

---

## Fase 2 — Sistema Undo/Redo

### Objetivo
Historial de operaciones reversibles. Esencial para cualquier flujo de edición profesional.

### Proyecto: MonoGame.Editor.Core

**`IEditorCommand`** — interfaz base
```csharp
interface IEditorCommand
{
    string Description { get; }  // mostrado en menú Edit → Undo "Move Entity"
    void Execute();
    void Undo();
}
```

**`CommandStack`** — historial
```csharp
sealed class CommandStack
{
    int MaxHistory { get; } = 100;
    string? UndoDescription { get; }  // null si pila vacía
    string? RedoDescription { get; }

    void Execute(IEditorCommand command);  // ejecuta y apila en undo; limpia redo
    void Undo();
    void Redo();
    void Clear();
}
```

**Comandos concretos** en `MonoGame.Editor.Core/Commands/`:

| Clase | Operación |
|-------|-----------|
| `CreateEntityCommand` | Crear game object |
| `DeleteEntityCommand` | Eliminar game object (y hijos) |
| `RenameEntityCommand` | Renombrar |
| `ReparentEntityCommand` | Cambiar padre en jerarquía |
| `MoveEntityCommand` | Cambiar posición (TransformBehaviour) |
| `RotateEntityCommand` | Cambiar rotación |
| `ScaleEntityCommand` | Cambiar escala |
| `SetPropertyCommand` | Cambiar propiedad genérica via reflexión |
| `AddBehaviourCommand` | Adjuntar GameBehaviour |
| `RemoveBehaviourCommand` | Eliminar GameBehaviour |
| `PaintTileCommand` | Pintar tile en tilemap |
| `EraseTileCommand` | Borrar tile en tilemap |
| `ApplyPrefabCommand` | Aplicar cambios prefab a instancias |
| `RevertPrefabCommand` | Revertir instancia a definición del prefab |

**Integración con shortcuts:**
- Ctrl+Z → `CommandStack.Undo()` via `InputActionMap`
- Ctrl+Y / Ctrl+Shift+Z → `CommandStack.Redo()`
- El menú Edit → Undo/Redo actualiza su texto con `UndoDescription` / `RedoDescription`

---

## Fase 3 — Jerarquía de escena e Inspector

### Objetivo
Árbol de game objects enlazado directamente al `GameWorld` del Kernel. Inspector de propiedades por reflexión sobre `GameBehaviour`.

### Clases del Kernel reutilizadas
- `GameWorld` — fuente de verdad de todas las entidades
- `GameEntity` — nodo del árbol (nombre, GUID, activo, hijos, padre)
- `GameBehaviour` — componentes inspeccionables
- `TransformBehaviour` — sección de transform siempre visible en inspector
- `GameEntityPool` — pool para creación eficiente de entidades

### Panel: `SceneHierarchyPanel`

**TreeView** mapeado sobre `GameWorld`:
- Cada nodo = `GameEntity`; los hijos aparecen anidados
- Checkbox de activación (activo/inactivo) por nodo
- Icono según behaviours adjuntos (sprite, camera, audio, etc.)
- Estado de expansión persistido en la sesión

**Operaciones (menú contextual + atajos):**

| Operación | Atajo | Comando generado |
|-----------|-------|-----------------|
| Create Empty | — | `CreateEntityCommand` |
| Create Child | — | `CreateEntityCommand` (padre = seleccionado) |
| Duplicate | Ctrl+D | `CreateEntityCommand` (copia profunda) |
| Rename | F2 | `RenameEntityCommand` |
| Delete | Supr | `DeleteEntityCommand` |
| Set Active/Inactive | — | `SetPropertyCommand` |

**Drag & drop:**
- Arrastrar nodo sobre otro → `ReparentEntityCommand`
- Arrastrar fuera de padre → mover a raíz
- Indicadores visuales: resaltado del destino + línea de inserción entre nodos

**Selección:**
- Click simple → selecciona + publica `GameObjectSelectedEvent`
- Ctrl+Click / Shift+Click → selección múltiple
- Click en viewport → ray picking 2D con `Camera2D` + `ResolutionManager`
- Game object seleccionado resaltado en árbol y con bounding box en viewport

### Panel: `InspectorPanel`

**Sección de transform** (siempre visible, enlazada a `TransformBehaviour`):
- Position (X, Y), Rotation, Scale (X, Y)
- Cambios inmediatos en viewport via `MoveEntityCommand` / `RotateEntityCommand` / `ScaleEntityCommand`

**Sección por behaviour** (colapsable, una sección por `GameBehaviour` adjunto):
- Cabecera: nombre del tipo + checkbox de habilitado + botón eliminar (×)
- Propiedades marcadas con `[EditorProperty]` generadas dinámicamente por reflexión
- Controles de edición por tipo:

| Tipo C# | Control WinForms |
|---------|-----------------|
| `float`, `int` | `NumericUpDown` |
| `bool` | `CheckBox` |
| `string` | `TextBox` |
| `Vector2` | Dos `NumericUpDown` (X, Y) en línea |
| `Color` | Botón que abre `ColorDialog` |
| `enum` | `ComboBox` |
| Referencia a asset | `TextBox` readonly + botón Browse + acepta drop desde Asset Browser |

- Todos los cambios van a `CommandStack` via `SetPropertyCommand`

**Botón Add Behaviour:**
- Popup con búsqueda incremental sobre `GameObjectRegistry`
- Opción `Create new...` → lanza CodeGen (Fase 14)
- Selección → `AddBehaviourCommand`

### Modelos de datos en `MonoGame.Editor.Core`

```
EditorScene
├── string Name
├── string ScenePath
├── List<EditorGameObject> RootGameObjects
├── EditorCameraConfig Camera
└── EditorAmbientConfig Lighting

EditorGameObject
├── Guid Id
├── string Name
├── bool Active
├── Vector2 Position
├── float Rotation
├── Vector2 Scale
├── List<EditorBehaviour> Behaviours
└── List<EditorGameObject> Children

EditorBehaviour
├── string TypeName
├── Dictionary<string, JsonElement> Properties
└── bool Enabled
```

**`SceneSerializer`** — `System.Text.Json`
- Tipos soportados: `int`, `float`, `bool`, `string`, `Vector2`, `Vector3`, `Color`, `enum`, rutas relativas a assets
- Instanciación: `Activator.CreateInstance(Type.GetType(typeName))`

**`GameObjectRegistry`**
- Al arrancar, escanea ensamblados cargados por reflexión buscando subclases de `GameBehaviour`
- `IReadOnlyDictionary<string, Type> RegisteredTypes`
- Actualizado automáticamente tras CodeGen al recargar el ensamblado

---

## Fase 4 — Gizmos de transform y grid/snap

### Objetivo
Herramientas visuales de transformación en el viewport: move, rotate, scale. Grid configurable con snap.

### Clases del Kernel reutilizadas
- `DrawHelper` — primitivas 2D en el viewport
- `PrimitiveBatch` — líneas y formas para gizmos
- `Camera2D` — conversión entre espacio mundo y espacio pantalla
- `GeometryUtility` — cálculo de bounding box para selección
- `ResolutionManager` — conversión coordenadas pantalla → mundo

### `GizmoRenderer` en `MonoGame.Editor.Core`

Modo activo seleccionable en toolbar (y por atajo de teclado):

| Modo | Atajo | Descripción |
|------|-------|-------------|
| Select | Q | Solo selección; sin gizmo visible |
| Move | W | Flechas X/Y arrastrables |
| Rotate | E | Arco circular arrastrable |
| Scale | R | Cuadrados en extremos arrastrables |

**Move gizmo:**
- Flecha roja = eje X, flecha verde = eje Y, cuadrado amarillo = XY libre
- Al soltar genera `MoveEntityCommand` con posición inicial y final
- Coordenadas calculadas con `Camera2D.ScreenToWorld()` aplicando `ResolutionManager`

**Rotate gizmo:**
- Arco circular blanco alrededor del pivot del game object
- Al soltar genera `RotateEntityCommand`

**Scale gizmo:**
- Cuadrados en los extremos del bounding box
- Al soltar genera `ScaleEntityCommand`

**Bounding box de selección:**
- Rectángulo de puntos punteados alrededor del game object seleccionado
- Calculado con `GeometryUtility.CalculateBoundingBox()`
- Solo visible en modo `Editing`

**Grid:**
- Overlay de líneas equidistantes sobre el viewport (renderizado con `PrimitiveBatch`)
- Tamaño de celda configurable en Project Settings (Fase 12)
- Color del grid: gris claro semi-transparente
- Toggle con la tecla G

**Snap:**
- Al mantener Ctrl, la posición se redondea al grid al soltar el gizmo
- Tamaño del snap = tamaño de celda del grid
- Configurable en Project Settings

---

## Fase 5 — Asset Browser e integración Content Pipeline

### Objetivo
Browser de assets con drag & drop al viewport y al Inspector. Importación y recompilación automática.

### Clases del Kernel reutilizadas
- `AsyncContentLoader` — carga de assets en background sin bloquear la UI
- `ContentLoadGroup` — recarga de grupos de assets relacionados
- `Sprite` — asignación de textura al arrastrar al viewport

### Panel: `AssetBrowserPanel`

**TreeView** que replica la estructura de carpetas de `Content/`:
- Iconos por tipo: textura, audio, fuente, tilemap, escena, prefab, partículas, animación, input
- Panel de preview al seleccionar: miniatura de textura con `SpriteBatch` en mini-viewport, info de dimensiones y formato

**Drag & drop al Inspector:**
- Arrastrar un asset sobre un campo de referencia en el Inspector lo asigna directamente
- El campo acepta el asset si el tipo es compatible

**Drag & drop al viewport:**
- Textura → crea `GameEntity` con `Sprite` (del Kernel) en la posición del drop
- `.tmx` → crea `GameEntity` con `TiledMapRenderer` adjunto
- `.prefab.json` → instancia el prefab (ver Fase 7)

**Drag & drop desde explorador de Windows:**
- Archivos externos arrastrados a la ventana del editor se copian a `Content/` correspondiente
- Se importan automáticamente

**FileWatcher:**
- `FileSystemWatcher` sobre `Content/`
- Al detectar cambios, usa `ContentLoadGroup` del Kernel para recargar el grupo afectado
- Actualiza Asset Browser y viewport automáticamente
- Publica `AssetImportedEvent` via `IEditorEventBus`

**MGCB:**
- El editor invoca `dotnet mgcb Content/Content.mgcb` antes de entrar en modo Play si hay assets sin compilar
- Output en `ConsolePanel`
- `AsyncContentLoader` del Kernel para cargar assets compilados en background

---

## Fase 6 — Editor de Tilemaps

### Objetivo
Edición visual de tilemaps `.tmx` con palette de tiles, selección de capa y pintura directa en viewport.

### Clases del Kernel reutilizadas
- `TiledMapRenderer` — render del tilemap en el viewport
- `TiledObjectLayer` — acceso y modificación de capas de objetos

### Importación

- `MonoGame.Extended.Tiled` provee el parser de `.tmx`
- Al importar un `.tmx` → crear `EditorTilemapAsset` con rutas de tilesets, capas y propiedades por tile
- Renderizar con `TiledMapRenderer` del Kernel en el viewport
- Capas del tilemap como `GameEntity` hijos en la jerarquía de escena

### Edición en viewport

Al seleccionar un game object con tilemap, el viewport entra en modo tilemap:

- **Palette panel** (panel flotante o tab en el inspector): tiles del tileset activo, organizados por categoría
- **Dropdown** para seleccionar capa activa
- Click izquierdo: pinta tile seleccionado → `PaintTileCommand`
- Click derecho: borra tile → `EraseTileCommand`
- Ctrl+Z / Ctrl+Y: deshacer/rehacer via `CommandStack`
- Los cambios se guardan en el `.tmx` directamente vía `TiledObjectLayer` del Kernel

---

## Fase 7 — Sistema de Prefabs

### Objetivo
Guardar y reutilizar configuraciones de game objects como plantillas reutilizables, al estilo Unity.

### Proyecto: MonoGame.Editor.Core

Un prefab es un `EditorGameObject` serializado a `.prefab.json` en `Content/Prefabs/`.

**`PrefabManager`:**
```csharp
sealed class PrefabManager
{
    void Save(EditorGameObject source, string prefabPath);
    EditorGameObject Instantiate(string prefabPath);
    void ApplyToPrefab(EditorGameObject instance, string prefabPath);  // ApplyPrefabCommand
    void RevertFromPrefab(EditorGameObject instance, string prefabPath);  // RevertPrefabCommand
}
```

**Flujo:**
- Menú contextual en jerarquía: `Save as Prefab...` → abre diálogo de nombre → serializa con `SceneSerializer` → `Content/Prefabs/nombre.prefab.json`
- Drag & drop de `.prefab.json` desde Asset Browser al viewport → `PrefabManager.Instantiate()` → `CreateEntityCommand`
- Prefabs en jerarquía marcados con icono azul (diferenciados de game objects normales)
- Inspector de prefab muestra botones `Apply` y `Revert` en la cabecera
- `Apply` → `ApplyPrefabCommand` (actualiza el `.prefab.json` con el estado actual de la instancia)
- `Revert` → `RevertPrefabCommand` (restaura la instancia al estado del `.prefab.json`)

---

## Fase 8 — Editor de Partículas

### Objetivo
Panel especializado para diseñar efectos de partículas con preview en tiempo real.

### Clases del Kernel reutilizadas
- `ParticleBuilder` — modelo de datos del efecto y serialización
- `ParticleEffectWrapper` — update/draw del efecto en el viewport

### Panel: `ParticleEditorPanel`

Se abre al seleccionar un `GameEntity` con `ParticleEffectWrapper` adjunto, o desde el Asset Browser al hacer doble clic en un `.particles.json`.

**Propiedades editables:**

| Sección | Propiedades |
|---------|-------------|
| Emission | Rate (particles/sec), Burst count |
| Lifetime | Min, Max (segundos) |
| Velocity | Direction, Speed min/max, Spread angle |
| Color | Start color, End color (gradiente) |
| Size | Start size, End size |
| Rotation | Initial rotation, Angular velocity |
| Texture | Referencia a textura (acepta drop desde Asset Browser) |
| Blend | Additive / Alpha blend |

**Preview en vivo:** el viewport renderiza el efecto con `ParticleEffectWrapper.Update()` / `.Draw()` en tiempo real mientras se editan los valores.

**Serialización:**
- `ParticleBuilder.ToJson()` / `ParticleBuilder.FromJson()` ← añadir al Kernel en esta fase
- Guardado en `Content/Particles/nombre.particles.json`

---

## Fase 9 — Timeline de Animación

### Objetivo
Editor de animaciones de sprites con preview en tiempo real.

### Clases del Kernel reutilizadas
- `AnimatedSprite` — componente de animación
- `Animation` — datos de frames
- `TextureAtlas` — sprite sheet de origen
- `TextureRegion` — región individual del atlas

### Panel: `AnimationEditorPanel`

Se abre al seleccionar un `GameEntity` con `AnimatedSprite`, o desde Asset Browser en un `.anim.json`.

**Timeline horizontal:**
- Regla de tiempo con fotogramas numerados
- Cada frame representado como miniatura de su `TextureRegion`
- Barra de playback: Play/Pause/Stop, checkbox Loop, velocidad (fps)

**Operaciones:**
- Añadir frame: drag & drop de `TextureRegion` desde Asset Browser al timeline
- Eliminar frame: tecla Supr sobre el frame seleccionado
- Reordenar: drag & drop entre posiciones del timeline
- Editar duración: doble clic en un frame → `NumericUpDown` de duración en milisegundos

**Preview en vivo:** el viewport muestra el `AnimatedSprite` reproduciéndose con los cambios en tiempo real.

**Serialización:**
- Guardar en `Content/Animations/nombre.anim.json`
- `AnimatedSprite` en el Kernel carga este formato ← añadir `AnimatedSprite.LoadFromJson()` en esta fase

---

## Fase 10 — Editor de Input Mapping

### Objetivo
Panel visual para configurar y exportar los bindings de input del juego.

### Clases del Kernel reutilizadas
- `InputActionMap` — mapa de acciones
- `InputAction` — acción individual
- `InputBinding` — binding de tecla/botón
- `InputSerializer` — serialización/deserialización a JSON

### Panel: `InputMappingPanel`

**DataGridView** con columnas:

| Columna | Tipo | Descripción |
|---------|------|-------------|
| Action | `TextBox` | Nombre de la acción |
| Device | `ComboBox` | Keyboard, Mouse, GamePad |
| Primary | `TextBox` readonly | Binding primario |
| Secondary | `TextBox` readonly | Binding alternativo |

**Captura de binding:** al hacer doble clic en Primary o Secondary:
- Diálogo modal "Press any key..." que captura la siguiente tecla/botón pulsado
- Actualiza el binding en el `InputActionMap`

**Operaciones:**
- Botón `+` → añadir acción nueva
- Botón `−` → eliminar acción seleccionada
- Drag & drop para reordenar

**Exportación:**
- Guardar con `InputSerializer.Serialize()` del Kernel → `Content/input.json`
- El juego carga con `InputSerializer.Deserialize()` sin depender del editor

---

## Fase 11 — Panel de Audio

### Objetivo
Gestión de assets de audio, configuración de pools y preview de reproducción.

### Clases del Kernel reutilizadas
- `AudioController` — backend de reproducción
- `SoundEffectPool` — pooling por efecto
- `AudioEmitter3D` — fuente de audio 3D
- `AudioListener3D` — receptor de audio 3D

### Panel: `AudioPanel`

**Lista de assets de audio** (TreeView por categoría: SoundEffects, Music):
- Iconos por tipo: onda de sonido, nota musical
- Click → selecciona y muestra propiedades en panel derecho
- Doble clic → Preview: reproduce con `AudioController`
- Botón Stop → para reproducción actual

**Propiedades de SoundEffect:**
- Pool size: `NumericUpDown` → configura `SoundEffectPool` del Kernel
- Volume, Pitch, Pan: `Slider` del Kernel (o `TrackBar` nativo)
- Loop: `CheckBox`

**Propiedades de AudioEmitter3D** (si el game object tiene 3D audio):
- Position: igual que la del `TransformBehaviour`
- DopplerScale, RolloffFactor, MaxDistance: `NumericUpDown`

**Propiedades de AudioListener3D:**
- Campos de posición y forward vector

---

## Fase 12 — Panel de Traducciones y Project Settings

### Objetivo
Editor de localización integrado con `LocalizationManager`. Configuración de parámetros globales del proyecto.

### Clases del Kernel reutilizadas
- `LocalizationManager` — backend de localización
- `ResolutionManager` — resolución virtual del juego
- `PlatformManager` — modo ventana/fullscreen
- `PostProcessEffect` — efectos de post-proceso
- `TweeningManager` — valores por defecto de animación

### Panel de traducciones

- `DataGridView`: columna `Key` + una columna por idioma del proyecto
- Datos desde `Localization/{lang}.json` cargados con `LocalizationManager` del Kernel
- Edición inline con `SetPropertyCommand` para soporte de Undo/Redo
- Botones: añadir clave, eliminar clave, añadir idioma
- Formato JSON plano `{ "key": "valor" }` con soporte de namespaces por punto
- Guardar con Ctrl+S

### Project Settings (`Editor/project.json`)

| Sección | Propiedades | Kernel |
|---------|------------|--------|
| Paths | ContentPath, LocalizationPath | — |
| Resolution | VirtualWidth, VirtualHeight, Letterbox mode | `ResolutionManager` |
| Window | Title, FullScreen, WindowMode | `PlatformManager` |
| Localization | DefaultLocale, AvailableLocales | `LocalizationManager` |
| Camera | DefaultZoom, WorldBounds, FollowMode | `Camera2D` |
| PostProcess | EnabledEffects, EffectParams | `PostProcessEffect` |
| Tweening | DefaultDuration, DefaultEasing | `TweeningManager` |
| Grid | CellSize, SnapEnabled | `GizmoRenderer` (Fase 4) |
| Build | OutputPath, DefaultPlatform | `PlatformManager` |

**Sección Paths** — editable en Project Settings (rutas relativas al `RootPath`):
- `ContentPath`: selector de carpeta; apunta a la carpeta de assets del juego (default `Content`)
- `LocalizationPath`: selector de carpeta; apunta a los ficheros JSON de localización (default `Localization`)
- Al cambiar cualquier ruta, el editor recarga el `AssetBrowserPanel` y el panel de traducciones

---

## Fase 13 — Build y Export

### Objetivo
Pipeline de compilación y exportación del juego para plataforma objetivo.

### Clases del Kernel reutilizadas
- `PlatformManager` — detección de plataformas objetivo

### Configuraciones de build

Guardadas en `project.json` (sección Build):

| Campo | Descripción |
|-------|-------------|
| `OutputPath` | Carpeta de salida del ejecutable |
| `Platform` | Windows, Linux (vía `PlatformType` del Kernel) |
| `Configuration` | Debug / Release |
| `BundleAssets` | Incluir Content/ en el output |

**Pipeline en `BuildManager`:**
1. Compilar assets: `dotnet mgcb Content/Content.mgcb` (si hay cambios)
2. Publicar: `dotnet publish -c {config} -r {rid} -o {outputPath}`
3. Copiar `Content/` compilado a la carpeta de output
4. Generar `game.manifest.json` con versión, plataforma, lista de assets
5. Output en `ConsolePanel` en tiempo real via stream del proceso

**`game.manifest.json`:**
```json
{
  "name": "MyGame",
  "version": "1.0.0",
  "platform": "Windows",
  "assets": ["Content/Textures/player.xnb", "..."]
}
```

---

## Fase 14 — Generación de código y [EditorProperty]

### Atributo `[EditorProperty]`

Definido en `MonoGame.Editor.Core`:
```csharp
[AttributeUsage(AttributeTargets.Property)]
sealed class EditorPropertyAttribute : Attribute
{
    string? Label { get; init; }
    float Min { get; init; } = float.MinValue;
    float Max { get; init; } = float.MaxValue;
    string? Tooltip { get; init; }
}
```

El Inspector solo muestra propiedades marcadas con este atributo.

### CodeGen

Al pulsar `Create new...` en el popup de `Add Behaviour`:
1. El editor solicita nombre de clase
2. Genera `.cs` en `src/Behaviours/` con estructura mínima del Kernel:
   ```csharp
   namespace MyGame.Behaviours;

   public sealed class NombreClass : GameBehaviour
   {
       [EditorProperty(Label = "Speed", Min = 0f, Max = 100f)]
       public float Speed { get; set; } = 5f;

       public override void Update(GameTime gameTime) { }
   }
   ```
3. Abre el archivo en Visual Studio via `Process.Start`
4. Al recompilar, `GameObjectRegistry` detecta el nuevo tipo automáticamente

---

## Integración con Visual Studio

- El proyecto del juego no referencia el editor
- Mismo proceso: breakpoints en cualquier `GameBehaviour` sin configuración
- Flujo: detener depuración → cambiar código → relanzar
- `ConsolePanel` captura `Debug.WriteLine` y stdout

**Estructura de carpetas del proyecto de juego:**
```
MyGame/
├── MyGame.sln
├── src/
│   ├── MyGame.csproj             # Referencia a Alca.MonoGame.Kernel
│   ├── Game.cs
│   └── Behaviours/               # Generado por CodeGen, editado por el usuario
├── Content/                      # Ruta configurable en Editor/project.json
│   ├── Content.mgcb
│   ├── Textures/
│   ├── Audio/
│   ├── Fonts/
│   ├── Maps/
│   ├── Particles/                # .particles.json
│   └── Animations/               # .anim.json
├── Localization/                 # Ruta configurable en Editor/project.json
│   ├── es.json
│   └── en.json
└── Editor/                       # Ficheros del editor (ignorable en .gitignore si se desea)
    ├── project.json              # Descriptor + Project Settings
    ├── Scenes/                   # .json versionables con git
    └── Prefabs/                  # .prefab.json
```

---

## Resumen de clases del Kernel reutilizadas

| Fase | Clases del Kernel |
|------|-------------------|
| 0 | `EventBus` |
| 1 | `Camera2D`, `InputActionMap`, `InputSerializer`, `ResolutionManager` |
| 2 | — |
| 3 | `GameWorld`, `GameEntity`, `GameBehaviour`, `TransformBehaviour`, `GameEntityPool` |
| 4 | `Camera2D`, `DrawHelper`, `PrimitiveBatch`, `GeometryUtility`, `ResolutionManager` |
| 5 | `AsyncContentLoader`, `ContentLoadGroup`, `Sprite` |
| 6 | `TiledMapRenderer`, `TiledObjectLayer` |
| 7 | `SceneSerializer` (Editor.Core) |
| 8 | `ParticleBuilder`, `ParticleEffectWrapper` |
| 9 | `AnimatedSprite`, `Animation`, `TextureAtlas`, `TextureRegion` |
| 10 | `InputActionMap`, `InputAction`, `InputBinding`, `InputSerializer` |
| 11 | `AudioController`, `SoundEffectPool`, `AudioEmitter3D`, `AudioListener3D` |
| 12 | `LocalizationManager`, `ResolutionManager`, `PlatformManager`, `PostProcessEffect`, `TweeningManager` |
| 13 | `PlatformManager` |
| 14 | `GameBehaviour` (base de clases generadas) |

---

## Convenciones técnicas

- Comunicación entre paneles exclusivamente via `IEditorEventBus` — los paneles nunca se llaman directamente entre sí
- Todas las operaciones de edición van a `CommandStack` para soporte de Undo/Redo
- `EditorContext` (singleton) como fuente de verdad: escena activa, selección, estado, proyecto
- Los `.json` de escena son la fuente de verdad para edición (human-readable, versionables con git)
- El Kernel gestiona la ejecución del juego; el editor construye sobre él sin duplicar código
- Todos los textos de la UI del editor en inglés
- Las adiciones al Kernel necesarias (e.g. `ParticleBuilder.ToJson()`, `AnimatedSprite.LoadFromJson()`) se implementan en el Kernel antes de la fase correspondiente
