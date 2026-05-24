namespace MonoGame.Editor.WinForms;

/// <summary>Main editor window. Logic and event wiring.</summary>
public sealed partial class EditorForm : Form
{
    private readonly EditorContext _context = null!;
    private readonly EditorPreferences _preferences = null!;
    private readonly GameObjectRegistry _registry = null!;
    private readonly GizmoController _gizmoCtrl = new();
    private GizmoRenderer? _gizmoRenderer;
    private readonly ContentWatcher _contentWatcher = null!;
    private ToolStripMenuItem _saveProjectMenuItem = null!;
    private ToolStripMenuItem _openRecentMenuItem = null!;

    /// <summary>Designer-only constructor.</summary>
    public EditorForm() => InitializeComponent();

    public EditorForm(EditorContext context)
    {
        _context   = context;
        _registry  = new GameObjectRegistry();
        _preferences = new EditorPreferences();
        _preferences.Load();
        _contentWatcher = new ContentWatcher(context.EventBus);

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
        _context.EventBus.Subscribe<ProjectOpenedEvent>(OnProjectOpened);

        FormClosing += (_, _) => SavePreferences();
        _viewport.RenderFrame += OnViewportRenderFrame;
        _toolbarTable.Resize  += (_, _) => CenterPlaybackStrip();

        // Gizmo mouse interaction
        _viewport.MouseDown += OnViewportMouseDown;
        _viewport.MouseMove += OnViewportMouseMove;
        _viewport.MouseUp   += OnViewportMouseUp;

        // Initialize gizmo renderer (GPU resources allocated lazily on first render)
        _gizmoRenderer = new GizmoRenderer(_gizmoCtrl);

        // Initialize panels
        _hierarchyPanel.Initialize(_context);
        _inspectorPanel.Initialize(_context, _registry);
        _assetBrowserPanel.Initialize(_context);

        // Build menus programmatically (avoids Designer.cs C#-version concerns)
        BuildFileMenuExtras();
        BuildProjectMenu();
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
        _context.EventBus.Unsubscribe<ProjectOpenedEvent>(OnProjectOpened);
        _contentWatcher.Dispose();
        _gizmoRenderer?.Dispose();
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
        else if (!e.Control && !e.Alt)
        {
            switch (e.KeyCode)
            {
                case Keys.Q: SetGizmoMode(GizmoMode.Select); e.Handled = true; break;
                case Keys.W: SetGizmoMode(GizmoMode.Move);   e.Handled = true; break;
                case Keys.E: SetGizmoMode(GizmoMode.Rotate); e.Handled = true; break;
                case Keys.R: SetGizmoMode(GizmoMode.Scale);  e.Handled = true; break;
                case Keys.G: _gizmoCtrl.ShowGrid = !_gizmoCtrl.ShowGrid; e.Handled = true; break;
            }
        }
    }

    #endregion

    #region Toolbar — Play / Pause / Stop

    private async void OnPlayClick(object? sender, EventArgs e)
    {
        EditorState next = _context.State switch
        {
            EditorState.Editing => EditorState.Playing,
            EditorState.Paused  => EditorState.Playing,
            _                   => EditorState.Playing,
        };

        if (next == EditorState.Playing && _context.State == EditorState.Editing)
            await BuildContentIfNeededAsync().ConfigureAwait(true);

        _context.SetState(next);
    }

    private async Task BuildContentIfNeededAsync()
    {
        EditorProject? project = _context.ActiveProject;
        if (project is null) return;

        string mgcbFile = Path.Combine(project.ContentPath, "Content.mgcb");
        if (!File.Exists(mgcbFile)) return;

        _consolePanel.AppendLine("[Play] Building content before play…");
        _statusLabel.Text = "Building content…";

        try
        {
            int exitCode = await MgcbRunner.RunAsync(mgcbFile,
                line => _consolePanel.AppendLine(line)).ConfigureAwait(false);

            if (exitCode != 0)
            {
                _consolePanel.AppendLine($"[Play] Content build failed (exit {exitCode}). Aborting play.");
                _statusLabel.Text = "Build failed.";
                throw new InvalidOperationException($"Content build failed with exit code {exitCode}.");
            }

            _statusLabel.Text = "Build succeeded.";
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _consolePanel.AppendLine($"[Play] Build error: {ex.Message}");
            _statusLabel.Text = "Build error.";
        }
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
        GizmoMode mode = sender switch
        {
            _ when ReferenceEquals(sender, _moveModeButton)   => GizmoMode.Move,
            _ when ReferenceEquals(sender, _rotateModeButton) => GizmoMode.Rotate,
            _ when ReferenceEquals(sender, _scaleModeButton)  => GizmoMode.Scale,
            _                                                  => GizmoMode.Select,
        };
        SetGizmoMode(mode);
    }

    private void SetGizmoMode(GizmoMode mode)
    {
        _gizmoCtrl.Mode = mode;
        _selectModeButton.Checked = mode == GizmoMode.Select;
        _moveModeButton.Checked   = mode == GizmoMode.Move;
        _rotateModeButton.Checked = mode == GizmoMode.Rotate;
        _scaleModeButton.Checked  = mode == GizmoMode.Scale;
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

    #region Menu — Project

    private void BuildProjectMenu()
    {
        ToolStripMenuItem buildContentItem = new()
        {
            Text             = "Build Content",
            ShortcutKeys     = Keys.F7,
            ShowShortcutKeys = true,
        };
        buildContentItem.Click += OnBuildContentClick;
        _projectMenu.DropDownItems.Add(buildContentItem);
    }

    private async void OnBuildContentClick(object? sender, EventArgs e)
    {
        EditorProject? project = _context.ActiveProject;
        if (project is null)
        {
            _consolePanel.AppendLine("[Build] No project is open.");
            return;
        }

        string mgcbFile = Path.Combine(project.ContentPath, "Content.mgcb");
        if (!File.Exists(mgcbFile))
        {
            _consolePanel.AppendLine($"[Build] Content.mgcb not found at: {mgcbFile}");
            return;
        }

        _consolePanel.AppendLine("[Build] Starting content build…");
        _statusLabel.Text = "Building content…";

        try
        {
            int exitCode = await MgcbRunner.RunAsync(mgcbFile,
                line => _consolePanel.AppendLine(line)).ConfigureAwait(true);

            string result = exitCode == 0 ? "Build succeeded." : $"Build failed (exit {exitCode}).";
            _consolePanel.AppendLine($"[Build] {result}");
            _statusLabel.Text = result;
        }
        catch (Exception ex)
        {
            _consolePanel.AppendLine($"[Build] Error: {ex.Message}");
            _statusLabel.Text = "Build error.";
        }
    }

    #endregion

    #region Menu — File

    private void BuildFileMenuExtras()
    {
        // "Open Recent ▶" submenu
        _openRecentMenuItem = new ToolStripMenuItem("Open Recent");

        // "Save Project" Ctrl+S
        _saveProjectMenuItem = new ToolStripMenuItem("Save Project")
        {
            ShortcutKeys = Keys.Control | Keys.S,
            ShowShortcutKeys = true,
        };
        _saveProjectMenuItem.Click += OnFileSaveProjectClick;

        // Insert before _fileSeparator: [New | Open] → [New | Open | Recent ▶ | ─── | Save | ─── | Exit]
        int sepIdx = _fileMenu.DropDownItems.IndexOf(_fileSeparator);
        _fileMenu.DropDownItems.Insert(sepIdx, _openRecentMenuItem);

        sepIdx = _fileMenu.DropDownItems.IndexOf(_fileSeparator);
        _fileMenu.DropDownItems.Insert(sepIdx, new ToolStripSeparator());

        sepIdx = _fileMenu.DropDownItems.IndexOf(_fileSeparator);
        _fileMenu.DropDownItems.Insert(sepIdx, _saveProjectMenuItem);

        RebuildOpenRecentsMenu();
    }

    private void RebuildOpenRecentsMenu()
    {
        _openRecentMenuItem.DropDownItems.Clear();
        _openRecentMenuItem.Enabled = _preferences.RecentProjects.Count > 0;

        for (int i = 0; i < _preferences.RecentProjects.Count; i++)
        {
            string path = _preferences.RecentProjects[i];
            string label = $"{i + 1}. {path}";
            ToolStripMenuItem item = new(label) { Tag = path };
            item.Click += OnOpenRecentProjectClick;
            _openRecentMenuItem.DropDownItems.Add(item);
        }
    }

    private async void OnOpenRecentProjectClick(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item || item.Tag is not string path) return;

        try
        {
            EditorProject? project = await Task.Run(() => ProjectManager.Load(path)).ConfigureAwait(true);
            if (project is null)
            {
                MessageBox.Show(this,
                    $"Could not load project from:\n{path}\n\nThe project file may have been moved or deleted.",
                    "Project Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _preferences.RecentProjects.Remove(path);
                _preferences.Save();
                RebuildOpenRecentsMenu();
                return;
            }

            _context.SetActiveProject(project);
            _preferences.AddRecentProject(project.RootPath);
            RebuildOpenRecentsMenu();
            _consolePanel.AppendLine($"[Editor] Project '{project.Name}' opened from {project.RootPath}");
        }
        catch (Exception ex)
        {
            _consolePanel.AppendLine($"[Editor] Failed to open recent project: {ex.Message}");
        }
    }

    private async void OnFileSaveProjectClick(object? sender, EventArgs e)
    {
        EditorScene? scene = _context.ActiveScene;
        EditorProject? project = _context.ActiveProject;

        if (scene is null && project is null)
        {
            _consolePanel.AppendLine("[Save] Nothing to save — no project or scene is open.");
            return;
        }

        try
        {
            if (scene is not null)
            {
                string scenePath = scene.ScenePath;

                if (string.IsNullOrEmpty(scenePath))
                {
                    string initialDir = project is not null && Directory.Exists(project.ScenesPath)
                        ? project.ScenesPath
                        : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    using SaveFileDialog dlg = new()
                    {
                        Title = "Save Scene",
                        Filter = "Scene files (*.scene.json)|*.scene.json|All files (*.*)|*.*",
                        InitialDirectory = initialDir,
                        FileName = string.IsNullOrEmpty(scene.Name) ? "NewScene.scene.json" : $"{scene.Name}.scene.json",
                    };

                    if (dlg.ShowDialog(this) != DialogResult.OK) return;
                    scenePath = dlg.FileName;
                    scene.ScenePath = scenePath;
                    scene.Name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(scenePath));
                }

                await SceneSerializer.SaveAsync(scene, scenePath).ConfigureAwait(true);
                _consolePanel.AppendLine($"[Save] Scene saved to {scenePath}");
                _statusLabel.Text = "Saved.";
            }
        }
        catch (Exception ex)
        {
            _consolePanel.AppendLine($"[Save] Error: {ex.Message}");
            _statusLabel.Text = "Save failed.";
        }
    }

    private async void OnFileNewProjectClick(object? sender, EventArgs e)
    {
        using NewProjectDialog dlg = new();
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            EditorProject project = await Task.Run(() => ProjectManager.Create(dlg.ProjectName, dlg.ParentPath));
            _context.SetActiveProject(project);
            _preferences.LastProjectPath = project.RootPath;
            _preferences.AddRecentProject(project.RootPath);
            _preferences.Save();
            _consolePanel.AppendLine($"[Editor] Project '{project.Name}' created at {project.RootPath}");
        }
        catch (Exception ex)
        {
            _consolePanel.AppendLine($"[Editor] Failed to create project: {ex.Message}");
            MessageBox.Show(this, $"Failed to create project:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnFileOpenProjectClick(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dlg = new()
        {
            Description = "Select a MonoGame Editor project folder",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(_preferences.LastProjectPath)
                ? _preferences.LastProjectPath
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            EditorProject? project = await Task.Run(() => ProjectManager.Load(dlg.SelectedPath));

            if (project is null)
            {
                string? slnName = await Task.Run(() => ProjectManager.FindSolutionName(dlg.SelectedPath));

                if (slnName is null)
                {
                    MessageBox.Show(this,
                        $"The selected folder does not contain a valid MonoGame Editor project.\n\nExpected a '{ProjectManager.ProjectFileName}' file.",
                        "Invalid Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult answer = MessageBox.Show(this,
                    $"The folder contains an existing MonoGame solution '{slnName}' but has not been initialized as an editor project yet.\n\nInitialize it now? This will create '{ProjectManager.ProjectFileName}' and any missing standard folders.",
                    "Initialize Project", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (answer != DialogResult.Yes) return;

                project = await Task.Run(() => ProjectManager.Initialize(dlg.SelectedPath));
            }

            _context.SetActiveProject(project);
            _preferences.LastProjectPath = project.RootPath;
            _preferences.AddRecentProject(project.RootPath);
            _preferences.Save();
            _consolePanel.AppendLine($"[Editor] Project '{project.Name}' opened from {project.RootPath}");
        }
        catch (Exception ex)
        {
            _consolePanel.AppendLine($"[Editor] Failed to open project: {ex.Message}");
            MessageBox.Show(this, $"Failed to open project:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnFileExitClick(object? sender, EventArgs e) => Close();

    private void OnProjectOpened(ProjectOpenedEvent evt)
    {
        if (InvokeRequired) { BeginInvoke(() => OnProjectOpened(evt)); return; }
        Text = evt.Project is null ? "MonoGame Editor" : $"MonoGame Editor — {evt.Project.Name}";

        string contentPath = evt.Project?.ContentPath ?? string.Empty;
        _contentWatcher.Watch(contentPath);

        RebuildOpenRecentsMenu();
    }

    #endregion

    #region Viewport rendering

    private void OnViewportRenderFrame(object? sender, RenderEventArgs e)
    {
        if (_gizmoRenderer == null) return;

        if (!_gizmoRenderer.IsInitialized)
            _gizmoRenderer.Initialize(e.GraphicsDevice);

        int w = _viewport.ClientSize.Width;
        int h = _viewport.ClientSize.Height;
        if (w <= 0 || h <= 0) return;

        Viewport vp = new(0, 0, w, h);
        Matrix cameraTransform = _viewport.Camera.GetTransformMatrix(vp);

        _gizmoRenderer.Draw(_context.SelectedObject, cameraTransform, w, h);
    }

    #endregion

    #region Viewport mouse — gizmo interaction

    private void OnViewportMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        EditorGameObject? selected = _context.SelectedObject;
        if (selected == null) return;

        int w = _viewport.ClientSize.Width;
        int h = _viewport.ClientSize.Height;
        if (w <= 0 || h <= 0) return;

        Viewport vp = new(0, 0, w, h);
        Matrix camMatrix = _viewport.Camera.GetTransformMatrix(vp);
        Matrix inverse   = Matrix.Invert(camMatrix);

        Vector2 screenPos = new(e.X, e.Y);
        Vector2 worldPos  = Vector2.Transform(screenPos, inverse);
        Vector2 objScreen = Vector2.Transform(
            new Vector2(selected.Position.X, selected.Position.Y), camMatrix);

        _gizmoCtrl.BeginDrag(
            e.X, e.Y,
            objScreen.X, objScreen.Y,
            worldPos.X,  worldPos.Y,
            selected);
    }

    private void OnViewportMouseMove(object? sender, MouseEventArgs e)
    {
        EditorGameObject? selected = _context.SelectedObject;
        if (selected == null) return;

        int w = _viewport.ClientSize.Width;
        int h = _viewport.ClientSize.Height;
        if (w <= 0 || h <= 0) return;

        Viewport vp = new(0, 0, w, h);
        Matrix camMatrix = _viewport.Camera.GetTransformMatrix(vp);
        Matrix inverse   = Matrix.Invert(camMatrix);

        Vector2 worldPos  = Vector2.Transform(new Vector2(e.X, e.Y), inverse);
        Vector2 objScreen = Vector2.Transform(
            new Vector2(selected.Position.X, selected.Position.Y), camMatrix);

        _gizmoCtrl.UpdateDrag(
            worldPos.X, worldPos.Y,
            e.X, e.Y,
            objScreen.X, objScreen.Y,
            selected);
    }

    private void OnViewportMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        bool ctrlHeld         = ModifierKeys.HasFlag(Keys.Control);
        IEditorCommand? cmd   = _gizmoCtrl.EndDrag(_context.SelectedObject, ctrlHeld);

        if (cmd != null)
            _context.Commands.Execute(cmd);
    }

    #endregion
}
