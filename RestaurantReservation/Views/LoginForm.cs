using RestaurantReservation.Controllers;

namespace RestaurantReservation.Views;

public class LoginForm : Form
{
    private readonly AuthController _auth = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new() { UseSystemPasswordChar = true };
    private readonly Label _error = new() { ForeColor = Color.Firebrick, AutoSize = false };

    public LoginForm()
    {
        Text = "Fiacre Eats - Sign in";
        ClientSize = new Size(400, 290);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;

        var title = new Label
        {
            Text = "Fiacre Eats", Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = UiHelper.Brand, Left = 15, Top = 15, Width = 370, Height = 40,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var sub = new Label
        {
            Text = "Restaurant reservation system", Left = 15, Top = 55, Width = 370,
            TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray
        };
        Controls.Add(title);
        Controls.Add(sub);

        UiHelper.AddField(this, "Email", _email, 95, 230, 100);
        UiHelper.AddField(this, "Password", _password, 130, 230, 100);

        _error.SetBounds(15, 165, 370, 35);
        Controls.Add(_error);

        var login = UiHelper.MakeButton("Sign in", 160, primary: true);
        login.Left = 115; login.Top = 205;
        login.Click += (_, _) => DoLogin();
        Controls.Add(login);

        var register = new LinkLabel { Text = "Create a customer account", Left = 115, Top = 250, Width = 200, TextAlign = ContentAlignment.MiddleCenter };
        register.LinkClicked += (_, _) =>
        {
            using var f = new RegisterForm();
            if (f.ShowDialog(this) == DialogResult.OK) { DialogResult = DialogResult.OK; Close(); }
        };
        Controls.Add(register);

        AcceptButton = login;
    }

    private void DoLogin()
    {
        var result = _auth.Login(_email.Text, _password.Text);
        if (result.Success) { DialogResult = DialogResult.OK; Close(); }
        else { _error.Text = result.Message; _password.Clear(); _password.Focus(); }
    }
}
