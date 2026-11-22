using Microsoft.Data.Sqlite;
using RestaurantReservation.Models;

namespace RestaurantReservation.Data;

public class ReservationRepository
{
    private const string Cols =
        "Id,RefNumber,UserId,FullName,Email,Phone,Date,Time,Guests,Seating,Occasion,SpecialRequests,Status,PaymentMethod,PaymentAccount,PaymentStatus,PaymentNote";

    public List<Reservation> Search(int? userId, string? search, string? status, string? from, string? to)
    {
        var list = new List<Reservation>();
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        var where = new List<string>();
        if (userId.HasValue) { where.Add("UserId=$uid"); cmd.Parameters.AddWithValue("$uid", userId.Value); }
        if (!string.IsNullOrWhiteSpace(status)) { where.Add("Status=$st"); cmd.Parameters.AddWithValue("$st", status); }
        if (!string.IsNullOrWhiteSpace(from)) { where.Add("Date>=$from"); cmd.Parameters.AddWithValue("$from", from); }
        if (!string.IsNullOrWhiteSpace(to)) { where.Add("Date<=$to"); cmd.Parameters.AddWithValue("$to", to); }
        if (!string.IsNullOrWhiteSpace(search))
        {
            where.Add("(lower(FullName) LIKE $q OR lower(Email) LIKE $q OR lower(RefNumber) LIKE $q)");
            cmd.Parameters.AddWithValue("$q", "%" + search.Trim().ToLower() + "%");
        }
        cmd.CommandText = $"SELECT {Cols} FROM Reservations" +
                          (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "") +
                          " ORDER BY Date ASC, Time ASC";
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public Reservation? GetById(int id)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Cols} FROM Reservations WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map(r) : null;
    }

    /// <summary>Number of active bookings for a slot (optionally ignoring one reservation).</summary>
    public int CountForSlot(string date, string time, int? excludeId = null)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT COUNT(*) FROM Reservations
                            WHERE Date=$d AND Time=$t AND Status NOT IN ('cancelled','no-show')
                              AND ($ex IS NULL OR Id<>$ex)";
        cmd.Parameters.AddWithValue("$d", date);
        cmd.Parameters.AddWithValue("$t", time);
        cmd.Parameters.AddWithValue("$ex", (object?)excludeId ?? DBNull.Value);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int Insert(Reservation x)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Reservations
            (RefNumber,UserId,FullName,Email,Phone,Date,Time,Guests,Seating,Occasion,SpecialRequests,Status,PaymentMethod,PaymentAccount,PaymentStatus,PaymentNote)
            VALUES ($ref,$uid,$fn,$em,$ph,$d,$t,$g,$se,$oc,$sr,$st,$pm,$pa,$ps,$pn);
            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$ref", x.RefNumber);
        cmd.Parameters.AddWithValue("$uid", x.UserId);
        Bind(cmd, x);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Reservation x)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Reservations SET
            FullName=$fn,Email=$em,Phone=$ph,Date=$d,Time=$t,Guests=$g,Seating=$se,Occasion=$oc,
            SpecialRequests=$sr,Status=$st,PaymentMethod=$pm,PaymentAccount=$pa,PaymentStatus=$ps,PaymentNote=$pn
            WHERE Id=$id";
        Bind(cmd, x);
        cmd.Parameters.AddWithValue("$id", x.Id);
        cmd.ExecuteNonQuery();
    }

    public void UpdateStatus(int id, string status)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Reservations SET Status=$s WHERE Id=$id";
        cmd.Parameters.AddWithValue("$s", status);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = Database.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Reservations WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public DashboardStats GetStats(DateTime today)
    {
        int dow = (int)today.DayOfWeek;
        DateTime monday = today.AddDays(-((dow + 6) % 7));
        static string D(DateTime x) => x.ToString("yyyy-MM-dd");

        using var conn = Database.Open();
        int Count(string sql, params (string, object)[] ps)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            foreach (var (k, v) in ps) cmd.Parameters.AddWithValue(k, v);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        int tonight = Count("SELECT COUNT(*) FROM Reservations WHERE Date=$d AND Status<>'cancelled'", ("$d", D(today)));
        int covers = Count("SELECT COALESCE(SUM(Guests),0) FROM Reservations WHERE Date BETWEEN $a AND $b AND Status<>'cancelled'",
                           ("$a", D(monday)), ("$b", D(monday.AddDays(6))));
        int pending = Count("SELECT COUNT(*) FROM Reservations WHERE Status='pending'");
        int weekend = Count("SELECT COUNT(*) FROM Reservations WHERE Date IN ($sat,$sun) AND Status<>'cancelled'",
                            ("$sat", D(monday.AddDays(5))), ("$sun", D(monday.AddDays(6))));
        const int weekendCapacity = 50;
        return new DashboardStats
        {
            TonightCount = tonight,
            WeeklyCovers = covers,
            PendingCount = pending,
            WeekendOccupancyPercent = (int)Math.Round(weekend * 100.0 / weekendCapacity)
        };
    }

    // Binds the shared (editable) columns.
    private static void Bind(SqliteCommand cmd, Reservation x)
    {
        cmd.Parameters.AddWithValue("$fn", x.FullName);
        cmd.Parameters.AddWithValue("$em", x.Email);
        cmd.Parameters.AddWithValue("$ph", x.Phone);
        cmd.Parameters.AddWithValue("$d", x.Date);
        cmd.Parameters.AddWithValue("$t", x.Time);
        cmd.Parameters.AddWithValue("$g", x.Guests);
        cmd.Parameters.AddWithValue("$se", x.Seating);
        cmd.Parameters.AddWithValue("$oc", x.Occasion);
        cmd.Parameters.AddWithValue("$sr", x.SpecialRequests);
        cmd.Parameters.AddWithValue("$st", x.Status);
        cmd.Parameters.AddWithValue("$pm", x.PaymentMethod);
        cmd.Parameters.AddWithValue("$pa", x.PaymentAccount);
        cmd.Parameters.AddWithValue("$ps", x.PaymentStatus);
        cmd.Parameters.AddWithValue("$pn", x.PaymentNote);
    }

    private static Reservation Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), RefNumber = r.GetString(1), UserId = r.GetInt32(2),
        FullName = r.GetString(3), Email = r.GetString(4), Phone = r.GetString(5),
        Date = r.GetString(6), Time = r.GetString(7), Guests = r.GetInt32(8),
        Seating = r.GetString(9), Occasion = r.GetString(10), SpecialRequests = r.GetString(11),
        Status = r.GetString(12), PaymentMethod = r.GetString(13), PaymentAccount = r.GetString(14),
        PaymentStatus = r.GetString(15), PaymentNote = r.GetString(16)
    };
}
