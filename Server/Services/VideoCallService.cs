using System;
using System.Collections.Generic;
using YourChatApp.Shared.Models;
using YourChatApp.Server.Database;

namespace YourChatApp.Server.Services
{
    /// <summary>
    /// Xử lý logic cuộc gọi video
    /// </summary>
    public class VideoCallService
    {
        private Dictionary<string, VideoCallRequest> _activeVideoCallRequests = new Dictionary<string, VideoCallRequest>();
        private object _lockObj = new object();

        /// <summary>
        /// Tạo yêu cầu cuộc gọi video
        /// </summary>
        public VideoCallRequest CreateVideoCallRequest(int initiatorId, int receiverId)
        {
            try
            {
                var callRequest = new VideoCallRequest(initiatorId, receiverId);

                lock (_lockObj)
                {
                    _activeVideoCallRequests[callRequest.CallId] = callRequest;
                }

                Console.WriteLine($"[+] Video call request created: {initiatorId} -> {receiverId}");
                return callRequest;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CreateVideoCallRequest failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Chấp nhận cuộc gọi video
        /// </summary>
        public bool AcceptVideoCall(string callId)
        {
            try
            {
                lock (_lockObj)
                {
                    if (_activeVideoCallRequests.TryGetValue(callId, out var callRequest))
                    {
                        callRequest.Status = VideoCallStatus.Accepted;
                        callRequest.StartedAt = DateTime.Now;
                        Console.WriteLine($"[+] Video call accepted: {callId}");
                        return true;
                    }
                }

                Console.WriteLine($"[WARN] Call request {callId} not found");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AcceptVideoCall failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Từ chối cuộc gọi video
        /// </summary>
        public bool RejectVideoCall(string callId)
        {
            try
            {
                lock (_lockObj)
                {
                    if (_activeVideoCallRequests.TryGetValue(callId, out var callRequest))
                    {
                        callRequest.Status = VideoCallStatus.Rejected;
                        _activeVideoCallRequests.Remove(callId);
                        Console.WriteLine($"[+] Video call rejected: {callId}");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RejectVideoCall failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kết thúc cuộc gọi video
        /// </summary>
        public bool EndVideoCall(string callId)
        {
            try
            {
                lock (_lockObj)
                {
                    if (_activeVideoCallRequests.TryGetValue(callId, out var callRequest))
                    {
                        callRequest.Status = VideoCallStatus.Ended;
                        callRequest.EndedAt = DateTime.Now;
                        _activeVideoCallRequests.Remove(callId);
                        Console.WriteLine($"[+] Video call ended: {callId}");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] EndVideoCall failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy thông tin cuộc gọi video
        /// </summary>
        public VideoCallRequest GetVideoCallRequest(string callId)
        {
            lock (_lockObj)
            {
                _activeVideoCallRequests.TryGetValue(callId, out var callRequest);
                return callRequest;
            }
        }

        /// <summary>
        /// Kiểm tra user đang trong cuộc gọi video không
        /// </summary>
        public bool IsUserInCall(int userId)
        {
            lock (_lockObj)
            {
                foreach (var call in _activeVideoCallRequests.Values)
                {
                    if ((call.InitiatorId == userId || call.ReceiverId == userId) &&
                        call.Status == VideoCallStatus.InProgress)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Lấy cuộc gọi video đang diễn ra của user
        /// </summary>
        public VideoCallRequest GetActiveCall(int userId)
        {
            lock (_lockObj)
            {
                foreach (var call in _activeVideoCallRequests.Values)
                {
                    if ((call.InitiatorId == userId || call.ReceiverId == userId) &&
                        (call.Status == VideoCallStatus.InProgress || call.Status == VideoCallStatus.Accepted))
                    {
                        return call;
                    }
                }
            }

            return null;
        }
    }
}
