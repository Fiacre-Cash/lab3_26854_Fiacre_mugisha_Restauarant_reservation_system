using RestaurantReservation.Data;
using RestaurantReservation.Models;

namespace RestaurantReservation.Controllers;

public class TableController
{
    private readonly TableRepository _repo = new();

    public List<RestaurantTable> GetTables() => _repo.GetAll();

    public OperationResult Add(RestaurantTable t) => Save(t, isNew: true);
    public OperationResult Update(RestaurantTable t) => Save(t, isNew: false);

    public OperationResult CycleStatus(RestaurantTable t)
    {
        if (!Session.IsAdmin) return OperationResult.Fail("Admin access required.");
        try
        {
            int next = (Array.IndexOf(RestaurantTable.Statuses, t.Status) + 1) % RestaurantTable.Statuses.Length;
            t.Status = RestaurantTable.Statuses[next];
            _repo.Update(t);
            return OperationResult.Ok($"Table {t.Code} is now {t.Status}.");
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    public OperationResult Delete(int id)
    {
        if (!Session.IsAdmin) return OperationResult.Fail("Admin access required.");
        try { _repo.Delete(id); return OperationResult.Ok("Table deleted."); }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    private OperationResult Save(RestaurantTable t, bool isNew)
    {
        if (!Session.IsAdmin) return OperationResult.Fail("Admin access required.");
        if (string.IsNullOrWhiteSpace(t.Code)) return OperationResult.Fail("Table code is required.");
        if (t.Capacity < 1 || t.Capacity > 20) return OperationResult.Fail("Capacity must be between 1 and 20.");
        try
        {
            if (_repo.CodeExists(t.Code.Trim(), isNew ? null : t.Id))
                return OperationResult.Fail("A table with this code already exists.");
            t.Code = t.Code.Trim();
            if (isNew) t.Id = _repo.Insert(t); else _repo.Update(t);
            return OperationResult.Ok(isNew ? "Table added." : "Table updated.");
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }
}
