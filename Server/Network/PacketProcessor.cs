using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using YourChatApp.Shared.Models;

namespace YourChatApp.Server.Network
{
    /// <summary>
    /// Xử lý mã hóa/giải mã các gói tin truyền qua Socket
    /// </summary>
    public class PacketProcessor
    {
        private const int PACKET_LENGTH_SIZE = 4; // 4 bytes cho độ dài gói tin

        /// <summary>
        /// Đóng gói CommandPacket thành chuỗi JSON với kích thước tiền tố
        /// </summary>
        public static byte[] SerializePacket(CommandPacket packet)
        {
            try
            {
                string json = JsonConvert.SerializeObject(packet);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                
                // Tạo buffer: 4 bytes độ dài + JSON data
                byte[] lengthBytes = BitConverter.GetBytes(jsonBytes.Length);
                byte[] result = new byte[lengthBytes.Length + jsonBytes.Length];
                
                Buffer.BlockCopy(lengthBytes, 0, result, 0, lengthBytes.Length);
                Buffer.BlockCopy(jsonBytes, 0, result, lengthBytes.Length, jsonBytes.Length);
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Serialize packet failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Đọc gói tin từ NetworkStream
        /// </summary>
        public static CommandPacket DeserializePacket(Stream stream)
        {
            try
            {
                // Đọc 4 bytes độ dài
                byte[] lengthBuffer = new byte[PACKET_LENGTH_SIZE];
                int bytesRead = stream.Read(lengthBuffer, 0, PACKET_LENGTH_SIZE);
                
                if (bytesRead < PACKET_LENGTH_SIZE)
                {
                    Console.WriteLine("[WARN] Received incomplete length header");
                    return null;
                }

                int packetLength = BitConverter.ToInt32(lengthBuffer, 0);
                
                // Kiểm tra kích thước hợp lý (max 10MB)
                if (packetLength > 10 * 1024 * 1024 || packetLength < 0)
                {
                    Console.WriteLine($"[WARN] Invalid packet size: {packetLength}");
                    return null;
                }

                // Đọc JSON data
                byte[] jsonBuffer = new byte[packetLength];
                bytesRead = 0;
                while (bytesRead < packetLength)
                {
                    int chunk = stream.Read(jsonBuffer, bytesRead, packetLength - bytesRead);
                    if (chunk == 0)
                    {
                        Console.WriteLine("[WARN] Connection closed unexpectedly");
                        return null;
                    }
                    bytesRead += chunk;
                }

                string json = Encoding.UTF8.GetString(jsonBuffer);
                CommandPacket packet = JsonConvert.DeserializeObject<CommandPacket>(json);
                
                return packet;
            }
            catch (IOException)
            {
                // Network connection issues - don't log as error, it's normal disconnection
                return null;
            }
            catch (System.Net.Sockets.SocketException)
            {
                // Socket-specific error - client disconnected
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Deserialize packet failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tạo command packet
        /// </summary>
        public static CommandPacket CreateCommand(CommandType command, Dictionary<string, object> data = null)
        {
            return new CommandPacket(command, data);
        }

        /// <summary>
        /// Tạo response packet
        /// </summary>
        public static CommandPacket CreateResponse(CommandType command, int statusCode, string message, Dictionary<string, object> data = null)
        {
            var packet = new CommandPacket(command, data);
            packet.StatusCode = statusCode;
            packet.Message = message;
            return packet;
        }

        /// <summary>
        /// Tạo error response packet
        /// </summary>
        public static CommandPacket CreateErrorResponse(string errorMessage)
        {
            var packet = new CommandPacket(CommandType.ERROR);
            packet.StatusCode = 500;
            packet.Message = errorMessage;
            return packet;
        }
    }
}
