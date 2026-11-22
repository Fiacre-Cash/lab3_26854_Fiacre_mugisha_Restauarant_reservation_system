namespace RestaurantReservation.Models;

public class Reservation
{
    public int Id { get; set; }
    public string RefNumber { get; set; } = "";
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Date { get; set; } = "";          // yyyy-MM-dd
    public string Time { get; set; } = "";          // HH:mm
    public int Guests { get; set; }
    public string Seating { get; set; } = "Any";
    public string Occasion { get; set; } = "";
    public string SpecialRequests { get; set; } = "";
    public string Status { get; set; } = "pending"; // pending | confirmed | completed | no-show | cancelled
    public string PaymentMethod { get; set; } = "";
    public string PaymentAccount { get; set; } = "";
    public string PaymentStatus { get; set; } = "unpaid";
    public string PaymentNote { get; set; } = "";
}

public static class ReservationOptions
{
    public static readonly string[] TimeSlots = { "12:00", "13:00", "14:00", "15:00", "18:00", "19:00", "20:00", "21:00" };
    public static readonly string[] Seating = { "Any", "Window", "Terrace", "Private Room", "Bar Counter" };
    public static readonly string[] Occasions = { "", "Birthday", "Anniversary", "Business Dinner", "Date Night", "Other" };
    public static readonly string[] PaymentMethods = { "", "MTN MoMo", "Airtel Money", "POS / Card" };
    public static readonly string[] PaymentStatuses = { "unpaid", "paid", "refunded" };
    public static readonly string[] Statuses = { "pending", "confirmed", "completed", "no-show", "cancelled" };
}
