using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YourChatApp.Shared.Models
{
    /// <summary>
    /// Giao thức truyền thông chuẩn giữa Client và Server
    /// Tất cả dữ liệu được gửi dưới dạng JSON
    /// </summary>
    [Serializable]
    public class CommandPacket
    {
        [JsonProperty("command")]
        public CommandType Command { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; } = 200; // HTTP-like status codes

        [JsonProperty("message")]
        public string Message { get; set; } = "OK";

        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public CommandPacket() { }

        public CommandPacket(CommandType command, Dictionary<string, object> data = null)
        {
            Command = command;
            Data = data ?? new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static CommandPacket FromJson(string json)
        {
            return JsonConvert.DeserializeObject<CommandPacket>(json);
        }
    }

    public enum CommandType
    {
        // Authentication
        LOGIN = 1,
        REGISTER = 2,
        LOGOUT = 3,

        // Chat
        CHAT_MESSAGE = 10,
        GET_MESSAGES = 11,
        MESSAGE_RECEIVED = 12,

        // Friends
        ADD_FRIEND = 20,
        GET_FRIENDS = 21,
        FRIEND_REQUEST = 22,
        ACCEPT_FRIEND = 23,
        REJECT_FRIEND = 24,

        // Groups
        CREATE_GROUP = 30,
        GET_GROUPS = 31,
        JOIN_GROUP = 32,
        LEAVE_GROUP = 33,
        GROUP_MESSAGE = 34,

        // Video Call
        VIDEO_CALL_REQUEST = 40,
        VIDEO_CALL_ACCEPT = 41,
        VIDEO_CALL_REJECT = 42,
        VIDEO_CALL_END = 43,
        VIDEO_FRAME = 44,
        VIDEO_AUDIO_DATA = 45,

        // Status
        USER_STATUS_UPDATE = 50,
        PING = 51,
        PONG = 52,

        // Error
        ERROR = 99
    }
}
