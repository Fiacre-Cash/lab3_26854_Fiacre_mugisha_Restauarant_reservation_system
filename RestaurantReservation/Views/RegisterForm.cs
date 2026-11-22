using RestaurantReservation.Controllers;

namespace RestaurantReservation.Views;

public class RegisterForm : Form
{
    private readonly AuthController _auth = new();
    private readonly TextBox _name = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new() { UseSystemPasswordChar = true };
    private readonly TextBox _confirm = new() { UseSystemPasswordChar = true };
    private readonly Label _error = new() { ForeColor = Color.Firebrick, AutoSize = false };

    public RegisterForm()
    {
        Text = "Create account";
        ClientSize = new Size(400, 290);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;

        UiHelper.AddField(this, "Full name", _name, 20, 230, 110);
        UiHelper.AddField(this, "Email", _email, 55, 230, 110);
        UiHelper.AddField(this, "Password", _password, 90, 230, 110);
        UiHelper.AddField(this, "Confirm password", _confirm, 125, 230, 110);

        _error.SetBounds(15, 165, 370, 35);
        Controls.Add(_error);

        var create = UiHelper.MakeButton("Create account", 140, primary: true);
        create.SetBounds(125, 210, 140, 32);
        create.Click += (_, _) =>
        {
            var r = _auth.Register(_name.Text, _email.Text, _password.Text, _confirm.Text);
            if (r.Success) { DialogResult = DialogResult.OK; Close(); }
            else _error.Text = r.Message;
        };
        Controls.Add(create);
        AcceptButton = create;
    }
}
