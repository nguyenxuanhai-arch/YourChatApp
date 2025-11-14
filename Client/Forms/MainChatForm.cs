using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using YourChatApp.Client.Network;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Forms
{
    public class MainChatForm : Form
    {
        private class FriendInfo
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
        }

        private ClientSocket _clientSocket;
        private List<FriendRequest> _pendingRequests;
        private Dictionary<string, int> _friendUserIds; // Map friendUsername -> userId
        private List<FriendInfo> _friends; // Store friend details
        private int _currentChatFriendId = 0; // Currently selected friend for chat
        private string _currentChatFriendName = ""; // Currently selected friend's name
        private int _currentUserId = 0; // Current logged-in user's ID

        public MainChatForm(ClientSocket clientSocket, int userId = 0, List<FriendRequest> pendingRequests = null)
        {
            _clientSocket = clientSocket;
            _currentUserId = userId;
            _pendingRequests = pendingRequests ?? new List<FriendRequest>();
            _friendUserIds = new Dictionary<string, int>();
            _friends = new List<FriendInfo>();
            Console.WriteLine($"[MainChatForm] Initialized with userId={userId}, pendingRequests={_pendingRequests.Count}");
            BuildUI();
            _clientSocket.OnPacketReceived += HandleServerMessage;
            this.Load += MainChatForm_Load;
            this.FormClosing += MainChatForm_FormClosing;
        }

        private void MainChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Disconnect from server when closing main chat form
                if (_clientSocket != null && _clientSocket.IsConnected)
                {
                    _clientSocket.Disconnect();
                    Console.WriteLine("[MainChatForm] Disconnected from server on form close");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainChatForm] Error disconnecting: {ex.Message}");
            }
        }

        private void MainChatForm_Load(object sender, EventArgs e)
        {
            // Auto-load friends list on form load
            CommandPacket packet = new CommandPacket(CommandType.GET_FRIENDS);
            _clientSocket.SendPacket(packet);
            
            // Populate pending friend requests in notification panel
            PopulatePendingRequests();
        }

        private void PopulatePendingRequests()
        {
            ListBox notificationListBox = (ListBox)this.Controls.Find("notificationListBox", true).FirstOrDefault();
            if (notificationListBox != null)
            {
                notificationListBox.Items.Clear();
                foreach (var request in _pendingRequests)
                {
                    notificationListBox.Items.Add($"📝 {request.DisplayName} (@{request.Username})");
                }
            }
        }

        private void BuildUI()
        {
            this.Text = "YourChatApp - Main Chat";
            this.Size = new System.Drawing.Size(1200, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // MenuStrip
            MenuStrip menuStrip = new MenuStrip();
            
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&Logout", null, Logout_Click);
            fileMenu.DropDownItems.Add("E&xit", null, (s, e) => this.Close());
            menuStrip.Items.Add(fileMenu);

            ToolStripMenuItem friendsMenu = new ToolStripMenuItem("&Friends");
            friendsMenu.DropDownItems.Add("&Add Friend", null, AddFriend_Click);
            friendsMenu.DropDownItems.Add("&View Friend List", null, ViewFriends_Click);
            menuStrip.Items.Add(friendsMenu);

            ToolStripMenuItem callMenu = new ToolStripMenuItem("&Call");
            callMenu.DropDownItems.Add("&Video Call", null, VideoCall_Click);
            menuStrip.Items.Add(callMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Friends ListBox (Bên trái)
            Label friendsLabel = new Label();
            friendsLabel.Text = "Friends";
            friendsLabel.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            friendsLabel.Location = new System.Drawing.Point(10, 30);
            friendsLabel.Size = new System.Drawing.Size(200, 25);
            this.Controls.Add(friendsLabel);

            ListBox friendsListBox = new ListBox();
            friendsListBox.Name = "friendsListBox";
            friendsListBox.Location = new System.Drawing.Point(10, 60);
            friendsListBox.Size = new System.Drawing.Size(200, 480);
            friendsListBox.DoubleClick += FriendsList_DoubleClick;
            friendsListBox.SelectedIndexChanged += FriendsList_SelectedIndexChanged;
            this.Controls.Add(friendsListBox);

            // Chat Area (Ở giữa và bên phải)
            Label chatLabel = new Label();
            chatLabel.Text = "Messages";
            chatLabel.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            chatLabel.Location = new System.Drawing.Point(220, 30);
            chatLabel.Size = new System.Drawing.Size(500, 25);
            this.Controls.Add(chatLabel);

            // Video Call Button (top right of chat)
            Button videoCallButton = new Button();
            videoCallButton.Name = "videoCallButton";
            videoCallButton.Text = "📞 Video Call";
            videoCallButton.Location = new System.Drawing.Point(730, 30);
            videoCallButton.Size = new System.Drawing.Size(140, 25);
            videoCallButton.Click += VideoCall_Click;
            this.Controls.Add(videoCallButton);

            RichTextBox chatTextBox = new RichTextBox();
            chatTextBox.Name = "chatTextBox";
            chatTextBox.Location = new System.Drawing.Point(220, 60);
            chatTextBox.Size = new System.Drawing.Size(650, 380);
            chatTextBox.ReadOnly = true;
            this.Controls.Add(chatTextBox);

            // Message Input
            TextBox messageInputTextBox = new TextBox();
            messageInputTextBox.Name = "messageInputTextBox";
            messageInputTextBox.Location = new System.Drawing.Point(220, 450);
            messageInputTextBox.Size = new System.Drawing.Size(570, 90);
            messageInputTextBox.Multiline = true;
            messageInputTextBox.KeyDown += MessageInput_KeyDown;
            this.Controls.Add(messageInputTextBox);

            // Send Button
            Button sendButton = new Button();
            sendButton.Text = "Send";
            sendButton.Location = new System.Drawing.Point(795, 450);
            sendButton.Size = new System.Drawing.Size(75, 90);
            sendButton.Click += SendButton_Click;
            this.Controls.Add(sendButton);

            // Notifications Panel (Bên phải)
            Panel notificationPanel = new Panel();
            notificationPanel.Name = "notificationPanel";
            notificationPanel.Location = new System.Drawing.Point(880, 30);
            notificationPanel.Size = new System.Drawing.Size(300, 510);
            notificationPanel.BorderStyle = BorderStyle.FixedSingle;
            notificationPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(notificationPanel);

            Label notificationLabel = new Label();
            notificationLabel.Text = "🔔 Friend Requests";
            notificationLabel.Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold);
            notificationLabel.Location = new System.Drawing.Point(10, 10);
            notificationLabel.Size = new System.Drawing.Size(280, 25);
            notificationPanel.Controls.Add(notificationLabel);

            ListBox notificationListBox = new ListBox();
            notificationListBox.Name = "notificationListBox";
            notificationListBox.Location = new System.Drawing.Point(10, 40);
            notificationListBox.Size = new System.Drawing.Size(280, 460);
            notificationListBox.BackColor = System.Drawing.Color.White;
            notificationListBox.DoubleClick += NotificationListBox_DoubleClick;
            notificationPanel.Controls.Add(notificationListBox);

            // Status Label
            Label statusLabel = new Label();
            statusLabel.Name = "statusLabel";
            statusLabel.Location = new System.Drawing.Point(10, 550);
            statusLabel.Size = new System.Drawing.Size(850, 20);
            statusLabel.Text = "Connected";
            statusLabel.ForeColor = System.Drawing.Color.Green;
            this.Controls.Add(statusLabel);
        }

        private void AddFriend_Click(object sender, EventArgs e)
        {
            string friendUsername = PromptDialog("Enter friend's username:");
            if (!string.IsNullOrEmpty(friendUsername))
            {
                // Gửi ADD_FRIEND command tới server
                // Server sẽ lookup username và lấy userId
                var addFriendData = new Dictionary<string, object>
                {
                    { "friendUsername", friendUsername }
                };
                CommandPacket packet = new CommandPacket(CommandType.ADD_FRIEND, addFriendData);
                _clientSocket.SendPacket(packet);
                MessageBox.Show($"Friend request sent to {friendUsername}");
            }
        }

        private void ViewFriends_Click(object sender, EventArgs e)
        {
            // Gửi GET_FRIENDS command tới server
            CommandPacket packet = new CommandPacket(CommandType.GET_FRIENDS);
            _clientSocket.SendPacket(packet);
        }


        private void VideoCall_Click(object sender, EventArgs e)
        {
            if (_currentChatFriendId <= 0 || string.IsNullOrEmpty(_currentChatFriendName))
            {
                MessageBox.Show("Please select a friend first");
                return;
            }

            try
            {
                // Send video call request to server
                var callData = new Dictionary<string, object>
                {
                    { "receiverId", _currentChatFriendId }
                };
                CommandPacket packet = PacketProcessor.CreateCommand(CommandType.VIDEO_CALL_REQUEST, callData);
                _clientSocket.SendPacket(packet);

                RichTextBox chatTextBox = (RichTextBox)this.Controls["chatTextBox"];
                chatTextBox.AppendText($"📞 You initiated a video call with {_currentChatFriendName}\n");

                // Note: VideoCallForm will open when receiving VIDEO_CALL_ACCEPT response
                // Don't open it immediately - wait for confirmation
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initiating video call: {ex.Message}");
            }
        }

        private void FriendsList_DoubleClick(object sender, EventArgs e)
        {
            ListBox friendsListBox = (ListBox)this.Controls["friendsListBox"];
            if (friendsListBox.SelectedIndex >= 0)
            {
                int selectedIndex = friendsListBox.SelectedIndex;
                if (selectedIndex < _friends.Count)
                {
                    var friend = _friends[selectedIndex];
                    _currentChatFriendId = friend.UserId;
                    _currentChatFriendName = friend.DisplayName;
                    
                    // Load chat history in main form
                    RichTextBox chatTextBox = (RichTextBox)this.Controls["chatTextBox"];
                    chatTextBox.Clear();
                    chatTextBox.AppendText($"=== Chat with {friend.DisplayName} (@{friend.Username}) ===\n\n");
                    chatTextBox.AppendText("Loading messages...\n");
                    
                    // Request message history from server
                    var messageData = new Dictionary<string, object>
                    {
                        { "fromUserId", friend.UserId }
                    };
                    CommandPacket packet = PacketProcessor.CreateCommand(CommandType.GET_MESSAGES, messageData);
                    _clientSocket.SendPacket(packet);
                }
            }
        }

        private void FriendsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox friendsListBox = (ListBox)this.Controls["friendsListBox"];
            if (friendsListBox.SelectedIndex >= 0)
            {
                int selectedIndex = friendsListBox.SelectedIndex;
                if (selectedIndex < _friends.Count)
                {
                    var friend = _friends[selectedIndex];
                    _currentChatFriendId = friend.UserId;
                    _currentChatFriendName = friend.DisplayName;
                    Console.WriteLine($"[CLIENT] Friend selected: {_currentChatFriendName} (ID: {_currentChatFriendId})");
                }
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            TextBox messageInputTextBox = (TextBox)this.Controls["messageInputTextBox"];
            RichTextBox chatTextBox = (RichTextBox)this.Controls["chatTextBox"];
            string message = messageInputTextBox.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            if (_currentChatFriendId <= 0)
            {
                MessageBox.Show("Please select a friend first");
                return;
            }

            // Send message to server
            var messageData = new Dictionary<string, object>
            {
                { "receiverId", _currentChatFriendId },
                { "content", message }
            };
            CommandPacket packet = PacketProcessor.CreateCommand(CommandType.CHAT_MESSAGE, messageData);
            bool sent = _clientSocket.SendPacket(packet);

            if (sent)
            {
                // Display sent message (right-aligned - "You")
                chatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Right;
                chatTextBox.SelectionColor = System.Drawing.Color.Blue;
                chatTextBox.AppendText($"You: {message}\n");
                chatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                chatTextBox.SelectionColor = System.Drawing.Color.Black;
                messageInputTextBox.Text = "";
                chatTextBox.ScrollToCaret();
            }
            else
            {
                MessageBox.Show("Failed to send message");
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && !e.Shift)
            {
                SendButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void HandleServerMessage(CommandPacket packet)
        {
            this.Invoke(new Action(() =>
            {
                RichTextBox chatTextBox = (RichTextBox)this.Controls["chatTextBox"];
                ListBox notificationListBox = (ListBox)this.Controls.Find("notificationListBox", true).FirstOrDefault() as ListBox;

                switch (packet.Command)
                {
                    case CommandType.FRIEND_REQUEST:
                        if (packet.Data.ContainsKey("fromUsername"))
                        {
                            string fromUsername = packet.Data["fromUsername"].ToString();
                            int fromUserId = Convert.ToInt32(packet.Data["fromUserId"]);
                            string displayName = packet.Data.ContainsKey("fromDisplayName") 
                                ? packet.Data["fromDisplayName"].ToString() 
                                : fromUsername;
                            
                            // Add to pending requests list
                            var request = new FriendRequest 
                            { 
                                UserId = fromUserId, 
                                Username = fromUsername,
                                DisplayName = displayName
                            };
                            _pendingRequests.Add(request);
                            Console.WriteLine($"[CLIENT] Added pending friend request from {fromUsername}, total: {_pendingRequests.Count}");
                            
                            // Update notification list box with formatted item
                            if (notificationListBox != null)
                            {
                                notificationListBox.Items.Add($"📝 {displayName} (@{fromUsername})");
                            }
                            
                            // Also notify in chat
                            chatTextBox.AppendText($"[📬 New] Friend request from {displayName}\n");
                        }
                        break;

                    case CommandType.GET_FRIENDS:
                        if (packet.Data.ContainsKey("friends"))
                        {
                            ListBox friendsListBox = (ListBox)this.Controls["friendsListBox"];
                            friendsListBox.Items.Clear();
                            _friendUserIds.Clear();
                            _friends.Clear();
                            
                            chatTextBox.Clear();
                            chatTextBox.AppendText("=== Friends List ===\n\n");
                            
                            var friendsList = packet.Data["friends"];
                            
                            if (friendsList is System.Collections.IEnumerable enumerable)
                            {
                                int count = 0;
                                foreach (var friendObj in enumerable)
                                {
                                    count++;
                                    int userId = 0;
                                    string username = "Unknown";
                                    string displayName = "Unknown";
                                    string statusStr = "Offline";
                                    
                                    // Handle Dictionary<string, object>
                                    if (friendObj is Dictionary<string, object> friendDict)
                                    {
                                        userId = friendDict.ContainsKey("userId") ? Convert.ToInt32(friendDict["userId"]) : 0;
                                        username = friendDict.ContainsKey("username") ? friendDict["username"].ToString() : "Unknown";
                                        displayName = friendDict.ContainsKey("displayName") ? friendDict["displayName"].ToString() : username;
                                        int status = friendDict.ContainsKey("status") ? Convert.ToInt32(friendDict["status"]) : 0;
                                        statusStr = status == 1 ? "Online" : "Offline";
                                    }
                                    // Handle dynamic/JObject from Newtonsoft.Json
                                    else
                                    {
                                        dynamic friend = friendObj;
                                        try
                                        {
                                            userId = friend.userId ?? 0;
                                            username = friend.username?.ToString() ?? "Unknown";
                                            displayName = friend.displayName?.ToString() ?? username;
                                            int status = friend.status ?? 0;
                                            statusStr = status == 1 ? "Online" : "Offline";
                                        }
                                        catch
                                        {
                                            // Fallback if dynamic access fails
                                        }
                                    }
                                    
                                    // Store friend data
                                    var friendInfo = new FriendInfo 
                                    { 
                                        UserId = userId, 
                                        Username = username, 
                                        DisplayName = displayName 
                                    };
                                    _friends.Add(friendInfo);
                                    
                                    if (!_friendUserIds.ContainsKey(username))
                                        _friendUserIds[username] = userId;
                                    
                                    // Add to sidebar ListBox with online/offline indicator
                                    string indicator = statusStr == "Online" ? "🟢" : "⚪";
                                    friendsListBox.Items.Add($"{indicator} {displayName}");
                                    
                                    chatTextBox.AppendText($"• {displayName} (@{username}) - {statusStr}\n");
                                }
                                
                                if (count == 0)
                                {
                                    chatTextBox.AppendText("No friends yet\n");
                                }
                            }
                            else
                            {
                                chatTextBox.AppendText("No friends yet\n");
                            }
                        }
                        break;

                    case CommandType.GET_MESSAGES:
                        if (packet.Data.ContainsKey("messages"))
                        {
                            chatTextBox.Clear();
                            
                            if (_currentChatFriendId > 0)
                                chatTextBox.AppendText($"=== Chat with {_currentChatFriendName} ===\n\n");
                            
                            var messagesList = packet.Data["messages"];
                            
                            if (messagesList is System.Collections.IEnumerable enumerable)
                            {
                                int count = 0;
                                foreach (var msgObj in enumerable)
                                {
                                    count++;
                                    int senderId = 0;
                                    string content = "";
                                    
                                    // Handle Dictionary<string, object>
                                    if (msgObj is Dictionary<string, object> msgDict)
                                    {
                                        senderId = msgDict.ContainsKey("senderId") ? Convert.ToInt32(msgDict["senderId"]) : 0;
                                        content = msgDict.ContainsKey("content") ? msgDict["content"].ToString() : "";
                                    }
                                    // Handle dynamic/JObject from Newtonsoft.Json
                                    else
                                    {
                                        dynamic msg = msgObj;
                                        try
                                        {
                                            senderId = msg.senderId ?? 0;
                                            content = msg.content?.ToString() ?? "";
                                        }
                                        catch
                                        {
                                            // Fallback if dynamic access fails
                                        }
                                    }
                                    
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        if (senderId == _currentUserId)
                                        {
                                            // Your message - right aligned, blue color
                                            chatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Right;
                                            chatTextBox.SelectionColor = System.Drawing.Color.Blue;
                                            chatTextBox.AppendText($"You: {content}\n");
                                        }
                                        else
                                        {
                                            // Friend's message - left aligned, black color
                                            chatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                                            chatTextBox.SelectionColor = System.Drawing.Color.Black;
                                            chatTextBox.AppendText($"{_currentChatFriendName}: {content}\n");
                                        }
                                        chatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                                        chatTextBox.SelectionColor = System.Drawing.Color.Black;
                                    }
                                }
                                
                                if (count == 0)
                                    chatTextBox.AppendText("[No messages yet]\n");
                            }
                            else
                            {
                                chatTextBox.AppendText("[No messages available]\n");
                            }
                            
                            chatTextBox.ScrollToCaret();
                        }
                        break;

                    case CommandType.CHAT_MESSAGE:
                        if (packet.Data.ContainsKey("content"))
                        {
                            string fromUsername = packet.Data.ContainsKey("fromUsername") 
                                ? packet.Data["fromUsername"].ToString() 
                                : _currentChatFriendName;
                            string content = packet.Data["content"].ToString();
                            
                            // Friend's message - left aligned, black color
                            chatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                            chatTextBox.SelectionColor = System.Drawing.Color.Black;
                            chatTextBox.AppendText($"{fromUsername}: {content}\n");
                            chatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                            chatTextBox.SelectionColor = System.Drawing.Color.Black;
                            chatTextBox.ScrollToCaret();
                        }
                        break;


                    case CommandType.VIDEO_CALL_REQUEST:
                        if (packet.Data.ContainsKey("callerId"))
                        {
                            try
                            {
                                string callId = packet.Data.ContainsKey("callId") 
                                    ? packet.Data["callId"].ToString() 
                                    : Guid.NewGuid().ToString();
                                string callerName = packet.Data.ContainsKey("callerName") 
                                    ? packet.Data["callerName"].ToString() 
                                    : "Unknown";
                                int callerId = Convert.ToInt32(packet.Data["callerId"]);
                                
                                chatTextBox.AppendText($"📞 Incoming video call from {callerName}\n");
                                
                                DialogResult result = MessageBox.Show(
                                    $"Incoming video call from {callerName}. Accept?",
                                    "Video Call",
                                    MessageBoxButtons.YesNo);
                                
                                if (result == DialogResult.Yes)
                                {
                                    // Send accept to server
                                    var acceptData = new Dictionary<string, object>
                                    {
                                        { "callId", callId }
                                    };
                                    CommandPacket acceptPacket = PacketProcessor.CreateCommand(CommandType.VIDEO_CALL_ACCEPT, acceptData);
                                    _clientSocket.SendPacket(acceptPacket);

                                    chatTextBox.AppendText($"📞 You accepted the video call\n");
                                    
                                    // Open video call form
                                    VideoCallForm videoForm = new VideoCallForm(_clientSocket, callerId, callerName, _currentUserId);
                                    videoForm.FormClosed += (s, e) =>
                                    {
                                        try
                                        {
                                            RichTextBox chatBox = (RichTextBox)this.Controls["chatTextBox"];
                                            if (chatBox != null)
                                                chatBox.AppendText($"📞 Video call ended\n");
                                        }
                                        catch { }
                                    };
                                    videoForm.Show();
                                }
                                else
                                {
                                    // Send reject to server
                                    var rejectData = new Dictionary<string, object>
                                    {
                                        { "callId", callId }
                                    };
                                    CommandPacket rejectPacket = PacketProcessor.CreateCommand(CommandType.VIDEO_CALL_REJECT, rejectData);
                                    _clientSocket.SendPacket(rejectPacket);

                                    chatTextBox.AppendText($"📞 You rejected the video call\n");
                                }
                            }
                            catch (Exception ex)
                            {
                                chatTextBox.AppendText($"📞 Error handling video call: {ex.Message}\n");
                            }
                        }
                        break;

                    case CommandType.VIDEO_CALL_ACCEPT:
                        try
                        {
                            // Only open form for COMMAND packets (from server notifying caller)
                            // Response packets have Message = "Call accepted", Command packets have Message = "OK"
                            if (packet.Message != null && packet.Message.Contains("Call accepted"))
                            {
                                // This is a response confirmation to receiver - don't open form (they already opened it)
                                chatTextBox.AppendText($"📞 Your call acceptance confirmed\n");
                                break;
                            }
                            
                            string callId = packet.Data.ContainsKey("callId") ? packet.Data["callId"].ToString() : "";
                            chatTextBox.AppendText($"📞 Video call accepted by friend\n");
                            
                            // Open video call form when call is accepted (CALLER SIDE ONLY)
                            if (_currentChatFriendId > 0 && !string.IsNullOrEmpty(_currentChatFriendName))
                            {
                                VideoCallForm videoForm = new VideoCallForm(_clientSocket, _currentChatFriendId, _currentChatFriendName, _currentUserId);
                                videoForm.FormClosed += (s, e) =>
                                {
                                    try
                                    {
                                        RichTextBox chatBox = (RichTextBox)this.Controls["chatTextBox"];
                                        if (chatBox != null)
                                            chatBox.AppendText($"📞 Video call ended\n");
                                    }
                                    catch { }
                                };
                                videoForm.Show();
                            }
                        }
                        catch (Exception ex)
                        {
                            chatTextBox.AppendText($"📞 Error: {ex.Message}\n");
                        }
                        break;

                    case CommandType.VIDEO_CALL_REJECT:
                        try
                        {
                            chatTextBox.AppendText($"📞 Video call was rejected\n");
                        }
                        catch { }
                        break;
                }
            }));
        }

        private void NotificationListBox_DoubleClick(object sender, EventArgs e)
        {
            ListBox notificationListBox = sender as ListBox;
            if (notificationListBox != null && notificationListBox.SelectedIndex >= 0 && _pendingRequests.Count > 0)
            {
                int selectedIndex = notificationListBox.SelectedIndex;
                if (selectedIndex < _pendingRequests.Count)
                {
                    FriendRequest request = _pendingRequests[selectedIndex];
                    
                    // Show dialog to accept/reject
                    DialogResult result = MessageBox.Show(
                        $"Accept friend request from {request.DisplayName}?",
                        "Friend Request",
                        MessageBoxButtons.YesNo);
                    
                    if (result == DialogResult.Yes)
                    {
                        var acceptData = new Dictionary<string, object>
                        {
                            { "friendUserId", request.UserId }
                        };
                        CommandPacket acceptPacket = new CommandPacket(CommandType.ACCEPT_FRIEND, acceptData);
                        _clientSocket.SendPacket(acceptPacket);
                        
                        RichTextBox chatTextBox = (RichTextBox)this.Controls["chatTextBox"];
                        chatTextBox.AppendText($"✓ Accepted friend request from {request.DisplayName}\n");
                    }
                    else
                    {
                        var rejectData = new Dictionary<string, object>
                        {
                            { "friendUserId", request.UserId }
                        };
                        CommandPacket rejectPacket = new CommandPacket(CommandType.REJECT_FRIEND, rejectData);
                        _clientSocket.SendPacket(rejectPacket);
                        
                        RichTextBox chatTextBox = (RichTextBox)this.Controls["chatTextBox"];
                        chatTextBox.AppendText($"✗ Rejected friend request from {request.DisplayName}\n");
                    }
                    
                    // Remove from UI and list
                    notificationListBox.Items.RemoveAt(selectedIndex);
                    _pendingRequests.RemoveAt(selectedIndex);
                }
            }
        }

        private string PromptDialog(string prompt)
        {
            Form form = new Form();
            form.Text = "Input";
            form.Width = 300;
            form.Height = 150;
            form.StartPosition = FormStartPosition.CenterParent;

            Label label = new Label();
            label.Left = 20;
            label.Top = 20;
            label.Text = prompt;
            label.Width = 250;

            TextBox textBox = new TextBox();
            textBox.Left = 20;
            textBox.Top = 50;
            textBox.Width = 250;

            Button button = new Button();
            button.Text = "OK";
            button.Left = 120;
            button.Top = 80;
            button.DialogResult = DialogResult.OK;
            button.Click += (s, e) => form.Close();

            form.Controls.Add(label);
            form.Controls.Add(textBox);
            form.Controls.Add(button);
            form.AcceptButton = button;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to logout?",
                    "Logout Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Disconnect from server
                    if (_clientSocket != null && _clientSocket.IsConnected)
                    {
                        _clientSocket.Disconnect();
                        Console.WriteLine("[MainChatForm] Logged out and disconnected");
                    }

                    // Close this form and return to login
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Logout Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Helper class for pending friend requests
    public class FriendRequest
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }
}
