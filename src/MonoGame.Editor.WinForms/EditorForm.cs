namespace MonoGame.Editor.WinForms;

/// <summary>Main editor window. Logic and event wiring.</summary>
public sealed partial class EditorForm : Form
{
    private readonly EditorContext _context;
    private readonly EditorPreferences _preferences;

    public EditorForm(EditorContext context)
    {
        _context = context;
        _preferences = new EditorPreferences();
        _preferences.Load();

        InitializeComponent();
        WireEvents();
    }

    #region Startup / Shutdown

    private void ApplyPreferences()
    {
        // Apply min-size constraints now that controls have real dimensions.
        _innerSplit.Panel2MinSize = 220;
        _outerSplit.Panel1MinSize = 180;
        _mainSplit.Panel2MinSize  = 80;

        // Outer split: left panel width — default 220 px
        int leftWidth = _preferences.LeftPanelWidth > 0 ? _preferences.LeftPanelWidth : 220;
        _outerSplit.SplitterDistance = ClampSplitter(leftWidth, 180, _outerSplit.Width - 180);

        // Inner split: right panel width — default 280 px
        int rightWidth = _preferences.RightPanelWidth > 0 ? _preferences.RightPanelWidth : 280;
        _innerSplit.SplitterDistance = ClampSplitter(_innerSplit.Width - rightWidth, 320, _innerSplit.Width - 220);

        // Main split: console height — default 160 px
        int consoleH = _preferences.ConsolePanelHeight > 0 ? _preferences.ConsolePanelHeight : 160;
        _mainSplit.SplitterDistance = ClampSplitter(_mainSplit.Height - consoleH, 240, _mainSplit.Height - 80);

        _viewHierarchyMenuItem.Checked    = _preferences.HierarchyVisible;
        _viewInspectorMenuItem.Checked    = _preferences.InspectorVisible;
        _viewAssetBrowserMenuItem.Checked = _preferences.AssetBrowserVisible;
        _viewConsoleMenuItem.Checked      = _preferences.ConsoleVisible;

        UpdatePanelVisibility();
    }

    private static int ClampSplitter(int value, int min, int max)
        => max < min ? min : Math.Clamp(value, min, max);

    private void WireEvents()
    {
        Shown += (_, _) => ApplyPreferences();
        _context.EventBus.Subscribe<EditorStateChangedEvent>(OnEditorStateChanged);

        FormClosing += (_, _) => SavePreferences();
        _viewport.RenderFrame += OnViewportRenderFrame;
    }

    private void SavePreferences()
    {
        _preferences.LeftPanelWidth = _outerSplit.SplitterDistance;
        _preferences.RightPanelWidth = _innerSplit.Width - _innerSplit.SplitterDistance;
        _preferences.ConsolePanelHeight = _mainSplit.Height - _mainSplit.SplitterDistance;
        _preferences.HierarchyVisible = _viewHierarchyMenuItem.Checked;
        _preferences.InspectorVisible = _viewInspectorMenuItem.Checked;
        _preferences.AssetBrowserVisible = _viewAssetBrowserMenuItem.Checked;
        _preferences.ConsoleVisible = _viewConsoleMenuItem.Checked;
        _preferences.Save();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _context.EventBus.Unsubscribe<EditorStateChangedEvent>(OnEditorStateChanged);
        base.OnFormClosed(e);
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

        _playButton.BackColor  = state == EditorState.Playing  ? accent : normal;
        _pauseButton.BackColor = state == EditorState.Paused   ? accent : normal;
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
        _leftTabControl.TabPages.Clear();

        if (_viewHierarchyMenuItem.Checked)
            _leftTabControl.TabPages.Add(_hierarchyTab);

        if (_viewAssetBrowserMenuItem.Checked)
            _leftTabControl.TabPages.Add(_assetBrowserTab);

        _outerSplit.Panel1Collapsed = _leftTabControl.TabPages.Count == 0;
        _innerSplit.Panel2Collapsed = !_viewInspectorMenuItem.Checked;
        _mainSplit.Panel2Collapsed  = !_viewConsoleMenuItem.Checked;
    }

    private void OnViewMenuItemClick(object? sender, EventArgs e) => UpdatePanelVisibility();

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
