using System;

namespace YourChatApp.Shared.Models
{
    [Serializable]
    public class Message
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; }
        public int? ReceiverId { get; set; } // Null nếu là tin nhắn group
        public int? GroupId { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? ReadAt { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;

        public Message() { }

        public Message(int senderId, int receiverId, string content)
        {
            SenderId = senderId;
            ReceiverId = receiverId;
            Content = content;
            SentAt = DateTime.Now;
            MessageType = MessageType.Text;
        }

        public Message(int senderId, int groupId, string content, bool isGroupMessage)
        {
            SenderId = senderId;
            GroupId = groupId;
            Content = content;
            SentAt = DateTime.Now;
            MessageType = MessageType.Text;
        }
    }

    public enum MessageType
    {
        Text = 0,
        Image = 1,
        Audio = 2,
        Video = 3,
        File = 4
    }
}
