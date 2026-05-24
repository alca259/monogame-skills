namespace MonoGame.Editor.Core.Events;

/// <summary>Published when a new asset is detected by the FileWatcher.</summary>
/// <param name="AssetPath">Absolute path of the imported asset.</param>
public sealed record AssetImportedEvent(string AssetPath) : IEditorEvent;
