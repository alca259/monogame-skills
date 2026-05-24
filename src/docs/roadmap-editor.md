# MonoGame Editor — Roadmap técnico

**Stack**: .NET 10 · C# 14 · WinForms · MonoGame · MonoGame.Extended · Alca.MonoGame.Kernel  
**Objetivo**: Editor de juegos 2D estilo Unity, integrado con Visual Studio, con viewport de MonoGame embebido, gestor de escenas, inspector de game objects, editor de tilemaps y generación de código.

---

## Estilo visual e interfaz

El editor usa el modo oscuro nativo de WinForms introducido en .NET 10. No hay modo claro ni selector de tema.

**Activación del modo oscuro**
- En el punto de entrada (`Program.cs`): `Application.SetColorMode(SystemColorMode.Dark)` antes de lanzar el formulario
- Todos los controles estándar de WinForms heredan automáticamente los colores oscuros del sistema sin configuración adicional
- Los controles personalizados (viewport, inspector dinámico, palette de tilemap) usan las variables de color del sistema (`SystemColors`) para mantenerse coherentes con el tema

**Criterios de diseño**
- Sin bordes decorativos, sin gradientes, sin sombras artificiales — superficies planas
- Tipografía: `Segoe UI` 9pt como fuente base en todos los controles, consistente con el estilo de Visual Studio
- Iconos en la barra de herramientas y el árbol de jerarquía: Segoe Fluent Icons o iconos propios en formato PNG con fondo transparente, tamaño 16×16 y 24×24
- Separadores y splitters visualmente discretos, un tono más claro que el fondo
- Los botones Play / Pause / Stop usan iconos reconocibles (triángulo, doble barra, cuadrado) con color de acento al estar activos

---

## Estructura de la solución

```
MonoGame.Editor.sln
├── Alca.MonoGame.Kernel          # Librería existente (referencia, no modificar)
├── MonoGame.Editor.Core          # Lógica del editor, sin UI
├── MonoGame.Editor.WinForms      # Host WinForms, la aplicación del editor
└── MonoGame.Editor.Templates     # Plantillas de proyecto para juegos nuevos
```

`Core` no referencia nada de `System.Windows.Forms`. `WinForms` referencia `Core`. Ninguno se referencia desde el proyecto del juego final.

---

## Fase 1 — WinForms host con viewport MonoGame embebido

### Objetivo
Ventana de editor funcional con viewport de MonoGame, controles Play / Pause / Stop, layout de paneles con `SplitContainer` y soporte para ocultar/mostrar cada panel.

### Proyecto: MonoGame.Editor.WinForms

**Framework y dependencias**
- `<TargetFramework>net10.0-windows</TargetFramework>`
- `<UseWindowsForms>true</UseWindowsForms>`
- NuGet: `MonoGame.Framework.WindowsDX`
- Referencia a `MonoGame.Editor.Core`

**Layout de la ventana principal (`EditorForm`)**
- `MenuStrip`: File · Edit · View · Project · Debug
- `ToolStrip` debajo del menú: botones Play / Pause / Stop + indicador del estado actual
- `SplitContainer` horizontal principal: panel izquierdo (jerarquía + asset browser en `TabControl`) + resto
- Segundo `SplitContainer` vertical en el resto: viewport (centro) + inspector (derecha)
- `Panel` inferior colapsable anclado al fondo: consola de mensajes

**Tamaños mínimos**
- Panel izquierdo: mínimo 180 px de ancho
- Viewport: mínimo 320 × 240 px
- Inspector: mínimo 220 px de ancho
- Consola: mínimo 80 px de alto cuando está visible
- Aplicados con `Panel.MinimumSize` y `SplitContainer.Panel1MinSize` / `Panel2MinSize`

**Mostrar / ocultar paneles**
- Menú `View` con checkbox por panel: `Scene Hierarchy`, `Inspector`, `Asset Browser`, `Console`
- Ocultar: `SplitContainer` colapsa ese lado (`IsSplitterFixed = true`, `SplitterDistance` al mínimo), `Visible = false`
- Mostrar: restaura la distancia del splitter al último valor guardado, `IsSplitterFixed = false`, `Visible = true`
- El último tamaño visible y el estado de visibilidad de cada panel se persisten en un JSON de preferencias del editor entre sesiones

**Viewport MonoGame embebido**
- Clase `MonoGameControl` que hereda de `Control`
- `SwapChainRenderTarget` inicializado con el `Handle` del control
- Game loop en hilo separado; actualizaciones de UI vía `Control.Invoke`
- Al redimensionar el panel, recrear el `SwapChainRenderTarget`
- Input de teclado y ratón capturado con eventos nativos de WinForms y traducido al sistema de input de MonoGame.Extended

**Estados del editor**
- `EditorState` enum: `Editing`, `Playing`, `Paused`
- `Play`: serializa el estado actual de la escena a memoria (snapshot JSON), activa el game loop real
- `Pause`: game loop activo para mantener el render, `Update` no ejecuta lógica de juego
- `Stop`: restaura el snapshot, vuelve a `Editing`
- Debugging nativo con Visual Studio: mismo proceso, los breakpoints funcionan en cualquier game object sin configuración. Flujo estándar MonoGame + VS: detener depuración → cambiar código → relanzar.

**Cámara del editor**
- En modo `Editing`: cámara independiente de la cámara del juego
- Pan: botón central del ratón o Alt + arrastrar
- Zoom: rueda del ratón
- En modo `Playing`: cámara de la escena

---

## Fase 2 — Jerarquía de escena e Inspector

### Objetivo
Árbol de game objects similar al Hierarchy de Unity: creación, anidamiento, selección, renombrado y eliminación de game objects. Inspector con las propiedades del game object seleccionado y sus behaviours adjuntos.

### Jerarquía de escena (`SceneHierarchyPanel`)

El panel muestra un `TreeView` con todos los game objects de la escena activa. Refleja fielmente la estructura de la escena tal como la gestiona `Alca.MonoGame.Kernel`.

**Árbol de game objects**
- Cada nodo del árbol representa un game object
- Los game objects hijo aparecen anidados bajo su padre, con indentación visual
- El anidamiento puede ser arbitrariamente profundo
- Los nodos tienen un checkbox de activación (activo/inactivo) y un icono que indica si tiene behaviours adjuntos
- Los nodos se pueden expandir y colapsar; el estado de expansión se persiste en la sesión

**Operaciones sobre game objects (clic derecho → menú contextual y atajos de teclado)**
- `Create Empty` — crea un game object vacío en la raíz o como hijo del seleccionado
- `Create Child` — crea un game object vacío como hijo del seleccionado
- `Duplicate` — duplica el game object y todos sus hijos y behaviours (Ctrl+D)
- `Rename` — edición inline del nombre directamente en el nodo del árbol (F2)
- `Delete` — elimina el game object y todos sus hijos (Supr)
- `Set Active` / `Set Inactive` — activa o desactiva el game object en la escena

**Anidamiento por drag & drop**
- Arrastrar un nodo del árbol sobre otro lo convierte en hijo
- Arrastrar un nodo fuera de su padre actual lo mueve a la raíz
- Arrastrar entre nodos del mismo nivel reordena
- Indicadores visuales durante el arrastre: resaltado del nodo destino, línea de inserción entre nodos

**Selección**
- Click simple: selecciona el game object y actualiza el Inspector
- Click en el viewport: selecciona el game object bajo el cursor (ray picking 2D)
- El game object seleccionado se resalta en el árbol y en el viewport con un bounding box o gizmo de selección
- Selección múltiple con Ctrl+Click o Shift+Click (el Inspector muestra propiedades comunes)

### Inspector (`InspectorPanel`)

Muestra y edita las propiedades del game object seleccionado y los behaviours que tiene adjuntos. Diseño similar al Inspector de Unity: secciones colapsables por behaviour, separadas con cabeceras.

**Sección de transform**
- Siempre visible en la parte superior del Inspector
- Campos editables: Position (X, Y), Rotation, Scale (X, Y)
- Los cambios se reflejan inmediatamente en el viewport

**Sección por behaviour adjunto**
- Una sección colapsable por cada behaviour adjunto al game object
- Cabecera de sección: nombre del tipo, checkbox de habilitado, botón de eliminar (×)
- Cuerpo: propiedades marcadas con `[EditorProperty]`, generadas dinámicamente por reflexión
- Controles de edición según el tipo de la propiedad:
  - `float`, `int` → `NumericUpDown`
  - `bool` → `CheckBox`
  - `string` → `TextBox`
  - `Vector2` → dos `NumericUpDown` en línea (X, Y)
  - `Color` → botón que abre `ColorDialog`
  - `enum` → `ComboBox`
  - referencia a asset → `TextBox` de solo lectura + botón browse; también acepta drop desde el Asset Browser

**Añadir behaviour**
- Botón `Add Behaviour` al pie del Inspector
- Abre un popup con búsqueda incremental sobre la lista de tipos registrados en `GameObjectRegistry`
- Si el tipo no existe, opción `Create new...` que lanza el CodeGen (ver Fase 4)
- El behaviour añadido aparece inmediatamente como nueva sección en el Inspector

### Modelo de datos del editor (`MonoGame.Editor.Core`)

`EditorScene`:
```
EditorScene
├── string Name
├── string ScenePath
├── List<EditorGameObject> RootGameObjects
├── EditorCameraConfig Camera
└── EditorLightConfig Lighting
```

`EditorGameObject`:
```
EditorGameObject
├── Guid Id
├── string Name
├── bool Active
├── Vector2 Position
├── float Rotation
├── Vector2 Scale
├── List<EditorBehaviour> Behaviours
└── List<EditorGameObject> Children
```

`EditorBehaviour`:
```
EditorBehaviour
├── string TypeName
├── Dictionary<string, JsonElement> Properties
└── bool Enabled
```

**SceneSerializer**
- Serializa y deserializa `EditorScene` con `System.Text.Json`
- Tipos soportados en propiedades: `int`, `float`, `bool`, `string`, `Vector2`, `Vector3`, `Color`, `enum`, referencias a assets como rutas relativas al proyecto
- Deserializar instancias: `Activator.CreateInstance(Type.GetType(typeName))`

**GameObjectRegistry**
- Al arrancar, escanea los ensamblados cargados por reflexión para encontrar todos los tipos adjuntables a un game object según lo que define `Alca.MonoGame.Kernel`
- Diccionario `string TypeName → Type`, usado por el Inspector y el popup de `Add Behaviour`

---

## Fase 3 — Asset Browser, drag & drop y tilemap

### Objetivo
Browser de assets con drag & drop al viewport y al Inspector. Importación y edición de tilemaps `.tmx`.

### Asset Browser (`AssetBrowserPanel`)

- `TreeView` que replica la estructura de carpetas de `Content`
- Iconos por tipo de archivo: textura, audio, fuente, tilemap, escena
- Panel de preview al seleccionar: miniatura para texturas, info de dimensiones y formato
- **Drag & drop al Inspector**: arrastrar un asset sobre un campo de referencia a asset en el Inspector lo asigna directamente
- **Drag & drop al viewport**: arrastrar una textura al viewport crea un nuevo game object en la posición de drop con la textura asignada; arrastrar un `.tmx` crea un game object con el tilemap adjunto
- **Drag & drop desde el explorador de Windows**: arrastrar archivos externos a la ventana del editor los copia a la carpeta `Content` correspondiente y los importa automáticamente

**FileWatcher**
- `FileSystemWatcher` sobre la carpeta `Content`
- Al detectar cambios, recarga el asset y actualiza el Asset Browser y el viewport automáticamente

**MGCB**
- El editor invoca `dotnet mgcb` antes de entrar en modo Play si hay assets sin compilar
- La salida se muestra en `ConsolePanel`

### Tilemap

**Importación**
- `MonoGame.Extended.Tiled` provee el parser de `.tmx`
- Al importar un `.tmx`, crear `EditorTilemapAsset` con rutas de tilesets, lista de capas y propiedades por tile
- Renderizar con `TiledMapRenderer` en el viewport
- Las capas del tilemap aparecen en `SceneHierarchyPanel` como hijos del game object que lo contiene

**Edición**
- Al seleccionar un game object con tilemap en el Inspector, el viewport entra en modo tilemap
- Palette panel (panel flotante o integrado en el Inspector): muestra los tiles del tileset activo
- Click izquierdo en viewport: pintar tile seleccionado
- Click derecho: borrar tile
- Dropdown para seleccionar la capa activa
- Las ediciones se guardan en el `.tmx` directamente

---

## Fase 4 — Generación de código y paneles restantes

### Atributo `[EditorProperty]`
- Definido en `MonoGame.Editor.Core`
- `[AttributeUsage(AttributeTargets.Property)]`
- Parámetros opcionales: `Min`, `Max`, `Label`
- El Inspector solo muestra propiedades marcadas con este atributo
- Los behaviours generados por CodeGen incluyen ejemplos comentados de su uso

### CodeGen

Al pulsar `Create new...` en el popup de `Add Behaviour`:
1. El editor pide el nombre de la clase
2. Genera el `.cs` en `src/GameObjects/` del proyecto del juego con la estructura mínima esperada por `Alca.MonoGame.Kernel` (clase base correcta, métodos de ciclo de vida)
3. Las propiedades de ejemplo se marcan con `[EditorProperty]`
4. El archivo se abre en Visual Studio vía `Process.Start`
5. Al relanzar la depuración, `GameObjectRegistry` detecta el nuevo tipo automáticamente

### Panel de traducciones

- `DataGridView`: columna `Key` + una columna por idioma definido en el proyecto
- Datos desde `Localization/es.json`, `Localization/en.json`, etc.
- Edición inline
- Botones: añadir clave, eliminar clave, añadir idioma
- Formato JSON plano `{ "key": "valor" }` con soporte de namespaces por punto
- Guarda con Ctrl+S

### Configuración de cámara

- Sección en el Inspector al seleccionar el game object de cámara
- Propiedades: zoom, posición inicial, límites del mundo, modo follow
- Los límites se dibujan como rectángulo overlay en el viewport en modo Editing

### Configuración de luz

- Si `Alca.MonoGame.Kernel` expone un sistema de iluminación, el Inspector lo muestra por reflexión sobre el game object correspondiente
- Si no, configuración básica: color de luz ambiental + lista de fuentes de luz puntuales
- Las fuentes de luz se visualizan como overlays circulares con radio editable, solo en modo Editing

---

## Integración con Visual Studio

- El proyecto del juego no referencia el editor
- Mismo proceso: breakpoints en cualquier game object sin configuración adicional
- Flujo estándar: detener depuración → cambiar código → relanzar
- `ConsolePanel` captura `Debug.WriteLine` y el output estándar

**Estructura de carpetas del proyecto de juego**
```
MyGame/
├── MyGame.sln
├── src/
│   ├── MyGame.csproj
│   ├── Game.cs
│   ├── GameObjects/           # Generado por CodeGen, editado por el usuario
│   └── Scenes/                # .json versionable con git
├── Content/
│   ├── Content.mgcb
│   ├── Textures/
│   ├── Audio/
│   ├── Fonts/
│   └── Maps/
└── Localization/
    ├── es.json
    └── en.json
```

---

## Convenciones técnicas

- Comunicación entre paneles mediante bus de eventos central (`IEditorEventBus`) con eventos tipados: `GameObjectSelectedEvent`, `SceneLoadedEvent`, `AssetImportedEvent`, `BehaviourAddedEvent`. Los paneles no se llaman directamente entre sí.
- Estado global en `EditorContext` (singleton): escena activa, game object seleccionado, estado Play/Pause/Stop, proyecto activo.
- Los `.json` de escena del editor son la fuente de verdad para la edición. El sistema de escenas de `Alca.MonoGame.Kernel` gestiona la ejecución; el editor construye sobre él.
- Sin sistema de Undo/Redo.
- El editor no genera código salvo cuando el usuario lo solicita explícitamente.
- Todos los textos de la UI del editor en inglés.
