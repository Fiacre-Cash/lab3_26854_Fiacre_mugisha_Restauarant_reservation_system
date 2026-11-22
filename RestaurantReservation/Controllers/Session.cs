using RestaurantReservation.Models;

namespace RestaurantReservation.Controllers;

/// <summary>Holds the currently logged-in user for the running application.</summary>
public static class Session
{
    public static User? CurrentUser { get; set; }
    public static bool IsAdmin => CurrentUser?.IsAdmin == true;
}
