using RestaurantReservation.Controllers;
using RestaurantReservation.Models;

namespace RestaurantReservation.Views;

public class MainForm : Form
{
    private readonly ReservationController _reservations = new();
    private readonly MenuController _menu = new();
    private readonly TableController _tables = new();
    private readonly AuthController _auth = new();

    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly ToolStripStatusLabel _status = new() { Text = "Ready" };

    // Reservations tab
    private readonly TextBox _search = new() { Width = 170, PlaceholderText = "Name, email or ref..." };
    private readonly ComboBox _statusFilter = UiHelper.MakeCombo(new[] { "All" }.Concat(ReservationOptions.Statuses));
    private readonly DateTimePicker _from = new() { Width = 110, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private readonly DateTimePicker _to = new() { Width = 110, Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private readonly DataGridView _resGrid = new();

    // Menu tab
    private readonly ComboBox _categoryFilter = UiHelper.MakeCombo(new[] { "All" }.Concat(MenuEntry.Categories));
    private readonly DataGridView _menuGrid = new();

    // Tables tab
    private readonly DataGridView _tableGrid = new();
    private readonly Label _availableLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

    // Dashboard tab
    private readonly Label _tonight = StatLabel();
    private readonly Label _covers = StatLabel();
    private readonly Label _pending = StatLabel();
    private readonly Label _weekend = StatLabel();

    public MainForm()
    {
        var user = Session.CurrentUser!;
        Text = "Fiacre Eats - Reservation System";
        Size = new Size(1150, 680);
        MinimumSize = new Size(900, 500);
        StartPosition = FormStartPosition.CenterScreen;

        // Header
        var header = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = UiHelper.Brand };
        var title = new Label
        {
            Text = "Fiacre Eats", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Left = 15, Top = 12, AutoSize = true
        };
        var logout = new Button
        {
            Text = "Log out", Width = 90, Height = 30, Top = 12, Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat, ForeColor = Color.White
        };
        logout.FlatAppearance.BorderColor = Color.White;
        var who = new Label
        {
            Text = $"{user.Name}  ({(user.IsAdmin ? "Admin" : "Customer")})", ForeColor = Color.White,
            AutoSize = false, Width = 280, Height = 30, Top = 12, TextAlign = ContentAlignment.MiddleRight,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        header.Controls.AddRange(new Control[] { title, who, logout });
        header.Resize += (_, _) => { logout.Left = header.Width - 105; who.Left = logout.Left - 290; };
        logout.Click += (_, _) => { _auth.Logout(); Close(); };

        var strip = new StatusStrip();
        strip.Items.Add(_status);

        Controls.Add(_tabs);
        Controls.Add(strip);
        Controls.Add(header);

        BuildReservationsTab();
        BuildMenuTab();
        if (user.IsAdmin)
        {
            BuildTablesTab();
            BuildDashboardTab();
        }

        _tabs.SelectedIndexChanged += (_, _) => RefreshCurrentTab();
        Shown += (_, _) => RefreshCurrentTab();
    }

    // ------------------------------------------------------------ helpers
    private void Report(OperationResult r)
    {
        if (r.Success) _status.Text = r.Message;
        else MessageBox.Show(this, r.Message, "Fiacre Eats", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void RefreshCurrentTab()
    {
        switch (_tabs.SelectedTab?.Name)
        {
            case "res": LoadReservations(); break;
            case "menu": LoadMenu(); break;
            case "tables": LoadTables(); break;
            case "dash": LoadDashboard(); break;
        }
    }

    private static Label StatLabel() => new()
    {
        Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = UiHelper.Brand,
        AutoSize = false, Width = 220, Height = 60, TextAlign = ContentAlignment.MiddleCenter
    };

    private static FlowLayoutPanel ButtonBar(DockStyle dock) =>
        new() { Dock = dock, Height = 46, Padding = new Padding(6, 6, 6, 4), FlowDirection = FlowDirection.LeftToRight };

    private Button AddButton(FlowLayoutPanel bar, string text, EventHandler onClick, int width = 110, bool primary = false)
    {
        var b = UiHelper.MakeButton(text, width, primary);
        b.Click += onClick;
        bar.Controls.Add(b);
        return b;
    }

    // ------------------------------------------------------------ reservations
    private void BuildReservationsTab()
    {
        var page = new TabPage("Reservations") { Name = "res" };

        var filters = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(6, 8, 6, 0) };
        filters.Controls.Add(new Label { Text = "Search", AutoSize = true, Margin = new Padding(3, 6, 3, 0) });
        filters.Controls.Add(_search);
        filters.Controls.Add(new Label { Text = "Status", AutoSize = true, Margin = new Padding(10, 6, 3, 0) });
        _statusFilter.Width = 110;
        filters.Controls.Add(_statusFilter);
        filters.Controls.Add(new Label { Text = "From", AutoSize = true, Margin = new Padding(10, 6, 3, 0) });
        filters.Controls.Add(_from);
        filters.Controls.Add(new Label { Text = "To", AutoSize = true, Margin = new Padding(10, 6, 3, 0) });
        filters.Controls.Add(_to);
        var go = UiHelper.MakeButton("Search", 80, primary: true); go.Margin = new Padding(10, 0, 3, 0);
        go.Click += (_, _) => LoadReservations();
        var clear = UiHelper.MakeButton("Clear", 70);
        clear.Click += (_, _) =>
        {
            _search.Clear(); _statusFilter.SelectedIndex = 0;
            _from.Checked = false; _to.Checked = false;
            LoadReservations();
        };
        filters.Controls.Add(go);
        filters.Controls.Add(clear);
        _search.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { LoadReservations(); e.SuppressKeyPress = true; } };

        UiHelper.StyleGrid(_resGrid);
        _resGrid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.Value is not string s) return;
            var name = _resGrid.Columns[e.ColumnIndex].Name;
            if (name is "Status" or "PaymentStatus")
            {
                e.CellStyle!.ForeColor = UiHelper.StatusColor(s);
                e.CellStyle.Font = new Font(_resGrid.Font, FontStyle.Bold);
            }
        };
        _resGrid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditReservation(); };

        var bar = ButtonBar(DockStyle.Bottom);
        AddButton(bar, "New reservation", (_, _) => NewReservation(), 130, primary: true);
        AddButton(bar, "Edit", (_, _) => EditReservation(), 80);
        AddButton(bar, "Cancel booking", (_, _) => CancelReservation(), 120);
        if (Session.IsAdmin)
        {
            AddButton(bar, "Confirm", (_, _) => SetStatus("confirmed"), 80);
            AddButton(bar, "Complete", (_, _) => SetStatus("completed"), 80);
            AddButton(bar, "No-show", (_, _) => SetStatus("no-show"), 80);
            AddButton(bar, "Delete", (_, _) => DeleteReservation(), 80);
            AddButton(bar, "Export CSV", (_, _) => ExportCsv(), 100);
        }

        page.Controls.Add(_resGrid);
        page.Controls.Add(filters);
        page.Controls.Add(bar);
        _tabs.TabPages.Add(page);
    }

    private void LoadReservations()
    {
        try
        {
            string? status = _statusFilter.SelectedIndex > 0 ? _statusFilter.SelectedItem!.ToString() : null;
            var rows = _reservations.GetReservations(
                _search.Text, status,
                _from.Checked ? _from.Value : null,
                _to.Checked ? _to.Value : null);
            _resGrid.DataSource = rows;
            UiHelper.HideColumns(_resGrid, "Id", "UserId", "SpecialRequests", "PaymentAccount", "PaymentNote");
            _status.Text = $"{rows.Count} reservation(s)";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not load reservations:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Reservation? SelectedReservation()
    {
        var r = _resGrid.CurrentRow?.DataBoundItem as Reservation;
        if (r == null) MessageBox.Show(this, "Select a reservation first.", "Fiacre Eats", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return r;
    }

    private void NewReservation()
    {
        using var f = new ReservationForm(null, r =>
        {
            var res = _reservations.Create(r);
            return res.Success ? OperationResult.Ok(res.Message) : OperationResult.Fail(res.Message);
        });
        if (f.ShowDialog(this) == DialogResult.OK) { _status.Text = "Reservation created."; LoadReservations(); }
    }

    private void EditReservation()
    {
        var sel = SelectedReservation();
        if (sel == null) return;
        using var f = new ReservationForm(sel, r => _reservations.Update(r));
        if (f.ShowDialog(this) == DialogResult.OK) { _status.Text = "Reservation updated."; LoadReservations(); }
    }

    private void CancelReservation()
    {
        var sel = SelectedReservation();
        if (sel == null) return;
        if (MessageBox.Show(this, $"Cancel reservation {sel.RefNumber}?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        Report(_reservations.Cancel(sel.Id));
        LoadReservations();
    }

    private void SetStatus(string status)
    {
        var sel = SelectedReservation();
        if (sel == null) return;
        Report(_reservations.UpdateStatus(sel.Id, status));
        LoadReservations();
    }

    private void DeleteReservation()
    {
        var sel = SelectedReservation();
        if (sel == null) return;
        if (MessageBox.Show(this, $"Permanently delete reservation {sel.RefNumber}?", "Confirm delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        Report(_reservations.Delete(sel.Id));
        LoadReservations();
    }

    private void ExportCsv()
    {
        if (_resGrid.DataSource is not List<Reservation> rows || rows.Count == 0)
        {
            MessageBox.Show(this, "There is nothing to export.", "Fiacre Eats", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dlg = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = $"reservations-{DateTime.Today:yyyyMMdd}.csv" };
        if (dlg.ShowDialog(this) == DialogResult.OK) Report(_reservations.ExportCsv(rows, dlg.FileName));
    }

    // ------------------------------------------------------------ menu
    private void BuildMenuTab()
    {
        var page = new TabPage("Menu") { Name = "menu" };

        var filters = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(6, 8, 6, 0) };
        filters.Controls.Add(new Label { Text = "Category", AutoSize = true, Margin = new Padding(3, 6, 3, 0) });
        _categoryFilter.Width = 140;
        filters.Controls.Add(_categoryFilter);
        _categoryFilter.SelectedIndexChanged += (_, _) => LoadMenu();

        UiHelper.StyleGrid(_menuGrid);
        _menuGrid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            var col = _menuGrid.Columns[e.ColumnIndex].Name;
            if (col == "Price" && e.Value is decimal d) { e.Value = $"RWF {d:N0}"; e.FormattingApplied = true; }
            if (col == "Available" && e.Value is bool b)
            {
                e.Value = b ? "Available" : "Sold out";
                e.CellStyle!.ForeColor = b ? UiHelper.StatusColor("available") : UiHelper.StatusColor("cancelled");
                e.FormattingApplied = true;
            }
        };

        page.Controls.Add(_menuGrid);
        page.Controls.Add(filters);

        if (Session.IsAdmin)
        {
            var bar = ButtonBar(DockStyle.Bottom);
            AddButton(bar, "Add item", (_, _) => AddMenuItem(), 100, primary: true);
            AddButton(bar, "Edit", (_, _) => EditMenuItem(), 80);
            AddButton(bar, "Toggle sold out", (_, _) => ToggleMenuItem(), 120);
            AddButton(bar, "Delete", (_, _) => DeleteMenuItem(), 80);
            _menuGrid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditMenuItem(); };
            page.Controls.Add(bar);
        }
        _tabs.TabPages.Add(page);
    }

    private void LoadMenu()
    {
        try
        {
            string? cat = _categoryFilter.SelectedIndex > 0 ? _categoryFilter.SelectedItem!.ToString() : null;
            var items = _menu.GetItems(cat);
            _menuGrid.DataSource = items;
            UiHelper.HideColumns(_menuGrid, "Id");
            _status.Text = $"{items.Count} menu item(s)";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not load menu:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private MenuEntry? SelectedMenuItem()
    {
        var m = _menuGrid.CurrentRow?.DataBoundItem as MenuEntry;
        if (m == null) MessageBox.Show(this, "Select a menu item first.", "Fiacre Eats", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return m;
    }

    private void AddMenuItem()
    {
        using var f = new MenuItemForm(null, m => _menu.Add(m));
        if (f.ShowDialog(this) == DialogResult.OK) { _status.Text = "Item added."; LoadMenu(); }
    }

    private void EditMenuItem()
    {
        var sel = SelectedMenuItem();
        if (sel == null) return;
        using var f = new MenuItemForm(sel, m => _menu.Update(m));
        if (f.ShowDialog(this) == DialogResult.OK) { _status.Text = "Item updated."; LoadMenu(); }
    }

    private void ToggleMenuItem()
    {
        var sel = SelectedMenuItem();
        if (sel == null) return;
        Report(_menu.ToggleAvailability(sel));
        LoadMenu();
    }

    private void DeleteMenuItem()
    {
        var sel = SelectedMenuItem();
        if (sel == null) return;
        if (MessageBox.Show(this, $"Delete '{sel.Name}'?", "Confirm delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        Report(_menu.Delete(sel.Id));
        LoadMenu();
    }

    // ------------------------------------------------------------ tables (admin)
    private void BuildTablesTab()
    {
        var page = new TabPage("Tables") { Name = "tables" };

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(10, 10, 6, 0) };
        top.Controls.Add(_availableLabel);

        UiHelper.StyleGrid(_tableGrid);
        _tableGrid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.Value is not string s) return;
            if (_tableGrid.Columns[e.ColumnIndex].Name == "Status")
            {
                e.CellStyle!.ForeColor = UiHelper.StatusColor(s);
                e.CellStyle.Font = new Font(_tableGrid.Font, FontStyle.Bold);
            }
        };
        _tableGrid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditTable(); };

        var bar = ButtonBar(DockStyle.Bottom);
        AddButton(bar, "Add table", (_, _) => AddTable(), 100, primary: true);
        AddButton(bar, "Edit", (_, _) => EditTable(), 80);
        AddButton(bar, "Cycle status", (_, _) => CycleTable(), 110);
        AddButton(bar, "Delete", (_, _) => DeleteTable(), 80);

        page.Controls.Add(_tableGrid);
        page.Controls.Add(top);
        page.Controls.Add(bar);
        _tabs.TabPages.Add(page);
    }

    private void LoadTables()
    {
        try
        {
            var list = _tables.GetTables();
            _tableGrid.DataSource = list;
            UiHelper.HideColumns(_tableGrid, "Id");
            int free = list.Count(t => t.Status == "available");
            _availableLabel.Text = $"{free} of {list.Count} tables available";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not load tables:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private RestaurantTable? SelectedTable()
    {
        var t = _tableGrid.CurrentRow?.DataBoundItem as RestaurantTable;
        if (t == null) MessageBox.Show(this, "Select a table first.", "Fiacre Eats", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return t;
    }

    private void AddTable()
    {
        using var f = new TableForm(null, t => _tables.Add(t));
        if (f.ShowDialog(this) == DialogResult.OK) { _status.Text = "Table added."; LoadTables(); }
    }

    private void EditTable()
    {
        var sel = SelectedTable();
        if (sel == null) return;
        using var f = new TableForm(sel, t => _tables.Update(t));
        if (f.ShowDialog(this) == DialogResult.OK) { _status.Text = "Table updated."; LoadTables(); }
    }

    private void CycleTable()
    {
        var sel = SelectedTable();
        if (sel == null) return;
        Report(_tables.CycleStatus(sel));
        LoadTables();
    }

    private void DeleteTable()
    {
        var sel = SelectedTable();
        if (sel == null) return;
        if (MessageBox.Show(this, $"Delete table {sel.Code}?", "Confirm delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        Report(_tables.Delete(sel.Id));
        LoadTables();
    }

    // ------------------------------------------------------------ dashboard (admin)
    private void BuildDashboardTab()
    {
        var page = new TabPage("Dashboard") { Name = "dash" };
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top, Height = 150, ColumnCount = 4, RowCount = 2, Padding = new Padding(20)
        };
        for (int i = 0; i < 4; i++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        grid.Controls.Add(_tonight, 0, 0); grid.Controls.Add(_covers, 1, 0);
        grid.Controls.Add(_pending, 2, 0); grid.Controls.Add(_weekend, 3, 0);
        string[] captions = { "Reservations today", "Covers this week", "Pending approval", "Weekend occupancy" };
        for (int i = 0; i < 4; i++)
            grid.Controls.Add(new Label { Text = captions[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopCenter, ForeColor = Color.Gray }, i, 1);

        var refresh = UiHelper.MakeButton("Refresh", 100, primary: true);
        refresh.Left = 25; refresh.Top = 170;
        refresh.Click += (_, _) => LoadDashboard();

        page.Controls.Add(refresh);
        page.Controls.Add(grid);
        _tabs.TabPages.Add(page);
    }

    private void LoadDashboard()
    {
        try
        {
            var s = _reservations.GetStats();
            _tonight.Text = s.TonightCount.ToString();
            _covers.Text = s.WeeklyCovers.ToString();
            _pending.Text = s.PendingCount.ToString();
            _weekend.Text = s.WeekendOccupancyPercent + "%";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not load statistics:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
