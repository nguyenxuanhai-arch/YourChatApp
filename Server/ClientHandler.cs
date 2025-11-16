using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using YourChatApp.Shared.Models;
using YourChatApp.Server.Network;
using YourChatApp.Server.Database;
using YourChatApp.Server.Services;

namespace YourChatApp.Server
{
    /// <summary>
    /// Xử lý một client kết nối
    /// Mỗi client sẽ được đóng gói trong một instance riêng, chạy trong một thread riêng
    /// </summary>
    public class ClientHandler
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private int _clientId;
        private User _user;
        private bool _isConnected;
        private readonly Server _server;
        private readonly object _sendLock = new object();

        public int ClientId => _clientId;
        public int? UserId => _user?.UserId;

        public ClientHandler(TcpClient client, Server server, int clientId)
        {
            _client = client;
            _server = server;
            _clientId = clientId;
            _isConnected = true;
            
            try
            {
                // Enable TCP keep-alive and disable Nagle's algorithm for better performance
                _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                _client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                
                _stream = client.GetStream();
                _stream.ReadTimeout = 120000; // 120 seconds timeout
                _stream.WriteTimeout = 120000;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get stream: {ex.Message}");
                _isConnected = false;
            }
        }

        /// <summary>
        /// Bắt đầu xử lý client
        /// </summary>
        public void Start()
        {
            Task.Run(() => HandleClient());
        }

        /// <summary>
        /// Vòng lặp chính xử lý client
        /// </summary>
        private async void HandleClient()
        {
            Console.WriteLine($"[+] Client #{_clientId} connected from {_client.Client.RemoteEndPoint}");

            try
            {
                while (_isConnected && _client.Connected)
                {
                    try
                    {
                        // Đọc gói tin từ client
                        CommandPacket packet = PacketProcessor.DeserializePacket(_stream);

                        if (packet == null)
                        {
                            Console.WriteLine($"[-] Client #{_clientId} disconnected");
                            break;
                        }

                        // Don't log PING/PONG to reduce spam
                        if (packet.Command != CommandType.PING && packet.Command != CommandType.PONG)
                        {
                            Console.WriteLine($"[RECV] Client #{_clientId}: {packet.Command}");
                        }

                        // Xử lý lệnh
                        await ProcessCommand(packet);
                    }
                    catch (IOException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Processing command failed: {ex.Message}");
                    }
                }
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Xử lý các lệnh từ client
        /// </summary>
        private Task ProcessCommand(CommandPacket packet)
        {
            CommandPacket response = null;

            switch (packet.Command)
            {
                case CommandType.LOGIN:
                    response = HandleLogin(packet);
                    break;

                case CommandType.REGISTER:
                    response = HandleRegister(packet);
                    break;

                case CommandType.LOGOUT:
                    response = HandleLogout(packet);
                    break;

                case CommandType.GET_FRIENDS:
                    response = HandleGetFriends(packet);
                    break;

                case CommandType.ADD_FRIEND:
                    response = HandleAddFriend(packet);
                    break;

                case CommandType.ACCEPT_FRIEND:
                    response = HandleAcceptFriendRequest(packet);
                    break;

                case CommandType.REJECT_FRIEND:
                    response = HandleRejectFriendRequest(packet);
                    break;

                case CommandType.CHAT_MESSAGE:
                    response = HandleChatMessage(packet);
                    break;

                case CommandType.GET_MESSAGES:
                    response = HandleGetMessages(packet);
                    break;

                case CommandType.VIDEO_CALL_REQUEST:
                    response = HandleVideoCallRequest(packet);
                    break;

                case CommandType.VIDEO_CALL_ACCEPT:
                    response = HandleVideoCallAccept(packet);
                    break;

                case CommandType.VIDEO_CALL_REJECT:
                    response = HandleVideoCallReject(packet);
                    break;

                case CommandType.VIDEO_CALL_END:
                    response = HandleVideoCallEnd(packet);
                    break;

                case CommandType.VIDEO_AUDIO_DATA:
                    response = HandleVideoAudioData(packet);
                    break;

                case CommandType.PING:
                    response = HandlePing(packet);
                    break;

                case CommandType.CREATE_GROUP:
                    response = HandleCreateGroup(packet);
                    break;

                case CommandType.GET_GROUPS:
                    response = HandleGetGroups(packet);
                    break;
                
                case CommandType.INVITE_TO_GROUP:
                    response = HandleInviteToGroup(packet);
                    break;

                case CommandType.LEAVE_GROUP:
                    response = HandleLeaveGroup(packet);
                    break;

                case CommandType.GROUP_MESSAGE:
                    response = HandleGroupMessage(packet);
                    break;

                case CommandType.DELETE_GROUP:
                    response = HandleDeleteGroup(packet);
                    break;

                case CommandType.ERROR:
                    // noop - error forwarded
                    break;

                default:
                    response = PacketProcessor.CreateErrorResponse($"Unknown command: {packet.Command}");
                    break;
            }

            if (response != null)
            {
                SendPacket(response);
            }

            return Task.CompletedTask;
        }

        #region Command Handlers

        private CommandPacket HandleLogin(CommandPacket packet)
        {
            try
            {
                if (!packet.Data.ContainsKey("username") || !packet.Data.ContainsKey("passwordHash"))
                {
                    return PacketProcessor.CreateErrorResponse("Missing username or password");
                }

                string username = packet.Data["username"].ToString();
                string passwordHash = packet.Data["passwordHash"].ToString();

                // Xác thực từ database
                var userRepo = new UserRepository();
                if (!userRepo.AuthenticateUser(username, passwordHash))
                {
                    Console.WriteLine($"[AUTH] Failed: Invalid credentials for {username}");
                    return PacketProcessor.CreateErrorResponse("Invalid username or password");
                }

                // Lấy thông tin user
                _user = userRepo.GetUserByUsername(username);
                if (_user == null)
                {
                    return PacketProcessor.CreateErrorResponse("User not found");
                }

                // Cập nhật status thành ONLINE
                userRepo.UpdateUserStatus(_user.UserId, UserStatus.Online);
                _user.Status = UserStatus.Online;

                // Register client with server to enable messaging
                _server.RegisterClient(_user.UserId, _clientId);

                Console.WriteLine($"[AUTH] Success: {username} (UserId: {_user.UserId})");
                
                // Fetch and send pending friend requests
                var friendRepo = new FriendRepository();
                var pendingRequests = friendRepo.GetPendingFriendRequests(_user.UserId);
                foreach (var requester in pendingRequests)
                {
                    var pendingNotification = new Dictionary<string, object>
                    {
                        { "fromUserId", requester.UserId },
                        { "fromUsername", requester.Username },
                        { "fromDisplayName", requester.DisplayName }
                    };
                    var pendingPacket = PacketProcessor.CreateCommand(CommandType.FRIEND_REQUEST, pendingNotification);
                    SendPacket(pendingPacket);
                    Console.WriteLine($"[FRIEND] Sent pending request from {requester.Username} to {username}");
                }
                
                var responseData = new Dictionary<string, object>
                {
                    { "userId", _user.UserId },
                    { "username", _user.Username },
                    { "displayName", _user.DisplayName }
                };

                return PacketProcessor.CreateResponse(CommandType.LOGIN, 200, "Login successful", responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Login failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Login failed: {ex.Message}");
            }
        }

        private CommandPacket HandleRegister(CommandPacket packet)
        {
            try
            {
                if (!packet.Data.ContainsKey("username") || !packet.Data.ContainsKey("email") || !packet.Data.ContainsKey("passwordHash"))
                {
                    return PacketProcessor.CreateErrorResponse("Missing registration fields");
                }

                string username = packet.Data["username"].ToString();
                string email = packet.Data["email"].ToString();
                string displayName = packet.Data.ContainsKey("displayName") ? packet.Data["displayName"].ToString() : username;
                string passwordHash = packet.Data["passwordHash"].ToString();

                // Kiểm tra username đã tồn tại chưa
                var userRepo = new UserRepository();
                var existingUser = userRepo.GetUserByUsername(username);
                if (existingUser != null)
                {
                    Console.WriteLine($"[REGISTER] Failed: Username '{username}' already exists");
                    return PacketProcessor.CreateErrorResponse("Username already exists");
                }

                // Tạo user mới
                if (!userRepo.CreateUser(username, email, displayName, passwordHash))
                {
                    Console.WriteLine($"[REGISTER] Failed: Could not create user '{username}'");
                    return PacketProcessor.CreateErrorResponse("Registration failed - could not create user");
                }

                Console.WriteLine($"[REGISTER] Success: New user '{username}' created");

                var responseData = new Dictionary<string, object>
                {
                    { "message", "Registration successful" }
                };

                return PacketProcessor.CreateResponse(CommandType.REGISTER, 201, "User registered", responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Registration failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Registration failed: {ex.Message}");
            }
        }

        private CommandPacket HandleLogout(CommandPacket packet)
        {
            _user = null;
            return PacketProcessor.CreateResponse(CommandType.LOGOUT, 200, "Logged out successfully");
        }

        private CommandPacket HandleGetFriends(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                var friendRepo = new FriendRepository();
                var friends = friendRepo.GetFriends(_user.UserId);
                
                var friendsList = new List<object>();
                foreach (var friend in friends)
                {
                    friendsList.Add(new
                    {
                        userId = friend.UserId,
                        username = friend.Username,
                        displayName = friend.DisplayName,
                        status = friend.Status
                    });
                }

                var responseData = new Dictionary<string, object>
                {
                    { "friends", friendsList }
                };

                return PacketProcessor.CreateResponse(CommandType.GET_FRIENDS, 200, "Friends retrieved", responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Get friends failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Get friends failed: {ex.Message}");
            }
        }

        private CommandPacket HandleAddFriend(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                int friendUserId = -1;
                User friendUser = null;
                var userRepo = new UserRepository();

                // Kiểm tra có friendUserId hay friendUsername
                if (packet.Data.ContainsKey("friendUserId"))
                {
                    friendUserId = Convert.ToInt32(packet.Data["friendUserId"]);
                    friendUser = userRepo.GetUserById(friendUserId);
                }
                else if (packet.Data.ContainsKey("friendUsername"))
                {
                    string friendUsername = packet.Data["friendUsername"].ToString();
                    friendUser = userRepo.GetUserByUsername(friendUsername);
                    if (friendUser != null)
                        friendUserId = friendUser.UserId;
                }
                else
                {
                    return PacketProcessor.CreateErrorResponse("Missing friendUserId or friendUsername");
                }

                if (friendUser == null)
                {
                    Console.WriteLine($"[FRIEND] User not found: UserId={friendUserId}");
                    return PacketProcessor.CreateErrorResponse("User not found");
                }

                // Không thể thêm chính mình làm bạn
                if (friendUserId == _user.UserId)
                    return PacketProcessor.CreateErrorResponse("Cannot add yourself as friend");

                var friendRepo = new FriendRepository();
                
                // Kiểm tra đã là bạn chưa
                if (friendRepo.IsFriend(_user.UserId, friendUserId))
                {
                    Console.WriteLine($"[FRIEND] Already friends: {_user.Username} <-> {friendUser.Username}");
                    return PacketProcessor.CreateErrorResponse("Already friends");
                }

                // Gửi friend request
                if (friendRepo.SendFriendRequest(_user.UserId, friendUserId))
                {
                    Console.WriteLine($"[FRIEND] {_user.Username} sent friend request to {friendUser.Username}");
                    
                    // Gửi notification tới friend nếu online
                    var notificationData = new Dictionary<string, object>
                    {
                        { "fromUserId", _user.UserId },
                        { "fromUsername", _user.Username },
                        { "fromDisplayName", _user.DisplayName }
                    };
                    var notification = PacketProcessor.CreateCommand(CommandType.FRIEND_REQUEST, notificationData);
                    _server.SendToUser(friendUserId, notification);
                    
                    return PacketProcessor.CreateResponse(CommandType.ADD_FRIEND, 200, "Friend request sent");
                }
                else
                {
                    Console.WriteLine($"[FRIEND] Failed to send request: {_user.Username} -> {friendUser.Username}");
                    return PacketProcessor.CreateErrorResponse("Failed to send friend request");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Add friend failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Add friend failed: {ex.Message}");
            }
        }

        private CommandPacket HandleAcceptFriendRequest(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("friendUserId"))
                    return PacketProcessor.CreateErrorResponse("Missing friendUserId");

                int friendUserId = Convert.ToInt32(packet.Data["friendUserId"]);

                var userRepo = new UserRepository();
                var friendUser = userRepo.GetUserById(friendUserId);
                if (friendUser == null)
                    return PacketProcessor.CreateErrorResponse("User not found");

                var friendRepo = new FriendRepository();
                
                if (friendRepo.AcceptFriendRequest(_user.UserId, friendUserId))
                {
                    Console.WriteLine($"[FRIEND] {_user.Username} accepted friend request from {friendUser.Username}");
                    return PacketProcessor.CreateResponse(CommandType.ACCEPT_FRIEND, 200, "Friend request accepted");
                }
                else
                {
                    return PacketProcessor.CreateErrorResponse("Failed to accept friend request");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Accept friend request failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Accept friend request failed: {ex.Message}");
            }
        }

        private CommandPacket HandleRejectFriendRequest(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("friendUserId"))
                    return PacketProcessor.CreateErrorResponse("Missing friendUserId");

                int friendUserId = Convert.ToInt32(packet.Data["friendUserId"]);

                var userRepo = new UserRepository();
                var friendUser = userRepo.GetUserById(friendUserId);
                if (friendUser == null)
                    return PacketProcessor.CreateErrorResponse("User not found");

                var friendRepo = new FriendRepository();
                
                if (friendRepo.RejectFriendRequest(_user.UserId, friendUserId))
                {
                    Console.WriteLine($"[FRIEND] {_user.Username} rejected friend request from {friendUser.Username}");
                    return PacketProcessor.CreateResponse(CommandType.REJECT_FRIEND, 200, "Friend request rejected");
                }
                else
                {
                    return PacketProcessor.CreateErrorResponse("Failed to reject friend request");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Reject friend request failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Reject friend request failed: {ex.Message}");
            }
        }

        private CommandPacket HandleChatMessage(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("receiverId") || !packet.Data.ContainsKey("content"))
                    return PacketProcessor.CreateErrorResponse("Missing message data");

                int receiverId = Convert.ToInt32(packet.Data["receiverId"]);
                string content = packet.Data["content"].ToString();

                // Save message to database
                var message = new Message
                {
                    SenderId = _user.UserId,
                    ReceiverId = receiverId,
                    Content = content,
                    MessageType = MessageType.Text,
                    SentAt = DateTime.Now
                };

                var messageRepo = new MessageRepository();
                if (messageRepo.SaveMessage(message))
                {
                    Console.WriteLine($"[MSG] {_user.Username} -> UserId({receiverId}): {content}");

                    // Route message to recipient if online
                    var messageData = new Dictionary<string, object>
                    {
                        { "fromUserId", _user.UserId },
                        { "fromUsername", _user.Username },
                        { "content", content },
                        { "sentAt", message.SentAt }
                    };
                    var messagePacket = PacketProcessor.CreateCommand(CommandType.CHAT_MESSAGE, messageData);
                    _server.SendToUser(receiverId, messagePacket);
                }

                return PacketProcessor.CreateResponse(CommandType.CHAT_MESSAGE, 200, "Message sent");
            }
            catch (Exception ex)
            {
                return PacketProcessor.CreateErrorResponse($"Send message failed: {ex.Message}");
            }
        }

        private CommandPacket HandleGetMessages(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("fromUserId"))
                    return PacketProcessor.CreateErrorResponse("Missing fromUserId");

                int fromUserId = Convert.ToInt32(packet.Data["fromUserId"]);

                // Get message history from database
                var messageRepo = new MessageRepository();
                var messages = messageRepo.GetMessages(_user.UserId, fromUserId, 100);

                var messageList = new List<object>();
                foreach (var msg in messages)
                {
                    messageList.Add(new Dictionary<string, object>
                    {
                        { "messageId", msg.MessageId },
                        { "senderId", msg.SenderId },
                        { "content", msg.Content },
                        { "sentAt", msg.SentAt },
                        { "readAt", msg.ReadAt }
                    });
                }

                var responseData = new Dictionary<string, object>
                {
                    { "messages", messageList }
                };

                Console.WriteLine($"[MSG] Retrieved {messageList.Count} messages between UserId({_user.UserId}) and UserId({fromUserId})");
                return PacketProcessor.CreateResponse(CommandType.GET_MESSAGES, 200, "Messages retrieved", responseData);
            }
            catch (Exception ex)
            {
                return PacketProcessor.CreateErrorResponse($"Get messages failed: {ex.Message}");
            }
        }

        private CommandPacket HandleVideoCallRequest(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("receiverId"))
                    return PacketProcessor.CreateErrorResponse("Missing receiverId");

                int receiverId = Convert.ToInt32(packet.Data["receiverId"]);
                int callerId = _user.UserId;
                string callerName = _user.DisplayName;

                // Generate unique callId
                string callId = Guid.NewGuid().ToString();

                Console.WriteLine($"[VIDEO] Call request from {callerName} (UserId: {callerId}) to UserId: {receiverId}, CallId: {callId}");

                // Save to database
                try
                {
                    using (var connection = DbConnection.Instance.OpenConnection())
                    {
                        if (connection != null)
                        {
                            string query = "INSERT INTO videocallrequests (CallId, CallerId, CallerName, ReceiverId, Status, InitiatedAt) VALUES (@callId, @callerId, @callerName, @receiverId, @status, @initiatedAt)";
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = query;
                                command.Parameters.AddWithValue("@callId", callId);
                                command.Parameters.AddWithValue("@callerId", callerId);
                                command.Parameters.AddWithValue("@callerName", callerName);
                                command.Parameters.AddWithValue("@receiverId", receiverId);
                                command.Parameters.AddWithValue("@status", "pending");
                                command.Parameters.AddWithValue("@initiatedAt", DateTime.Now);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    Console.WriteLine($"[DB] Video call request saved to database");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"[WARN] Failed to save video call to database: {dbEx.Message}");
                    // Continue anyway - the call can still proceed
                }

                // Send call request to receiver if online
                var callRequestData = new Dictionary<string, object>
                {
                    { "callId", callId },
                    { "callerId", callerId },
                    { "callerName", callerName }
                };
                CommandPacket requestPacket = PacketProcessor.CreateCommand(CommandType.VIDEO_CALL_REQUEST, callRequestData);
                
                _server.SendToUser(receiverId, requestPacket);
                Console.WriteLine($"[VIDEO] Call request sent to UserId: {receiverId}");
                
                // Send response with callId to caller
                var responseData = new Dictionary<string, object>
                {
                    { "callId", callId },
                    { "status", "sent" }
                };
                return PacketProcessor.CreateResponse(CommandType.VIDEO_CALL_REQUEST, 200, "Call request sent", responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Video call request failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Video call request failed: {ex.Message}");
            }
        }

        private CommandPacket HandleVideoCallAccept(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("callId"))
                    return PacketProcessor.CreateErrorResponse("Missing callId");

                string callId = packet.Data["callId"].ToString();

                Console.WriteLine($"[VIDEO] Call accepted: {callId} by UserId {_user.UserId}");

                // Update database
                try
                {
                    using (var connection = DbConnection.Instance.OpenConnection())
                    {
                        if (connection != null)
                        {
                            string query = "UPDATE videocallrequests SET Status = @status, AcceptedAt = @acceptedAt WHERE CallId = @callId";
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = query;
                                command.Parameters.AddWithValue("@status", "accepted");
                                command.Parameters.AddWithValue("@acceptedAt", DateTime.Now);
                                command.Parameters.AddWithValue("@callId", callId);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    Console.WriteLine($"[DB] Video call status updated to accepted");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"[WARN] Failed to update video call status: {dbEx.Message}");
                }

                // Get caller info from database
                int callerId = 0;
                try
                {
                    using (var connection = DbConnection.Instance.OpenConnection())
                    {
                        if (connection != null)
                        {
                            string query = "SELECT CallerId FROM videocallrequests WHERE CallId = @callId";
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = query;
                                command.Parameters.AddWithValue("@callId", callId);
                                var result = command.ExecuteScalar();
                                if (result != null)
                                    callerId = Convert.ToInt32(result);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] Failed to get caller info: {ex.Message}");
                }

                // Notify caller that call was accepted
                if (callerId > 0)
                {
                    var acceptData = new Dictionary<string, object>
                    {
                        { "callId", callId },
                        { "status", "accepted" }
                    };
                    CommandPacket acceptPacket = PacketProcessor.CreateCommand(CommandType.VIDEO_CALL_ACCEPT, acceptData);
                    _server.SendToUser(callerId, acceptPacket);
                    Console.WriteLine($"[VIDEO] Call accepted notification sent to UserId {callerId}");
                }

                return PacketProcessor.CreateResponse(CommandType.VIDEO_CALL_ACCEPT, 200, "Call accepted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Video call accept failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Video call accept failed: {ex.Message}");
            }
        }

        private CommandPacket HandleVideoCallReject(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("callId"))
                    return PacketProcessor.CreateErrorResponse("Missing callId");

                string callId = packet.Data["callId"].ToString();

                Console.WriteLine($"[VIDEO] Call rejected: {callId} by UserId {_user.UserId}");

                // Update database
                try
                {
                    using (var connection = DbConnection.Instance.OpenConnection())
                    {
                        if (connection != null)
                        {
                            string query = "UPDATE videocallrequests SET Status = @status, RejectedAt = @rejectedAt WHERE CallId = @callId";
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = query;
                                command.Parameters.AddWithValue("@status", "rejected");
                                command.Parameters.AddWithValue("@rejectedAt", DateTime.Now);
                                command.Parameters.AddWithValue("@callId", callId);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    Console.WriteLine($"[DB] Video call status updated to rejected");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"[WARN] Failed to update video call status: {dbEx.Message}");
                }

                // Get caller info from database
                int callerId = 0;
                try
                {
                    using (var connection = DbConnection.Instance.OpenConnection())
                    {
                        if (connection != null)
                        {
                            string query = "SELECT CallerId FROM videocallrequests WHERE CallId = @callId";
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = query;
                                command.Parameters.AddWithValue("@callId", callId);
                                var result = command.ExecuteScalar();
                                if (result != null)
                                    callerId = Convert.ToInt32(result);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] Failed to get caller info: {ex.Message}");
                }

                // Notify caller that call was rejected
                if (callerId > 0)
                {
                    var rejectData = new Dictionary<string, object>
                    {
                        { "callId", callId },
                        { "status", "rejected" }
                    };
                    CommandPacket rejectPacket = PacketProcessor.CreateCommand(CommandType.VIDEO_CALL_REJECT, rejectData);
                    _server.SendToUser(callerId, rejectPacket);
                    Console.WriteLine($"[VIDEO] Call rejected notification sent to UserId {callerId}");
                }

                return PacketProcessor.CreateResponse(CommandType.VIDEO_CALL_REJECT, 200, "Call rejected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Video call reject failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse(ex.Message);
            }
        }

        private CommandPacket HandleVideoCallEnd(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("friendId"))
                    return PacketProcessor.CreateErrorResponse("Missing friendId");

                int friendId = Convert.ToInt32(packet.Data["friendId"]);
                string callId = packet.Data.ContainsKey("callId") ? packet.Data["callId"].ToString() : "";

                Console.WriteLine($"[VIDEO] Call ended by UserId {_user.UserId}, notifying UserId {friendId}");

                // Forward END_CALL signal to the friend
                var endData = new Dictionary<string, object>
                {
                    { "callId", callId },
                    { "userId", _user.UserId },
                    { "status", "ended" }
                };
                CommandPacket endPacket = PacketProcessor.CreateCommand(CommandType.VIDEO_CALL_END, endData);
                _server.SendToUser(friendId, endPacket);
                Console.WriteLine($"[VIDEO] Call end notification sent to UserId {friendId}");

                return PacketProcessor.CreateResponse(CommandType.VIDEO_CALL_END, 200, "Call ended");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Video call end failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse(ex.Message);
            }
        }

        private CommandPacket HandleVideoAudioData(CommandPacket packet)
        {
            try
            {
                if (!packet.Data.ContainsKey("callId"))
                    return null; // No response for performance

                string callId = packet.Data["callId"].ToString();
                
                // Find the other participant in this call
                var otherClients = _server.GetAllClients()
                    .Where(c => c != this && c.UserId.HasValue)
                    .ToList();

                // Forward to all other connected clients in the call
                foreach (var client in otherClients)
                {
                    try
                    {
                        client.SendPacket(packet);
                    }
                    catch (Exception fwdEx)
                    {
                        Console.WriteLine($"[WARN] Failed to forward video/audio to Client #{client.ClientId}: {fwdEx.Message}");
                    }
                }

                // No response to sender for performance
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Video audio forward failed: {ex.Message}");
                return null; // No response for performance
            }
        }

        private CommandPacket HandlePing(CommandPacket packet)
        {
            return PacketProcessor.CreateResponse(CommandType.PONG, 200, "Pong");
        }

        private CommandPacket HandleCreateGroup(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("groupName"))
                    return PacketProcessor.CreateErrorResponse("Missing groupName");

                string groupName = packet.Data["groupName"].ToString();
                var description = packet.Data.ContainsKey("description") ? packet.Data["description"].ToString() : "";

                var group = new YourChatApp.Shared.Models.Group
                {
                    GroupName = groupName,
                    Description = description,
                    CreatedBy = _user.UserId,
                    CreatedAt = DateTime.Now
                };

                var groupRepo = new GroupRepository(DbConnection.Instance);
                int groupId = groupRepo.CreateGroup(group);

                // Add additional members if provided (expecting list of ints)
                if (packet.Data.ContainsKey("memberIds"))
                {
                    try
                    {
                        var raw = packet.Data["memberIds"];
                        if (raw is System.Collections.IEnumerable enumerable)
                        {
                            foreach (var obj in enumerable)
                            {
                                try
                                {
                                    int memberId = Convert.ToInt32(obj);
                                    if (memberId != _user.UserId)
                                        groupRepo.AddGroupMember(groupId, memberId);
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }

                var responseData = new Dictionary<string, object>
                {
                    { "groupId", groupId },
                    { "groupName", groupName }
                };

                return PacketProcessor.CreateResponse(CommandType.CREATE_GROUP, 201, "Group created", responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Create group failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Create group failed: {ex.Message}");
            }
        }

        private CommandPacket HandleGetGroups(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                var groupRepo = new GroupRepository(DbConnection.Instance);

                // If client requested members for a specific group
                if (packet.Data.ContainsKey("groupId"))
                {
                    int groupId = Convert.ToInt32(packet.Data["groupId"]);

                    var members = groupRepo.GetGroupMembers(groupId);

                    var memberList = new List<object>();
                    foreach (var m in members)
                    {
                        memberList.Add(new
                        {
                            userId = m.UserId,
                            username = m.Username,
                            displayName = m.DisplayName
                        });
                    }

                    var responseDataMembers = new Dictionary<string, object>
                    {
                        { "groupId", groupId },
                        { "members", memberList }
                    };

                    // Optionally include recent messages if client asked for them
                    if (packet.Data.ContainsKey("includeMessages") && Convert.ToBoolean(packet.Data["includeMessages"]))
                    {
                        var msgRepo = new MessageRepository();
                        var msgs = msgRepo.GetGroupMessages(groupId, 200);
                        var msgList = new List<object>();
                        var userRepo = new UserRepository();
                        foreach (var msg in msgs)
                        {
                            // Try to include sender username and display name for client convenience
                            string senderUsername = null;
                            string senderDisplayName = null;
                            try
                            {
                                var senderUser = userRepo.GetUserById(msg.SenderId);
                                if (senderUser != null)
                                {
                                    senderUsername = senderUser.Username;
                                    senderDisplayName = senderUser.DisplayName;
                                }
                            }
                            catch { }

                            msgList.Add(new Dictionary<string, object>
                            {
                                { "messageId", msg.MessageId },
                                { "senderId", msg.SenderId },
                                { "senderUsername", senderUsername },
                                { "senderDisplayName", senderDisplayName },
                                { "content", msg.Content },
                                { "sentAt", msg.SentAt }
                            });
                        }
                        responseDataMembers["messages"] = msgList;
                    }

                    return PacketProcessor.CreateResponse(CommandType.GET_GROUPS, 200, "Group members retrieved", responseDataMembers);
                }

                // Otherwise return the list of groups the user belongs to
                var groups = groupRepo.GetUserGroups(_user.UserId);

                var groupsList = new List<object>();
                foreach (var g in groups)
                {
                    groupsList.Add(new
                    {
                        groupId = g.GroupId,
                        groupName = g.GroupName,
                        description = g.Description,
                        createdBy = g.CreatedBy,
                        createdAt = g.CreatedAt
                    });
                }

                var responseData = new Dictionary<string, object>
                {
                    { "groups", groupsList }
                };

                return PacketProcessor.CreateResponse(CommandType.GET_GROUPS, 200, "Groups retrieved", responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Get groups failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Get groups failed: {ex.Message}");
            }
        }

        private CommandPacket HandleInviteToGroup(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("groupId") || !packet.Data.ContainsKey("userId"))
                    return PacketProcessor.CreateErrorResponse("Missing groupId or userId");

                int groupId = Convert.ToInt32(packet.Data["groupId"]);
                int userIdToAdd = Convert.ToInt32(packet.Data["userId"]);

                var groupRepo = new GroupRepository(DbConnection.Instance);
                var group = groupRepo.GetGroupById(groupId);
                if (group == null)
                    return PacketProcessor.CreateErrorResponse("Group not found");

                // Only allow add if requester is a member of the group (or creator)
                if (!groupRepo.IsGroupMember(groupId, _user.UserId) && group.CreatedBy != _user.UserId)
                    return PacketProcessor.CreateErrorResponse("Not authorized to add members");

                bool added = groupRepo.AddGroupMember(groupId, userIdToAdd);
                if (added)
                {
                    // Notify the added user if online
                    var notify = new Dictionary<string, object>
                    {
                        { "groupId", groupId },
                        { "groupName", group.GroupName },
                        { "addedBy", _user.UserId }
                    };
                    var packetNotify = PacketProcessor.CreateCommand(CommandType.INVITE_TO_GROUP, notify);
                    _server.SendToUser(userIdToAdd, packetNotify);

                    return PacketProcessor.CreateResponse(CommandType.INVITE_TO_GROUP, 200, "Member added");
                }

                return PacketProcessor.CreateErrorResponse("Failed to add member");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Invite to group failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Invite to group failed: {ex.Message}");
            }
        }

        private CommandPacket HandleLeaveGroup(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("groupId") || !packet.Data.ContainsKey("userId"))
                    return PacketProcessor.CreateErrorResponse("Missing groupId or userId");

                int groupId = Convert.ToInt32(packet.Data["groupId"]);
                int userId = Convert.ToInt32(packet.Data["userId"]);

                var groupRepo = new GroupRepository(DbConnection.Instance);
                var group = groupRepo.GetGroupById(groupId);
                if (group == null)
                    return PacketProcessor.CreateErrorResponse("Group not found");

                // Allow removal if requester is the user themselves or group creator
                if (userId != _user.UserId && group.CreatedBy != _user.UserId)
                    return PacketProcessor.CreateErrorResponse("Not authorized to remove this member");

                bool removed = groupRepo.RemoveGroupMember(groupId, userId);
                if (removed)
                {
                    var notify = new Dictionary<string, object>
                    {
                        { "groupId", groupId },
                        { "userId", userId }
                    };
                    var packetNotify = PacketProcessor.CreateCommand(CommandType.LEAVE_GROUP, notify);
                    _server.SendToUser(userId, packetNotify);

                    return PacketProcessor.CreateResponse(CommandType.LEAVE_GROUP, 200, "Member removed");
                }

                return PacketProcessor.CreateErrorResponse("Failed to remove member");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Leave group failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Leave group failed: {ex.Message}");
            }
        }

        private CommandPacket HandleGroupMessage(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("groupId") || !packet.Data.ContainsKey("content"))
                    return PacketProcessor.CreateErrorResponse("Missing groupId or content");

                int groupId = Convert.ToInt32(packet.Data["groupId"]);
                string content = packet.Data["content"].ToString();

                var groupRepo = new GroupRepository(DbConnection.Instance);
                if (!groupRepo.IsGroupMember(groupId, _user.UserId))
                    return PacketProcessor.CreateErrorResponse("Not a member of the group");

                // Save message to DB (MessageRepository supports GroupId)
                var message = new YourChatApp.Shared.Models.Message
                {
                    SenderId = _user.UserId,
                    GroupId = groupId,
                    Content = content,
                    MessageType = MessageType.Text,
                    SentAt = DateTime.Now
                };

                var messageRepo = new MessageRepository();
                if (messageRepo.SaveMessage(message))
                {
                    // Broadcast to all group members
                    var members = groupRepo.GetGroupMembers(groupId);
                    var userIds = members.Select(m => m.UserId).ToList();

                    var msgData = new Dictionary<string, object>
                    {
                        { "groupId", groupId },
                        { "fromUserId", _user.UserId },
                        { "fromUsername", _user.Username },
                        { "fromDisplayName", _user.DisplayName },
                        { "content", content },
                        { "sentAt", message.SentAt }
                    };
                    var groupPacket = PacketProcessor.CreateCommand(CommandType.GROUP_MESSAGE, msgData);
                    _server.SendToUsers(userIds, groupPacket);
                }

                return PacketProcessor.CreateResponse(CommandType.GROUP_MESSAGE, 200, "Group message sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Group message failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Group message failed: {ex.Message}");
            }
        }

        private CommandPacket HandleDeleteGroup(CommandPacket packet)
        {
            try
            {
                if (_user == null)
                    return PacketProcessor.CreateErrorResponse("Not authenticated");

                if (!packet.Data.ContainsKey("groupId"))
                    return PacketProcessor.CreateErrorResponse("Missing groupId");

                int groupId = Convert.ToInt32(packet.Data["groupId"]);
                var groupRepo = new GroupRepository(DbConnection.Instance);
                var group = groupRepo.GetGroupById(groupId);
                if (group == null)
                    return PacketProcessor.CreateErrorResponse("Group not found");

                // Only creator can delete group
                if (group.CreatedBy != _user.UserId)
                    return PacketProcessor.CreateErrorResponse("Only the group creator can delete the group");

                bool deleted = groupRepo.DeleteGroup(groupId);
                if (deleted)
                {
                    // Notify members (best effort)
                    // Getting members before deletion isn't available here because DeleteGroup already removed them.
                    return PacketProcessor.CreateResponse(CommandType.DELETE_GROUP, 200, "Group deleted");
                }

                return PacketProcessor.CreateErrorResponse("Failed to delete group");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Delete group failed: {ex.Message}");
                return PacketProcessor.CreateErrorResponse($"Delete group failed: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// Gửi gói tin tới client
        /// </summary>
        public void SendPacket(CommandPacket packet)
        {
            lock (_sendLock) // Prevent concurrent sends
            {
                try
                {
                    if (!_isConnected || _stream == null)
                        return;

                    byte[] data = PacketProcessor.SerializePacket(packet);
                    if (data != null)
                    {
                        _stream.Write(data, 0, data.Length);
                        _stream.Flush();
                        
                        // Don't log PING/PONG to reduce spam
                        if (packet.Command != CommandType.PING && packet.Command != CommandType.PONG)
                        {
                            Console.WriteLine($"[SEND] Client #{_clientId}: {packet.Command}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Send packet failed: {ex.Message}");
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Ngắt kết nối client
        /// </summary>
        public void Disconnect()
        {
            _isConnected = false;
            
            try
            {
                if (_user != null)
                {
                    _server.UnregisterClient(_user.UserId);
                }

                _stream?.Close();
                _client?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Disconnect failed: {ex.Message}");
            }

            Console.WriteLine($"[-] Client #{_clientId} disconnected");
        }

        /// <summary>
        /// Lấy userId của client nếu đã xác thực
        /// </summary>
        public int GetUserId()
        {
            return _user?.UserId ?? -1;
        }

        /// <summary>
        /// Kiểm tra client có kết nối không
        /// </summary>
        public bool IsConnected()
        {
            return _isConnected && _client?.Connected == true;
        }
    }
}
