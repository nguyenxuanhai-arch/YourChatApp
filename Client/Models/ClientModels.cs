using System;
using System.Collections.Generic;

namespace YourChatApp.Client.Models
{
    /// <summary>
    /// Model cho session client
    /// </summary>
    public class ClientSession
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public List<int> FriendIds { get; set; } = new List<int>();
        public Dictionary<int, int> UnreadMessageCounts { get; set; } = new Dictionary<int, int>();

        public ClientSession() { }

        public ClientSession(int userId, string username, string displayName)
        {
            UserId = userId;
            Username = username;
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// Model cho cuộc hội thoại trong UI
    /// </summary>
    public class Conversation
    {
        public int ConversationId { get; set; }
        public string ConversationName { get; set; }
        public bool IsGroup { get; set; }
        public int LastMessageTime { get; set; }
        public string LastMessagePreview { get; set; }
        public int UnreadCount { get; set; }
    }
}
