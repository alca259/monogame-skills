namespace MonoGame.Editor.WinForms.Panels;

/// <summary>Displays the game object hierarchy tree. Implemented in Fase 3.</summary>
public sealed class SceneHierarchyPanel : UserControl
{
    public SceneHierarchyPanel()
    {
        Label placeholder = new()
        {
            Text = "Scene Hierarchy",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            ForeColor = System.Drawing.SystemColors.GrayText,
        };
        Controls.Add(placeholder);
    }
}
