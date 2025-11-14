using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Network
{
    /// <summary>
    /// Quản lý kết nối Socket của client tới server
    /// </summary>
    public class ClientSocket
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected = false;
        private User _currentUser;
        private Thread _receiveThread;

        // Delegate cho các sự kiện
        public delegate void PacketReceivedHandler(CommandPacket packet);
        public delegate void ConnectionStatusHandler(bool isConnected);
        public delegate void ErrorHandler(string errorMessage);

        // Các sự kiện
        public event PacketReceivedHandler OnPacketReceived;
        public event ConnectionStatusHandler OnConnectionStatusChanged;
        public event ErrorHandler OnError;

        private const int SERVER_PORT = 5000;
        
        // Đổi IP này thành IP của máy Server (dùng ipconfig để xem)
        // Ví dụ: "192.168.1.100" nếu Server ở máy khác trong LAN
        // Hoặc giữ "127.0.0.1" nếu Server và Client cùng máy
        private const string SERVER_HOST = "127.0.0.1";

        public ClientSocket()
        {
            _client = new TcpClient();
        }

        /// <summary>
        /// Kết nối tới server
        /// </summary>
        public async Task<bool> ConnectAsync(string host = SERVER_HOST, int port = SERVER_PORT)
        {
            try
            {
                Console.WriteLine($"[*] Connecting to {host}:{port}...");
                
                await _client.ConnectAsync(host, port);
                _stream = _client.GetStream();
                _stream.ReadTimeout = 60000;
                _stream.WriteTimeout = 60000;

                _isConnected = true;
                OnConnectionStatusChanged?.Invoke(true);

                Console.WriteLine("[+] Connected to server");

                // Bắt đầu thread nhận dữ liệu
                _receiveThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true
                };
                _receiveThread.Start();

                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                OnConnectionStatusChanged?.Invoke(false);
                OnError?.Invoke($"Connection failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Connection failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ngắt kết nối
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _isConnected = false;
                _stream?.Close();
                _client?.Close();
                OnConnectionStatusChanged?.Invoke(false);
                Console.WriteLine("[-] Disconnected from server");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Disconnect failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gửi gói tin tới server
        /// </summary>
        public bool SendPacket(CommandPacket packet)
        {
            try
            {
                if (!_isConnected || _stream == null)
                {
                    OnError?.Invoke("Not connected to server");
                    return false;
                }

                byte[] data = PacketProcessor.SerializePacket(packet);
                if (data != null)
                {
                    _stream.Write(data, 0, data.Length);
                    _stream.Flush();
                    Console.WriteLine($"[SEND] {packet.Command}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Send failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Send failed: {ex.Message}");
                Disconnect();
                return false;
            }
        }

        /// <summary>
        /// Vòng lặp nhận dữ liệu từ server
        /// </summary>
        private void ReceiveLoop()
        {
            try
            {
                while (_isConnected)
                {
                    try
                    {
                        CommandPacket packet = PacketProcessor.DeserializePacket(_stream);

                        if (packet == null)
                        {
                            break;
                        }

                        Console.WriteLine($"[RECV] {packet.Command}");
                        OnPacketReceived?.Invoke(packet);
                    }
                    catch (IOException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Receive error: {ex.Message}");
                    }
                }
            }
            finally
            {
                _isConnected = false;
                OnConnectionStatusChanged?.Invoke(false);
                Disconnect();
            }
        }

        /// <summary>
        /// Kiểm tra đã kết nối hay chưa
        /// </summary>
        public bool IsConnected => _isConnected && _client?.Connected == true;

        /// <summary>
        /// Lấy user hiện tại
        /// </summary>
        public User GetCurrentUser() => _currentUser;

        /// <summary>
        /// Đặt user hiện tại
        /// </summary>
        public void SetCurrentUser(User user) => _currentUser = user;
    }
}
