---
name: MonoGame Architecture Guide
description: Reglas estrictas para el desarrollo del videojuego usando la API oficial de MonoGame.
applyTo: "**/*.cs"
---

# MonoGame Expert Developer Skill (SDD Standard)

## 1. Fuentes de Verdad y Referencia API
- La referencia absoluta para clases, métodos y firmas de este proyecto es la **[Documentación de la API de MonoGame](https://docs.monogame.net/api/index.html)**.
- Espacios de nombres primarios obligatorios para importar según contexto:
  - `Microsoft.Xna.Framework` Core framework classes including Game, GameTime, Vector2, Matrix, and fundamental types
  - `Microsoft.Xna.Framework.Audio` Audio playback, sound effects, and music management
  - `Microsoft.Xna.Framework.Content` Content loading and management with the ContentManager
  - `Microsoft.Xna.Framework.Graphics` Rendering, textures, shaders, and all graphics-related functionality
  - `Microsoft.Xna.Framework.Input` Keyboard, mouse, gamepad, and touch input handling
  - `Microsoft.Xna.Framework.Media` Media playback for songs and videos

## 2. Restricciones de Rendimiento y Memoria (Game Loop)
- **PROHIBIDO:** Realizar instanciaciones (`new Object()`, `new Vector2()`, etc.) dentro de los métodos `Update(GameTime)` o `Draw(GameTime)`. Genera basura que activa el Garbage Collector (GC) y provoca stuttering.
- **SOLUCIÓN:** Declara las variables de control, vectores de posición intermedios o estructuras de datos como campos privados de la clase y reutilízalos.

## 3. Patrón de Ciclo de Vida Estricto
Cualquier clase que actúe como entidad o manejador de pantallas debe implementar la interfaz o estructura limpia basada en XNA:
1. `Initialize()` -> Configuración de variables lógicas sin dependencias de assets.
2. `LoadContent()` -> Carga de texturas, fuentes y efectos mediante `Content.Load<T>()`.
3. `Update(GameTime)` -> Cálculos lógicos y físicas dependientes del tiempo delta (`gameTime.ElapsedGameTime.TotalSeconds`).
4. `Draw(GameTime)` -> Únicamente llamadas de renderizado (`SpriteBatch.Draw`). No procesar lógica aquí.

## 4. Reglas de Spec-Driven Development (SDD)
- Antes de escribir código para una tarea, debes leer el archivo de especificación correspondiente dentro del proyecto en `docs/specs/`.
- No comiences a picar código de forma directa ("vibe coding"). Diseña primero las firmas públicas del componente, valida que cumplen con las firmas de la API de MonoGame y espera confirmación.
