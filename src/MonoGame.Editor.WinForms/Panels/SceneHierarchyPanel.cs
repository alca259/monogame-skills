namespace MonoGame.Editor.WinForms.Panels;

/// <summary>
/// Displays the scene's game object hierarchy in a <see cref="TreeView"/>.
/// Subscribes to <see cref="SceneLoadedEvent"/> and <see cref="GameObjectSelectedEvent"/> via the event bus.
/// All edits are routed through <see cref="CommandStack"/> for full undo/redo support.
/// </summary>
public sealed class SceneHierarchyPanel : UserControl
{
    #region Fields

    private EditorContext? _context;
    private bool _suppressSelectionEvent;
    private bool _suppressCheckEvent;
    private readonly List<EditorGameObject> _multiSelected = [];

    private readonly TreeView _tree;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _createEmptyItem;
    private readonly ToolStripMenuItem _createChildItem;
    private readonly ToolStripMenuItem _duplicateItem;
    private readonly ToolStripMenuItem _renameItem;
    private readonly ToolStripMenuItem _deleteItem;
    private readonly ToolStripMenuItem _setActiveItem;

    private Action<UndoPerformedEvent>? _onUndo;
    private Action<RedoPerformedEvent>? _onRedo;

    #endregion

    #region Constructor

    /// <summary>Creates the panel with a TreeView and context menu. Call <see cref="Initialize"/> to connect to the editor context.</summary>
    public SceneHierarchyPanel()
    {
        _createEmptyItem = new ToolStripMenuItem("Create Empty");
        _createChildItem = new ToolStripMenuItem("Create Child");
        _duplicateItem   = new ToolStripMenuItem("Duplicate\tCtrl+D");
        _renameItem      = new ToolStripMenuItem("Rename\tF2");
        _deleteItem      = new ToolStripMenuItem("Delete\tDel");
        _setActiveItem   = new ToolStripMenuItem("Toggle Active");

        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.AddRange(new ToolStripItem[]
        {
            _createEmptyItem,
            _createChildItem,
            new ToolStripSeparator(),
            _duplicateItem,
            new ToolStripSeparator(),
            _renameItem,
            _deleteItem,
            new ToolStripSeparator(),
            _setActiveItem,
        });

        _tree = new TreeView
        {
            Dock        = DockStyle.Fill,
            CheckBoxes  = true,
            AllowDrop   = true,
            LabelEdit   = true,
            HideSelection = false,
            FullRowSelect = true,
            BorderStyle = BorderStyle.None,
            ContextMenuStrip = _contextMenu,
        };

        Controls.Add(_tree);
        WireTreeEvents();
        WireMenuEvents();
    }

    #endregion

    #region Initialization

    /// <summary>Connects this panel to the editor context. Must be called before any scene is loaded.</summary>
    public void Initialize(EditorContext context)
    {
        _context = context;

        _onUndo = _ => RefreshTreeSafe();
        _onRedo = _ => RefreshTreeSafe();

        _context.EventBus.Subscribe<SceneLoadedEvent>(OnSceneLoaded);
        _context.EventBus.Subscribe<GameObjectSelectedEvent>(OnGameObjectSelected);
        _context.EventBus.Subscribe<UndoPerformedEvent>(_onUndo);
        _context.EventBus.Subscribe<RedoPerformedEvent>(_onRedo);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _context is not null)
        {
            _context.EventBus.Unsubscribe<SceneLoadedEvent>(OnSceneLoaded);
            _context.EventBus.Unsubscribe<GameObjectSelectedEvent>(OnGameObjectSelected);
            if (_onUndo is not null) _context.EventBus.Unsubscribe<UndoPerformedEvent>(_onUndo);
            if (_onRedo is not null) _context.EventBus.Unsubscribe<RedoPerformedEvent>(_onRedo);
        }
        base.Dispose(disposing);
    }

    #endregion

    #region Event bus handlers

    private void OnSceneLoaded(SceneLoadedEvent evt)
    {
        if (InvokeRequired) { BeginInvoke(() => OnSceneLoaded(evt)); return; }
        RebuildTree(evt.Scene);
    }

    private void OnGameObjectSelected(GameObjectSelectedEvent evt)
    {
        if (InvokeRequired) { BeginInvoke(() => OnGameObjectSelected(evt)); return; }
        _suppressSelectionEvent = true;
        try
        {
            _tree.SelectedNode = evt.GameObject is null ? null : FindNode(evt.GameObject);
        }
        finally
        {
            _suppressSelectionEvent = false;
        }
    }

    #endregion

    #region Tree building

    private void RebuildTree(EditorScene? scene)
    {
        _tree.BeginUpdate();
        _tree.Nodes.Clear();
        _multiSelected.Clear();

        if (scene is not null)
        {
            for (int i = 0; i < scene.RootGameObjects.Count; i++)
                _tree.Nodes.Add(BuildNode(scene.RootGameObjects[i]));
            _tree.ExpandAll();
        }

        _tree.EndUpdate();
    }

    private void RefreshTreeSafe()
    {
        if (InvokeRequired) { BeginInvoke(RefreshTreeSafe); return; }
        RebuildTree(_context?.ActiveScene);
    }

    private static TreeNode BuildNode(EditorGameObject obj)
    {
        TreeNode node = new TreeNode(obj.Name) { Tag = obj, Checked = obj.Active };
        for (int i = 0; i < obj.Children.Count; i++)
            node.Nodes.Add(BuildNode(obj.Children[i]));
        return node;
    }

    private TreeNode? FindNode(EditorGameObject obj) =>
        FindNodeIn(_tree.Nodes, obj);

    private static TreeNode? FindNodeIn(TreeNodeCollection nodes, EditorGameObject obj)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Tag == obj) return nodes[i];
            TreeNode? found = FindNodeIn(nodes[i].Nodes, obj);
            if (found is not null) return found;
        }
        return null;
    }

    #endregion

    #region Tree event wiring

    private void WireTreeEvents()
    {
        _tree.AfterSelect    += OnAfterSelect;
        _tree.NodeMouseClick += OnNodeMouseClick;
        _tree.AfterCheck     += OnAfterCheck;
        _tree.AfterLabelEdit += OnAfterLabelEdit;
        _tree.KeyDown        += OnTreeKeyDown;
        _tree.ItemDrag       += OnItemDrag;
        _tree.DragEnter      += (_, e) => e.Effect = DragDropEffects.Move;
        _tree.DragOver       += OnDragOver;
        _tree.DragDrop       += OnDragDrop;
    }

    private void WireMenuEvents()
    {
        _createEmptyItem.Click += OnCreateEmpty;
        _createChildItem.Click += OnCreateChild;
        _duplicateItem.Click   += OnDuplicate;
        _renameItem.Click      += (_, _) => _tree.SelectedNode?.BeginEdit();
        _deleteItem.Click      += OnDelete;
        _setActiveItem.Click   += OnToggleActive;
        _contextMenu.Opening   += OnContextMenuOpening;
    }

    #endregion

    #region Selection

    private void OnAfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (_suppressSelectionEvent) return;
        if (e.Node?.Tag is not EditorGameObject obj) return;
        if ((ModifierKeys & Keys.Control) == 0)
        {
            _multiSelected.Clear();
            _multiSelected.Add(obj);
            _context?.SetSelection(obj);
        }
    }

    private void OnNodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (_suppressSelectionEvent || e.Button != MouseButtons.Left) return;
        if (e.Node?.Tag is not EditorGameObject obj) return;

        if ((ModifierKeys & Keys.Control) != 0)
        {
            if (_multiSelected.Contains(obj))
                _multiSelected.Remove(obj);
            else
                _multiSelected.Add(obj);
            _context?.SetMultiSelection(_multiSelected);
        }
    }

    #endregion

    #region Checkbox (active toggle)

    private void OnAfterCheck(object? sender, TreeViewEventArgs e)
    {
        if (_suppressCheckEvent) return;
        if (e.Node?.Tag is not EditorGameObject obj) return;
        bool newActive = e.Node.Checked;
        if (obj.Active == newActive) return;
        bool oldActive = obj.Active;
        _context!.Commands.Execute(new SetPropertyCommand<bool>(
            "Set Active", oldActive, newActive, v => obj.Active = v));
    }

    #endregion

    #region Label edit (rename)

    private void OnAfterLabelEdit(object? sender, NodeLabelEditEventArgs e)
    {
        e.CancelEdit = true;
        if (e.Label is null || e.Node?.Tag is not EditorGameObject obj) return;
        string newName = e.Label.Trim();
        if (string.IsNullOrEmpty(newName) || newName == obj.Name) return;
        TreeNode node = e.Node;
        _context!.Commands.Execute(new RenameEntityCommand(obj, newName));
        node.Text = obj.Name;
    }

    #endregion

    #region Keyboard shortcuts

    private void OnTreeKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Delete:
                OnDelete(sender, e);
                e.Handled = true;
                break;
            case Keys.F2:
                _tree.SelectedNode?.BeginEdit();
                e.Handled = true;
                break;
            case Keys.D when e.Control:
                OnDuplicate(sender, e);
                e.Handled = true;
                break;
        }
    }

    #endregion

    #region Context menu operations

    private void OnContextMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        bool hasScene    = _context?.ActiveScene is not null;
        bool hasSelected = _tree.SelectedNode?.Tag is EditorGameObject;
        _createEmptyItem.Enabled = hasScene;
        _createChildItem.Enabled = hasSelected;
        _duplicateItem.Enabled   = hasSelected;
        _renameItem.Enabled      = hasSelected;
        _deleteItem.Enabled      = hasSelected;
        _setActiveItem.Enabled   = hasSelected;
    }

    private void OnCreateEmpty(object? sender, EventArgs e)
    {
        EditorScene? scene = _context?.ActiveScene;
        if (scene is null) return;
        EditorGameObject newObj = new() { Name = "GameObject" };
        _context!.Commands.Execute(new CreateEntityCommand(newObj, scene));
        RefreshTreeSafe();
        SelectAndEditNode(newObj);
    }

    private void OnCreateChild(object? sender, EventArgs e)
    {
        EditorScene? scene = _context?.ActiveScene;
        if (scene is null || _tree.SelectedNode?.Tag is not EditorGameObject parent) return;
        EditorGameObject newObj = new() { Name = "GameObject" };
        _context!.Commands.Execute(new CreateEntityCommand(newObj, scene, parent));
        RefreshTreeSafe();
        SelectAndEditNode(newObj);
    }

    private void OnDuplicate(object? sender, EventArgs e)
    {
        EditorScene? scene = _context?.ActiveScene;
        if (scene is null || _tree.SelectedNode?.Tag is not EditorGameObject source) return;
        EditorGameObject copy = DeepCopy(source);
        _context!.Commands.Execute(new CreateEntityCommand(copy, scene, source.Parent));
        RefreshTreeSafe();
    }

    private void OnDelete(object? sender, EventArgs e)
    {
        EditorScene? scene = _context?.ActiveScene;
        if (scene is null || _tree.SelectedNode?.Tag is not EditorGameObject obj) return;
        _context!.Commands.Execute(new DeleteEntityCommand(obj, scene));
        _context.SetSelection(null);
        RefreshTreeSafe();
    }

    private void OnToggleActive(object? sender, EventArgs e)
    {
        if (_tree.SelectedNode?.Tag is not EditorGameObject obj) return;
        bool newActive = !obj.Active;
        _context!.Commands.Execute(new SetPropertyCommand<bool>(
            "Set Active", obj.Active, newActive, v => obj.Active = v));
        _suppressCheckEvent = true;
        try
        {
            TreeNode? node = FindNode(obj);
            if (node is not null) node.Checked = obj.Active;
        }
        finally { _suppressCheckEvent = false; }
    }

    #endregion

    #region Drag and drop

    private void OnItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (e.Item is TreeNode node)
            DoDragDrop(node, DragDropEffects.Move);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        TreeNode? target = _tree.GetNodeAt(_tree.PointToClient(new System.Drawing.Point(e.X, e.Y)));
        if (target is not null) _tree.SelectedNode = target;
        e.Effect = DragDropEffects.Move;
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(typeof(TreeNode)) is not TreeNode draggedNode) return;
        if (draggedNode.Tag is not EditorGameObject draggedObj) return;

        EditorScene? scene = _context?.ActiveScene;
        if (scene is null) return;

        System.Drawing.Point pt = _tree.PointToClient(new System.Drawing.Point(e.X, e.Y));
        TreeNode? targetNode = _tree.GetNodeAt(pt);

        EditorGameObject? newParent = null;
        if (targetNode is not null)
        {
            if (targetNode == draggedNode) return;
            if (targetNode.Tag is not EditorGameObject targetObj) return;
            if (IsAncestorOf(draggedNode, targetNode)) return;
            newParent = targetObj;
        }

        _context!.Commands.Execute(new ReparentEntityCommand(draggedObj, scene, newParent));
        RefreshTreeSafe();
    }

    private static bool IsAncestorOf(TreeNode ancestor, TreeNode node)
    {
        TreeNode? current = node.Parent;
        while (current is not null)
        {
            if (current == ancestor) return true;
            current = current.Parent;
        }
        return false;
    }

    #endregion

    #region Helpers

    private void SelectAndEditNode(EditorGameObject obj)
    {
        TreeNode? node = FindNode(obj);
        if (node is null) return;
        _suppressSelectionEvent = true;
        _tree.SelectedNode = node;
        _suppressSelectionEvent = false;
        node.BeginEdit();
    }

    private static EditorGameObject DeepCopy(EditorGameObject source)
    {
        EditorGameObject copy = new()
        {
            Name     = source.Name + " Copy",
            Active   = source.Active,
            Position = source.Position,
            Rotation = source.Rotation,
            Scale    = source.Scale,
        };
        for (int i = 0; i < source.Behaviours.Count; i++)
        {
            EditorBehaviour b = source.Behaviours[i];
            EditorBehaviour bCopy = new() { TypeName = b.TypeName, Enabled = b.Enabled };
            foreach (System.Collections.Generic.KeyValuePair<string, JsonElement> kv in b.Properties)
                bCopy.Properties[kv.Key] = kv.Value;
            copy.Behaviours.Add(bCopy);
        }
        for (int i = 0; i < source.Children.Count; i++)
        {
            EditorGameObject childCopy = DeepCopy(source.Children[i]);
            childCopy.Parent = copy;
            copy.Children.Add(childCopy);
        }
        return copy;
    }

    #endregion
}
