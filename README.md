# YourChatApp - Network Programming Chat Application

## 📋 Giới thiệu

YourChatApp là một ứng dụng chat mạng được xây dựng cho môn **Lập trình Mạng** sử dụng kiến trúc Client-Server với **Socket TCP/IP**. Ứng dụng hỗ trợ:

- Đăng nhập và đăng ký
- Chat 1-1 giữa các user (lưu lịch sử)
- Quản lý bạn bè với yêu cầu kết bạn
- Xem yêu cầu kết bạn ở sidebar bên phải
- Accept/Reject friend requests
- Danh sách bạn bè với trạng thái online/offline
- Cuộc gọi video P2P (relay qua server)
- Ghi âm/phát âm thanh
- Lưu trữ tin nhắn trên MySQL

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
├── Shared/
│   ├── Models/
│   │   ├── CommandPacket.cs
│   │   ├── User.cs
│   │   ├── Message.cs
│   │   ├── Group.cs
│   │   ├── Friend.cs
│   │   └── VideoCallRequest.cs
│   └── Shared.csproj
│
├── Server/
│   ├── Program.cs
│   ├── Server.cs
│   ├── ClientHandler.cs
│   ├── Network/
│   │   └── PacketProcessor.cs
│   ├── Database/
│   │   ├── DbConnection.cs
│   │   ├── UserRepository.cs
│   │   ├── MessageRepository.cs
│   │   └── FriendRepository.cs
│   ├── Services/
│   │   ├── AuthenticationService.cs
│   │   ├── ChatService.cs
│   │   ├── FriendService.cs
│   │   └── VideoCallService.cs
│   └── Server.csproj
│
├── Client/
│   ├── Program.cs
│   ├── Network/
│   │   ├── ClientSocket.cs
│   │   └── PacketProcessor.cs
│   ├── Forms/
│   │   ├── LoginForm.cs
│   │   ├── RegisterForm.cs
│   │   ├── MainChatForm.cs
│   │   ├── ChatWindowForm.cs
│   │   └── VideoCallForm.cs
│   ├── Models/
│   │   └── ClientModels.cs
│   ├── VideoAudio/
│   │   ├── CameraCapture.cs
│   │   └── AudioCapturePlayback.cs
│   └── Client.csproj
│
└── YourChatApp.sln
```

