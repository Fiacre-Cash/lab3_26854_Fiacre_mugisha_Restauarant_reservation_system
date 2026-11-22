using RestaurantReservation.Controllers;
using RestaurantReservation.Models;

namespace RestaurantReservation.Views;

public class MenuItemForm : Form
{
    private readonly MenuEntry? _existing;
    private readonly Func<MenuEntry, OperationResult> _save;

    private readonly TextBox _name = new();
    private readonly ComboBox _category = UiHelper.MakeCombo(MenuEntry.Categories);
    private readonly TextBox _description = new() { Multiline = true, Height = 60 };
    private readonly NumericUpDown _price = new() { Minimum = 0, Maximum = 10_000_000, Increment = 500, ThousandsSeparator = true };
    private readonly CheckBox _available = new() { Text = "Available", Checked = true };

    public MenuItemForm(MenuEntry? existing, Func<MenuEntry, OperationResult> save)
    {
        _existing = existing;
        _save = save;
        Text = existing == null ? "Add menu item" : "Edit menu item";
        ClientSize = new Size(420, 280);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;

        UiHelper.AddField(this, "Name *", _name, 15);
        UiHelper.AddField(this, "Category", _category, 50);
        UiHelper.AddField(this, "Description", _description, 85);
        UiHelper.AddField(this, "Price (RWF) *", _price, 155, 120);
        _available.Left = 145; _available.Top = 190; _available.Width = 200;
        Controls.Add(_available);

        var ok = UiHelper.MakeButton("Save", 100, primary: true);
        ok.SetBounds(190, 230, 100, 32);
        var cancel = UiHelper.MakeButton("Cancel", 100);
        cancel.SetBounds(300, 230, 100, 32);
        cancel.DialogResult = DialogResult.Cancel;
        ok.Click += (_, _) => OnSave();
        Controls.Add(ok); Controls.Add(cancel);
        AcceptButton = ok; CancelButton = cancel;

        if (existing != null)
        {
            _name.Text = existing.Name;
            _category.SelectedItem = existing.Category;
            _description.Text = existing.Description;
            _price.Value = Math.Min(_price.Maximum, existing.Price);
            _available.Checked = existing.Available;
        }
    }

    private void OnSave()
    {
        var item = new MenuEntry
        {
            Id = _existing?.Id ?? 0,
            Name = _name.Text.Trim(),
            Category = _category.SelectedItem?.ToString() ?? "",
            Description = _description.Text.Trim(),
            Price = _price.Value,
            Available = _available.Checked
        };
        var r = _save(item);
        if (r.Success) { DialogResult = DialogResult.OK; Close(); }
        else MessageBox.Show(this, r.Message, "Cannot save item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
