#nullable enable
namespace MonoGame.Editor.WinForms;

partial class EditorForm
{
    private System.ComponentModel.IContainer? components = null;

    // ── Menu ──────────────────────────────────────────────────────────────
    private MenuStrip _mainMenuStrip = null!;
    private ToolStripMenuItem _fileMenu = null!;
    private ToolStripMenuItem _newProjectItem = null!;
    private ToolStripMenuItem _openProjectItem = null!;
    private ToolStripSeparator _fileSeparator = null!;
    private ToolStripMenuItem _exitItem = null!;
    private ToolStripMenuItem _editMenu = null!;
    private ToolStripMenuItem _viewMenu = null!;
    private ToolStripMenuItem _viewHierarchyMenuItem = null!;
    private ToolStripMenuItem _viewInspectorMenuItem = null!;
    private ToolStripMenuItem _viewAssetBrowserMenuItem = null!;
    private ToolStripMenuItem _viewConsoleMenuItem = null!;
    private ToolStripMenuItem _projectMenu = null!;
    private ToolStripMenuItem _debugMenu = null!;

    // ── Status ────────────────────────────────────────────────────────────
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _statusLabel = null!;

    // ── Vertical tool strip (left of viewport) ────────────────────────────
    private ToolStrip _toolStrip = null!;
    private ToolStripButton _playButton = null!;
    private ToolStripButton _pauseButton = null!;
    private ToolStripButton _stopButton = null!;
    private ToolStripSeparator _toolStripSeparator1 = null!;
    private ToolStripButton _selectModeButton = null!;
    private ToolStripButton _moveModeButton = null!;
    private ToolStripButton _rotateModeButton = null!;
    private ToolStripButton _scaleModeButton = null!;

    // ── Split containers ──────────────────────────────────────────────────
    private SplitContainer _mainSplit = null!;
    private SplitContainer _outerSplit = null!;
    private SplitContainer _innerSplit = null!;

    // ── Panels ────────────────────────────────────────────────────────────
    private Panel _viewportArea = null!;
    private SceneHierarchyPanel _hierarchyPanel = null!;
    private MonoGameControl _viewport = null!;
    private InspectorPanel _inspectorPanel = null!;

    // ── Bottom tabs ───────────────────────────────────────────────────────
    private TabControl _bottomTabControl = null!;
    private TabPage _assetsTab = null!;
    private TabPage _consoleTab = null!;
    private AssetBrowserPanel _assetBrowserPanel = null!;
    private ConsolePanel _consolePanel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _mainMenuStrip        = new MenuStrip();
        _fileMenu             = new ToolStripMenuItem();
        _newProjectItem       = new ToolStripMenuItem();
        _openProjectItem      = new ToolStripMenuItem();
        _fileSeparator        = new ToolStripSeparator();
        _exitItem             = new ToolStripMenuItem();
        _editMenu             = new ToolStripMenuItem();
        _viewMenu             = new ToolStripMenuItem();
        _viewHierarchyMenuItem    = new ToolStripMenuItem();
        _viewInspectorMenuItem    = new ToolStripMenuItem();
        _viewAssetBrowserMenuItem = new ToolStripMenuItem();
        _viewConsoleMenuItem      = new ToolStripMenuItem();
        _projectMenu          = new ToolStripMenuItem();
        _debugMenu            = new ToolStripMenuItem();
        _statusStrip          = new StatusStrip();
        _statusLabel          = new ToolStripStatusLabel();
        _toolStrip            = new ToolStrip();
        _playButton           = new ToolStripButton();
        _pauseButton          = new ToolStripButton();
        _stopButton           = new ToolStripButton();
        _toolStripSeparator1  = new ToolStripSeparator();
        _selectModeButton     = new ToolStripButton();
        _moveModeButton       = new ToolStripButton();
        _rotateModeButton     = new ToolStripButton();
        _scaleModeButton      = new ToolStripButton();
        _mainSplit            = new SplitContainer();
        _outerSplit           = new SplitContainer();
        _hierarchyPanel       = new SceneHierarchyPanel();
        _innerSplit           = new SplitContainer();
        _viewportArea         = new Panel();
        _viewport             = new MonoGameControl();
        _inspectorPanel       = new InspectorPanel();
        _bottomTabControl     = new TabControl();
        _assetsTab            = new TabPage();
        _assetBrowserPanel    = new AssetBrowserPanel();
        _consoleTab           = new TabPage();
        _consolePanel         = new ConsolePanel();

        ((System.ComponentModel.ISupportInitialize)_mainSplit).BeginInit();
        _mainSplit.Panel1.SuspendLayout();
        _mainSplit.Panel2.SuspendLayout();
        _mainSplit.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_outerSplit).BeginInit();
        _outerSplit.Panel1.SuspendLayout();
        _outerSplit.Panel2.SuspendLayout();
        _outerSplit.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_innerSplit).BeginInit();
        _innerSplit.Panel1.SuspendLayout();
        _innerSplit.Panel2.SuspendLayout();
        _innerSplit.SuspendLayout();
        _viewportArea.SuspendLayout();
        _assetsTab.SuspendLayout();
        _consoleTab.SuspendLayout();
        SuspendLayout();

        // _mainMenuStrip
        _mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _fileMenu, _editMenu, _viewMenu, _projectMenu, _debugMenu });
        _mainMenuStrip.Location = new System.Drawing.Point(0, 0);
        _mainMenuStrip.Name = "_mainMenuStrip";
        _mainMenuStrip.Size = new System.Drawing.Size(1280, 24);
        _mainMenuStrip.TabIndex = 0;
        _mainMenuStrip.Text = "_mainMenuStrip";

        // _fileMenu
        _fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { _newProjectItem, _openProjectItem, _fileSeparator, _exitItem });
        _fileMenu.Name = "_fileMenu";
        _fileMenu.Size = new System.Drawing.Size(37, 20);
        _fileMenu.Text = "File";

        // _newProjectItem
        _newProjectItem.Name = "_newProjectItem";
        _newProjectItem.Size = new System.Drawing.Size(163, 22);
        _newProjectItem.Text = "New Project...";
        _newProjectItem.Click += OnFileNewProjectClick;

        // _openProjectItem
        _openProjectItem.Name = "_openProjectItem";
        _openProjectItem.Size = new System.Drawing.Size(163, 22);
        _openProjectItem.Text = "Open Project...";
        _openProjectItem.Click += OnFileOpenProjectClick;

        // _fileSeparator
        _fileSeparator.Name = "_fileSeparator";
        _fileSeparator.Size = new System.Drawing.Size(160, 6);

        // _exitItem
        _exitItem.Name = "_exitItem";
        _exitItem.Size = new System.Drawing.Size(163, 22);
        _exitItem.Text = "Exit";
        _exitItem.Click += OnFileExitClick;

        // _editMenu
        _editMenu.Name = "_editMenu";
        _editMenu.Size = new System.Drawing.Size(39, 20);
        _editMenu.Text = "Edit";

        // _viewMenu
        _viewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { _viewHierarchyMenuItem, _viewInspectorMenuItem, _viewAssetBrowserMenuItem, _viewConsoleMenuItem });
        _viewMenu.Name = "_viewMenu";
        _viewMenu.Size = new System.Drawing.Size(44, 20);
        _viewMenu.Text = "View";

        // _viewHierarchyMenuItem
        _viewHierarchyMenuItem.CheckOnClick = true;
        _viewHierarchyMenuItem.Checked = true;
        _viewHierarchyMenuItem.CheckState = CheckState.Checked;
        _viewHierarchyMenuItem.Name = "_viewHierarchyMenuItem";
        _viewHierarchyMenuItem.Size = new System.Drawing.Size(163, 22);
        _viewHierarchyMenuItem.Text = "Hierarchy";
        _viewHierarchyMenuItem.Click += OnViewMenuItemClick;

        // _viewInspectorMenuItem
        _viewInspectorMenuItem.CheckOnClick = true;
        _viewInspectorMenuItem.Checked = true;
        _viewInspectorMenuItem.CheckState = CheckState.Checked;
        _viewInspectorMenuItem.Name = "_viewInspectorMenuItem";
        _viewInspectorMenuItem.Size = new System.Drawing.Size(163, 22);
        _viewInspectorMenuItem.Text = "Inspector";
        _viewInspectorMenuItem.Click += OnViewMenuItemClick;

        // _viewAssetBrowserMenuItem
        _viewAssetBrowserMenuItem.CheckOnClick = true;
        _viewAssetBrowserMenuItem.Checked = true;
        _viewAssetBrowserMenuItem.CheckState = CheckState.Checked;
        _viewAssetBrowserMenuItem.Name = "_viewAssetBrowserMenuItem";
        _viewAssetBrowserMenuItem.Size = new System.Drawing.Size(163, 22);
        _viewAssetBrowserMenuItem.Text = "Asset Browser";
        _viewAssetBrowserMenuItem.Click += OnViewMenuItemClick;

        // _viewConsoleMenuItem
        _viewConsoleMenuItem.CheckOnClick = true;
        _viewConsoleMenuItem.Checked = true;
        _viewConsoleMenuItem.CheckState = CheckState.Checked;
        _viewConsoleMenuItem.Name = "_viewConsoleMenuItem";
        _viewConsoleMenuItem.Size = new System.Drawing.Size(163, 22);
        _viewConsoleMenuItem.Text = "Console";
        _viewConsoleMenuItem.Click += OnViewMenuItemClick;

        // _projectMenu
        _projectMenu.Name = "_projectMenu";
        _projectMenu.Size = new System.Drawing.Size(56, 20);
        _projectMenu.Text = "Project";

        // _debugMenu
        _debugMenu.Name = "_debugMenu";
        _debugMenu.Size = new System.Drawing.Size(54, 20);
        _debugMenu.Text = "Debug";

        // _statusStrip
        _statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _statusLabel });
        _statusStrip.Location = new System.Drawing.Point(0, 778);
        _statusStrip.Name = "_statusStrip";
        _statusStrip.Size = new System.Drawing.Size(1280, 22);
        _statusStrip.TabIndex = 2;

        // _statusLabel
        _statusLabel.Name = "_statusLabel";
        _statusLabel.Size = new System.Drawing.Size(43, 17);
        _statusLabel.Text = "Editing";

        // _toolStrip
        _toolStrip.AutoSize = false;
        _toolStrip.Dock = DockStyle.Left;
        _toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        _toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _playButton, _pauseButton, _stopButton, _toolStripSeparator1, _selectModeButton, _moveModeButton, _rotateModeButton, _scaleModeButton });
        _toolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
        _toolStrip.Location = new System.Drawing.Point(0, 0);
        _toolStrip.Name = "_toolStrip";
        _toolStrip.Size = new System.Drawing.Size(90, 730);
        _toolStrip.TabIndex = 0;
        _toolStrip.Text = "_toolStrip";

        // _playButton
        _playButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _playButton.Name = "_playButton";
        _playButton.Size = new System.Drawing.Size(88, 19);
        _playButton.Text = "▶ Play";
        _playButton.ToolTipText = "Play (F5)";
        _playButton.Click += OnPlayClick;

        // _pauseButton
        _pauseButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _pauseButton.Enabled = false;
        _pauseButton.Name = "_pauseButton";
        _pauseButton.Size = new System.Drawing.Size(88, 19);
        _pauseButton.Text = "⏸ Pause";
        _pauseButton.ToolTipText = "Pause";
        _pauseButton.Click += OnPauseClick;

        // _stopButton
        _stopButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _stopButton.Enabled = false;
        _stopButton.Name = "_stopButton";
        _stopButton.Size = new System.Drawing.Size(88, 19);
        _stopButton.Text = "■ Stop";
        _stopButton.ToolTipText = "Stop";
        _stopButton.Click += OnStopClick;

        // _toolStripSeparator1
        _toolStripSeparator1.Name = "_toolStripSeparator1";
        _toolStripSeparator1.Size = new System.Drawing.Size(88, 6);

        // _selectModeButton
        _selectModeButton.CheckOnClick = true;
        _selectModeButton.Checked = true;
        _selectModeButton.CheckState = CheckState.Checked;
        _selectModeButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _selectModeButton.Name = "_selectModeButton";
        _selectModeButton.Size = new System.Drawing.Size(88, 19);
        _selectModeButton.Text = "Q Select";
        _selectModeButton.ToolTipText = "Select (Q)";
        _selectModeButton.Click += OnGizmoModeClick;

        // _moveModeButton
        _moveModeButton.CheckOnClick = true;
        _moveModeButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _moveModeButton.Name = "_moveModeButton";
        _moveModeButton.Size = new System.Drawing.Size(88, 19);
        _moveModeButton.Text = "W Move";
        _moveModeButton.ToolTipText = "Move (W)";
        _moveModeButton.Click += OnGizmoModeClick;

        // _rotateModeButton
        _rotateModeButton.CheckOnClick = true;
        _rotateModeButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _rotateModeButton.Name = "_rotateModeButton";
        _rotateModeButton.Size = new System.Drawing.Size(88, 19);
        _rotateModeButton.Text = "E Rotate";
        _rotateModeButton.ToolTipText = "Rotate (E)";
        _rotateModeButton.Click += OnGizmoModeClick;

        // _scaleModeButton
        _scaleModeButton.CheckOnClick = true;
        _scaleModeButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _scaleModeButton.Name = "_scaleModeButton";
        _scaleModeButton.Size = new System.Drawing.Size(88, 19);
        _scaleModeButton.Text = "R Scale";
        _scaleModeButton.ToolTipText = "Scale (R)";
        _scaleModeButton.Click += OnGizmoModeClick;

        // _hierarchyPanel
        _hierarchyPanel.Dock = DockStyle.Fill;
        _hierarchyPanel.Location = new System.Drawing.Point(0, 0);
        _hierarchyPanel.Name = "_hierarchyPanel";
        _hierarchyPanel.Size = new System.Drawing.Size(220, 554);
        _hierarchyPanel.TabIndex = 0;

        // _viewport
        _viewport.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
        _viewport.Dock = DockStyle.Fill;
        _viewport.Location = new System.Drawing.Point(90, 0);
        _viewport.Name = "_viewport";
        _viewport.Size = new System.Drawing.Size(686, 554);
        _viewport.TabIndex = 1;

        // _viewportArea — toolstrip added first (Left), viewport added last (Fill)
        _viewportArea.Controls.Add(_viewport);
        _viewportArea.Controls.Add(_toolStrip);
        _viewportArea.Dock = DockStyle.Fill;
        _viewportArea.Location = new System.Drawing.Point(0, 0);
        _viewportArea.Name = "_viewportArea";
        _viewportArea.Size = new System.Drawing.Size(776, 554);
        _viewportArea.TabIndex = 0;

        // _inspectorPanel
        _inspectorPanel.Dock = DockStyle.Fill;
        _inspectorPanel.Location = new System.Drawing.Point(0, 0);
        _inspectorPanel.Name = "_inspectorPanel";
        _inspectorPanel.Size = new System.Drawing.Size(276, 554);
        _inspectorPanel.TabIndex = 0;

        // _innerSplit — Panel1: viewport area | Panel2: inspector
        _innerSplit.Dock = DockStyle.Fill;
        _innerSplit.Location = new System.Drawing.Point(0, 0);
        _innerSplit.Name = "_innerSplit";
        _innerSplit.Orientation = Orientation.Vertical;
        _innerSplit.Panel1.Controls.Add(_viewportArea);
        _innerSplit.Panel2.Controls.Add(_inspectorPanel);
        _innerSplit.Size = new System.Drawing.Size(1056, 554);
        _innerSplit.SplitterDistance = 776;
        _innerSplit.TabIndex = 0;

        // _outerSplit — Panel1: hierarchy | Panel2: innerSplit
        _outerSplit.Dock = DockStyle.Fill;
        _outerSplit.Location = new System.Drawing.Point(0, 0);
        _outerSplit.Name = "_outerSplit";
        _outerSplit.Orientation = Orientation.Vertical;
        _outerSplit.Panel1.Controls.Add(_hierarchyPanel);
        _outerSplit.Panel2.Controls.Add(_innerSplit);
        _outerSplit.Size = new System.Drawing.Size(1280, 554);
        _outerSplit.SplitterDistance = 220;
        _outerSplit.TabIndex = 0;

        // _assetBrowserPanel
        _assetBrowserPanel.Dock = DockStyle.Fill;
        _assetBrowserPanel.Location = new System.Drawing.Point(0, 0);
        _assetBrowserPanel.Name = "_assetBrowserPanel";
        _assetBrowserPanel.Size = new System.Drawing.Size(1280, 172);
        _assetBrowserPanel.TabIndex = 0;

        // _assetsTab
        _assetsTab.Controls.Add(_assetBrowserPanel);
        _assetsTab.Location = new System.Drawing.Point(4, 24);
        _assetsTab.Name = "_assetsTab";
        _assetsTab.Padding = new System.Windows.Forms.Padding(0);
        _assetsTab.Size = new System.Drawing.Size(1280, 172);
        _assetsTab.TabIndex = 0;
        _assetsTab.Text = "Assets";

        // _consolePanel
        _consolePanel.Dock = DockStyle.Fill;
        _consolePanel.Location = new System.Drawing.Point(0, 0);
        _consolePanel.Name = "_consolePanel";
        _consolePanel.Size = new System.Drawing.Size(1280, 172);
        _consolePanel.TabIndex = 0;

        // _consoleTab
        _consoleTab.Controls.Add(_consolePanel);
        _consoleTab.Location = new System.Drawing.Point(4, 24);
        _consoleTab.Name = "_consoleTab";
        _consoleTab.Padding = new System.Windows.Forms.Padding(0);
        _consoleTab.Size = new System.Drawing.Size(1280, 172);
        _consoleTab.TabIndex = 1;
        _consoleTab.Text = "Console";

        // _bottomTabControl
        _bottomTabControl.Controls.Add(_assetsTab);
        _bottomTabControl.Controls.Add(_consoleTab);
        _bottomTabControl.Dock = DockStyle.Fill;
        _bottomTabControl.Location = new System.Drawing.Point(0, 0);
        _bottomTabControl.Name = "_bottomTabControl";
        _bottomTabControl.SelectedIndex = 0;
        _bottomTabControl.Size = new System.Drawing.Size(1280, 200);
        _bottomTabControl.TabIndex = 0;

        // _mainSplit — Panel1: outerSplit (top) | Panel2: bottomTabControl (bottom)
        _mainSplit.Dock = DockStyle.Fill;
        _mainSplit.Location = new System.Drawing.Point(0, 24);
        _mainSplit.Name = "_mainSplit";
        _mainSplit.Orientation = Orientation.Horizontal;
        _mainSplit.Panel1.Controls.Add(_outerSplit);
        _mainSplit.Panel2.Controls.Add(_bottomTabControl);
        _mainSplit.Size = new System.Drawing.Size(1280, 754);
        _mainSplit.SplitterDistance = 554;
        _mainSplit.TabIndex = 1;

        // EditorForm
        AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1280, 800);
        Controls.Add(_mainSplit);
        Controls.Add(_statusStrip);
        Controls.Add(_mainMenuStrip);
        Font = new System.Drawing.Font("Segoe UI", 9f);
        MainMenuStrip = _mainMenuStrip;
        MinimumSize = new System.Drawing.Size(800, 600);
        Name = "EditorForm";
        Text = "MonoGame Editor";
        WindowState = FormWindowState.Maximized;

        _mainSplit.Panel1.ResumeLayout(false);
        _mainSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_mainSplit).EndInit();
        _mainSplit.ResumeLayout(false);
        _outerSplit.Panel1.ResumeLayout(false);
        _outerSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_outerSplit).EndInit();
        _outerSplit.ResumeLayout(false);
        _innerSplit.Panel1.ResumeLayout(false);
        _innerSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_innerSplit).EndInit();
        _innerSplit.ResumeLayout(false);
        _viewportArea.ResumeLayout(false);
        _viewportArea.PerformLayout();
        _assetsTab.ResumeLayout(false);
        _consoleTab.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
