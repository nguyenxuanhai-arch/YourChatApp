using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using YourChatApp.Client.Network;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Forms
{
    public partial class MainChatForm : Form
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
        private List<Shared.Models.Group> _groups = new List<Shared.Models.Group>();
        private int _currentGroupId = 0;
        private string _currentGroupName = "";
        // In-memory storage for group members and friend usernames (UI moved to modals)
        private List<string> _currentGroupMembers = new List<string>();
        // Structured current group members with IDs for management operations
        private List<YourChatApp.Shared.Models.User> _currentGroupMemberUsers = new List<YourChatApp.Shared.Models.User>();
        private List<string> _friendsUsernames = new List<string>();

        public MainChatForm(ClientSocket clientSocket, int userId = 0, List<FriendRequest> pendingRequests = null)
        {
            InitializeComponent();

            _clientSocket = clientSocket;
            _currentUserId = userId;
            _pendingRequests = pendingRequests ?? new List<FriendRequest>();
            _friendUserIds = new Dictionary<string, int>();
            _friends = new List<FriendInfo>();
            Console.WriteLine($"[MainChatForm] Initialized with userId={userId}, pendingRequests={_pendingRequests.Count}");
            _clientSocket.OnPacketReceived += HandleServerMessage;
            this.Load += MainChatForm_Load;
            this.FormClosing += MainChatForm_FormClosing;
            // Wire double-click on groups list to load group messages
            try { this.groupsListBox.DoubleClick += GroupsListBox_DoubleClick; } catch { }
            // Wire Enter key on group message input to send (Shift+Enter -> newline)
            try { this.groupMessageInput.KeyDown += GroupMessageInput_KeyDown; } catch { }
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
            // Request groups list
            CommandPacket groupPacket = new CommandPacket(CommandType.GET_GROUPS);
            _clientSocket.SendPacket(groupPacket);
            
            // Populate pending friend requests in notification panel
            PopulatePendingRequests();
        }

        private void PopulatePendingRequests()
        {
            notificationListBox.Items.Clear();
            foreach (var request in _pendingRequests)
            {
                notificationListBox.Items.Add($"📝 {request.DisplayName} (@{request.Username})");
            }
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
            if (friendsListBox.SelectedIndex >= 0)
            {
                int selectedIndex = friendsListBox.SelectedIndex;
                if (selectedIndex < _friends.Count)
                {
                    var friend = _friends[selectedIndex];
                    _currentChatFriendId = friend.UserId;
                    _currentChatFriendName = friend.DisplayName;
                    
                    // Load chat history in main form
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

                        case CommandType.GROUP_MESSAGE:
                            // Broadcasted group message
                            HandleIncomingGroupMessage(packet);
                            break;

                    case CommandType.GET_FRIENDS:
                        if (packet.Data.ContainsKey("friends"))
                        {
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
                            // Update friends selection list for group creation
                            try { UpdateFriendsForGroup(); } catch { }
                        }
                        break;

                    case CommandType.GET_GROUPS:
                        // Handle groups list
                        if (packet.Data.ContainsKey("groups"))
                        {
                            try
                            {
                                var groupsObj = packet.Data["groups"];
                                _groups.Clear();
                                groupsListBox.Items.Clear();

                                if (groupsObj is System.Collections.IEnumerable enumerable)
                                {
                                    foreach (var g in enumerable)
                                    {
                                        try
                                        {
                                            // Expect dictionary-like objects
                                            if (g is System.Collections.Generic.IDictionary<string, object> dict)
                                            {
                                                int gid = dict.ContainsKey("groupId") ? Convert.ToInt32(dict["groupId"]) : 0;
                                                string gname = dict.ContainsKey("groupName") ? dict["groupName"].ToString() : "Unnamed";
                                                string desc = dict.ContainsKey("description") ? dict["description"].ToString() : "";
                                                int createdBy = dict.ContainsKey("createdBy") ? Convert.ToInt32(dict["createdBy"]) : 0;
                                                DateTime createdAt = dict.ContainsKey("createdAt") ? Convert.ToDateTime(dict["createdAt"]) : DateTime.Now;

                                                var group = new Shared.Models.Group
                                                {
                                                    GroupId = gid,
                                                    GroupName = gname,
                                                    Description = desc,
                                                    CreatedBy = createdBy,
                                                    CreatedAt = createdAt
                                                };
                                                _groups.Add(group);
                                                groupsListBox.Items.Add(gname);
                                            }
                                            else
                                            {
                                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(g);
                                                var group = Newtonsoft.Json.JsonConvert.DeserializeObject<Shared.Models.Group>(json);
                                                if (group != null)
                                                {
                                                    _groups.Add(group);
                                                    groupsListBox.Items.Add(group.GroupName);
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                            catch { }
                        }

                        // Handle members list response (when GET_GROUPS with groupId requested)
                        if (packet.Data.ContainsKey("members"))
                        {
                            try
                            {
                                _currentGroupMembers.Clear();
                        // Handle messages for the group (when GET_GROUPS with includeMessages=true)
                        if (packet.Data.ContainsKey("messages"))
                        {
                            try
                            {
                                groupChatTextBox.Clear();
                                if (!string.IsNullOrEmpty(_currentGroupName))
                                    groupChatTextBox.AppendText($"=== Group: {_currentGroupName} ===\n\n");

                                var messagesObj = packet.Data["messages"];
                                if (messagesObj is System.Collections.IEnumerable msgEnum)
                                {
                                    int mcount = 0;
                                    foreach (var mo in msgEnum)
                                    {
                                        mcount++;
                                        int senderId = 0;
                                        string fromUsername = null;
                                        string senderDisplayName = null;
                                        string content = "";

                                        // Support multiple runtime representations (Dictionary, JObject, dynamic)
                                            if (mo is System.Collections.Generic.IDictionary<string, object> md)
                                            {
                                                senderId = md.ContainsKey("senderId") ? Convert.ToInt32(md["senderId"]) : 0;
                                                fromUsername = md.ContainsKey("fromUsername") ? md["fromUsername"]?.ToString() : (md.ContainsKey("senderUsername") ? md["senderUsername"]?.ToString() : null);
                                                senderDisplayName = md.ContainsKey("senderDisplayName") ? md["senderDisplayName"]?.ToString() : null;
                                                content = md.ContainsKey("content") ? md["content"]?.ToString() : "";
                                            }
                                            else if (mo is Newtonsoft.Json.Linq.JObject jObj)
                                            {
                                                try { senderId = jObj["senderId"] != null ? jObj.Value<int?>("senderId") ?? 0 : 0; } catch { senderId = 0; }
                                                try { fromUsername = jObj["fromUsername"] != null ? jObj.Value<string>("fromUsername") : (jObj["senderUsername"] != null ? jObj.Value<string>("senderUsername") : null); } catch { fromUsername = null; }
                                                try { senderDisplayName = jObj["senderDisplayName"] != null ? jObj.Value<string>("senderDisplayName") : null; } catch { senderDisplayName = null; }
                                                try { content = jObj["content"] != null ? jObj.Value<string>("content") : ""; } catch { content = ""; }
                                            }
                                        else
                                        {
                                            try
                                            {
                                                dynamic dm = mo;
                                                senderId = dm.senderId ?? 0;
                                                fromUsername = dm.fromUsername ?? dm.senderUsername ?? null;
                                                content = dm.content ?? "";
                                            }
                                            catch { }
                                        }

                                        if (!string.IsNullOrEmpty(content))
                                        {
                                            if (senderId == _currentUserId)
                                            {
                                                groupChatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Right;
                                                groupChatTextBox.SelectionColor = System.Drawing.Color.Blue;
                                                groupChatTextBox.AppendText($"You: {content}\n");
                                            }
                                                                    else
                                                                    {
                                                                        groupChatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                                                                        groupChatTextBox.SelectionColor = System.Drawing.Color.Black;
                                                                        string who = !string.IsNullOrEmpty(senderDisplayName) ? senderDisplayName : (!string.IsNullOrEmpty(fromUsername) ? fromUsername : "Member");
                                                                        groupChatTextBox.AppendText($"{who}: {content}\n");
                                                                    }
                                            groupChatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                                            groupChatTextBox.SelectionColor = System.Drawing.Color.Black;
                                        }
                                    }

                                    if (mcount == 0)
                                        groupChatTextBox.AppendText("[No messages yet]\n");
                                }
                                else
                                {
                                    groupChatTextBox.AppendText("[No messages available]\n");
                                }

                                groupChatTextBox.ScrollToCaret();
                            }
                            catch { }
                        }
                                var membersObj = packet.Data["members"];
                                if (membersObj is System.Collections.IEnumerable menEnum)
                                {
                                    foreach (var m in menEnum)
                                    {
                                        try
                                        {
                                            int uid = 0;
                                            string uname = "";
                                            string dname = "";
                                            if (m is System.Collections.Generic.IDictionary<string, object> md)
                                            {
                                                uid = md.ContainsKey("userId") ? Convert.ToInt32(md["userId"]) : 0;
                                                uname = md.ContainsKey("username") ? md["username"].ToString() : "";
                                                dname = md.ContainsKey("displayName") ? md["displayName"].ToString() : uname;
                                            }
                                            else
                                            {
                                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(m);
                                                var user = Newtonsoft.Json.JsonConvert.DeserializeObject<Shared.Models.User>(json);
                                                if (user != null)
                                                {
                                                    uid = user.UserId;
                                                    uname = user.Username;
                                                    dname = user.DisplayName;
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(uname))
                                            {
                                                _currentGroupMembers.Add($"{dname} (@{uname})");
                                                var u = new YourChatApp.Shared.Models.User { UserId = uid, Username = uname, DisplayName = dname };
                                                _currentGroupMemberUsers.Add(u);
                                            }
                                        }
                                        catch { }
                                    }
                                }
                            }
                            catch { }
                        }
                        break;

                    case CommandType.CREATE_GROUP:
                        // On successful group creation, refresh group list
                        if (packet.StatusCode >= 200 && packet.StatusCode < 300)
                        {
                            CommandPacket refresh = new CommandPacket(CommandType.GET_GROUPS);
                            _clientSocket.SendPacket(refresh);
                            MessageBox.Show("Group created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                            if (chatTextBox != null)
                                                chatTextBox.AppendText($"📞 Video call ended\n");
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
                                        if (chatTextBox != null)
                                            chatTextBox.AppendText($"📞 Video call ended\n");
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

        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // When switching to Groups tab, hide chat controls; when switching back, show them
                var tabControl = sender as System.Windows.Forms.TabControl;
                if (tabControl == null) return;

                var selected = tabControl.SelectedTab;
                bool showChats = selected != null && selected.Text == "Chats";

                friendsLabel.Visible = showChats;
                friendsListBox.Visible = showChats;
                groupsListBox.Visible = !showChats;
                chatLabel.Visible = showChats;
                videoCallButton.Visible = showChats;
                chatTextBox.Visible = showChats;
                messageInputTextBox.Visible = showChats;
                sendButton.Visible = showChats;
                // Group-specific controls
                groupChatTextBox.Visible = !showChats;
                groupMessageInput.Visible = !showChats;
                sendGroupMessageButton.Visible = !showChats;
                createGroupButton.Visible = !showChats;
                // viewMembersButton is added to tabGroups controls; hide by default when switching tabs
                viewMembersButton.Visible = false;
                viewMembersButton.Enabled = false;
                // manageGroupButton visibility will be updated when a group is selected
                manageGroupButton.Visible = !showChats ? manageGroupButton.Visible : false;
                notificationPanel.Visible = true; // keep notifications visible

                if (!showChats)
                {
                    // If entering Groups tab, request friends (for selection) and groups list
                    CommandPacket f = new CommandPacket(CommandType.GET_FRIENDS);
                    _clientSocket.SendPacket(f);
                    CommandPacket g = new CommandPacket(CommandType.GET_GROUPS);
                    _clientSocket.SendPacket(g);
                }
            }
            catch { }
        }

        private void GroupsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (groupsListBox.SelectedIndex >= 0 && groupsListBox.SelectedIndex < _groups.Count)
                {
                    var g = _groups[groupsListBox.SelectedIndex];
                    _currentGroupId = g.GroupId;
                    _currentGroupName = g.GroupName;

                    // Request members and recent messages for this group
                    var data = new Dictionary<string, object>
                    {
                        { "groupId", _currentGroupId },
                        { "includeMessages", true }
                    };
                    var packet = new CommandPacket(CommandType.GET_GROUPS, data);
                    _clientSocket.SendPacket(packet);

                    // Clear group chat view
                    groupChatTextBox.Clear();
                    groupChatTextBox.AppendText($"=== Group: {_currentGroupName} ===\n\n");

                    // Show manage button only if current user is the group's creator
                    try
                    {
                        manageGroupButton.Visible = (g.CreatedBy == _currentUserId);
                    }
                    catch { manageGroupButton.Visible = false; }

                    // Enable the View Members button now that a group is selected
                    viewMembersButton.Visible = true;
                    viewMembersButton.Enabled = true;
                }
                else
                {
                    // No valid selection: hide/disable view/manage buttons
                    viewMembersButton.Visible = false;
                    viewMembersButton.Enabled = false;
                    manageGroupButton.Visible = false;
                }
            }
            catch { }
        }

        private void GroupsListBox_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                // Reuse the same loading logic as selection change
                GroupsListBox_SelectedIndexChanged(sender, e);
            }
            catch { }
        }

        // Handle incoming broadcasted group messages
        private void HandleIncomingGroupMessage(CommandPacket packet)
        {
            try
            {
                if (packet.Data.ContainsKey("groupId") && packet.Data.ContainsKey("content"))
                {
                    int gid = Convert.ToInt32(packet.Data["groupId"]);
                    // If server provided sender id, and it's this client, ignore (we already appended locally)
                    if (packet.Data.ContainsKey("fromUserId"))
                    {
                        try
                        {
                            int fromId = Convert.ToInt32(packet.Data["fromUserId"]);
                            if (fromId == _currentUserId)
                                return; // skip duplicate for sender
                        }
                        catch { }
                    }

                    string fromUser = packet.Data.ContainsKey("fromUsername") ? packet.Data["fromUsername"].ToString() : null;
                    string fromDisplay = packet.Data.ContainsKey("fromDisplayName") ? packet.Data["fromDisplayName"].ToString() : null;
                    string content = packet.Data["content"].ToString();
                    DateTime sentAt = packet.Data.ContainsKey("sentAt") ? Convert.ToDateTime(packet.Data["sentAt"]) : DateTime.Now;

                    string who = !string.IsNullOrEmpty(fromDisplay) ? fromDisplay : (!string.IsNullOrEmpty(fromUser) ? fromUser : "Member");

                    // If this message belongs to the currently open group, append to the group chat box
                    if (gid == _currentGroupId)
                    {
                        groupChatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                        groupChatTextBox.SelectionColor = System.Drawing.Color.Black;
                        groupChatTextBox.AppendText($"{who}: {content}\n");
                        groupChatTextBox.ScrollToCaret();
                    }
                    else
                    {
                        // Optionally: add a notification for other group activity
                        chatTextBox.AppendText($"[Group {gid}] {who}: {content}\n");
                    }
                }
            }
            catch { }
        }

        private void AddMemberButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentGroupId <= 0)
                {
                    MessageBox.Show("Select a group first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Inline add/remove controls were moved to the Manage dialog.
                MessageBox.Show("Use Manage Group to add members (right-click or Manage Group button).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
        }

        private void RemoveMemberButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentGroupId <= 0)
                {
                    MessageBox.Show("Select a group first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Inline remove member control was removed; use Manage Group dialog.
                MessageBox.Show("Use Manage Group to remove members.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
        }

        private void DeleteGroupButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentGroupId <= 0)
                {
                    MessageBox.Show("Select a group first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this group?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) return;

                var data = new Dictionary<string, object>
                {
                    { "groupId", _currentGroupId }
                };
                var packet = new CommandPacket(CommandType.DELETE_GROUP, data);
                _clientSocket.SendPacket(packet);

                // Refresh groups list
                var refresh = new CommandPacket(CommandType.GET_GROUPS);
                _clientSocket.SendPacket(refresh);
            }
            catch { }
        }

        private void SendGroupMessageButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentGroupId <= 0)
                {
                    MessageBox.Show("Select a group first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string msg = groupMessageInput.Text?.Trim();
                if (string.IsNullOrEmpty(msg)) return;

                var data = new Dictionary<string, object>
                {
                    { "groupId", _currentGroupId },
                    { "content", msg }
                };

                var packet = PacketProcessor.CreateCommand(CommandType.GROUP_MESSAGE, data);
                _clientSocket.SendPacket(packet);
                groupMessageInput.Clear();

                // Show locally
                groupChatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Right;
                groupChatTextBox.SelectionColor = System.Drawing.Color.Blue;
                groupChatTextBox.AppendText($"You: {msg}\n");
                groupChatTextBox.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                groupChatTextBox.SelectionColor = System.Drawing.Color.Black;
            }
            catch { }
        }

        private void GroupMessageInput_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            try
            {
                // Enter to send, Shift+Enter to insert newline
                if (e.KeyCode == System.Windows.Forms.Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true; // prevent ding / newline
                    SendGroupMessageButton_Click(sendGroupMessageButton, EventArgs.Empty);
                }
            }
            catch { }
        }

        private void CreateGroupButton_Click(object sender, EventArgs e)
        {
            // legacy direct create - replaced by OpenCreateGroupForm
            OpenCreateGroupForm_Click(sender, e);
        }

        private void OpenCreateGroupForm_Click(object sender, EventArgs e)
        {
            try
            {
                var friendTuples = _friends.Select(f => Tuple.Create(f.UserId, f.Username)).ToList();
                var form = new CreateGroupForm(friendTuples);
                var res = form.ShowDialog(this);
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    var groupName = form.GroupName;
                    var memberIds = form.SelectedUserIds ?? new List<int>();

                    var data = new Dictionary<string, object>
                    {
                        { "groupName", groupName },
                        { "memberIds", memberIds }
                    };
                    CommandPacket packet = PacketProcessor.CreateCommand(CommandType.CREATE_GROUP, data);
                    _clientSocket.SendPacket(packet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening create group form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewMembersButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentGroupId <= 0)
                {
                    MessageBox.Show("Select a group first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Use the cached member list populated from server responses
                var members = new List<string>(_currentGroupMembers);
                var mf = new GroupMembersForm(members, _currentGroupName);
                mf.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening members view: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ManageGroupButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentGroupId <= 0)
                {
                    MessageBox.Show("Select a group first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Prepare friend list as shared User objects for the manage dialog
                var friendsUsers = new List<YourChatApp.Shared.Models.User>();
                foreach (var f in _friends)
                {
                    friendsUsers.Add(new YourChatApp.Shared.Models.User
                    {
                        UserId = f.UserId,
                        Username = f.Username,
                        DisplayName = f.DisplayName
                    });
                }

                var mf = new GroupManageForm(_currentGroupId, _currentGroupName, _currentGroupMemberUsers, friendsUsers, _clientSocket, _currentUserId);
                var res = mf.ShowDialog(this);
                if (res == System.Windows.Forms.DialogResult.OK && mf.DeleteRequested)
                {
                    var data = new Dictionary<string, object> { { "groupId", _currentGroupId } };
                    var packet = new CommandPacket(CommandType.DELETE_GROUP, data);
                    _clientSocket.SendPacket(packet);

                    // Refresh groups list
                    var refresh = new CommandPacket(CommandType.GET_GROUPS);
                    _clientSocket.SendPacket(refresh);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening manage form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateFriendsForGroup()
        {
            try
            {
                _friendsUsernames.Clear();
                foreach (var f in _friends)
                {
                    _friendsUsernames.Add(f.Username);
                }
            }
            catch { }
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

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
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
