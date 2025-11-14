using System;

namespace YourChatApp.Shared.Models
{
    [Serializable]
    public class VideoCallRequest
    {
        public string CallId { get; set; } = Guid.NewGuid().ToString();
        public int InitiatorId { get; set; }
        public int ReceiverId { get; set; }
        public VideoCallStatus Status { get; set; }
        public DateTime InitiatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public VideoCallRequest() { }

        public VideoCallRequest(int initiatorId, int receiverId)
        {
            InitiatorId = initiatorId;
            ReceiverId = receiverId;
            Status = VideoCallStatus.Pending;
            InitiatedAt = DateTime.Now;
        }
    }

    public enum VideoCallStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        InProgress = 3,
        Ended = 4,
        Missed = 5
    }
}
