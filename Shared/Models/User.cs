using System;
using System.Collections.Generic;

namespace YourChatApp.Shared.Models
{
    [Serializable]
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Offline;
        public DateTime LastSeen { get; set; }

        public User() { }

        public User(int userId, string username, string email, string displayName)
        {
            UserId = userId;
            Username = username;
            Email = email;
            DisplayName = displayName;
            CreatedAt = DateTime.Now;
        }
    }

    public enum UserStatus
    {
        Offline = 0,
        Online = 1,
        Away = 2,
        Busy = 3
    }
}
