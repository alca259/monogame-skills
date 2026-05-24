namespace MonoGame.Editor.Core;

/// <summary>
/// Singleton source of truth for the editor's runtime state: active scene, selection, and play mode.
/// Panels communicate state changes exclusively through <see cref="EventBus"/>.
/// </summary>
public sealed class EditorContext
{
    private static EditorContext? _instance;
    private static readonly Lock _instanceLock = new();

    private readonly Lock _stateLock = new();
    private EditorState _state = EditorState.Editing;
    private EditorScene? _activeScene;
    private EditorGameObject? _selectedObject;
    private readonly List<EditorGameObject> _multiSelection = [];
    private EditorProject? _activeProject;

    #region Singleton

    /// <summary>Returns the global editor context instance, creating it on first access.</summary>
    public static EditorContext Instance
    {
        get
        {
            lock (_instanceLock)
                return _instance ??= new EditorContext(new EditorEventBus());
        }
    }

    /// <summary>Resets the singleton (for unit testing only).</summary>
    internal static void Reset()
    {
        lock (_instanceLock)
            _instance = null;
    }

    #endregion

    /// <summary>Initializes with a custom event bus (used in tests or DI containers).</summary>
    public EditorContext(IEditorEventBus eventBus)
    {
        EventBus = eventBus;
        Commands = new CommandStack(100, eventBus);
    }

    #region Properties

    /// <summary>The shared event bus for this context.</summary>
    public IEditorEventBus EventBus { get; }

    /// <summary>Undo/redo history for all editor operations.</summary>
    public CommandStack Commands { get; }

    /// <summary>Current editor state (Editing, Playing, or Paused).</summary>
    public EditorState State { get { lock (_stateLock) return _state; } }

    /// <summary>Currently loaded scene, or <c>null</c> if no scene is open.</summary>
    public EditorScene? ActiveScene { get { lock (_stateLock) return _activeScene; } }

    /// <summary>Primary selected object, or <c>null</c> if nothing is selected.</summary>
    public EditorGameObject? SelectedObject { get { lock (_stateLock) return _selectedObject; } }

    /// <summary>All currently selected objects (single or multi-selection).</summary>
    public IReadOnlyList<EditorGameObject> MultiSelection
    {
        get { lock (_stateLock) return _multiSelection.ToArray(); }
    }

    /// <summary>Active game project, or <c>null</c> if no project is open.</summary>
    public EditorProject? ActiveProject { get { lock (_stateLock) return _activeProject; } }

    #endregion

    #region State mutations

    /// <summary>Transitions the editor to <paramref name="state"/> and publishes <see cref="EditorStateChangedEvent"/>.</summary>
    public void SetState(EditorState state)
    {
        EditorState old;
        lock (_stateLock)
        {
            old = _state;
            _state = state;
        }

        EventBus.Publish(new EditorStateChangedEvent(old, state));
    }

    /// <summary>Sets the single selected object and publishes <see cref="GameObjectSelectedEvent"/>.</summary>
    public void SetSelection(EditorGameObject? obj)
    {
        lock (_stateLock)
        {
            _selectedObject = obj;
            _multiSelection.Clear();
            if (obj is not null)
                _multiSelection.Add(obj);
        }

        EventBus.Publish(new GameObjectSelectedEvent(obj));
    }

    /// <summary>
    /// Sets a multi-object selection. The first item becomes <see cref="SelectedObject"/>.
    /// Publishes <see cref="GameObjectSelectedEvent"/> with the first (or <c>null</c>) object.
    /// </summary>
    public void SetMultiSelection(IEnumerable<EditorGameObject> objects)
    {
        EditorGameObject? first;
        lock (_stateLock)
        {
            _multiSelection.Clear();
            _multiSelection.AddRange(objects);
            first = _multiSelection.Count > 0 ? _multiSelection[0] : null;
            _selectedObject = first;
        }

        EventBus.Publish(new GameObjectSelectedEvent(first));
    }

    /// <summary>Sets the active scene and publishes <see cref="SceneLoadedEvent"/>.</summary>
    public void SetActiveScene(EditorScene? scene)
    {
        lock (_stateLock)
            _activeScene = scene;

        EventBus.Publish(new SceneLoadedEvent(scene));
    }

    /// <summary>Sets the active project (does not publish an event).</summary>
    public void SetActiveProject(EditorProject? project)
    {
        lock (_stateLock)
            _activeProject = project;
    }

    #endregion
}
