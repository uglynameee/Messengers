using System;
using System.Windows.Forms;

namespace LibrarySystem
{
    public partial class LoginForm : Form
    {
        public string UserName { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            errorLabel.Text = "";
            if (!AccessDatabase.Authenticate(userNameTextBox.Text.Trim(), passwordTextBox.Text))
            {
                errorLabel.Text = "Неверный логин или пароль";
                passwordTextBox.SelectAll();
                passwordTextBox.Focus();
                return;
            }
            UserName = userNameTextBox.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
