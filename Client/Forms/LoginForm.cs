using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using YourChatApp.Client.Network;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Forms
{
    public partial class LoginForm : Form
    {
        private ClientSocket _clientSocket;
        private List<FriendRequest> _pendingRequests;

        public LoginForm()
        {
            InitializeComponent();
            _clientSocket = new ClientSocket();
            _pendingRequests = new List<FriendRequest>();
            _clientSocket.OnPacketReceived += HandleServerResponse;
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            try
            {
                string username = usernameTextBox.Text.Trim();
                string password = passwordTextBox.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    statusLabel.Text = "Please enter username and password";
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

                // Gửi yêu cầu login
                statusLabel.Text = "Logging in...";
                this.Refresh();

                string passwordHash = HashPassword(password);
                var loginData = new Dictionary<string, object>
                {
                    { "username", username },
                    { "passwordHash", passwordHash }
                };

                CommandPacket loginPacket = PacketProcessor.CreateCommand(CommandType.LOGIN, loginData);
                _clientSocket.SendPacket(loginPacket);
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error: {ex.Message}";
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            // Mở form đăng ký
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
        }

        private void HandleServerResponse(CommandPacket packet)
        {
            Console.WriteLine($"[LoginForm] Received packet: {packet.Command}");
            // Chạy trên UI thread
            this.Invoke(new Action(() =>
            {
                // Collect pending friend requests
                if (packet.Command == CommandType.FRIEND_REQUEST)
                {
                    if (packet.Data.ContainsKey("fromUsername"))
                    {
                        string fromUsername = packet.Data["fromUsername"].ToString();
                        int fromUserId = Convert.ToInt32(packet.Data["fromUserId"]);
                        string displayName = packet.Data.ContainsKey("fromDisplayName")
                            ? packet.Data["fromDisplayName"].ToString()
                            : fromUsername;

                        var request = new FriendRequest
                        {
                            UserId = fromUserId,
                            Username = fromUsername,
                            DisplayName = displayName
                        };
                        _pendingRequests.Add(request);
                        Console.WriteLine($"[LoginForm] Collected pending request from {fromUsername}");
                    }
                    return; // Don't process further
                }

                if (packet.Command == CommandType.LOGIN && packet.StatusCode == 200)
                {
                    statusLabel.Text = "Login successful!";
                    statusLabel.ForeColor = System.Drawing.Color.Green;

                    // Extract user ID from response
                    int userId = 0;
                    if (packet.Data.ContainsKey("userId"))
                    {
                        userId = Convert.ToInt32(packet.Data["userId"]);
                    }

                    Console.WriteLine($"[LoginForm] Creating MainChatForm with userId={userId}, pendingRequests={_pendingRequests.Count}");
                    // Chuyển tới main chat form with user ID and pending requests
                    MainChatForm mainForm = new MainChatForm(_clientSocket, userId, _pendingRequests);
                    Console.WriteLine($"[LoginForm] MainChatForm created, showing...");
                    mainForm.Show();
                    this.Hide();
                }
                else if (packet.Command == CommandType.ERROR)
                {
                    statusLabel.Text = $"Login failed: {packet.Message}";
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
