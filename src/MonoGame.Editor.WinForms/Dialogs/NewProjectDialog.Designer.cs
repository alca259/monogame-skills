namespace MonoGame.Editor.WinForms.Dialogs;

partial class NewProjectDialog
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Label _nameLabel;
    private System.Windows.Forms.TextBox _nameTextBox;
    private System.Windows.Forms.Label _locationLabel;
    private System.Windows.Forms.TextBox _locationTextBox;
    private System.Windows.Forms.Button _browseButton;
    private System.Windows.Forms.Label _previewLabel;
    private System.Windows.Forms.Label _previewValueLabel;
    private System.Windows.Forms.Button _okButton;
    private System.Windows.Forms.Button _cancelButton;
    private System.Windows.Forms.TableLayoutPanel _gridPanel;
    private System.Windows.Forms.TableLayoutPanel _locationRow;
    private System.Windows.Forms.FlowLayoutPanel _buttonPanel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _nameLabel         = new System.Windows.Forms.Label();
        _nameTextBox       = new System.Windows.Forms.TextBox();
        _locationLabel     = new System.Windows.Forms.Label();
        _locationTextBox   = new System.Windows.Forms.TextBox();
        _browseButton      = new System.Windows.Forms.Button();
        _previewLabel      = new System.Windows.Forms.Label();
        _previewValueLabel = new System.Windows.Forms.Label();
        _okButton          = new System.Windows.Forms.Button();
        _cancelButton      = new System.Windows.Forms.Button();
        _gridPanel         = new System.Windows.Forms.TableLayoutPanel();
        _locationRow       = new System.Windows.Forms.TableLayoutPanel();
        _buttonPanel       = new System.Windows.Forms.FlowLayoutPanel();

        _gridPanel.SuspendLayout();
        _locationRow.SuspendLayout();
        _buttonPanel.SuspendLayout();
        SuspendLayout();

        // _nameLabel
        _nameLabel.AutoSize  = true;
        _nameLabel.Dock      = System.Windows.Forms.DockStyle.Fill;
        _nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        _nameLabel.Text      = "Project Name:";

        // _nameTextBox
        _nameTextBox.Dock = System.Windows.Forms.DockStyle.Fill;

        // _locationLabel
        _locationLabel.AutoSize  = true;
        _locationLabel.Dock      = System.Windows.Forms.DockStyle.Fill;
        _locationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        _locationLabel.Text      = "Location:";

        // _locationTextBox
        _locationTextBox.Dock     = System.Windows.Forms.DockStyle.Fill;
        _locationTextBox.ReadOnly = true;

        // _browseButton
        _browseButton.AutoSize = true;
        _browseButton.Text     = "Browse...";
        _browseButton.Dock     = System.Windows.Forms.DockStyle.Right;

        // _locationRow
        _locationRow.ColumnCount = 2;
        _locationRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        _locationRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        _locationRow.Controls.Add(_locationTextBox, 0, 0);
        _locationRow.Controls.Add(_browseButton, 1, 0);
        _locationRow.Dock      = System.Windows.Forms.DockStyle.Fill;
        _locationRow.RowCount  = 1;
        _locationRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

        // _previewLabel
        _previewLabel.AutoSize  = true;
        _previewLabel.Dock      = System.Windows.Forms.DockStyle.Fill;
        _previewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        _previewLabel.Text      = "Full path:";

        // _previewValueLabel
        _previewValueLabel.AutoSize  = true;
        _previewValueLabel.Dock      = System.Windows.Forms.DockStyle.Fill;
        _previewValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        _previewValueLabel.ForeColor = System.Drawing.SystemColors.GrayText;

        // _gridPanel
        _gridPanel.ColumnCount = 2;
        _gridPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
        _gridPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        _gridPanel.Controls.Add(_nameLabel, 0, 0);
        _gridPanel.Controls.Add(_nameTextBox, 1, 0);
        _gridPanel.Controls.Add(_locationLabel, 0, 1);
        _gridPanel.Controls.Add(_locationRow, 1, 1);
        _gridPanel.Controls.Add(_previewLabel, 0, 2);
        _gridPanel.Controls.Add(_previewValueLabel, 1, 2);
        _gridPanel.Dock     = System.Windows.Forms.DockStyle.Top;
        _gridPanel.Height   = 90;
        _gridPanel.Padding  = new System.Windows.Forms.Padding(8);
        _gridPanel.RowCount = 3;
        _gridPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        _gridPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        _gridPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));

        // _okButton
        _okButton.Text    = "OK";
        _okButton.Width   = 80;
        _okButton.Enabled = false;

        // _cancelButton
        _cancelButton.Text          = "Cancel";
        _cancelButton.Width         = 80;
        _cancelButton.DialogResult  = System.Windows.Forms.DialogResult.Cancel;

        // _buttonPanel
        _buttonPanel.Controls.Add(_cancelButton);
        _buttonPanel.Controls.Add(_okButton);
        _buttonPanel.Dock          = System.Windows.Forms.DockStyle.Bottom;
        _buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        _buttonPanel.Height        = 38;
        _buttonPanel.Padding       = new System.Windows.Forms.Padding(4);

        // NewProjectDialog
        AcceptButton      = _okButton;
        CancelButton      = _cancelButton;
        ClientSize        = new System.Drawing.Size(480, 130);
        Controls.Add(_gridPanel);
        Controls.Add(_buttonPanel);
        Font              = new System.Drawing.Font("Segoe UI", 9F);
        FormBorderStyle   = System.Windows.Forms.FormBorderStyle.FixedDialog;
        MaximizeBox       = false;
        MinimizeBox       = false;
        StartPosition     = System.Windows.Forms.FormStartPosition.CenterParent;
        Text              = "New Project";

        _gridPanel.ResumeLayout(false);
        _locationRow.ResumeLayout(false);
        _buttonPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
