using System.Net.Mail;
using RestaurantReservation.Data;
using RestaurantReservation.Models;

namespace RestaurantReservation.Controllers;

public class AuthController
{
    private readonly UserRepository _users = new();

    public OperationResult Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            return OperationResult.Fail("Email and password are required.");
        try
        {
            var user = _users.GetByEmail(email.Trim());
            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
                return OperationResult.Fail("Invalid credentials.");
            Session.CurrentUser = user;
            return OperationResult.Ok();
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    public OperationResult Register(string name, string email, string password, string confirm)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            return OperationResult.Fail("Name, email and password are required.");
        if (!IsValidEmail(email)) return OperationResult.Fail("Please enter a valid email address.");
        if (password.Length < 6) return OperationResult.Fail("Password must be at least 6 characters.");
        if (password != confirm) return OperationResult.Fail("Passwords do not match.");
        try
        {
            if (_users.GetByEmail(email.Trim()) != null) return OperationResult.Fail("Email already in use.");
            var user = new User
            {
                Name = name.Trim(),
                Email = email.Trim(),
                PasswordHash = PasswordHasher.Hash(password),
                Role = "user"
            };
            user.Id = _users.Insert(user);
            Session.CurrentUser = user;
            return OperationResult.Ok("Account created.");
        }
        catch (Exception ex) { return OperationResult.Fail("Database error: " + ex.Message); }
    }

    public void Logout() => Session.CurrentUser = null;

    internal static bool IsValidEmail(string email)
    {
        try { return new MailAddress(email.Trim()).Address == email.Trim(); }
        catch { return false; }
    }
}
