namespace RestaurantReservation.Models;

public class MenuEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "Starters";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }              // RWF
    public bool Available { get; set; } = true;

    public static readonly string[] Categories = { "Starters", "Mains", "Desserts", "Beverages" };
}
