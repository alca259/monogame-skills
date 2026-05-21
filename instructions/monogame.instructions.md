---
name: MonoGame Architecture Guide
description: Strict rules for video game development using the official MonoGame API.
applyTo: "**/*.cs"
---

# MonoGame Expert Developer Skill (SDD Standard)

## 1. Truth Sources and API Reference
- The absolute reference for classes, methods, and signatures in this project is the **[MonoGame API Documentation](https://docs.monogame.net/api/index.html)**.
- Mandatory primary namespaces to import based on context:
  - `Microsoft.Xna.Framework` Core framework classes including Game, GameTime, Vector2, Matrix, and fundamental types
  - `Microsoft.Xna.Framework.Audio` Audio playback, sound effects, and music management
  - `Microsoft.Xna.Framework.Content` Content loading and management with the ContentManager
  - `Microsoft.Xna.Framework.Graphics` Rendering, textures, shaders, and all graphics-related functionality
  - `Microsoft.Xna.Framework.Input` Keyboard, mouse, gamepad, and touch input handling
  - `Microsoft.Xna.Framework.Media` Media playback for songs and videos

## 2. Performance and Memory Restrictions (Game Loop)
- **FORBIDDEN (Heap Allocations):** Performing class instantiations (e.g., `new Object()`, `new List<T>()`, array creations, string concatenations, or boxing value types) inside `Update(GameTime)` or `Draw(GameTime)` methods. This generates garbage on the heap that triggers the Garbage Collector (GC) and causes stuttering.
- **ALLOWED (Stack Allocations):** Using `new` for value types (`structs`) like `new Vector2()`, `Rectangle`, or `Color` is perfectly safe, as they allocate on the stack and do not produce GC garbage.
- **SOLUTION:** For reference types (classes) or complex data structures, declare them as private class fields during `Initialize` or `LoadContent` and reuse/clear them each frame.

## 3. Strict Lifecycle Pattern
Any class acting as an entity or screen manager must implement the clean interface or structure based on XNA:
1. `Initialize()` -> Logical variable setup without asset dependencies.
2. `LoadContent()` -> Loading of textures, fonts, and effects via `Content.Load<T>()`.
3. `Update(GameTime)` -> Logical calculations and physics dependent on delta time (`gameTime.ElapsedGameTime.TotalSeconds`).
4. `Draw(GameTime)` -> Rendering calls only (`SpriteBatch.Draw`). Do not process logic here.

## 4. Spec-Driven Development (SDD) Rules
- Before writing code for a task, you must read the corresponding specification file within the project at `docs/specs/`.
- Do not start coding directly ("vibe coding"). Design the component's public signatures first, validate they comply with the MonoGame API signatures, and wait for confirmation.
