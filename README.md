# YourChatApp - Network Programming Chat Application

## 📋 Giới thiệu

YourChatApp là một ứng dụng chat mạng được xây dựng cho môn **Lập trình Mạng** sử dụng kiến trúc Client-Server với **Socket TCP/IP**. Ứng dụng hỗ trợ:

- ✅ Đăng nhập và đăng ký
- ✅ Chat 1-1 giữa các user
- ✅ Quản lý bạn bè
- ✅ Cuộc gọi video P2P (relay qua server)
- ✅ Ghi âm/phát âm thanh
- ✅ Lưu trữ tin nhắn trên MySQL

## 🏗️ Kiến trúc Hệ thống

```
┌─────────────────────────────────────────────────────┐
│                   ARCHITECTURE                      │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌──────────────────┐        ┌──────────────────┐  │
│  │   WinForms       │        │   Console        │  │
│  │   Client (UI)    │◄─────►│   Server         │  │
│  │                  │ Socket │  (Multi-threaded)│  │
│  │  • LoginForm     │  TCP   │                  │  │
│  │  • ChatWindow    │ Port   │  • TcpListener   │  │
│  │  • VideoCall     │ 5000   │  • ClientHandler │  │
│  │  • MainChat      │        │  • Services      │  │
│  │                  │        │                  │  │
│  └──────────────────┘        └──────┬───────────┘  │
│                                      │              │
│                              ┌───────▼────────┐    │
│                              │   MySQL DB     │    │
│                              │                │    │
│                              │  • Users       │    │
│                              │  • Messages    │    │
│                              │  • Friendships │    │
│                              │  • Groups      │    │
│                              └────────────────┘    │
└─────────────────────────────────────────────────────┘
```

## 📁 Cấu trúc Thư mục

```
YourChatApp/
├── Shared/                          # Thư viện dùng chung
│   ├── Models/
│   │   ├── CommandPacket.cs        # Giao thức truyền thông
│   │   ├── User.cs                 # Model người dùng
│   │   ├── Message.cs              # Model tin nhắn
│   │   ├── Group.cs                # Model nhóm
│   │   ├── Friend.cs               # Model bạn bè
│   │   └── VideoCallRequest.cs     # Model cuộc gọi video
│   └── Shared.csproj
│
├── Server/                          # Ứng dụng Server
│   ├── Program.cs                  # Entry point
│   ├── Server.cs                   # Lớp quản lý TcpListener
│   ├── ClientHandler.cs            # Xử lý mỗi client connection
│   │
│   ├── Network/
│   │   └── PacketProcessor.cs      # Serialization/Deserialization
│   │
│   ├── Database/
│   │   ├── DbConnection.cs         # Quản lý MySQL connection
│   │   ├── UserRepository.cs       # CRUD operations cho Users
│   │   ├── MessageRepository.cs    # CRUD operations cho Messages
│   │   └── FriendRepository.cs     # CRUD operations cho Friends
│   │
│   ├── Services/
│   │   ├── AuthenticationService.cs  # Xác thực người dùng
│   │   ├── ChatService.cs            # Logic chat
│   │   ├── FriendService.cs          # Logic bạn bè
│   │   └── VideoCallService.cs       # Logic cuộc gọi video
│   │
│   └── Server.csproj
│
├── Client/                          # Ứng dụng Client WinForms
│   ├── Program.cs                  # Entry point
│   │
│   ├── Network/
│   │   ├── ClientSocket.cs         # Quản lý kết nối socket
│   │   └── PacketProcessor.cs      # Serialization/Deserialization
│   │
│   ├── Forms/
│   │   ├── LoginForm.cs            # Form đăng nhập
│   │   ├── RegisterForm.cs         # Form đăng ký
│   │   ├── MainChatForm.cs         # Form chat chính
│   │   ├── ChatWindowForm.cs       # Form chat 1-1
│   │   └── VideoCallForm.cs        # Form cuộc gọi video
│   │
│   ├── Models/
│   │   └── ClientModels.cs         # Models local client
│   │
│   ├── VideoAudio/
│   │   ├── CameraCapture.cs        # Xử lý camera
│   │   └── AudioCapturePlayback.cs # Xử lý audio
│   │
│   └── Client.csproj
│
└── YourChatApp.sln                 # Solution file
```

## 🔧 Công nghệ Sử dụng

### Core
- **C#** (.NET 6.0)
- **Socket TCP/IP** (System.Net.Sockets)
- **WinForms** (UI)

### Database
- **MySQL** (lưu trữ dữ liệu)
- **ADO.NET** (truy cập DB)

### Thư viện
- **Newtonsoft.Json** (JSON serialization)
- **MySql.Data** (MySQL connector)

## 🚀 Cài đặt và Chạy

### 1. Yêu cầu tiên quyết
- **.NET 6.0 SDK** hoặc cao hơn
- **MySQL Server** 5.7+
- **Visual Studio 2022** hoặc **Visual Studio Code** (tùy chọn)

### 2. Cấu hình MySQL

```sql
-- Tạo database
CREATE DATABASE yourchatapp;

-- Tạo user MySQL (nếu cần)
CREATE USER 'chatapp_user'@'localhost' IDENTIFIED BY 'your_password';
GRANT ALL PRIVILEGES ON yourchatapp.* TO 'chatapp_user'@'localhost';
FLUSH PRIVILEGES;
```

### 3. Cấu hình Connection String

Chỉnh sửa file `Server/Database/DbConnection.cs`:

```csharp
private const string DEFAULT_HOST = "localhost";
private const string DEFAULT_USER = "root";      // hoặc "chatapp_user"
private const string DEFAULT_PASSWORD = "";       // Thêm password nếu có
private const string DEFAULT_DATABASE = "yourchatapp";
private const int DEFAULT_PORT = 3306;
```

### 4. Build Project

```bash
# Build toàn bộ solution
dotnet build

# Hoặc build từng project
cd Shared && dotnet build
cd ../Server && dotnet build
cd ../Client && dotnet build
```

### 5. Chạy Server

```bash
cd Server
dotnet run
```

Server sẽ bắt đầu lắng nghe trên `0.0.0.0:5000`

### 6. Chạy Client (có thể mở nhiều instances)

```bash
cd Client
dotnet run
```

## 📡 Giao thức Truyền Thông

### Cấu trúc Packet

Tất cả dữ liệu được truyền dưới dạng JSON với cấu trúc:

```json
{
    "command": "LOGIN",
    "statusCode": 200,
    "message": "OK",
    "data": {
        "username": "user123",
        "passwordHash": "..."
    },
    "timestamp": 1699869600
}
```

### Các Command Chính

#### Authentication
- `LOGIN` - Đăng nhập
- `REGISTER` - Đăng ký tài khoản mới
- `LOGOUT` - Đăng xuất

#### Chat
- `CHAT_MESSAGE` - Gửi tin nhắn
- `GET_MESSAGES` - Lấy lịch sử tin nhắn
- `MESSAGE_RECEIVED` - Xác nhận nhận tin nhắn

#### Friends
- `ADD_FRIEND` - Thêm bạn bè
- `GET_FRIENDS` - Lấy danh sách bạn bè
- `ACCEPT_FRIEND` - Chấp nhận yêu cầu kết bạn
- `REJECT_FRIEND` - Từ chối yêu cầu kết bạn

#### Video Call
- `VIDEO_CALL_REQUEST` - Yêu cầu cuộc gọi
- `VIDEO_CALL_ACCEPT` - Chấp nhận cuộc gọi
- `VIDEO_CALL_REJECT` - Từ chối cuộc gọi
- `VIDEO_CALL_END` - Kết thúc cuộc gọi
- `VIDEO_AUDIO_DATA` - Dữ liệu video/audio

#### Status
- `PING` - Kiểm tra kết nối
- `PONG` - Phản hồi PING
- `USER_STATUS_UPDATE` - Cập nhật trạng thái user

#### Error
- `ERROR` - Lỗi

## 🔐 Bảo Mật (Security)

**⚠️ Lưu ý:** Đây là ứng dụng học tập. Để sản phẩm thực tế, cần:

1. **Mã hóa mật khẩu**
   - Hiện tại: SHA256
   - Nên dùng: bcrypt hoặc PBKDF2

2. **SSL/TLS**
   - Mã hóa kết nối giữa Client-Server
   - Sử dụng `SslStream` thay vì `NetworkStream`

3. **Authentication Token**
   - JWT hoặc Session-based authentication
   - Validate token trên mỗi request

4. **Database Security**
   - Parameterized queries (đã implement)
   - Least privilege database user

## 📊 Database Schema

### Users Table
```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    DisplayName VARCHAR(100),
    Status INT DEFAULT 0,          -- 0: Offline, 1: Online, 2: Away, 3: Busy
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastSeen TIMESTAMP
);
```

### Messages Table
```sql
CREATE TABLE Messages (
    MessageId INT PRIMARY KEY AUTO_INCREMENT,
    SenderId INT NOT NULL,
    ReceiverId INT,                -- NULL cho tin nhắn group
    GroupId INT,                   -- NULL cho tin nhắn 1-1
    Content LONGTEXT NOT NULL,
    MessageType INT DEFAULT 0,     -- 0: Text, 1: Image, 2: Audio, 3: Video, 4: File
    SentAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ReadAt TIMESTAMP NULL,
    FOREIGN KEY (SenderId) REFERENCES Users(UserId),
    FOREIGN KEY (ReceiverId) REFERENCES Users(UserId)
);
```

### Friendships Table
```sql
CREATE TABLE Friendships (
    FriendshipId INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    FriendUserId INT NOT NULL,
    Status INT DEFAULT 0,          -- 0: Pending, 1: Accepted, 2: Blocked
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (FriendUserId) REFERENCES Users(UserId),
    UNIQUE KEY unique_friendship (UserId, FriendUserId)
);
```

## 🎯 Các Khái Niệm Lập Trình Mạng

### 1. Socket TCP/IP
- **TcpListener** - Server lắng nghe kết nối
- **TcpClient** - Client kết nối tới server
- **NetworkStream** - Luồng dữ liệu trên socket

### 2. Xử lý Đa Luồng (Multi-threading)
- Mỗi client được xử lý trong một thread riêng
- Thread-safe collections cho tránh race conditions
- Lock mechanism để bảo vệ critical sections

### 3. Serialization/Deserialization
- Chuyển đổi objects ↔ JSON
- Gửi dữ liệu qua mạng dưới dạng bytes

### 4. Protocol Design
- Định nghĩa cấu trúc packet
- Length-prefixed protocol (4 bytes độ dài + JSON data)

### 5. Video/Audio Streaming
- Frame capture từ camera
- Audio capture từ microphone
- Encoding thành byte array
- Relay qua server TCP socket
- Decode và display/playback trên receiver

## 🐛 Troubleshooting

### Lỗi: "Connection refused"
- Đảm bảo Server đang chạy
- Kiểm tra port 5000 có sẵn không
- Firewall có chặn port không?

### Lỗi: "MySQL connection failed"
- Kiểm tra MySQL server đang chạy
- Kiểm tra credentials trong `DbConnection.cs`
- Đảm bảo database `yourchatapp` tồn tại

### Lỗi: "The type initializer threw an exception"
- Xóa bin/obj folders
- Rebuild solution
- Kiểm tra dependencies

## 📝 Ví dụ Sử Dụng

### Đăng nhập
1. Chạy Client
2. Nhập username và password
3. Click "Login"
4. Nếu thành công, MainChatForm sẽ mở

### Gửi Tin Nhắn
1. Chọn bạn bè từ danh sách
2. Nhập tin nhắn trong ô input
3. Nhấn Enter hoặc click Send
4. Tin nhắn sẽ được gửi qua server tới bạn bè

### Cuộc Gọi Video
1. Chọn bạn bè
2. Click "Video Call"
3. VideoCallForm sẽ mở
4. Click "Start Call" để bắt đầu
5. Camera và Microphone sẽ được kích hoạt

## 🔄 Quy Trình Hoạt Động

### Login Flow
```
Client                          Server
  │                              │
  ├─ Login Request ─────────────►│
  │                              │
  │                    Validate credentials
  │                    Hash password & compare
  │                              │
  │◄─────────── Login Response ──┤
  │                              │
  ├─ Authenticated ──────────────►│
  │ (Register user online)       │
```

### Chat Message Flow
```
Client A                Server              Client B
  │                       │                    │
  ├─ CHAT_MESSAGE ───────►│                    │
  │                       │                    │
  │             Validate & Save to DB         │
  │                       │                    │
  │                       ├─ CHAT_MESSAGE ───►│
  │                       │                    │
  │                    ◄──── MESSAGE_RECEIVED─┤
  │                       │                    │
  │◄────── Ack ───────────┤                    │
```

### Video Call Flow
```
Caller                 Server             Callee
  │                      │                   │
  ├─ VIDEO_CALL_REQ ────►│                   │
  │                      │                   │
  │                      ├─ VIDEO_CALL_REQ ─►│
  │                      │                   │
  │                   ◄─ VIDEO_CALL_ACCEPT ─┤
  │◄─ VIDEO_CALL_ACCEPT ─┤                   │
  │                      │                   │
  │ ◄─────── VIDEO_AUDIO_DATA ──────────────►│
  │  (Relay qua server)   │                   │
  │                      │                   │
  │ ├─ VIDEO_CALL_END ──►│                   │
  │                      ├─ VIDEO_CALL_END ─►│
```

## 🎓 Bài Học Chính

Qua project này, bạn sẽ học được:

1. ✅ Socket Programming (TCP/IP)
2. ✅ Client-Server Architecture
3. ✅ Multi-threaded Server
4. ✅ Network Protocol Design
5. ✅ Data Serialization
6. ✅ Database Integration (MySQL)
7. ✅ GUI with WinForms
8. ✅ Threading & Synchronization
9. ✅ Error Handling & Logging
10. ✅ Real-time Communication

## 📚 Tài liệu Tham Khảo

- [System.Net.Sockets Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets)
- [C# Networking Guide](https://docs.microsoft.com/en-us/dotnet/framework/network-programming/)
- [TCP/IP Protocol](https://en.wikipedia.org/wiki/Internet_protocol_suite)
- [JSON Protocol Design](https://www.json.org/)

## 📄 License

Học tập & Nghiên cứu

## 👤 Tác Giả

YourChatApp - Network Programming Project

---

**Chúc bạn học tập vui vẻ! 🚀**
#   Y o u r C h a t A p p  
 