using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Network
{
    /// <summary>
    /// Xử lý mã hóa/giải mã các gói tin trên Client
    /// </summary>
    public class PacketProcessor
    {
        private const int PACKET_LENGTH_SIZE = 4;

        /// <summary>
        /// Đóng gói CommandPacket thành byte array
        /// </summary>
        public static byte[] SerializePacket(CommandPacket packet)
        {
            try
            {
                string json = JsonConvert.SerializeObject(packet);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

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
        /// Giải mã gói tin từ NetworkStream
        /// </summary>
        public static CommandPacket DeserializePacket(Stream stream)
        {
            try
            {
                byte[] lengthBuffer = new byte[PACKET_LENGTH_SIZE];
                int bytesRead = stream.Read(lengthBuffer, 0, PACKET_LENGTH_SIZE);

                if (bytesRead < PACKET_LENGTH_SIZE)
                {
                    Console.WriteLine("[WARN] Incomplete length header");
                    return null;
                }

                int packetLength = BitConverter.ToInt32(lengthBuffer, 0);

                if (packetLength > 10 * 1024 * 1024 || packetLength < 0)
                {
                    Console.WriteLine($"[WARN] Invalid packet size: {packetLength}");
                    return null;
                }

                byte[] jsonBuffer = new byte[packetLength];
                bytesRead = 0;
                while (bytesRead < packetLength)
                {
                    int chunk = stream.Read(jsonBuffer, bytesRead, packetLength - bytesRead);
                    if (chunk == 0)
                    {
                        Console.WriteLine("[WARN] Connection closed");
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
                // Network connection issues - client disconnected
                return null;
            }
            catch (System.Net.Sockets.SocketException)
            {
                // Socket-specific error
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
        public static CommandPacket CreateCommand(CommandType command, System.Collections.Generic.Dictionary<string, object> data = null)
        {
            return new CommandPacket(command, data ?? new System.Collections.Generic.Dictionary<string, object>());
        }
    }
}
