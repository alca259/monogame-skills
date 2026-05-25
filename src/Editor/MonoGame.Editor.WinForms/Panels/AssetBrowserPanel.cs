using MonoGame.Editor.Core.Events;
using MonoGame.Editor.Core.Project;

namespace MonoGame.Editor.WinForms.Panels;

/// <summary>
/// Two-pane asset browser: folder tree on the left, file list with type icons and
/// a metadata preview on the right. Integrates with <see cref="ContentWatcher"/> via
/// the editor event bus.
/// </summary>
public sealed class AssetBrowserPanel : UserControl
{
    #region Constants

    private const int DefaultSplitterDistance = 180;
    private const int MinPanelSize = 80;
    private const int PreviewHeight = 130;

    #endregion

    #region Icon indices (ImageList)

    private const int IconUnknown   = 0;
    private const int IconTexture   = 1;
    private const int IconAudio     = 2;
    private const int IconFont      = 3;
    private const int IconTiledMap  = 4;
    private const int IconScene     = 5;
    private const int IconPrefab    = 6;
    private const int IconParticles = 7;
    private const int IconAnimation = 8;
    private const int IconInputMap  = 9;
    private const int IconScript    = 10;
    private const int IconFolder    = 11;

    #endregion

    #region Fields

    private EditorContext? _context;
    private string _contentRoot = string.Empty;

    private readonly SplitContainer _outerSplit;
    private readonly TreeView       _folderTree;
    private readonly SplitContainer _rightSplit;
    private readonly ListView       _contentView;
    private readonly Panel          _previewPanel;
    private readonly PictureBox     _previewImage;
    private readonly Label          _previewInfo;
    private readonly ImageList      _typeIcons;

    #endregion

    #region Constructor

    /// <summary>Initializes the panel and builds the UI.</summary>
    public AssetBrowserPanel()
    {
        _typeIcons = BuildIconList();

        // ── Folder tree (left) ───────────────────────────────────────────
        _folderTree = new TreeView
        {
            Dock        = DockStyle.Fill,
            HideSelection = false,
            ShowLines   = true,
            ShowPlusMinus = true,
            BorderStyle = BorderStyle.None,
            ImageList   = _typeIcons,
        };

        // ── File list (top-right) ─────────────────────────────────────────
        _contentView = new ListView
        {
            Dock            = DockStyle.Fill,
            View            = View.Details,
            FullRowSelect   = true,
            MultiSelect     = false,
            ShowItemToolTips = true,
            BorderStyle     = BorderStyle.None,
            SmallImageList  = _typeIcons,
            AllowDrop       = true,
        };
        _contentView.Columns.Add("Name", 200);
        _contentView.Columns.Add("Type", 80);
        _contentView.Columns.Add("Size", 70);

        // ── Preview (bottom-right) ────────────────────────────────────────
        _previewImage = new PictureBox
        {
            Dock        = DockStyle.Left,
            Width       = 120,
            Height      = PreviewHeight,
            SizeMode    = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.None,
        };
        _previewInfo = new Label
        {
            Dock      = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.TopLeft,
            Padding   = new Padding(4),
            Font      = new System.Drawing.Font("Segoe UI", 8.5f),
        };

        _previewPanel = new Panel
        {
            Dock   = DockStyle.Fill,
            Height = PreviewHeight,
        };
        _previewPanel.Controls.Add(_previewInfo);
        _previewPanel.Controls.Add(_previewImage);

        // ── Right split (list / preview) ──────────────────────────────────
        _rightSplit = new SplitContainer
        {
            Dock            = DockStyle.Fill,
            Orientation     = Orientation.Horizontal,
            SplitterDistance = 200,
            Panel1MinSize   = 80,
            Panel2MinSize   = PreviewHeight,
            FixedPanel      = FixedPanel.Panel2,
        };
        _rightSplit.Panel1.Controls.Add(_contentView);
        _rightSplit.Panel2.Controls.Add(_previewPanel);

        // ── Outer split (tree / right) ────────────────────────────────────
        _outerSplit = new SplitContainer
        {
            Dock             = DockStyle.Fill,
            Orientation      = Orientation.Vertical,
            SplitterDistance = DefaultSplitterDistance,
            Panel1MinSize    = MinPanelSize,
            Panel2MinSize    = MinPanelSize,
        };
        _outerSplit.Panel1.Controls.Add(_folderTree);
        _outerSplit.Panel2.Controls.Add(_rightSplit);

        Controls.Add(_outerSplit);
        AllowDrop = true;

        // Wire up
        _folderTree.AfterSelect    += OnFolderSelected;
        _folderTree.BeforeExpand   += OnBeforeExpand;
        _contentView.SelectedIndexChanged += OnContentSelectionChanged;
        _contentView.ItemDrag      += OnItemDrag;
        _contentView.DragEnter     += OnDragEnter;
        _contentView.DragDrop      += OnDragDrop;
        DragEnter                  += OnDragEnter;
        DragDrop                   += OnDragDrop;
    }

    #endregion

    #region Public API

    /// <summary>Gets or sets the folder/content splitter distance in pixels.</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int SplitterDistance
    {
        get => _outerSplit.SplitterDistance;
        set { if (value >= MinPanelSize) _outerSplit.SplitterDistance = value; }
    }

    /// <summary>
    /// Subscribes to the event bus to refresh the panel when assets change or a project opens.
    /// Must be called once after the parent form is constructed.
    /// </summary>
    public void Initialize(EditorContext context)
    {
        _context = context;
        _context.EventBus.Subscribe<AssetImportedEvent>(OnAssetImported);
        _context.EventBus.Subscribe<ProjectOpenedEvent>(OnProjectOpened);
    }

    /// <summary>Loads the directory tree rooted at <paramref name="rootPath"/>.</summary>
    public void SetRootDirectory(string rootPath)
    {
        _contentRoot = Directory.Exists(rootPath) ? rootPath : string.Empty;

        _folderTree.BeginUpdate();
        _folderTree.Nodes.Clear();
        _contentView.Items.Clear();
        ClearPreview();

        if (string.IsNullOrEmpty(_contentRoot))
        {
            _folderTree.EndUpdate();
            return;
        }

        string displayName = Path.GetFileName(rootPath);
        if (string.IsNullOrEmpty(displayName)) displayName = rootPath;

        TreeNode root = CreateFolderNode(displayName, rootPath);
        _folderTree.Nodes.Add(root);
        root.Expand();

        _folderTree.EndUpdate();
    }

    /// <summary>Re-reads the currently selected folder, refreshing the file list.</summary>
    public new void Refresh()
    {
        if (_folderTree.SelectedNode?.Tag is string path)
            ShowFolderContents(path);
    }

    #endregion

    #region Event bus handlers

    private void OnProjectOpened(ProjectOpenedEvent evt)
    {
        if (InvokeRequired) { BeginInvoke(() => OnProjectOpened(evt)); return; }
        SetRootDirectory(evt.Project?.ContentPath ?? string.Empty);
    }

    private void OnAssetImported(AssetImportedEvent evt)
    {
        if (InvokeRequired) { BeginInvoke(() => OnAssetImported(evt)); return; }

        // Only refresh if the changed file is inside the current folder view
        if (_folderTree.SelectedNode?.Tag is string currentFolder)
        {
            string dir = Path.GetDirectoryName(evt.Asset.AbsolutePath) ?? string.Empty;
            if (string.Equals(dir, currentFolder, StringComparison.OrdinalIgnoreCase))
                ShowFolderContents(currentFolder);
        }
    }

    #endregion

    #region Folder tree — lazy load

    private static TreeNode CreateFolderNode(string label, string path)
    {
        TreeNode node = new(label)
        {
            Tag          = path,
            ImageIndex   = IconFolder,
            SelectedImageIndex = IconFolder,
        };
        try
        {
            if (Directory.GetDirectories(path).Length > 0)
                node.Nodes.Add(new TreeNode()); // lazy placeholder
        }
        catch (UnauthorizedAccessException) { }
        return node;
    }

    private void OnBeforeExpand(object? sender, TreeViewCancelEventArgs e)
    {
        if (e.Node is not { Tag: string path }) return;
        if (e.Node.Nodes.Count != 1 || e.Node.Nodes[0].Tag is not null) return;

        _folderTree.BeginUpdate();
        e.Node.Nodes.Clear();
        try
        {
            string[] dirs = Directory.GetDirectories(path);
            Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < dirs.Length; i++)
            {
                string name = Path.GetFileName(dirs[i]);
                if (!string.IsNullOrEmpty(name))
                    e.Node.Nodes.Add(CreateFolderNode(name, dirs[i]));
            }
        }
        catch (UnauthorizedAccessException) { }
        _folderTree.EndUpdate();
    }

    private void OnFolderSelected(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is not string path) return;
        ShowFolderContents(path);
    }

    #endregion

    #region Content list

    private void ShowFolderContents(string folderPath)
    {
        _contentView.BeginUpdate();
        _contentView.Items.Clear();
        ClearPreview();

        try
        {
            string[] files = Directory.GetFiles(folderPath);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < files.Length; i++)
            {
                AssetInfo info = AssetClassifier.CreateInfo(files[i], _contentRoot);
                ListViewItem item = new(info.Name)
                {
                    Tag        = info,
                    ImageIndex = TypeToIconIndex(info.Type),
                    ToolTipText = info.RelativePath,
                };
                item.SubItems.Add(TypeLabel(info.Type));
                item.SubItems.Add(FormatSize(info.SizeBytes));
                _contentView.Items.Add(item);
            }
        }
        catch (UnauthorizedAccessException) { }

        _contentView.EndUpdate();
    }

    private void OnContentSelectionChanged(object? sender, EventArgs e)
    {
        if (_contentView.SelectedItems.Count == 0)
        {
            ClearPreview();
            return;
        }

        if (_contentView.SelectedItems[0].Tag is not AssetInfo info)
        {
            ClearPreview();
            return;
        }

        ShowPreview(info);
    }

    #endregion

    #region Preview

    private void ShowPreview(AssetInfo info)
    {
        _previewInfo.Text = BuildPreviewText(info);

        if (info.Type == AssetType.Texture && File.Exists(info.AbsolutePath))
        {
            try
            {
                _previewImage.Image?.Dispose();
                _previewImage.Image = System.Drawing.Image.FromFile(info.AbsolutePath);

                string dims = string.Empty;
                if (_previewImage.Image is System.Drawing.Bitmap bmp)
                    dims = $"\nDimensions: {bmp.Width} × {bmp.Height} px";

                _previewInfo.Text = BuildPreviewText(info) + dims;
            }
            catch
            {
                _previewImage.Image = null;
            }
        }
        else
        {
            _previewImage.Image?.Dispose();
            _previewImage.Image = null;
        }
    }

    private void ClearPreview()
    {
        _previewImage.Image?.Dispose();
        _previewImage.Image = null;
        _previewInfo.Text   = string.Empty;
    }

    private static string BuildPreviewText(AssetInfo info)
        => $"{info.Name}{info.Extension}\nType: {TypeLabel(info.Type)}\nSize: {FormatSize(info.SizeBytes)}\n{info.RelativePath}";

    #endregion

    #region Drag & drop — from ListView to other panels / viewport

    private void OnItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (e.Item is ListViewItem { Tag: AssetInfo info })
            _contentView.DoDragDrop(info.AbsolutePath, DragDropEffects.Copy);
    }

    #endregion

    #region Drag & drop — from Windows Explorer into panel

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] droppedFiles) return;
        if (string.IsNullOrEmpty(_contentRoot)) return;

        string targetDir = _folderTree.SelectedNode?.Tag as string ?? _contentRoot;

        for (int i = 0; i < droppedFiles.Length; i++)
        {
            string src = droppedFiles[i];
            if (!File.Exists(src)) continue;

            string dest = Path.Combine(targetDir, Path.GetFileName(src));
            try
            {
                File.Copy(src, dest, overwrite: true);
                AssetInfo info = AssetClassifier.CreateInfo(dest, _contentRoot);
                _context?.EventBus.Publish(new AssetImportedEvent(info));
            }
            catch (IOException) { }
        }

        ShowFolderContents(targetDir);
    }

    #endregion

    #region Helpers

    private static int TypeToIconIndex(AssetType type) => type switch
    {
        AssetType.Texture   => IconTexture,
        AssetType.Audio     => IconAudio,
        AssetType.Font      => IconFont,
        AssetType.TiledMap  => IconTiledMap,
        AssetType.Scene     => IconScene,
        AssetType.Prefab    => IconPrefab,
        AssetType.Particles => IconParticles,
        AssetType.Animation => IconAnimation,
        AssetType.InputMap  => IconInputMap,
        AssetType.Script    => IconScript,
        _                   => IconUnknown,
    };

    private static string TypeLabel(AssetType type) => type switch
    {
        AssetType.Texture   => "Texture",
        AssetType.Audio     => "Audio",
        AssetType.Font      => "Font",
        AssetType.TiledMap  => "Tiled Map",
        AssetType.Scene     => "Scene",
        AssetType.Prefab    => "Prefab",
        AssetType.Particles => "Particles",
        AssetType.Animation => "Animation",
        AssetType.InputMap  => "Input Map",
        AssetType.Script    => "Script",
        _                   => "Unknown",
    };

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1_048_576 => $"{bytes / 1_048_576.0:F1} MB",
        >= 1_024     => $"{bytes / 1_024.0:F1} KB",
        _            => $"{bytes} B",
    };

    /// <summary>Builds the icon image list with 16×16 colored squares per asset type.</summary>
    private static ImageList BuildIconList()
    {
        ImageList list = new() { ImageSize = new System.Drawing.Size(16, 16) };
        list.Images.Add(MakeColorSquare(System.Drawing.Color.Gray));           // 0 Unknown
        list.Images.Add(MakeColorSquare(System.Drawing.Color.MediumPurple));   // 1 Texture
        list.Images.Add(MakeColorSquare(System.Drawing.Color.DodgerBlue));     // 2 Audio
        list.Images.Add(MakeColorSquare(System.Drawing.Color.Goldenrod));      // 3 Font
        list.Images.Add(MakeColorSquare(System.Drawing.Color.ForestGreen));    // 4 TiledMap
        list.Images.Add(MakeColorSquare(System.Drawing.Color.CornflowerBlue)); // 5 Scene
        list.Images.Add(MakeColorSquare(System.Drawing.Color.Orange));         // 6 Prefab
        list.Images.Add(MakeColorSquare(System.Drawing.Color.HotPink));        // 7 Particles
        list.Images.Add(MakeColorSquare(System.Drawing.Color.LimeGreen));      // 8 Animation
        list.Images.Add(MakeColorSquare(System.Drawing.Color.Tomato));         // 9 InputMap
        list.Images.Add(MakeColorSquare(System.Drawing.Color.Teal));           // 10 Script
        list.Images.Add(MakeColorSquare(System.Drawing.Color.SaddleBrown));    // 11 Folder
        return list;
    }

    private static System.Drawing.Bitmap MakeColorSquare(System.Drawing.Color color)
    {
        System.Drawing.Bitmap bmp = new(16, 16);
        using System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
        g.Clear(color);
        return bmp;
    }

    #endregion

    #region Dispose

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_context is not null)
            {
                _context.EventBus.Unsubscribe<AssetImportedEvent>(OnAssetImported);
                _context.EventBus.Unsubscribe<ProjectOpenedEvent>(OnProjectOpened);
            }
            _previewImage.Image?.Dispose();
            _typeIcons.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}
