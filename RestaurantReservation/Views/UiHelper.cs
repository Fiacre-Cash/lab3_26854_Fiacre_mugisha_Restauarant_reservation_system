namespace RestaurantReservation.Views;

/// <summary>Small helpers so the code-built forms stay short and consistent.</summary>
internal static class UiHelper
{
    public static readonly Color Brand = Color.FromArgb(139, 30, 63);
    public static readonly Color BrandDark = Color.FromArgb(100, 20, 45);

    public static Label AddField(Control parent, string label, Control input, int y, int inputWidth = 260, int labelWidth = 130)
    {
        var lbl = new Label { Text = label, Left = 15, Top = y + 4, Width = labelWidth, AutoSize = false };
        input.Left = 15 + labelWidth;
        input.Top = y;
        if (input.Width < inputWidth || input is not NumericUpDown) input.Width = inputWidth;
        parent.Controls.Add(lbl);
        parent.Controls.Add(input);
        return lbl;
    }

    public static Form MakeDialog(string title, int width, int height)
    {
        return new Form
        {
            Text = title,
            ClientSize = new Size(width, height),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false
        };
    }

    public static Button MakeButton(string text, int width = 100, bool primary = false)
    {
        var b = new Button { Text = text, Width = width, Height = 30, FlatStyle = FlatStyle.Flat, Margin = new Padding(4) };
        if (primary)
        {
            b.BackColor = Brand;
            b.ForeColor = Color.White;
            b.FlatAppearance.BorderSize = 0;
        }
        return b;
    }

    public static ComboBox MakeCombo(IEnumerable<string> items, string? selected = null)
    {
        var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        c.Items.AddRange(items.Cast<object>().ToArray());
        if (selected != null && c.Items.Contains(selected)) c.SelectedItem = selected;
        else if (c.Items.Count > 0) c.SelectedIndex = 0;
        return c;
    }

    public static void StyleGrid(DataGridView g)
    {
        g.ReadOnly = true;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.AllowUserToResizeRows = false;
        g.MultiSelect = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.RowHeadersVisible = false;
        g.BackgroundColor = Color.White;
        g.BorderStyle = BorderStyle.None;
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersDefaultCellStyle.BackColor = Brand;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = new Font(g.Font, FontStyle.Bold);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 245, 247);
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        g.Dock = DockStyle.Fill;
    }

    public static void HideColumns(DataGridView g, params string[] names)
    {
        foreach (var n in names)
            if (g.Columns.Contains(n)) g.Columns[n]!.Visible = false;
    }

    public static Color StatusColor(string status) => status switch
    {
        "confirmed" or "available" or "paid" => Color.FromArgb(47, 133, 90),
        "pending" or "reserved" => Color.FromArgb(221, 107, 32),
        "cancelled" or "no-show" or "occupied" => Color.FromArgb(197, 48, 48),
        "completed" => Color.FromArgb(74, 85, 104),
        _ => Color.Black
    };
}
