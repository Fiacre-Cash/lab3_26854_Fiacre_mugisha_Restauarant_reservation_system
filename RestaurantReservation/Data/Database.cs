using Microsoft.Data.Sqlite;

namespace RestaurantReservation.Data;

/// <summary>Creates the SQLite database file, tables and seed data.</summary>
public static class Database
{
    public static string DbPath { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "FiacreEats", "restaurant.db");

    public static SqliteConnection Open()
    {
        var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();
        return conn;
    }

    public static void Initialize()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
        using var conn = Open();

        Exec(conn, @"
CREATE TABLE IF NOT EXISTS Users (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    Name         TEXT NOT NULL,
    Email        TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role         TEXT NOT NULL DEFAULT 'user'
);
CREATE TABLE IF NOT EXISTS Reservations (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    RefNumber       TEXT NOT NULL,
    UserId          INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    FullName        TEXT NOT NULL,
    Email           TEXT NOT NULL,
    Phone           TEXT NOT NULL,
    Date            TEXT NOT NULL,
    Time            TEXT NOT NULL,
    Guests          INTEGER NOT NULL,
    Seating         TEXT NOT NULL DEFAULT 'Any',
    Occasion        TEXT NOT NULL DEFAULT '',
    SpecialRequests TEXT NOT NULL DEFAULT '',
    Status          TEXT NOT NULL DEFAULT 'pending',
    PaymentMethod   TEXT NOT NULL DEFAULT '',
    PaymentAccount  TEXT NOT NULL DEFAULT '',
    PaymentStatus   TEXT NOT NULL DEFAULT 'unpaid',
    PaymentNote     TEXT NOT NULL DEFAULT ''
);
CREATE TABLE IF NOT EXISTS MenuItems (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT NOT NULL,
    Category    TEXT NOT NULL,
    Description TEXT NOT NULL DEFAULT '',
    Price       REAL NOT NULL,
    Available   INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS RestaurantTables (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Code     TEXT NOT NULL UNIQUE,
    Type     TEXT NOT NULL,
    Capacity INTEGER NOT NULL,
    Status   TEXT NOT NULL DEFAULT 'available'
);
CREATE INDEX IF NOT EXISTS IX_Reservations_DateTime ON Reservations(Date, Time);
CREATE INDEX IF NOT EXISTS IX_Reservations_User ON Reservations(UserId);");

        Seed(conn);
    }

    private static void Seed(SqliteConnection conn)
    {
        if (Scalar(conn, "SELECT COUNT(*) FROM Users") == 0)
        {
            InsertUser(conn, "Fiacre Mugisha", "admin@fiacreeats.com", "Admin@123", "admin");
            InsertUser(conn, "Customer 1", "customer1@fiacre.rw", "pass1234", "user");
        }

        if (Scalar(conn, "SELECT COUNT(*) FROM MenuItems") == 0)
        {
            var items = new (string n, string c, string d, int p)[]
            {
                ("Grilled Tilapia Bites","Starters","Lake Kivu tilapia, lemon butter, fresh herbs",8500),
                ("Burrata & Tomato","Starters","Heritage tomatoes, basil oil, aged balsamic",7000),
                ("Chicken Liver Parfait","Starters","Toasted bread, fig chutney, micro herbs",7500),
                ("Roasted Beetroot Salad","Starters","Goat cheese, candied groundnuts, orange dressing",6000),
                ("Grilled Beef Brochette","Mains","Rwandan spiced beef, roasted plantain, chimichurri",22000),
                ("Pan-Roasted Nile Perch","Mains","Coconut cream sauce, sauteed spinach, rice",18500),
                ("Wild Mushroom Risotto","Mains","Local mushrooms, parmesan, truffle oil, chives",14000),
                ("Lamb Rack","Mains","Herb crust, sweet potato mash, rosemary jus",24000),
                ("Chocolate Fondant","Desserts","Vanilla ice cream, cocoa tuile",6500),
                ("Passion Fruit Tart","Desserts","Creme patissiere, fresh passion fruit glaze",5500),
                ("Cheese Selection","Desserts","Three artisan cheeses, quince, crackers",7000),
                ("Rwandan Red Wine","Beverages","Selected local vineyard, full-bodied",5000),
                ("Fresh Passion Juice","Beverages","Freshly squeezed, no added sugar",2500),
                ("Inyange Water","Beverages","Still or sparkling, 500ml",1500),
                ("Mocktail of the Day","Beverages","Ask your server for today's selection",4000),
            };
            foreach (var i in items)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO MenuItems (Name,Category,Description,Price,Available) VALUES ($n,$c,$d,$p,1)";
                cmd.Parameters.AddWithValue("$n", i.n);
                cmd.Parameters.AddWithValue("$c", i.c);
                cmd.Parameters.AddWithValue("$d", i.d);
                cmd.Parameters.AddWithValue("$p", i.p);
                cmd.ExecuteNonQuery();
            }
        }

        if (Scalar(conn, "SELECT COUNT(*) FROM RestaurantTables") == 0)
        {
            var tables = new (string code, string type, int cap, string status)[]
            {
                ("T1","2-Top",2,"available"), ("T2","2-Top",2,"occupied"),
                ("T3","4-Top",4,"reserved"),  ("T4","6-Top",6,"available"),
                ("T5","Booth",4,"available"), ("T6","Booth",4,"occupied"),
            };
            foreach (var t in tables)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO RestaurantTables (Code,Type,Capacity,Status) VALUES ($c,$t,$p,$s)";
                cmd.Parameters.AddWithValue("$c", t.code);
                cmd.Parameters.AddWithValue("$t", t.type);
                cmd.Parameters.AddWithValue("$p", t.cap);
                cmd.Parameters.AddWithValue("$s", t.status);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private static void InsertUser(SqliteConnection conn, string name, string email, string password, string role)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Users (Name,Email,PasswordHash,Role) VALUES ($n,$e,$h,$r)";
        cmd.Parameters.AddWithValue("$n", name);
        cmd.Parameters.AddWithValue("$e", email);
        cmd.Parameters.AddWithValue("$h", PasswordHasher.Hash(password));
        cmd.Parameters.AddWithValue("$r", role);
        cmd.ExecuteNonQuery();
    }

    private static void Exec(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static long Scalar(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return Convert.ToInt64(cmd.ExecuteScalar());
    }
}
