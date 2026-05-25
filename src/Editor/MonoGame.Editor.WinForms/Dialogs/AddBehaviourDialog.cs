namespace MonoGame.Editor.WinForms.Dialogs;

/// <summary>
/// Modal dialog that lets the user pick a <c>GameBehaviour</c> subclass from
/// the <see cref="GameObjectRegistry"/> to attach to a game object.
/// Types are grouped by namespace in a <see cref="TreeView"/>. A "Create New..." button
/// opens <see cref="NewBehaviourDialog"/> to scaffold a new behaviour.
/// </summary>
public sealed class AddBehaviourDialog : Form
{
    private readonly TextBox          _searchBox;
    private readonly TreeView         _tree;
    private readonly Button           _okButton;
    private readonly Button           _cancelButton;
    private readonly Button           _createNewButton;
    private readonly EditorProject?   _project;

    // Flat list of (namespace, shortName, fullName) for rebuild
    private readonly List<(string Ns, string Short, string Full)> _allTypes = [];

    /// <summary>Full type name selected by the user, or <c>null</c> if cancelled.</summary>
    public string? SelectedTypeName { get; private set; }

    /// <summary>Creates a new dialog populated from <paramref name="registry"/>.</summary>
    public AddBehaviourDialog(GameObjectRegistry registry, EditorProject? project = null)
    {
        _project = project;

        Text            = "Add Behaviour";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition   = FormStartPosition.CenterParent;
        ClientSize      = new System.Drawing.Size(340, 400);
        MaximizeBox     = false;
        MinimizeBox     = false;
        Font            = new System.Drawing.Font("Segoe UI", 9f);

        Label searchLabel = new()
        {
            Text     = "Search:",
            Location = new System.Drawing.Point(8, 10),
            AutoSize = true,
        };

        _searchBox = new TextBox
        {
            Location = new System.Drawing.Point(8, 28),
            Width    = 322,
        };

        _tree = new TreeView
        {
            Location      = new System.Drawing.Point(8, 56),
            Size          = new System.Drawing.Size(322, 264),
            HideSelection = false,
            ShowLines     = true,
            ShowPlusMinus = true,
            Sorted        = true,
        };

        _createNewButton = new Button
        {
            Text     = "Create New...",
            Location = new System.Drawing.Point(8, 328),
            Width    = 100,
        };
        _createNewButton.Click += OnCreateNewClick;

        _okButton = new Button
        {
            Text         = "OK",
            DialogResult = DialogResult.OK,
            Location     = new System.Drawing.Point(170, 328),
            Width        = 76,
        };
        _okButton.Click += OnOkClick;

        _cancelButton = new Button
        {
            Text         = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location     = new System.Drawing.Point(254, 328),
            Width        = 76,
        };

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        Controls.Add(searchLabel);
        Controls.Add(_searchBox);
        Controls.Add(_tree);
        Controls.Add(_createNewButton);
        Controls.Add(_okButton);
        Controls.Add(_cancelButton);

        // Populate data
        foreach (KeyValuePair<string, Type> kv in registry.RegisteredTypes)
        {
            string full  = kv.Key;
            string short_ = kv.Value.Name;
            string ns    = kv.Value.Namespace ?? string.Empty;
            _allTypes.Add((ns, short_, full));
        }
        _allTypes.Sort((a, b) =>
        {
            int nsComp = string.Compare(a.Ns, b.Ns, StringComparison.OrdinalIgnoreCase);
            return nsComp != 0 ? nsComp : string.Compare(a.Short, b.Short, StringComparison.OrdinalIgnoreCase);
        });

        RebuildTree(string.Empty);

        _searchBox.TextChanged += (_, _) => RebuildTree(_searchBox.Text);
        _tree.DoubleClick      += (_, _) => { if (GetSelectedTypeName() is not null) _okButton.PerformClick(); };
        _tree.AfterSelect      += (_, _) => _okButton.Enabled = GetSelectedTypeName() is not null;
        _okButton.Enabled = false;
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        SelectedTypeName = GetSelectedTypeName();
        if (SelectedTypeName is not null)
            DialogResult = DialogResult.OK;
    }

    private void OnCreateNewClick(object? sender, EventArgs e)
    {
        string gameSourcePath = _project?.GameSourcePath ?? string.Empty;
        using NewBehaviourDialog dlg = new(gameSourcePath);
        dlg.ShowDialog(this);
    }

    private string? GetSelectedTypeName()
    {
        TreeNode? node = _tree.SelectedNode;
        if (node is null || node.Level != 1) return null;
        return node.Tag as string;
    }

    private void RebuildTree(string filter)
    {
        _tree.BeginUpdate();
        _tree.Nodes.Clear();

        for (int i = 0; i < _allTypes.Count; i++)
        {
            (string ns, string shortName, string fullName) = _allTypes[i];
            if (!string.IsNullOrEmpty(filter) &&
                !shortName.Contains(filter, StringComparison.OrdinalIgnoreCase) &&
                !fullName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                continue;

            string nsKey = string.IsNullOrEmpty(ns) ? "(global)" : ns;
            TreeNode? nsNode = _tree.Nodes[nsKey];
            if (nsNode is null)
            {
                nsNode     = new TreeNode(nsKey) { Name = nsKey };
                _tree.Nodes.Add(nsNode);
            }

            TreeNode typeNode = new(shortName)
            {
                Tag         = fullName,
                ToolTipText = fullName,
            };
            nsNode.Nodes.Add(typeNode);
        }

        _tree.ExpandAll();
        _tree.EndUpdate();
        _okButton.Enabled = false;
    }
}
