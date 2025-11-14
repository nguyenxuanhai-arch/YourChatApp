using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using YourChatApp.Shared.Models;
using YourChatApp.Server.Network;

namespace YourChatApp.Server
{
    /// <summary>
    /// Server chính - Quản lý TcpListener và các kết nối client
    /// </summary>
    public class Server
    {
        private TcpListener _listener;
        private bool _isRunning;
        private int _port;
        private int _clientCounter = 0;
        private Dictionary<int, ClientHandler> _connectedClients = new Dictionary<int, ClientHandler>();
        private Dictionary<int, int> _userIdToClientId = new Dictionary<int, int>(); // Map userId -> clientId
        private object _lockObj = new object();

        public const int DEFAULT_PORT = 5000;

        public Server(int port = DEFAULT_PORT)
        {
            _port = port;
        }

        /// <summary>
        /// Bắt đầu server
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _isRunning = true;

                Console.WriteLine("╔════════════════════════════════════════════════╗");
                Console.WriteLine("║        YourChatApp Server Started              ║");
                Console.WriteLine($"║        Listening on 0.0.0.0:{_port}             ║");
                Console.WriteLine($"║        Status: RUNNING                         ║");
                Console.WriteLine("╚════════════════════════════════════════════════╝");

                // Bắt đầu vòng lặp chấp nhận kết nối
                await AcceptClientsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL ERROR] Server startup failed: {ex.Message}");
                _isRunning = false;
            }
        }

        /// <summary>
        /// Vòng lặp chấp nhận kết nối từ client
        /// </summary>
        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    
                    // Tạo một ClientHandler mới trong một thread riêng
                    int clientId = Interlocked.Increment(ref _clientCounter);
                    var handler = new ClientHandler(client, this, clientId);

                    lock (_lockObj)
                    {
                        _connectedClients[clientId] = handler;
                    }

                    handler.Start();
                }
                catch (ObjectDisposedException)
                {
                    // Listener đã bị đóng
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Accept client failed: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Đăng ký user khi đã xác thực
        /// </summary>
        public void RegisterClient(int userId, int clientId)
        {
            lock (_lockObj)
            {
                if (!_userIdToClientId.ContainsKey(userId))
                {
                    _userIdToClientId[userId] = clientId;
                    Console.WriteLine($"[+] User {userId} registered with client {clientId}");
                }
            }
        }

        /// <summary>
        /// Bỏ đăng ký user
        /// </summary>
        public void UnregisterClient(int userId)
        {
            lock (_lockObj)
            {
                if (_userIdToClientId.ContainsKey(userId))
                {
                    _userIdToClientId.Remove(userId);
                    Console.WriteLine($"[-] User {userId} unregistered");
                }
            }
        }

        /// <summary>
        /// Gửi gói tin đến một user cụ thể
        /// </summary>
        public void SendToUser(int userId, CommandPacket packet)
        {
            lock (_lockObj)
            {
                if (_userIdToClientId.TryGetValue(userId, out int clientId))
                {
                    if (_connectedClients.TryGetValue(clientId, out ClientHandler handler))
                    {
                        handler.SendPacket(packet);
                    }
                }
            }
        }

        /// <summary>
        /// Gửi gói tin đến nhiều user
        /// </summary>
        public void SendToUsers(List<int> userIds, CommandPacket packet)
        {
            foreach (int userId in userIds)
            {
                SendToUser(userId, packet);
            }
        }

        /// <summary>
        /// Gửi gói tin đến tất cả client đang kết nối
        /// </summary>
        public void BroadcastPacket(CommandPacket packet)
        {
            lock (_lockObj)
            {
                foreach (var handler in _connectedClients.Values)
                {
                    handler.SendPacket(packet);
                }
            }
        }

        /// <summary>
        /// Lấy thông tin client
        /// </summary>
        public ClientHandler GetClientHandler(int clientId)
        {
            lock (_lockObj)
            {
                _connectedClients.TryGetValue(clientId, out var handler);
                return handler;
            }
        }

        /// <summary>
        /// Lấy số client đang kết nối
        /// </summary>
        public int GetConnectedClientCount()
        {
            lock (_lockObj)
            {
                return _connectedClients.Count;
            }
        }

        /// <summary>
        /// Xóa client handler khỏi danh sách
        /// </summary>
        public void RemoveClientHandler(int clientId)
        {
            lock (_lockObj)
            {
                _connectedClients.Remove(clientId);
            }
        }

        /// <summary>
        /// Dừng server
        /// </summary>
        public void Stop()
        {
            _isRunning = false;

            try
            {
                _listener?.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Stop server failed: {ex.Message}");
            }

            Console.WriteLine("[*] Server stopped");
        }
    }
}
