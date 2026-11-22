namespace RestaurantReservation.Models;

public class RestaurantTable
{
    public int Id { get; set; }
    public string Code { get; set; } = "";          // e.g. T1
    public string Type { get; set; } = "4-Top";
    public int Capacity { get; set; } = 4;
    public string Status { get; set; } = "available"; // available | reserved | occupied

    public static readonly string[] Types = { "2-Top", "4-Top", "6-Top", "Booth" };
    public static readonly string[] Statuses = { "available", "reserved", "occupied" };
}
