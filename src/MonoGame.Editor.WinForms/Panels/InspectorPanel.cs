namespace MonoGame.Editor.WinForms.Panels;

/// <summary>Displays and edits properties of the selected game object. Implemented in Fase 3.</summary>
public sealed class InspectorPanel : UserControl
{
    public InspectorPanel()
    {
        Label placeholder = new()
        {
            Text = "Inspector",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            ForeColor = System.Drawing.SystemColors.GrayText,
        };
        Controls.Add(placeholder);
    }
}
