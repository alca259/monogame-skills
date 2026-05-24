namespace MonoGame.Editor.WinForms.Panels;

/// <summary>Two-pane asset browser: folder tree on the left, file contents on the right.</summary>
public sealed class AssetBrowserPanel : UserControl
{
    private const int DefaultSplitterDistance = 180;
    private const int MinPanelSize = 80;

    private readonly SplitContainer _split;
    private readonly TreeView _folderTree;
    private readonly ListView _contentView;

    public AssetBrowserPanel()
    {
        _folderTree = new TreeView
        {
            Dock = DockStyle.Fill,
            HideSelection = false,
            ShowLines = true,
            ShowPlusMinus = true,
            BorderStyle = BorderStyle.None,
        };

        _contentView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false,
            ShowItemToolTips = true,
            BorderStyle = BorderStyle.None,
        };
        _contentView.Columns.Add("Name", 200);
        _contentView.Columns.Add("Type", 80);

        _split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = DefaultSplitterDistance,
            Panel1MinSize = MinPanelSize,
            Panel2MinSize = MinPanelSize,
        };
        _split.Panel1.Controls.Add(_folderTree);
        _split.Panel2.Controls.Add(_contentView);

        Controls.Add(_split);

        _folderTree.AfterSelect += OnFolderSelected;
        _folderTree.BeforeExpand += OnBeforeExpand;
    }

    /// <summary>Gets or sets the pixel distance of the folder/content splitter.</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public int SplitterDistance
    {
        get => _split.SplitterDistance;
        set { if (value >= MinPanelSize) _split.SplitterDistance = value; }
    }

    /// <summary>Loads the directory tree starting from <paramref name="rootPath"/>.</summary>
    public void SetRootDirectory(string rootPath)
    {
        if (!Directory.Exists(rootPath)) return;

        _folderTree.BeginUpdate();
        _folderTree.Nodes.Clear();
        _contentView.Items.Clear();

        string displayName = Path.GetFileName(rootPath);
        if (string.IsNullOrEmpty(displayName)) displayName = rootPath;

        TreeNode root = CreateFolderNode(displayName, rootPath);
        _folderTree.Nodes.Add(root);
        root.Expand();

        _folderTree.EndUpdate();
    }

    private static TreeNode CreateFolderNode(string label, string path)
    {
        TreeNode node = new TreeNode(label) { Tag = path };
        try
        {
            if (Directory.GetDirectories(path).Length > 0)
                node.Nodes.Add(new TreeNode()); // lazy-load placeholder
        }
        catch (UnauthorizedAccessException) { }
        return node;
    }

    private void OnBeforeExpand(object? sender, TreeViewCancelEventArgs e)
    {
        if (e.Node is not { Tag: string path }) return;

        // Only replace the lazy placeholder (single node with null Tag)
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

    private void ShowFolderContents(string folderPath)
    {
        _contentView.BeginUpdate();
        _contentView.Items.Clear();
        try
        {
            string[] files = Directory.GetFiles(folderPath);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileNameWithoutExtension(files[i]);
                string ext = Path.GetExtension(files[i]).TrimStart('.');
                ListViewItem item = new ListViewItem(name) { Tag = files[i] };
                item.SubItems.Add(ext.ToUpperInvariant());
                _contentView.Items.Add(item);
            }
        }
        catch (UnauthorizedAccessException) { }
        _contentView.EndUpdate();
    }
}
