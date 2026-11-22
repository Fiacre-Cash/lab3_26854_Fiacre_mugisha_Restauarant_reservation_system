using System.Text;
using RestaurantReservation.Data;
using RestaurantReservation.Models;

namespace RestaurantReservation.Controllers;

public class ReservationController
{
    public const int MaxPerSlot = 5;
    public const int MaxGuests = 20;

    private readonly ReservationRepository _repo = new();

    /// <summary>Admins see every reservation; customers only their own.</summary>
    public List<Reservation> GetReservations(string? search = null, string? status = null,
                                             DateTime? from = null, DateTime? to = null)
    {
        int? userId = Session.IsAdmin ? null : Session.CurrentUser?.Id;
        return _repo.Search(userId, search, status,
            from?.ToString("yyyy-MM-dd"), to?.ToString("yyyy-MM-dd"));
    }

    public OperationResult<Reservation> Create(Reservation r)
    {
        var user = Session.CurrentUser;
        if (user == null) return OperationResult<Reservation>.Fail("You must be logged in.");

        var error = Validate(r, isNew: true);
        if (error != null) return OperationResult<Reservation>.Fail(error);

        try
        {
            if (_repo.CountForSlot(r.Date, r.Time) >= MaxPerSlot)
                return OperationResult<Reservation>.Fail("No tables available for this slot.");

            r.UserId = user.Id;
            r.RefNumber = "FE-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("X");
            // Only admins may choose status / payment status when creating.
            if (!user.IsAdmin) { r.Status = "pending"; r.PaymentStatus = "unpaid"; }
            r.Id = _repo.Insert(r);
            return OperationResult<Reservation>.Ok(r, $"Reservation created. Reference: {r.RefNumber}");
        }
        catch (Exception ex) { return OperationResult<Reservation>.Fail("Database error: " + ex.Message); }
    }

    public OperationResult Update(Reservation r)
    {
        var existing = _repo.GetById(r.Id);
        if (existing == null) return OperationResult.Fail("Reservation not found.");
        if (!CanModify(existing)) return OperationResult.Fail("You are not allowed to edit this reservation.");

        var error = Validate(r, isNew: false);
        if (error != null) return OperationResult.Fail(error);

        try
        {
            if (_repo.CountForSlot(r.Date, r.Time, r.Id) >= MaxPerSlot)
                return OperationResult.Fail("No tables available for this slot.");

            // Customers cannot change status or payment status.
            if (!Session.IsAdmin)
            {
                r.Status = existing.Status;
                r.PaymentStatus = existing.PaymentStatus;
            }
            _repo.Update(r);
            return OperationResult.Ok("Reservation updated.");
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    public OperationResult UpdateStatus(int id, string status)
    {
        if (!Session.IsAdmin) return OperationResult.Fail("Admin access required.");
        if (!ReservationOptions.Statuses.Contains(status)) return OperationResult.Fail("Unknown status.");
        try
        {
            if (_repo.GetById(id) == null) return OperationResult.Fail("Reservation not found.");
            _repo.UpdateStatus(id, status);
            return OperationResult.Ok($"Status set to {status}.");
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    /// <summary>Customer-friendly cancel: keeps the record but marks it cancelled.</summary>
    public OperationResult Cancel(int id)
    {
        try
        {
            var r = _repo.GetById(id);
            if (r == null) return OperationResult.Fail("Reservation not found.");
            if (!CanModify(r)) return OperationResult.Fail("You are not allowed to cancel this reservation.");
            if (r.Status is "cancelled" or "completed")
                return OperationResult.Fail($"A {r.Status} reservation cannot be cancelled.");
            _repo.UpdateStatus(id, "cancelled");
            return OperationResult.Ok("Reservation cancelled.");
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    public OperationResult Delete(int id)
    {
        if (!Session.IsAdmin) return OperationResult.Fail("Admin access required.");
        try
        {
            _repo.Delete(id);
            return OperationResult.Ok("Reservation deleted.");
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    public DashboardStats GetStats() => _repo.GetStats(DateTime.Today);

    public OperationResult ExportCsv(IEnumerable<Reservation> rows, string path)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Ref,Name,Email,Phone,Date,Time,Guests,Seating,Occasion,Payment,PaymentStatus,Status");
            foreach (var r in rows)
                sb.AppendLine(string.Join(",", new[]
                {
                    r.RefNumber, r.FullName, r.Email, r.Phone, r.Date, r.Time, r.Guests.ToString(),
                    r.Seating, r.Occasion, r.PaymentMethod, r.PaymentStatus, r.Status
                }.Select(Csv)));
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
            return OperationResult.Ok("Exported to " + path);
        }
        catch (Exception ex) { return OperationResult.Fail("Export failed: " + ex.Message); }
    }

    private static string Csv(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";

    private static bool CanModify(Reservation r) =>
        Session.IsAdmin || (Session.CurrentUser != null && r.UserId == Session.CurrentUser.Id);

    private static string? Validate(Reservation r, bool isNew)
    {
        if (string.IsNullOrWhiteSpace(r.FullName) || string.IsNullOrWhiteSpace(r.Email) ||
            string.IsNullOrWhiteSpace(r.Phone) || string.IsNullOrWhiteSpace(r.Date) ||
            string.IsNullOrWhiteSpace(r.Time))
            return "Please fill in all required fields.";
        if (!AuthController.IsValidEmail(r.Email)) return "Please enter a valid email address.";
        if (r.Phone.Count(char.IsDigit) < 7) return "Please enter a valid phone number.";
        if (!DateTime.TryParse(r.Date, out var date)) return "Invalid date.";
        if (isNew && date.Date < DateTime.Today) return "The date cannot be in the past.";
        if (!ReservationOptions.TimeSlots.Contains(r.Time)) return "Please choose a valid time slot.";
        if (r.Guests < 1 || r.Guests > MaxGuests) return $"Guests must be between 1 and {MaxGuests}.";
        return null;
    }
}
