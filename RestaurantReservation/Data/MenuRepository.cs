using Microsoft.Data.Sqlite;
using RestaurantReservation.Models;

namespace RestaurantReservation.Data;

public class MenuRepository
{
    public List<MenuEntry> GetAll(string? category = null)
    {
        var list = new List<MenuEntry>();
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id,Name,Category,Description,Price,Available FROM MenuItems" +
                          (string.IsNullOrWhiteSpace(category) ? "" : " WHERE Category=$c") +
                          " ORDER BY Category, Name";
        if (!string.IsNullOrWhiteSpace(category)) cmd.Parameters.AddWithValue("$c", category);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public int Insert(MenuEntry m)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO MenuItems (Name,Category,Description,Price,Available)
                            VALUES ($n,$c,$d,$p,$a); SELECT last_insert_rowid();";
        Bind(cmd, m);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(MenuEntry m)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE MenuItems SET Name=$n,Category=$c,Description=$d,Price=$p,Available=$a WHERE Id=$id";
        Bind(cmd, m);
        cmd.Parameters.AddWithValue("$id", m.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM MenuItems WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private static void Bind(SqliteCommand cmd, MenuEntry m)
    {
        cmd.Parameters.AddWithValue("$n", m.Name);
        cmd.Parameters.AddWithValue("$c", m.Category);
        cmd.Parameters.AddWithValue("$d", m.Description);
        cmd.Parameters.AddWithValue("$p", (double)m.Price);
        cmd.Parameters.AddWithValue("$a", m.Available ? 1 : 0);
    }

    private static MenuEntry Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), Name = r.GetString(1), Category = r.GetString(2),
        Description = r.GetString(3), Price = (decimal)r.GetDouble(4), Available = r.GetInt32(5) == 1
    };
}
