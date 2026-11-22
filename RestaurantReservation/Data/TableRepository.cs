using Microsoft.Data.Sqlite;
using RestaurantReservation.Models;

namespace RestaurantReservation.Data;

public class TableRepository
{
    public List<RestaurantTable> GetAll()
    {
        var list = new List<RestaurantTable>();
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id,Code,Type,Capacity,Status FROM RestaurantTables ORDER BY Code";
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public bool CodeExists(string code, int? excludeId = null)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM RestaurantTables WHERE lower(Code)=lower($c) AND ($ex IS NULL OR Id<>$ex)";
        cmd.Parameters.AddWithValue("$c", code);
        cmd.Parameters.AddWithValue("$ex", (object?)excludeId ?? DBNull.Value);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int Insert(RestaurantTable t)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO RestaurantTables (Code,Type,Capacity,Status) VALUES ($c,$t,$p,$s); SELECT last_insert_rowid();";
        Bind(cmd, t);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(RestaurantTable t)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE RestaurantTables SET Code=$c,Type=$t,Capacity=$p,Status=$s WHERE Id=$id";
        Bind(cmd, t);
        cmd.Parameters.AddWithValue("$id", t.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM RestaurantTables WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    private static void Bind(SqliteCommand cmd, RestaurantTable t)
    {
        cmd.Parameters.AddWithValue("$c", t.Code);
        cmd.Parameters.AddWithValue("$t", t.Type);
        cmd.Parameters.AddWithValue("$p", t.Capacity);
        cmd.Parameters.AddWithValue("$s", t.Status);
    }

    private static RestaurantTable Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), Code = r.GetString(1), Type = r.GetString(2),
        Capacity = r.GetInt32(3), Status = r.GetString(4)
    };
}
