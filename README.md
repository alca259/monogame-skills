# monogame-skills

An elite ecosystem of 30 highly granular, context-optimized development blueprints designed to turn Large Language Models (LLMs) into senior .NET game engine architects. Perfect for integration with Cursor (`.cursorrules`), Cline, Roo Code, Copilot, or Claude Projects.

---

## 💡 The Philosophy: Why this exists

Modern LLMs are incredible at writing C#, but they default to generic, memory-heavy Object-Oriented Programming (OOP) when asked to build game systems. They mix UI with game worlds, pollute the heap with LINQ inside the game loop, and hallucinate legacy, long-deprecated APIs.

**monogame-skills** solves this. It is not a compiled library (`.dll`). It is a **context-injection matrix** built from years of .NET experience. It enforces strict architectural constraints—such as Zero-Allocation game loops, clean Data-Oriented Design (DOD), and explicit rendering orders—directly into your AI's reasoning engine. 

By feeding these precise markdown guides to your AI agent, it will write code *exactly* like a senior software architect.

---

## ⚡ Key Features & Constraints Enforced

*   **Zero-Allocation Focus:** Strictly bans LINQ, runtime collection instantiations, and boxing inside `Update` and `Draw` cycles.
*   **XAML-Style UI Engine:** Guides the AI to build a custom Visual Tree with a strict two-pass `Measure` & `Arrange` layout system (supporting Pixel/Auto/Star sizing) without any third-party UI dependencies.
*   **Decoupled Architecture:** Enforces a clean separation between Game World coordinates (cameras, physics, ECS) and UI coordinates (absolute screen space, scissor clipping).
*   **Up-to-Date API Alignment:** Fully mapped to MonoGame and MonoGame.Extended v6.0.0 specifications, preventing the AI from fetching dead 2018 documentation.

---

## 📂 Matrix Directory (30 Skills)

Every skill folder contains a narrative instructional markdown (`SKILL.md`) under 500 lines to prevent AI context degradation, accompanied by zero-boilerplate, copy-pasteable C# reference templates.

| System | Included Skills |
| :--- | :--- |
| **Core Engine** | `game-loop`, `scenes`, `async`, `input`, `platform`, `localization` |
| **Graphics & Math** | `2d` (Matrix Cameras/RenderTargets), `3d`, `math` (Bounding volumes), `shaders`, `effects`, `camera-modes` |
| **Audio** | `audio` (SFX, MediaPlayer) |
| **Game Architecture** | `ecs` (Actor-Behavior/Domain Interfaces/Pooling), `content-pipeline` |
| **Custom UI Ecosystem** | `ui-core`, `ui-layout`, `ui-interaction`, `ui-focus` (Gamepad/D-Pad routing), `ui-controls`, `ui-slider`, `ui-textbox`, `ui-dropdown`, `ui-colorpicker`, `ui-radiobutton`, `ui-grid` |
| **Framework Extensions** | `extended-tweening`, `extended-bitmap-fonts`, `extended-tiled` (v6 API), `extended-particles` |

---

## 🚀 How to Use

### For Cursor (`.cursorrules`) or Roo Code / Cline
Simply copy the contents of the specific `SKILL.md` files you need into your project's rule definitions, or instruct your AI agent to read the local `skills/` directory before writing code.

### For Claude Projects / Custom GPTs
Upload the `.md` files of the systems you are currently building as project knowledge documents.

**Example Prompt:**
> "Using the context provided in `monogame-ui-layout` and `monogame-ui-controls`, build a custom pause menu panel with three centered buttons that scales cleanly across multiple virtual resolutions. Remember the zero-allocation rule."

---

## 💝 100% Free & Open Source

This project is shared with the community out of a pure love for game development and software engineering. It is **100% free**, with no paywalls, no donation links, and no monetization. 

### 🤝 Standing on the Shoulders of Giants
This matrix does not reinvent MonoGame. It is a curation and optimization layer built entirely upon the incredible APIs, documentation, and source code provided by the official **MonoGame** and **MonoGame.Extended** teams. Without their years of open-source dedication, this framework blueprint would not be possible. If you find this project useful, please support the upstream repositories!

---

## 📄 License

MIT License - Copyright (c) 2026. See `LICENSE` for details.