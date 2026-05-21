---
name: CSharp Expert Developer
description: Strict rules for any C# code, including .NET, Monogame and more. This is the most strict set of rules, and should be used for any code that is meant to be production-ready.
applyTo: "**/*.cs"
---

# CSharp Expert Developer

## 1. General Guidelines & Project Structure
- **Follow Microsoft Conventions:** Adhere strictly to official C# coding conventions.
- **Naming:** Use meaningful, descriptive names. PascalCase for classes, methods, and properties; camelCase for local variables and method arguments; `_camelCase` for private fields.
- **No Magic Numbers:** Absolutely no magic numbers or string literals in the logic. Extract them to `const` or `enum` within `readonly struct` containers, placed in dedicated files (e.g., `GameConstants.cs`).
- **Reusability:** If a component (utils, abstract bases, helpers) can be reused, it MUST be extracted into a `SharedProject`.
- **Documentation:** Ensure all public members are documented with XML comments. Use single-line `<summary>` tags if the description fits on one line; otherwise, use multi-line.
- **Regions:** Use `#region` to organize large classes logically, but avoid overusing them to hide bloated class responsibilities (if a file has too many regions, refactor it into smaller classes).

## 2. Architecture & Class Design
- **Sealed by Default:** Classes MUST be `sealed` unless they are explicitly designed to be inherited (marked `abstract` or documented as base classes).
- **Immutability:** Use `readonly` for fields whenever possible. Use `init` properties and `record` types for DTOs and value objects.
- **Encapsulation:** Keep access modifiers as restrictive as possible (`private`, `protected`, `internal`).
- **File-Scoped Namespaces:** Use file-scoped namespaces (`namespace MyProject.Core;`) to reduce indentation.
- **Single Responsibility:** Each file must contain only one class/struct/interface, unless grouping private helper classes specific to the main type.

## 3. Types, Variables & Language Features
- **Modern C#:** Utilize the latest language features (pattern matching, expression-bodied members, switch expressions) to write clear and concise code. Avoid outdated constructs.
- **Keyword Over Type:** Use language keywords for data types (e.g., `string`, `int`, `nint`) instead of runtime types (`System.String`, `System.Int32`).
- **Signed vs Unsigned:** Prefer `int` over `uint` for general use to maintain interoperability, unless documenting specific binary/hardware protocols.
- **Explicit vs Implicit Typing:** Use `var` ONLY when the type is explicitly visible on the right side of the assignment (e.g., `var list = new List<string>();`). Do not use `var` if the return type is ambiguous.

## 4. Performance & Memory Management (Critical for MonoGame/High-Load Backend)
- **Structs vs Classes:** Use `readonly struct` for small, immutable data structures (like vectors, coordinates) to avoid heap allocation. Use `in`, `ref`, and `out` modifiers for struct parameters to avoid copying.
- **LINQ Usage:** 
  - *Backend/Business Logic:* Use LINQ queries for collection manipulation to improve readability.
  - *Hot Paths / Game Loops (MonoGame):* Strictly FORBIDDEN. Do not use LINQ in `Update()`, `Draw()`, or high-frequency loops to prevent Garbage Collection (GC) pressure. Use standard `for` or `foreach` loops on arrays/spans.
- **Memory Spans:** Favor `Span<T>` and `Memory<T>` over arrays for slicing and parsing data without allocations.

## 5. Control Flow & Error Handling
- **Specific Exceptions:** Only catch exceptions you can explicitly handle. NEVER use a bare `catch (Exception ex)` without an exception filter (`when (...)`) or unless logging and immediately rethrowing at the application's top level.
- **Meaningful Errors:** Throw specific exception types (`InvalidOperationException`, `ArgumentNullException`) with clear, contextual error messages.
- **Nullability:** Nullable Reference Types (`<Nullable>enable</Nullable>`) are assumed active. Avoid returning `null`; use the Null Object Pattern, `Result<T>`, or explicitly return nullable types like `string?` and check them using pattern matching (`if (obj is not null)`).

## 6. Asynchronous Programming
- **Async/Await:** Use async programming for ALL I/O-bound operations.
- **Deadlock Prevention:** Always use `.ConfigureAwait(false)` in library or shared code to prevent UI/Main thread deadlocks, unless you explicitly need to resume on the original SynchronizationContext.
- **Task Returning:** Do not use `async void` except for event handlers. Return `Task` or `ValueTask`.

## 7. Formatting & Simplicity
- **Indentation:** Use consistent indentation (4 spaces per indentation level, no tabs).
- **Simplicity:** Write code with clarity in mind. Avoid overly complex, nested logic. If an `if` chain or method is deeply nested, extract it into separate methods or use early returns (Guard Clauses).