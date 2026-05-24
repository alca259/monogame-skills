#nullable enable
namespace MonoGame.Editor.WinForms;

partial class EditorForm
{
    private System.ComponentModel.IContainer? components = null;

    // ── Top bars ──────────────────────────────────────────────────────────
    private MenuStrip _mainMenuStrip = null!;
    private ToolStrip _mainToolStrip = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _statusLabel = null!;

    // ── Playback buttons ──────────────────────────────────────────────────
    private ToolStripButton _playButton = null!;
    private ToolStripButton _pauseButton = null!;
    private ToolStripButton _stopButton = null!;

    // ── Gizmo mode buttons ────────────────────────────────────────────────
    private ToolStripButton _selectModeButton = null!;
    private ToolStripButton _moveModeButton = null!;
    private ToolStripButton _rotateModeButton = null!;
    private ToolStripButton _scaleModeButton = null!;

    // ── Main layout ───────────────────────────────────────────────────────
    private SplitContainer _mainSplit = null!;   // horizontal: content | console
    private SplitContainer _outerSplit = null!;  // vertical: left tab | viewport+inspector
    private SplitContainer _innerSplit = null!;  // vertical: viewport | inspector

    // ── Left panels ───────────────────────────────────────────────────────
    private TabControl _leftTabControl = null!;
    private TabPage _hierarchyTab = null!;
    private TabPage _assetBrowserTab = null!;
    private SceneHierarchyPanel _hierarchyPanel = null!;
    private AssetBrowserPanel _assetBrowserPanel = null!;

    // ── Center / Right ────────────────────────────────────────────────────
    private MonoGameControl _viewport = null!;
    private InspectorPanel _inspectorPanel = null!;

    // ── Bottom ────────────────────────────────────────────────────────────
    private ConsolePanel _consolePanel = null!;

    // ── View menu items ───────────────────────────────────────────────────
    private ToolStripMenuItem _viewHierarchyMenuItem = null!;
    private ToolStripMenuItem _viewInspectorMenuItem = null!;
    private ToolStripMenuItem _viewAssetBrowserMenuItem = null!;
    private ToolStripMenuItem _viewConsoleMenuItem = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        BuildMenuStrip();
        BuildToolStrip();
        BuildStatusStrip();
        BuildPanels();
        BuildLayout();

        AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1280, 800);
        Font = new System.Drawing.Font("Segoe UI", 9f);
        MinimumSize = new System.Drawing.Size(800, 600);
        Text = "MonoGame Editor";
        WindowState = FormWindowState.Maximized;

        ResumeLayout(false);
        PerformLayout();
    }

    // ── Menu strip ────────────────────────────────────────────────────────

    private void BuildMenuStrip()
    {
        _mainMenuStrip = new MenuStrip();

        // File
        ToolStripMenuItem fileMenu = new("File");
        ToolStripMenuItem newProjectItem = new("New Project...");
        ToolStripMenuItem openProjectItem = new("Open Project...");
        ToolStripMenuItem exitItem = new("Exit");
        newProjectItem.Click  += OnFileNewProjectClick;
        openProjectItem.Click += OnFileOpenProjectClick;
        exitItem.Click        += OnFileExitClick;
        fileMenu.DropDownItems.AddRange([newProjectItem, openProjectItem, new ToolStripSeparator(), exitItem]);

        // Edit (placeholder)
        ToolStripMenuItem editMenu = new("Edit");

        // View
        ToolStripMenuItem viewMenu = new("View");
        _viewHierarchyMenuItem   = new ToolStripMenuItem("Hierarchy")   { CheckOnClick = true, Checked = true };
        _viewInspectorMenuItem   = new ToolStripMenuItem("Inspector")   { CheckOnClick = true, Checked = true };
        _viewAssetBrowserMenuItem = new ToolStripMenuItem("Asset Browser") { CheckOnClick = true, Checked = true };
        _viewConsoleMenuItem     = new ToolStripMenuItem("Console")     { CheckOnClick = true, Checked = true };
        _viewHierarchyMenuItem.Click    += OnViewMenuItemClick;
        _viewInspectorMenuItem.Click    += OnViewMenuItemClick;
        _viewAssetBrowserMenuItem.Click += OnViewMenuItemClick;
        _viewConsoleMenuItem.Click      += OnViewMenuItemClick;
        viewMenu.DropDownItems.AddRange([_viewHierarchyMenuItem, _viewInspectorMenuItem, _viewAssetBrowserMenuItem, _viewConsoleMenuItem]);

        // Project / Debug (placeholders)
        ToolStripMenuItem projectMenu = new("Project");
        ToolStripMenuItem debugMenu   = new("Debug");

        _mainMenuStrip.Items.AddRange([fileMenu, editMenu, viewMenu, projectMenu, debugMenu]);
        Controls.Add(_mainMenuStrip);
        MainMenuStrip = _mainMenuStrip;
    }

    // ── Tool strip ────────────────────────────────────────────────────────

    private void BuildToolStrip()
    {
        _mainToolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };

        _playButton  = new ToolStripButton("▶ Play")  { ToolTipText = "Play (F5)" };
        _pauseButton = new ToolStripButton("⏸ Pause") { ToolTipText = "Pause", Enabled = false };
        _stopButton  = new ToolStripButton("■ Stop")  { ToolTipText = "Stop", Enabled = false };

        _playButton.Click  += OnPlayClick;
        _pauseButton.Click += OnPauseClick;
        _stopButton.Click  += OnStopClick;

        _selectModeButton = new ToolStripButton("Q Select") { CheckOnClick = true, Checked = true, ToolTipText = "Select (Q)" };
        _moveModeButton   = new ToolStripButton("W Move")   { CheckOnClick = true, ToolTipText = "Move (W)" };
        _rotateModeButton = new ToolStripButton("E Rotate") { CheckOnClick = true, ToolTipText = "Rotate (E)" };
        _scaleModeButton  = new ToolStripButton("R Scale")  { CheckOnClick = true, ToolTipText = "Scale (R)" };

        foreach (ToolStripButton btn in new[] { _selectModeButton, _moveModeButton, _rotateModeButton, _scaleModeButton })
            btn.Click += OnGizmoModeClick;

        _mainToolStrip.Items.AddRange([
            _playButton, _pauseButton, _stopButton,
            new ToolStripSeparator(),
            _selectModeButton, _moveModeButton, _rotateModeButton, _scaleModeButton,
        ]);

        Controls.Add(_mainToolStrip);
    }

    // ── Status strip ──────────────────────────────────────────────────────

    private void BuildStatusStrip()
    {
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("Editing");
        _statusStrip.Items.Add(_statusLabel);
        Controls.Add(_statusStrip);
    }

    // ── Panels ────────────────────────────────────────────────────────────

    private void BuildPanels()
    {
        _hierarchyPanel    = new SceneHierarchyPanel { Dock = DockStyle.Fill };
        _assetBrowserPanel = new AssetBrowserPanel   { Dock = DockStyle.Fill };
        _inspectorPanel    = new InspectorPanel      { Dock = DockStyle.Fill };
        _consolePanel      = new ConsolePanel        { Dock = DockStyle.Fill };
        _viewport          = new MonoGameControl     { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.FromArgb(30, 30, 30) };

        _hierarchyTab    = new TabPage("Hierarchy")     { Padding = new System.Windows.Forms.Padding(0) };
        _assetBrowserTab = new TabPage("Asset Browser") { Padding = new System.Windows.Forms.Padding(0) };
        _hierarchyTab.Controls.Add(_hierarchyPanel);
        _assetBrowserTab.Controls.Add(_assetBrowserPanel);

        _leftTabControl = new TabControl { Dock = DockStyle.Fill };
        _leftTabControl.TabPages.AddRange([_hierarchyTab, _assetBrowserTab]);
    }

    // ── Split container layout ────────────────────────────────────────────

    private void BuildLayout()
    {
        // Inner split: viewport (left) | inspector (right)
        _innerSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
        _innerSplit.Panel1.Controls.Add(_viewport);
        _innerSplit.Panel2.Controls.Add(_inspectorPanel);

        // Outer split: left tab panel | inner split
        _outerSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
        _outerSplit.Panel1.Controls.Add(_leftTabControl);
        _outerSplit.Panel2.Controls.Add(_innerSplit);

        // Main split: outer split (top) | console (bottom)
        _mainSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
        _mainSplit.Panel1.Controls.Add(_outerSplit);
        _mainSplit.Panel2.Controls.Add(_consolePanel);

        Controls.Add(_mainSplit);
    }
}
