using System;
using System.Collections.Generic;
using YourChatApp.Shared.Models;
using YourChatApp.Server.Database;

namespace YourChatApp.Server.Services
{
    /// <summary>
    /// Xử lý logic chat
    /// </summary>
    public class ChatService
    {
        private MessageRepository _messageRepository = new MessageRepository();
        private FriendRepository _friendRepository = new FriendRepository();

        /// <summary>
        /// Gửi tin nhắn từ user này sang user khác
        /// </summary>
        public bool SendMessage(int senderId, int receiverId, string content)
        {
            try
            {
                // Kiểm tra người nhận tồn tại
                var userRepo = new UserRepository();
                var receiver = userRepo.GetUserById(receiverId);
                if (receiver == null)
                {
                    Console.WriteLine($"[WARN] Receiver user {receiverId} not found");
                    return false;
                }

                // Tạo message
                var message = new Message(senderId, receiverId, content);

                // Lưu vào database
                return _messageRepository.SaveMessage(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendMessage failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy lịch sử tin nhắn giữa hai user
        /// </summary>
        public List<Message> GetChatHistory(int userId1, int userId2, int limit = 50)
        {
            try
            {
                return _messageRepository.GetMessages(userId1, userId2, limit);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetChatHistory failed: {ex.Message}");
                return new List<Message>();
            }
        }

        /// <summary>
        /// Lấy tin nhắn chưa đọc
        /// </summary>
        public List<Message> GetUnreadMessages(int userId)
        {
            try
            {
                return _messageRepository.GetUnreadMessages(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetUnreadMessages failed: {ex.Message}");
                return new List<Message>();
            }
        }

        /// <summary>
        /// Đánh dấu tin nhắn đã đọc
        /// </summary>
        public bool MarkMessageAsRead(int messageId)
        {
            try
            {
                return _messageRepository.MarkMessageAsRead(messageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MarkMessageAsRead failed: {ex.Message}");
                return false;
            }
        }
    }
}
