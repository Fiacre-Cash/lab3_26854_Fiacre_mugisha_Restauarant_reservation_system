using RestaurantReservation.Controllers;
using RestaurantReservation.Models;

namespace RestaurantReservation.Views;

/// <summary>Dialog used both to create and to edit a reservation.</summary>
public class ReservationForm : Form
{
    private readonly Reservation? _existing;
    private readonly Func<Reservation, OperationResult> _save;

    private readonly TextBox _name = new();
    private readonly TextBox _email = new();
    private readonly TextBox _phone = new();
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short };
    private readonly ComboBox _time = UiHelper.MakeCombo(ReservationOptions.TimeSlots);
    private readonly NumericUpDown _guests = new() { Minimum = 1, Maximum = ReservationController.MaxGuests, Value = 2 };
    private readonly ComboBox _seating = UiHelper.MakeCombo(ReservationOptions.Seating);
    private readonly ComboBox _occasion = UiHelper.MakeCombo(ReservationOptions.Occasions);
    private readonly TextBox _requests = new() { Multiline = true, Height = 55, ScrollBars = ScrollBars.Vertical };
    private readonly ComboBox _payMethod = UiHelper.MakeCombo(ReservationOptions.PaymentMethods);
    private readonly TextBox _payAccount = new();
    private readonly ComboBox _payStatus = UiHelper.MakeCombo(ReservationOptions.PaymentStatuses);
    private readonly ComboBox _status = UiHelper.MakeCombo(ReservationOptions.Statuses);
    private readonly TextBox _payNote = new();

    public ReservationForm(Reservation? existing, Func<Reservation, OperationResult> save)
    {
        _existing = existing;
        _save = save;

        Text = existing == null ? "New reservation" : $"Edit reservation {existing.RefNumber}";
        ClientSize = new Size(440, 580);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;

        int y = 15;
        UiHelper.AddField(this, "Full name *", _name, y); y += 33;
        UiHelper.AddField(this, "Email *", _email, y); y += 33;
        UiHelper.AddField(this, "Phone *", _phone, y); y += 33;
        UiHelper.AddField(this, "Date *", _date, y); y += 33;
        UiHelper.AddField(this, "Time *", _time, y); y += 33;
        UiHelper.AddField(this, "Guests *", _guests, y, 80); y += 33;
        UiHelper.AddField(this, "Seating", _seating, y); y += 33;
        UiHelper.AddField(this, "Occasion", _occasion, y); y += 33;
        UiHelper.AddField(this, "Special requests", _requests, y); y += 63;
        UiHelper.AddField(this, "Payment method", _payMethod, y); y += 33;
        UiHelper.AddField(this, "Payment account", _payAccount, y); y += 33;
        UiHelper.AddField(this, "Payment note", _payNote, y); y += 33;
        UiHelper.AddField(this, "Payment status", _payStatus, y); y += 33;
        UiHelper.AddField(this, "Status", _status, y); y += 40;

        var save1 = UiHelper.MakeButton("Save", 100, primary: true);
        save1.SetBounds(215, y, 100, 32);
        var cancel = UiHelper.MakeButton("Cancel", 100);
        cancel.SetBounds(325, y, 100, 32);
        cancel.DialogResult = DialogResult.Cancel;
        save1.Click += (_, _) => OnSave();
        Controls.Add(save1);
        Controls.Add(cancel);
        AcceptButton = save1;
        CancelButton = cancel;

        // Only admins can change status / payment status.
        _status.Enabled = Session.IsAdmin;
        _payStatus.Enabled = Session.IsAdmin;

        if (existing == null)
        {
            _date.MinDate = DateTime.Today;
            _date.Value = DateTime.Today;
            _name.Text = Session.CurrentUser?.Name ?? "";
            _email.Text = Session.CurrentUser?.Email ?? "";
        }
        else
        {
            _name.Text = existing.FullName;
            _email.Text = existing.Email;
            _phone.Text = existing.Phone;
            if (DateTime.TryParse(existing.Date, out var d))
            {
                if (d.Date < DateTime.Today) _date.MinDate = d.Date; else _date.MinDate = DateTime.Today;
                _date.Value = d;
            }
            SetSelected(_time, existing.Time);
            _guests.Value = Math.Clamp(existing.Guests, 1, ReservationController.MaxGuests);
            SetSelected(_seating, existing.Seating);
            SetSelected(_occasion, existing.Occasion);
            _requests.Text = existing.SpecialRequests;
            SetSelected(_payMethod, existing.PaymentMethod);
            _payAccount.Text = existing.PaymentAccount;
            _payNote.Text = existing.PaymentNote;
            SetSelected(_payStatus, existing.PaymentStatus);
            SetSelected(_status, existing.Status);
        }
    }

    private static void SetSelected(ComboBox box, string value)
    {
        if (box.Items.Contains(value)) box.SelectedItem = value;
    }

    private void OnSave()
    {
        var r = new Reservation
        {
            Id = _existing?.Id ?? 0,
            RefNumber = _existing?.RefNumber ?? "",
            UserId = _existing?.UserId ?? 0,
            FullName = _name.Text.Trim(),
            Email = _email.Text.Trim(),
            Phone = _phone.Text.Trim(),
            Date = _date.Value.ToString("yyyy-MM-dd"),
            Time = _time.SelectedItem?.ToString() ?? "",
            Guests = (int)_guests.Value,
            Seating = _seating.SelectedItem?.ToString() ?? "Any",
            Occasion = _occasion.SelectedItem?.ToString() ?? "",
            SpecialRequests = _requests.Text.Trim(),
            PaymentMethod = _payMethod.SelectedItem?.ToString() ?? "",
            PaymentAccount = _payAccount.Text.Trim(),
            PaymentNote = _payNote.Text.Trim(),
            PaymentStatus = _payStatus.SelectedItem?.ToString() ?? "unpaid",
            Status = _status.SelectedItem?.ToString() ?? "pending"
        };

        var result = _save(r);
        if (result.Success) { DialogResult = DialogResult.OK; Close(); }
        else MessageBox.Show(this, result.Message, "Cannot save reservation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
