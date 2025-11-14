# QUICK START GUIDE - YourChatApp

## 🚀 Bắt đầu nhanh trong 5 phút

### Bước 1: Kiểm tra yêu cầu
```
✓ .NET 6.0 SDK
✓ MySQL Server
✓ Visual Studio 2022 hoặc VS Code
```

### Bước 2: Cài đặt MySQL Database
```sql
-- Mở MySQL Command Line Client hoặc MySQL Workbench
CREATE DATABASE yourchatapp;
USE yourchatapp;

-- Tạo tables (Server.cs sẽ tự tạo khi chạy)
```

### Bước 3: Cấu hình Connection String
```
File: Server/Database/DbConnection.cs

Thay đổi:
- DEFAULT_HOST = "localhost"
- DEFAULT_USER = "root"
- DEFAULT_PASSWORD = "" (hoặc password của bạn)
- DEFAULT_DATABASE = "yourchatapp"
```

### Bước 4: Build Solution
```bash
cd YourChatApp
dotnet build
```

### Bước 5: Chạy Server
```bash
cd Server
dotnet run

# Output:
# ╔════════════════════════════════════════════════╗
# ║        YourChatApp Server Started              ║
# ║        Listening on 0.0.0.0:5000               ║
# ║        Status: RUNNING                         ║
# ╚════════════════════════════════════════════════╝
```

### Bước 6: Chạy Client (mở terminal mới)
```bash
cd Client
dotnet run
```

### Bước 7: Test Application
```
1. Tạo tài khoản mới (click "Register")
   - Username: user1
   - Email: user1@example.com
   - Password: password123

2. Đăng nhập (click "Login")

3. Tạo tài khoản thứ 2 (mở client khác)
   - Username: user2
   - Email: user2@example.com
   - Password: password123

4. Test chat giữa 2 user

5. Test video call
```

## ⚠️ Common Issues & Solutions

### Issue: "Unable to connect to MySQL server"
**Solution:**
```
1. Kiểm tra MySQL đang chạy: 
   - Windows: Services > MySQL80 (hoặc phiên bản khác)
   - macOS: System Preferences > MySQL
   
2. Kiểm tra credentials:
   - Username: root (mặc định)
   - Password: (nếu có đặt password)
   - Host: 127.0.0.1
   - Port: 3306
```

### Issue: "Address already in use" (Port 5000)
**Solution:**
```
1. Tìm process dùng port 5000:
   Windows: netstat -ano | findstr :5000
   Linux/Mac: lsof -i :5000

2. Kill process hoặc thay đổi port trong Server.cs:
   public const int DEFAULT_PORT = 5001; // Thay 5000 bằng port khác
```

### Issue: "Cannot build solution"
**Solution:**
```
1. Clean all build artifacts:
   dotnet clean

2. Restore NuGet packages:
   dotnet restore

3. Rebuild:
   dotnet build
```

## 📁 Các file quan trọng

| File | Mô tả |
|------|-------|
| `Server/Server.cs` | Core server logic |
| `Server/ClientHandler.cs` | Xử lý mỗi client |
| `Server/Database/DbConnection.cs` | MySQL config |
| `Client/Network/ClientSocket.cs` | Socket client |
| `Client/Forms/LoginForm.cs` | Form đăng nhập |
| `Shared/Models/CommandPacket.cs` | Giao thức |

## 🔌 Network Configuration

### Server
- **Host:** 0.0.0.0 (listen on all interfaces)
- **Port:** 5000 (mặc định)
- **Protocol:** TCP/IP
- **Encoding:** UTF-8

### Client
- **Connect to:** 127.0.0.1:5000 (localhost)
- **Timeout:** 60 seconds

### Firewall (nếu cần)
```
Windows:
- Firewall & Network Protection
- Allow app through firewall
- Add YourChatApp.Server.exe for port 5000
```

## 📊 Testing Checklist

- [ ] Server khởi động thành công
- [ ] Client kết nối tới server
- [ ] Register tài khoản mới
- [ ] Login với tài khoản
- [ ] Gửi tin nhắn
- [ ] Nhận tin nhắn
- [ ] Thêm bạn bè
- [ ] Video call

## 🔧 Debugging

### Enable Console Logging
Console output sẽ hiển thị:
```
[+] Client connected
[RECV] LOGIN command
[SEND] Response OK
[-] Client disconnected
```

### Database Logging
Bạn có thể query trực tiếp:
```sql
SELECT * FROM Users;
SELECT * FROM Messages;
SELECT * FROM Friendships;
```

## 📞 Support Ports

- **Server:** 5000
- **MySQL:** 3306

## 🎯 Next Steps

Sau khi chạy thành công:
1. Xem source code để hiểu Socket programming
2. Thêm các feature khác (groups, file transfer)
3. Thực hiện encryption (SSL/TLS)
4. Optimize performance (connection pooling)
5. Deploy lên server thực tế

---

**Happy Coding! 🚀**
