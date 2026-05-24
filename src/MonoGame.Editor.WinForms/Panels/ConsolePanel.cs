namespace MonoGame.Editor.WinForms.Panels;

/// <summary>Outputs editor messages and game Debug.WriteLine calls. Implemented in Fase 1 / Fase 3.</summary>
public sealed class ConsolePanel : UserControl
{
    private readonly RichTextBox _output;

    public ConsolePanel()
    {
        _output = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
            ForeColor = System.Drawing.SystemColors.ControlText,
            Font = new System.Drawing.Font("Consolas", 9f),
            BorderStyle = BorderStyle.None,
        };
        Controls.Add(_output);
    }

    /// <summary>Appends a line to the console output (thread-safe).</summary>
    public void AppendLine(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLine(message));
            return;
        }

        _output.AppendText(message + Environment.NewLine);
        _output.ScrollToCaret();
    }

    /// <summary>Clears all console output.</summary>
    public void Clear() => _output.Clear();
}
