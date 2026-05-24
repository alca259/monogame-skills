namespace MonoGame.Editor.WinForms;

/// <summary>Main editor window. Logic and event wiring.</summary>
public sealed partial class EditorForm : Form
{
    private readonly EditorContext _context = null!;
    private readonly EditorPreferences _preferences = null!;
    private readonly GameObjectRegistry _registry = null!;

    /// <summary>Designer-only constructor.</summary>
    public EditorForm() => InitializeComponent();

    public EditorForm(EditorContext context)
    {
        _context   = context;
        _registry  = new GameObjectRegistry();
        _preferences = new EditorPreferences();
        _preferences.Load();

        InitializeComponent();
        WireEvents();
    }

    #region Startup / Shutdown

    private void ApplyPreferences()
    {
        _outerSplit.Panel1MinSize = 180;
        _innerSplit.Panel2MinSize = 220;
        _mainSplit.Panel2MinSize  = 80;

        int leftWidth = _preferences.LeftPanelWidth > 0 ? _preferences.LeftPanelWidth : 220;
        _outerSplit.SplitterDistance = ClampSplitter(leftWidth, 180, _outerSplit.Width - 180);

        int rightWidth = _preferences.RightPanelWidth > 0 ? _preferences.RightPanelWidth : 280;
        _innerSplit.SplitterDistance = ClampSplitter(_innerSplit.Width - rightWidth, 320, _innerSplit.Width - 220);

        int bottomH = _preferences.ConsolePanelHeight > 0 ? _preferences.ConsolePanelHeight : 200;
        _mainSplit.SplitterDistance = ClampSplitter(_mainSplit.Height - bottomH, 240, _mainSplit.Height - 80);

        _viewHierarchyMenuItem.Checked    = _preferences.HierarchyVisible;
        _viewInspectorMenuItem.Checked    = _preferences.InspectorVisible;
        _viewAssetBrowserMenuItem.Checked = _preferences.AssetBrowserVisible;
        _viewConsoleMenuItem.Checked      = _preferences.ConsoleVisible;
        _assetBrowserPanel.SplitterDistance = _preferences.AssetBrowserSplitterDistance;

        UpdatePanelVisibility();
    }

    private static int ClampSplitter(int value, int min, int max)
        => max < min ? min : Math.Clamp(value, min, max);

    private void CenterPlaybackStrip()
    {
        // Center relative to full toolbar width (not just the fill cell)
        int x = (_toolbarTable.Width - _playbackStrip.Width) / 2 - _playbackCell.Left;
        int y = (_playbackCell.Height - _playbackStrip.Height) / 2;
        _playbackStrip.Location = new System.Drawing.Point(Math.Max(0, x), Math.Max(0, y));
    }

    private void WireEvents()
    {
        Shown  += (_, _) => { ApplyPreferences(); CenterPlaybackStrip(); _registry.Scan(); };
        KeyPreview = true;
        KeyDown    += OnFormKeyDown;

        _context.EventBus.Subscribe<EditorStateChangedEvent>(OnEditorStateChanged);
        _context.EventBus.Subscribe<UndoPerformedEvent>(OnUndoPerformed);
        _context.EventBus.Subscribe<RedoPerformedEvent>(OnRedoPerformed);

        FormClosing += (_, _) => SavePreferences();
        _viewport.RenderFrame += OnViewportRenderFrame;
        _toolbarTable.Resize  += (_, _) => CenterPlaybackStrip();

        // Initialize panels
        _hierarchyPanel.Initialize(_context);
        _inspectorPanel.Initialize(_context, _registry);
    }

    private void SavePreferences()
    {
        _preferences.LeftPanelWidth               = _outerSplit.SplitterDistance;
        _preferences.RightPanelWidth              = _innerSplit.Width - _innerSplit.SplitterDistance;
        _preferences.ConsolePanelHeight           = _mainSplit.Height - _mainSplit.SplitterDistance;
        _preferences.AssetBrowserSplitterDistance = _assetBrowserPanel.SplitterDistance;
        _preferences.HierarchyVisible    = _viewHierarchyMenuItem.Checked;
        _preferences.InspectorVisible    = _viewInspectorMenuItem.Checked;
        _preferences.AssetBrowserVisible = _viewAssetBrowserMenuItem.Checked;
        _preferences.ConsoleVisible      = _viewConsoleMenuItem.Checked;
        _preferences.Save();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _context.EventBus.Unsubscribe<EditorStateChangedEvent>(OnEditorStateChanged);
        _context.EventBus.Unsubscribe<UndoPerformedEvent>(OnUndoPerformed);
        _context.EventBus.Unsubscribe<RedoPerformedEvent>(OnRedoPerformed);
        base.OnFormClosed(e);
    }

    #endregion

    #region Keyboard shortcuts

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Z)
        {
            _context.Commands.Undo();
            e.Handled = true;
        }
        else if (e.Control && e.KeyCode == Keys.Y)
        {
            _context.Commands.Redo();
            e.Handled = true;
        }
    }

    #endregion

    #region Toolbar — Play / Pause / Stop

    private void OnPlayClick(object? sender, EventArgs e)
    {
        EditorState next = _context.State switch
        {
            EditorState.Editing => EditorState.Playing,
            EditorState.Paused  => EditorState.Playing,
            _                   => EditorState.Playing,
        };
        _context.SetState(next);
    }

    private void OnPauseClick(object? sender, EventArgs e)
    {
        if (_context.State == EditorState.Playing)
            _context.SetState(EditorState.Paused);
    }

    private void OnStopClick(object? sender, EventArgs e)
    {
        if (_context.State != EditorState.Editing)
            _context.SetState(EditorState.Editing);
    }

    private void OnEditorStateChanged(EditorStateChangedEvent evt)
    {
        if (InvokeRequired) { BeginInvoke(() => OnEditorStateChanged(evt)); return; }

        UpdatePlaybackButtons(evt.NewState);
        _statusLabel.Text = evt.NewState switch
        {
            EditorState.Editing => "Editing",
            EditorState.Playing => "Playing",
            EditorState.Paused  => "Paused",
            _                   => string.Empty,
        };
    }

    private void UpdatePlaybackButtons(EditorState state)
    {
        System.Drawing.Color accent = System.Drawing.Color.FromArgb(0, 122, 204);
        System.Drawing.Color normal = System.Drawing.SystemColors.Control;

        _playButton.BackColor  = state == EditorState.Playing ? accent : normal;
        _pauseButton.BackColor = state == EditorState.Paused  ? accent : normal;
        _stopButton.BackColor  = normal;

        _playButton.Enabled  = state != EditorState.Playing;
        _pauseButton.Enabled = state == EditorState.Playing;
        _stopButton.Enabled  = state != EditorState.Editing;
    }

    #endregion

    #region Toolbar — Gizmo modes

    private void OnGizmoModeClick(object? sender, EventArgs e)
    {
        foreach (ToolStripButton btn in new[] { _selectModeButton, _moveModeButton, _rotateModeButton, _scaleModeButton })
            btn.Checked = ReferenceEquals(btn, sender);
    }

    #endregion

    #region View menu

    private void UpdatePanelVisibility()
    {
        _outerSplit.Panel1Collapsed = !_viewHierarchyMenuItem.Checked;
        _innerSplit.Panel2Collapsed = !_viewInspectorMenuItem.Checked;

        bool assetsVisible  = _viewAssetBrowserMenuItem.Checked;
        bool consoleVisible = _viewConsoleMenuItem.Checked;

        _bottomTabControl.TabPages.Clear();
        if (assetsVisible)  _bottomTabControl.TabPages.Add(_assetsTab);
        if (consoleVisible) _bottomTabControl.TabPages.Add(_consoleTab);

        _mainSplit.Panel2Collapsed = !assetsVisible && !consoleVisible;
    }

    private void OnViewMenuItemClick(object? sender, EventArgs e) => UpdatePanelVisibility();

    private void OnResetLayoutClick(object? sender, EventArgs e) => ResetLayout();

    private void ResetLayout()
    {
        EditorPreferences defaults = new();

        _viewHierarchyMenuItem.Checked    = defaults.HierarchyVisible;
        _viewInspectorMenuItem.Checked    = defaults.InspectorVisible;
        _viewAssetBrowserMenuItem.Checked = defaults.AssetBrowserVisible;
        _viewConsoleMenuItem.Checked      = defaults.ConsoleVisible;
        UpdatePanelVisibility();

        _outerSplit.Panel1MinSize = 180;
        _innerSplit.Panel2MinSize = 220;
        _mainSplit.Panel2MinSize  = 80;

        _outerSplit.SplitterDistance = ClampSplitter(defaults.LeftPanelWidth, 180, _outerSplit.Width - 180);
        _innerSplit.SplitterDistance = ClampSplitter(_innerSplit.Width - defaults.RightPanelWidth, 320, _innerSplit.Width - 220);
        _mainSplit.SplitterDistance  = ClampSplitter(_mainSplit.Height - defaults.ConsolePanelHeight, 240, _mainSplit.Height - 80);
        _assetBrowserPanel.SplitterDistance = defaults.AssetBrowserSplitterDistance;
    }

    #endregion

    #region Menu — Edit (Undo / Redo)

    private void OnUndoClick(object? sender, EventArgs e)
    {
        _context.Commands.Undo();
    }

    private void OnRedoClick(object? sender, EventArgs e)
    {
        _context.Commands.Redo();
    }

    private void OnUndoPerformed(UndoPerformedEvent _)
    {
        if (InvokeRequired) { BeginInvoke(UpdateEditMenu); return; }
        UpdateEditMenu();
    }

    private void OnRedoPerformed(RedoPerformedEvent _)
    {
        if (InvokeRequired) { BeginInvoke(UpdateEditMenu); return; }
        UpdateEditMenu();
    }

    private void UpdateEditMenu()
    {
        string? undoDesc = _context.Commands.UndoDescription;
        string? redoDesc = _context.Commands.RedoDescription;
        _undoMenuItem.Text    = undoDesc is null ? "Undo" : $"Undo {undoDesc}";
        _redoMenuItem.Text    = redoDesc is null ? "Redo" : $"Redo {redoDesc}";
        _undoMenuItem.Enabled = undoDesc is not null;
        _redoMenuItem.Enabled = redoDesc is not null;
    }

    #endregion

    #region Menu — File

    private void OnFileNewProjectClick(object? sender, EventArgs e) =>
        _consolePanel.AppendLine("[Editor] New Project — not yet implemented.");

    private void OnFileOpenProjectClick(object? sender, EventArgs e) =>
        _consolePanel.AppendLine("[Editor] Open Project — not yet implemented.");

    private void OnFileExitClick(object? sender, EventArgs e) => Close();

    #endregion

    #region Viewport rendering

    private void OnViewportRenderFrame(object? sender, RenderEventArgs e)
    {
        // Fase 1: viewport is intentionally empty — scene rendering added in later phases.
    }

    #endregion
}
