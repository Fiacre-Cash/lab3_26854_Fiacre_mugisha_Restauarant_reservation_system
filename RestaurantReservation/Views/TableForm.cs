using RestaurantReservation.Controllers;
using RestaurantReservation.Models;

namespace RestaurantReservation.Views;

public class TableForm : Form
{
    private readonly RestaurantTable? _existing;
    private readonly Func<RestaurantTable, OperationResult> _save;

    private readonly TextBox _code = new();
    private readonly ComboBox _type = UiHelper.MakeCombo(RestaurantTable.Types);
    private readonly NumericUpDown _capacity = new() { Minimum = 1, Maximum = 20, Value = 4 };
    private readonly ComboBox _status = UiHelper.MakeCombo(RestaurantTable.Statuses);

    public TableForm(RestaurantTable? existing, Func<RestaurantTable, OperationResult> save)
    {
        _existing = existing;
        _save = save;
        Text = existing == null ? "Add table" : "Edit table";
        ClientSize = new Size(400, 200);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;

        UiHelper.AddField(this, "Code *", _code, 15, 150);
        UiHelper.AddField(this, "Type", _type, 50, 150);
        UiHelper.AddField(this, "Capacity", _capacity, 85, 80);
        UiHelper.AddField(this, "Status", _status, 120, 150);

        var ok = UiHelper.MakeButton("Save", 100, primary: true);
        ok.SetBounds(170, 155, 100, 32);
        var cancel = UiHelper.MakeButton("Cancel", 100);
        cancel.SetBounds(280, 155, 100, 32);
        cancel.DialogResult = DialogResult.Cancel;
        ok.Click += (_, _) => OnSave();
        Controls.Add(ok); Controls.Add(cancel);
        AcceptButton = ok; CancelButton = cancel;

        if (existing != null)
        {
            _code.Text = existing.Code;
            _type.SelectedItem = existing.Type;
            _capacity.Value = existing.Capacity;
            _status.SelectedItem = existing.Status;
        }
    }

    private void OnSave()
    {
        var t = new RestaurantTable
        {
            Id = _existing?.Id ?? 0,
            Code = _code.Text.Trim(),
            Type = _type.SelectedItem?.ToString() ?? "4-Top",
            Capacity = (int)_capacity.Value,
            Status = _status.SelectedItem?.ToString() ?? "available"
        };
        var r = _save(t);
        if (r.Success) { DialogResult = DialogResult.OK; Close(); }
        else MessageBox.Show(this, r.Message, "Cannot save table", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
