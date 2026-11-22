using RestaurantReservation.Data;
using RestaurantReservation.Models;

namespace RestaurantReservation.Controllers;

public class MenuController
{
    private readonly MenuRepository _repo = new();

    public List<MenuEntry> GetItems(string? category = null) => _repo.GetAll(category);

    public OperationResult Add(MenuEntry item) => Save(item, isNew: true);
    public OperationResult Update(MenuEntry item) => Save(item, isNew: false);

    public OperationResult ToggleAvailability(MenuEntry item)
    {
        if (!Session.IsAdmin) return OperationResult.Fail("Admin access required.");
        try
        {
            item.Available = !item.Available;
            _repo.Update(item);
            return OperationResult.Ok(item.Available ? "Marked available." : "Marked sold out.");
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    public OperationResult Delete(int id)
    {
        if (!Session.IsAdmin) return OperationResult.Fail("Admin access required.");
        try { _repo.Delete(id); return OperationResult.Ok("Item deleted."); }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    private OperationResult Save(MenuEntry item, bool isNew)
    {
        if (!Session.IsAdmin) return OperationResult.Fail("Admin access required.");
        if (string.IsNullOrWhiteSpace(item.Name)) return OperationResult.Fail("Item name is required.");
        if (!MenuEntry.Categories.Contains(item.Category)) return OperationResult.Fail("Choose a valid category.");
        if (item.Price <= 0) return OperationResult.Fail("Price must be greater than zero.");
        try
        {
            if (isNew) item.Id = _repo.Insert(item); else _repo.Update(item);
            return OperationResult.Ok(isNew ? "Item added." : "Item updated.");
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }
}
