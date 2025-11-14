using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using YourChatApp.Client.Network;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Forms
{
    public partial class RegisterForm : Form
    {
        private ClientSocket _clientSocket;

        public RegisterForm()
        {
            InitializeComponent();
            _clientSocket = new ClientSocket();
            _clientSocket.OnPacketReceived += HandleServerResponse;
        }

        private void InitializeComponent()
        {
            this.Text = "YourChatApp - Register";
            this.Size = new System.Drawing.Size(450, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "Create New Account";
            titleLabel.Font = new System.Drawing.Font("Arial", 18, System.Drawing.FontStyle.Bold);
            titleLabel.Location = new System.Drawing.Point(100, 20);
            titleLabel.Size = new System.Drawing.Size(250, 40);
            this.Controls.Add(titleLabel);

            // Username Label & TextBox
            Label usernameLabel = new Label();
            usernameLabel.Text = "Username:";
            usernameLabel.Location = new System.Drawing.Point(20, 80);
            usernameLabel.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(usernameLabel);

            TextBox usernameTextBox = new TextBox();
            usernameTextBox.Name = "usernameTextBox";
            usernameTextBox.Location = new System.Drawing.Point(130, 80);
            usernameTextBox.Size = new System.Drawing.Size(280, 20);
            this.Controls.Add(usernameTextBox);

            // Email Label & TextBox
            Label emailLabel = new Label();
            emailLabel.Text = "Email:";
            emailLabel.Location = new System.Drawing.Point(20, 120);
            emailLabel.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(emailLabel);

            TextBox emailTextBox = new TextBox();
            emailTextBox.Name = "emailTextBox";
            emailTextBox.Location = new System.Drawing.Point(130, 120);
            emailTextBox.Size = new System.Drawing.Size(280, 20);
            this.Controls.Add(emailTextBox);

            // Display Name Label & TextBox
            Label displayNameLabel = new Label();
            displayNameLabel.Text = "Display Name:";
            displayNameLabel.Location = new System.Drawing.Point(20, 160);
            displayNameLabel.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(displayNameLabel);

            TextBox displayNameTextBox = new TextBox();
            displayNameTextBox.Name = "displayNameTextBox";
            displayNameTextBox.Location = new System.Drawing.Point(130, 160);
            displayNameTextBox.Size = new System.Drawing.Size(280, 20);
            this.Controls.Add(displayNameTextBox);

            // Password Label & TextBox
            Label passwordLabel = new Label();
            passwordLabel.Text = "Password:";
            passwordLabel.Location = new System.Drawing.Point(20, 200);
            passwordLabel.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(passwordLabel);

            TextBox passwordTextBox = new TextBox();
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.Location = new System.Drawing.Point(130, 200);
            passwordTextBox.Size = new System.Drawing.Size(280, 20);
            passwordTextBox.UseSystemPasswordChar = true;
            this.Controls.Add(passwordTextBox);

            // Confirm Password Label & TextBox
            Label confirmPasswordLabel = new Label();
            confirmPasswordLabel.Text = "Confirm Password:";
            confirmPasswordLabel.Location = new System.Drawing.Point(20, 240);
            confirmPasswordLabel.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(confirmPasswordLabel);

            TextBox confirmPasswordTextBox = new TextBox();
            confirmPasswordTextBox.Name = "confirmPasswordTextBox";
            confirmPasswordTextBox.Location = new System.Drawing.Point(130, 240);
            confirmPasswordTextBox.Size = new System.Drawing.Size(280, 20);
            confirmPasswordTextBox.UseSystemPasswordChar = true;
            this.Controls.Add(confirmPasswordTextBox);

            // Register Button
            Button registerButton = new Button();
            registerButton.Name = "registerButton";
            registerButton.Text = "Register";
            registerButton.Location = new System.Drawing.Point(130, 290);
            registerButton.Size = new System.Drawing.Size(100, 40);
            registerButton.Click += RegisterButton_Click;
            this.Controls.Add(registerButton);

            // Cancel Button
            Button cancelButton = new Button();
            cancelButton.Name = "cancelButton";
            cancelButton.Text = "Cancel";
            cancelButton.Location = new System.Drawing.Point(280, 290);
            cancelButton.Size = new System.Drawing.Size(100, 40);
            cancelButton.Click += (s, e) => this.Close();
            this.Controls.Add(cancelButton);

            // Status Label
            Label statusLabel = new Label();
            statusLabel.Name = "statusLabel";
            statusLabel.Location = new System.Drawing.Point(20, 340);
            statusLabel.Size = new System.Drawing.Size(400, 30);
            statusLabel.ForeColor = System.Drawing.Color.Red;
            this.Controls.Add(statusLabel);
        }

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                TextBox usernameTextBox = (TextBox)this.Controls["usernameTextBox"];
                TextBox emailTextBox = (TextBox)this.Controls["emailTextBox"];
                TextBox displayNameTextBox = (TextBox)this.Controls["displayNameTextBox"];
                TextBox passwordTextBox = (TextBox)this.Controls["passwordTextBox"];
                TextBox confirmPasswordTextBox = (TextBox)this.Controls["confirmPasswordTextBox"];
                Label statusLabel = (Label)this.Controls["statusLabel"];

                string username = usernameTextBox.Text.Trim();
                string email = emailTextBox.Text.Trim();
                string displayName = displayNameTextBox.Text.Trim();
                string password = passwordTextBox.Text;
                string confirmPassword = confirmPasswordTextBox.Text;

                // Validation
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    statusLabel.Text = "Please fill all fields";
                    return;
                }

                if (password != confirmPassword)
                {
                    statusLabel.Text = "Passwords do not match";
                    return;
                }

                if (password.Length < 6)
                {
                    statusLabel.Text = "Password must be at least 6 characters";
                    return;
                }

                statusLabel.Text = "Connecting to server...";
                this.Refresh();

                // Kết nối tới server
                bool connected = await _clientSocket.ConnectAsync();
                if (!connected)
                {
                    statusLabel.Text = "Failed to connect to server";
                    return;
                }

                statusLabel.Text = "Registering...";
                this.Refresh();

                string passwordHash = HashPassword(password);
                var registerData = new Dictionary<string, object>
                {
                    { "username", username },
                    { "email", email },
                    { "displayName", displayName },
                    { "passwordHash", passwordHash }
                };

                CommandPacket registerPacket = PacketProcessor.CreateCommand(CommandType.REGISTER, registerData);
                _clientSocket.SendPacket(registerPacket);
            }
            catch (Exception ex)
            {
                Label statusLabel = (Label)this.Controls["statusLabel"];
                statusLabel.Text = $"Error: {ex.Message}";
            }
        }

        private void HandleServerResponse(CommandPacket packet)
        {
            this.Invoke(new Action(() =>
            {
                Label statusLabel = (Label)this.Controls["statusLabel"];

                if (packet.Command == CommandType.REGISTER && packet.StatusCode == 201)
                {
                    statusLabel.Text = "Registration successful! Please login.";
                    statusLabel.ForeColor = System.Drawing.Color.Green;
                    System.Threading.Thread.Sleep(2000);
                    this.Close();
                }
                else if (packet.Command == CommandType.ERROR)
                {
                    statusLabel.Text = $"Registration failed: {packet.Message}";
                    statusLabel.ForeColor = System.Drawing.Color.Red;
                }
            }));
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
