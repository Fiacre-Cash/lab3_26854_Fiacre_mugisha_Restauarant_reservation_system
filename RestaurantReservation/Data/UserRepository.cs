using Microsoft.Data.Sqlite;
using RestaurantReservation.Models;

namespace RestaurantReservation.Data;

public class UserRepository
{
    public User? GetByEmail(string email)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id,Name,Email,PasswordHash,Role FROM Users WHERE lower(Email)=lower($e)";
        cmd.Parameters.AddWithValue("$e", email);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map(r) : null;
    }

    public int Insert(User u)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Users (Name,Email,PasswordHash,Role) VALUES ($n,$e,$h,$r);
                            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$n", u.Name);
        cmd.Parameters.AddWithValue("$e", u.Email);
        cmd.Parameters.AddWithValue("$h", u.PasswordHash);
        cmd.Parameters.AddWithValue("$r", u.Role);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static User Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        Email = r.GetString(2),
        PasswordHash = r.GetString(3),
        Role = r.GetString(4)
    };
}
