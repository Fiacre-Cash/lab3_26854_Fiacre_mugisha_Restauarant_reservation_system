using RestaurantReservation.Controllers;
using RestaurantReservation.Data;
using RestaurantReservation.Views;

namespace RestaurantReservation;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            Database.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not initialise the database:\n" + ex.Message,
                "Fiacre Eats", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        while (true)
        {
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() != DialogResult.OK) return;
            }
            Application.Run(new MainForm());
            // Logout clears the session; closing the window with X keeps it and exits.
            if (Session.CurrentUser != null) return;
        }
    }
}
