namespace MonoGame.Editor.WinForms.Panels;

/// <summary>Browses project assets with drag-and-drop support. Implemented in Fase 5.</summary>
public sealed class AssetBrowserPanel : UserControl
{
    public AssetBrowserPanel()
    {
        Label placeholder = new()
        {
            Text = "Asset Browser",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            ForeColor = System.Drawing.SystemColors.GrayText,
        };
        Controls.Add(placeholder);
    }
}
