namespace MonoGame.Editor.WinForms.Dialogs;

/// <summary>Dialog for creating a new MonoGame Editor project.</summary>
public sealed partial class NewProjectDialog : Form
{
    /// <summary>Designer-only constructor.</summary>
    public NewProjectDialog()
    {
        InitializeComponent();
        WireEvents();
    }

    /// <summary>Name entered by the user.</summary>
    public string ProjectName => _nameTextBox.Text.Trim();

    /// <summary>Parent folder selected by the user (the project subfolder will be created inside it).</summary>
    public string ParentPath => _locationTextBox.Text.Trim();

    private void WireEvents()
    {
        _nameTextBox.TextChanged     += (_, _) => UpdatePreviewAndOk();
        _locationTextBox.TextChanged += (_, _) => UpdatePreviewAndOk();
        _browseButton.Click          += OnBrowseClick;
        _okButton.Click              += (_, _) => { DialogResult = DialogResult.OK; Close(); };
        _cancelButton.Click          += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        string defaultLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _locationTextBox.Text = defaultLocation;
        UpdatePreviewAndOk();
    }

    private void OnBrowseClick(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dlg = new()
        {
            Description = "Select the parent folder for the new project",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(_locationTextBox.Text)
                ? _locationTextBox.Text
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };

        if (dlg.ShowDialog(this) == DialogResult.OK)
            _locationTextBox.Text = dlg.SelectedPath;
    }

    private void UpdatePreviewAndOk()
    {
        string name = ProjectName;
        string location = ParentPath;
        bool valid = !string.IsNullOrWhiteSpace(name)
                  && !string.IsNullOrWhiteSpace(location)
                  && IsValidFolderName(name)
                  && Directory.Exists(location);

        if (valid)
            _previewValueLabel.Text = Path.Combine(location, name);
        else if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(location))
            _previewValueLabel.Text = string.Empty;
        else if (!IsValidFolderName(name))
            _previewValueLabel.Text = "Invalid project name.";
        else
            _previewValueLabel.Text = "Location does not exist.";

        _okButton.Enabled = valid;
    }

    private static bool IsValidFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        char[] invalid = Path.GetInvalidFileNameChars();
        foreach (char c in name)
            if (Array.IndexOf(invalid, c) >= 0) return false;
        return true;
    }
}
