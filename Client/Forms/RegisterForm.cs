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

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
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
                statusLabel.Text = $"Error: {ex.Message}";
            }
        }

        private void HandleServerResponse(CommandPacket packet)
        {
            this.Invoke(new Action(() =>
            {
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
