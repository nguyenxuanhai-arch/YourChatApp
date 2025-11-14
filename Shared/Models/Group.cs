using System;
using System.Collections.Generic;

namespace YourChatApp.Shared.Models
{
    [Serializable]
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<int> MemberIds { get; set; } = new List<int>();

        public Group() { }

        public Group(int groupId, string groupName, int createdBy)
        {
            GroupId = groupId;
            GroupName = groupName;
            CreatedBy = createdBy;
            CreatedAt = DateTime.Now;
        }
    }
}
