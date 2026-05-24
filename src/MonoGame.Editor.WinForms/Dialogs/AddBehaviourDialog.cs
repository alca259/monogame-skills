namespace MonoGame.Editor.WinForms.Dialogs;

/// <summary>
/// Modal dialog that lets the user pick a <c>GameBehaviour</c> subclass from
/// the <see cref="GameObjectRegistry"/> to attach to a game object.
/// </summary>
public sealed class AddBehaviourDialog : Form
{
    private readonly TextBox _searchBox;
    private readonly ListBox _list;
    private readonly Button _okButton;
    private readonly Button _cancelButton;
    private readonly List<string> _allTypeNames = [];

    /// <summary>Full type name selected by the user, or <c>null</c> if cancelled.</summary>
    public string? SelectedTypeName { get; private set; }

    /// <summary>Creates a new dialog populated from <paramref name="registry"/>.</summary>
    public AddBehaviourDialog(GameObjectRegistry registry)
    {
        Text            = "Add Behaviour";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition   = FormStartPosition.CenterParent;
        ClientSize      = new System.Drawing.Size(320, 360);
        MaximizeBox     = false;
        MinimizeBox     = false;
        Font            = new System.Drawing.Font("Segoe UI", 9f);

        Label searchLabel = new Label
        {
            Text     = "Search:",
            Location = new System.Drawing.Point(8, 10),
            AutoSize = true,
        };

        _searchBox = new TextBox
        {
            Location = new System.Drawing.Point(8, 28),
            Width    = 304,
        };

        _list = new ListBox
        {
            Location      = new System.Drawing.Point(8, 60),
            Size          = new System.Drawing.Size(304, 240),
            IntegralHeight = false,
        };

        _okButton = new Button
        {
            Text         = "OK",
            DialogResult = DialogResult.OK,
            Location     = new System.Drawing.Point(152, 320),
            Width        = 76,
        };
        _okButton.Click += (_, _) =>
        {
            SelectedTypeName = _list.SelectedItem as string;
            DialogResult = DialogResult.OK;
        };

        _cancelButton = new Button
        {
            Text         = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location     = new System.Drawing.Point(236, 320),
            Width        = 76,
        };

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        Controls.Add(searchLabel);
        Controls.Add(_searchBox);
        Controls.Add(_list);
        Controls.Add(_okButton);
        Controls.Add(_cancelButton);

        foreach (string name in registry.RegisteredTypes.Keys)
            _allTypeNames.Add(name);
        _allTypeNames.Sort(StringComparer.OrdinalIgnoreCase);
        PopulateList(string.Empty);

        _searchBox.TextChanged += (_, _) => PopulateList(_searchBox.Text);
        _list.DoubleClick      += (_, _) => _okButton.PerformClick();
    }

    private void PopulateList(string filter)
    {
        _list.BeginUpdate();
        _list.Items.Clear();
        for (int i = 0; i < _allTypeNames.Count; i++)
        {
            if (string.IsNullOrEmpty(filter) ||
                _allTypeNames[i].Contains(filter, StringComparison.OrdinalIgnoreCase))
                _list.Items.Add(_allTypeNames[i]);
        }
        if (_list.Items.Count > 0)
            _list.SelectedIndex = 0;
        _list.EndUpdate();
    }
}
