namespace MonoGame.Editor.Core.Models;

/// <summary>Represents a game object node in the editor scene hierarchy.</summary>
public sealed class EditorGameObject
{
    /// <summary>Stable unique identifier for this object.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Display name shown in the hierarchy.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Whether this object and its children are active.</summary>
    public bool Active { get; set; } = true;

    /// <summary>World-space position of this object.</summary>
    public EditorVector2 Position { get; set; } = EditorVector2.Zero;

    /// <summary>Rotation in degrees.</summary>
    public float Rotation { get; set; }

    /// <summary>Scale applied to this object and its children.</summary>
    public EditorVector2 Scale { get; set; } = EditorVector2.One;

    /// <summary>Behaviours attached to this object.</summary>
    public List<EditorBehaviour> Behaviours { get; } = [];

    /// <summary>Child objects in the hierarchy.</summary>
    public List<EditorGameObject> Children { get; } = [];

    /// <summary>Parent object, or <c>null</c> if this is a root object. Excluded from serialization to avoid circular references.</summary>
    [JsonIgnore]
    public EditorGameObject? Parent { get; set; }

    /// <summary>
    /// Path to the source <c>.prefab.json</c> file when this object was instantiated from a prefab,
    /// or <c>null</c> if this is a plain game object.
    /// </summary>
    public string? PrefabPath { get; set; }
}
