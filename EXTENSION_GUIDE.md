# YourChatApp - Extension & Enhancement Guide

## 🚀 Hướng dẫn mở rộng tính năng

Tài liệu này hướng dẫn cách thêm các tính năng mới vào YourChatApp.

---

## 1. Thêm Command Mới

### Bước 1: Thêm CommandType vào enum

**File:** `Shared/Models/CommandPacket.cs`

```csharp
public enum CommandType
{
    // ... existing commands ...
    
    // New feature - File Transfer
    FILE_UPLOAD_REQUEST = 60,
    FILE_UPLOAD_START = 61,
    FILE_DATA_CHUNK = 62,
    FILE_UPLOAD_COMPLETE = 63
}
```

### Bước 2: Tạo Model Class (nếu cần)

**File:** `Shared/Models/FileTransfer.cs`

```csharp
namespace YourChatApp.Shared.Models
{
    [Serializable]
    public class FileTransferRequest
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

### Bước 3: Tạo Server Handler

**File:** `Server/ClientHandler.cs` - ProcessCommand method

```csharp
case CommandType.FILE_UPLOAD_REQUEST:
    response = HandleFileUploadRequest(packet);
    break;

case CommandType.FILE_DATA_CHUNK:
    response = HandleFileDataChunk(packet);
    break;
```

**Thêm methods:**

```csharp
private CommandPacket HandleFileUploadRequest(CommandPacket packet)
{
    try
    {
        if (_user == null)
            return PacketProcessor.CreateErrorResponse("Not authenticated");

        string fileName = packet.Data["fileName"].ToString();
        long fileSize = Convert.ToInt64(packet.Data["fileSize"]);
        int receiverId = Convert.ToInt32(packet.Data["receiverId"]);

        // Create transfer request
        var transferRequest = new FileTransferRequest
        {
            FileId = Guid.NewGuid().ToString(),
            FileName = fileName,
            FileSize = fileSize,
            SenderId = _user.UserId,
            ReceiverId = receiverId,
            CreatedAt = DateTime.Now
        };

        // TODO: Save to database
        
        var responseData = new Dictionary<string, object>
        {
            { "fileId", transferRequest.FileId }
        };

        return PacketProcessor.CreateResponse(
            CommandType.FILE_UPLOAD_REQUEST, 
            200, 
            "Upload authorized", 
            responseData
        );
    }
    catch (Exception ex)
    {
        return PacketProcessor.CreateErrorResponse($"File upload failed: {ex.Message}");
    }
}

private CommandPacket HandleFileDataChunk(CommandPacket packet)
{
    try
    {
        string fileId = packet.Data["fileId"].ToString();
        int chunkNumber = Convert.ToInt32(packet.Data["chunkNumber"]);
        string chunkData = packet.Data["chunkData"].ToString();

        // TODO: Process chunk
        
        return null; // No response needed for chunks
    }
    catch (Exception ex)
    {
        return PacketProcessor.CreateErrorResponse($"Chunk processing failed: {ex.Message}");
    }
}
```

### Bước 4: Tạo Service

**File:** `Server/Services/FileTransferService.cs`

```csharp
namespace YourChatApp.Server.Services
{
    public class FileTransferService
    {
        private string _uploadDirectory = "uploads/";
        
        public FileTransferService()
        {
            if (!Directory.Exists(_uploadDirectory))
                Directory.CreateDirectory(_uploadDirectory);
        }

        public bool SaveFileChunk(string fileId, int chunkNumber, byte[] chunkData)
        {
            try
            {
                string filePath = Path.Combine(_uploadDirectory, $"{fileId}.chunk{chunkNumber}");
                File.WriteAllBytes(filePath, chunkData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Save chunk failed: {ex.Message}");
                return false;
            }
        }

        public bool CombineChunks(string fileId, int totalChunks, string finalFileName)
        {
            try
            {
                string finalPath = Path.Combine(_uploadDirectory, finalFileName);
                
                using (var finalFile = File.Create(finalPath))
                {
                    for (int i = 0; i < totalChunks; i++)
                    {
                        string chunkPath = Path.Combine(_uploadDirectory, $"{fileId}.chunk{i}");
                        byte[] chunkData = File.ReadAllBytes(chunkPath);
                        finalFile.Write(chunkData, 0, chunkData.Length);
                        File.Delete(chunkPath); // Clean up chunk
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Combine chunks failed: {ex.Message}");
                return false;
            }
        }
    }
}
```

### Bước 5: Thêm UI Component trên Client

**File:** `Client/Forms/ChatWindowForm.cs`

```csharp
// Thêm button trong InitializeComponent
Button uploadFileButton = new Button();
uploadFileButton.Text = "Upload File";
uploadFileButton.Location = new System.Drawing.Point(515, 350);
uploadFileButton.Size = new System.Drawing.Size(60, 30);
uploadFileButton.Click += UploadFileButton_Click;
this.Controls.Add(uploadFileButton);
```

**Event handler:**

```csharp
private void UploadFileButton_Click(object sender, EventArgs e)
{
    OpenFileDialog openFileDialog = new OpenFileDialog();
    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
        string filePath = openFileDialog.FileName;
        string fileName = Path.GetFileName(filePath);
        
        // Gửi file upload request
        SendFileUpload(filePath, fileName);
    }
}

private void SendFileUpload(string filePath, string fileName)
{
    try
    {
        var fileInfo = new FileInfo(filePath);
        
        var uploadData = new Dictionary<string, object>
        {
            { "fileName", fileName },
            { "fileSize", fileInfo.Length },
            { "receiverId", _friendUserId }
        };

        CommandPacket packet = PacketProcessor.CreateCommand(
            CommandType.FILE_UPLOAD_REQUEST, 
            uploadData
        );

        _clientSocket.SendPacket(packet);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Upload failed: {ex.Message}");
    }
}
```

---

## 2. Thêm Feature: Group Chat

### Database Schema Addition

```sql
ALTER TABLE Messages ADD GroupId INT;

CREATE TABLE IF NOT EXISTS Groups (
    GroupId INT PRIMARY KEY AUTO_INCREMENT,
    GroupName VARCHAR(100) NOT NULL,
    Description TEXT,
    CreatedBy INT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);

CREATE TABLE IF NOT EXISTS GroupMembers (
    GroupMemberId INT PRIMARY KEY AUTO_INCREMENT,
    GroupId INT NOT NULL,
    UserId INT NOT NULL,
    JoinedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GroupId) REFERENCES Groups(GroupId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
```

### Command Types

```csharp
CREATE_GROUP = 30,
GET_GROUPS = 31,
JOIN_GROUP = 32,
LEAVE_GROUP = 33,
GROUP_MESSAGE = 34,
INVITE_TO_GROUP = 35
```

### Server Handler Implementation

```csharp
private CommandPacket HandleCreateGroup(CommandPacket packet)
{
    string groupName = packet.Data["groupName"].ToString();
    string description = packet.Data.ContainsKey("description") ? 
        packet.Data["description"].ToString() : "";

    // TODO: Validate input
    // TODO: Create group in database
    // TODO: Add creator as first member

    return PacketProcessor.CreateResponse(CommandType.CREATE_GROUP, 201, "Group created");
}

private CommandPacket HandleGroupMessage(CommandPacket packet)
{
    int groupId = Convert.ToInt32(packet.Data["groupId"]);
    string content = packet.Data["content"].ToString();

    // TODO: Save message
    // TODO: Send to all group members
    
    return PacketProcessor.CreateResponse(CommandType.GROUP_MESSAGE, 200, "Message sent");
}
```

---

## 3. Thêm Feature: Typing Indicator

### Network Command

```csharp
public enum CommandType
{
    // ... existing ...
    USER_TYPING = 70,
    USER_STOPPED_TYPING = 71
}
```

### Client Implementation

```csharp
private System.Timers.Timer _typingTimer;

private void MessageInputTextBox_TextChanged(object sender, EventArgs e)
{
    // Stop previous timer
    _typingTimer?.Stop();

    if (_messageInputTextBox.Text.Length > 0)
    {
        // Send typing indicator
        SendTypingIndicator();

        // Restart timer to clear indicator after 3 seconds
        _typingTimer = new System.Timers.Timer(3000);
        _typingTimer.Elapsed += (s, ea) =>
        {
            SendStoppedTypingIndicator();
            _typingTimer.Stop();
        };
        _typingTimer.Start();
    }
}

private void SendTypingIndicator()
{
    var data = new Dictionary<string, object>
    {
        { "receiverId", _friendUserId }
    };
    
    CommandPacket packet = PacketProcessor.CreateCommand(
        CommandType.USER_TYPING, 
        data
    );
    
    _clientSocket.SendPacket(packet);
}
```

---

## 4. Thêm Feature: Read Receipts

### Messages Status

```csharp
public enum MessageStatus
{
    Sent = 0,
    Delivered = 1,
    Read = 2
}
```

### Update Message Model

```csharp
public class Message
{
    // ... existing fields ...
    public MessageStatus Status { get; set; }
}
```

### Server Implementation

```csharp
private CommandPacket HandleMessageRead(CommandPacket packet)
{
    int messageId = Convert.ToInt32(packet.Data["messageId"]);
    
    // Update message status
    var message = messageRepository.GetMessage(messageId);
    if (message != null)
    {
        message.Status = MessageStatus.Read;
        messageRepository.Update(message);
        
        // Notify sender
        var notification = new Dictionary<string, object>
        {
            { "messageId", messageId },
            { "status", MessageStatus.Read }
        };
        
        _server.SendToUser(message.SenderId, 
            PacketProcessor.CreateCommand(CommandType.MESSAGE_RECEIVED, notification));
    }
    
    return null;
}
```

---

## 5. Thêm Feature: User Search

### Command Type

```csharp
SEARCH_USERS = 80
```

### Server Handler

```csharp
private CommandPacket HandleSearchUsers(CommandPacket packet)
{
    string query = packet.Data["query"].ToString();
    int limit = packet.Data.ContainsKey("limit") ? 
        Convert.ToInt32(packet.Data["limit"]) : 10;

    var userRepository = new UserRepository();
    var results = userRepository.SearchUsers(query, limit);

    var responseData = new Dictionary<string, object>
    {
        { "results", results.ConvertAll(u => new {
            u.UserId,
            u.Username,
            u.DisplayName,
            u.Status
        }) }
    };

    return PacketProcessor.CreateResponse(CommandType.SEARCH_USERS, 200, "Results found", responseData);
}
```

### Database Repository

```csharp
public List<User> SearchUsers(string query, int limit = 10)
{
    var users = new List<User>();
    
    using (var connection = _dbConnection.OpenConnection())
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT UserId, Username, Email, DisplayName, Status, CreatedAt, LastSeen
            FROM Users
            WHERE Username LIKE @Query OR DisplayName LIKE @Query
            LIMIT @Limit";
        
        command.Parameters.AddWithValue("@Query", $"%{query}%");
        command.Parameters.AddWithValue("@Limit", limit);

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                users.Add(MapReaderToUser(reader));
            }
        }
    }

    return users;
}
```

---

## 6. Thêm Feature: Encryption

### Implement TLS/SSL

```csharp
// Server: ClientHandler.cs
private void SetupSecureConnection()
{
    try
    {
        var certificate = new X509Certificate2("path/to/certificate.pfx", "password");
        var sslStream = new SslStream(_client.GetStream(), false);
        sslStream.AuthenticateAsServer(certificate);
        _stream = sslStream;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"SSL setup failed: {ex.Message}");
    }
}

// Client: ClientSocket.cs
private async Task<bool> SetupSecureConnection()
{
    try
    {
        var sslStream = new SslStream(_client.GetStream(), false);
        await sslStream.AuthenticateAsClientAsync("localhost");
        _stream = sslStream;
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"SSL setup failed: {ex.Message}");
        return false;
    }
}
```

---

## 7. Performance Optimization Tips

### 1. Connection Pooling

```csharp
// DbConnection.cs
private static readonly Queue<MySqlConnection> _connectionPool = new Queue<MySqlConnection>();
private const int POOL_SIZE = 10;

public MySqlConnection GetPooledConnection()
{
    lock (_lockObj)
    {
        if (_connectionPool.Count > 0)
        {
            return _connectionPool.Dequeue();
        }
    }

    return new MySqlConnection(_connectionString);
}

public void ReturnConnection(MySqlConnection connection)
{
    lock (_lockObj)
    {
        if (_connectionPool.Count < POOL_SIZE)
        {
            _connectionPool.Enqueue(connection);
        }
        else
        {
            connection.Close();
        }
    }
}
```

### 2. Caching

```csharp
private static Dictionary<int, User> _userCache = new Dictionary<int, User>();
private static DateTime _cacheExpiry = DateTime.Now.AddHours(1);

public User GetUserById(int userId)
{
    // Check cache
    if (_userCache.ContainsKey(userId) && DateTime.Now < _cacheExpiry)
    {
        return _userCache[userId];
    }

    // Get from database
    var user = GetUserFromDatabase(userId);
    
    // Update cache
    _userCache[userId] = user;
    
    return user;
}
```

### 3. Batch Operations

```csharp
// Send messages to multiple users efficiently
public void SendToUsersInBatch(List<int> userIds, CommandPacket packet)
{
    var tasks = new List<Task>();
    
    foreach (var userId in userIds)
    {
        tasks.Add(Task.Run(() => SendToUser(userId, packet)));
    }
    
    Task.WaitAll(tasks.ToArray());
}
```

---

## 8. Testing Improvements

### Unit Test Example

```csharp
// Tests/AuthenticationServiceTests.cs
[TestClass]
public class AuthenticationServiceTests
{
    private AuthenticationService _service;

    [TestInitialize]
    public void Setup()
    {
        _service = new AuthenticationService();
    }

    [TestMethod]
    public void RegisterUser_ValidInput_ReturnsTrue()
    {
        // Arrange
        string username = "testuser";
        string email = "test@example.com";
        string password = "password123";

        // Act
        bool result = _service.Register(username, email, username, password);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Login_InvalidPassword_ReturnsFalse()
    {
        // Arrange
        string username = "testuser";
        string wrongPassword = "wrong";

        // Act
        var user = _service.Login(username, wrongPassword);

        // Assert
        Assert.IsNull(user);
    }
}
```

---

## 9. Deployment Checklist

- [ ] Cấu hình production database
- [ ] Thiết lập SSL/TLS certificates
- [ ] Bật logging
- [ ] Tối ưu database indexes
- [ ] Cấu hình firewall
- [ ] Backup database
- [ ] Cấu hình error monitoring
- [ ] Load testing
- [ ] Security audit

---

## 10. Common Pitfalls & Solutions

### Problem: High CPU Usage
**Solution:** 
- Optimize database queries
- Use connection pooling
- Implement caching

### Problem: Memory Leak
**Solution:**
- Dispose streams properly
- Clear old data from collections
- Monitor GC pressure

### Problem: Slow Message Delivery
**Solution:**
- Batch messages
- Use compression
- Optimize packet size

---

**Happy Extending! 🚀**
