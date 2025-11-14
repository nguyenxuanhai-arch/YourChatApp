using System;
using System.Collections.Generic;
using YourChatApp.Shared.Models;
using YourChatApp.Server.Database;

namespace YourChatApp.Server.Services
{
    /// <summary>
    /// Xử lý logic bạn bè
    /// </summary>
    public class FriendService
    {
        private FriendRepository _friendRepository = new FriendRepository();
        private UserRepository _userRepository = new UserRepository();

        /// <summary>
        /// Lấy danh sách bạn bè của user
        /// </summary>
        public List<User> GetFriendList(int userId)
        {
            try
            {
                return _friendRepository.GetFriends(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetFriendList failed: {ex.Message}");
                return new List<User>();
            }
        }

        /// <summary>
        /// Gửi yêu cầu kết bạn
        /// </summary>
        public bool SendFriendRequest(int senderId, int targetUserId)
        {
            try
            {
                // Kiểm tra user tồn tại
                var user = _userRepository.GetUserById(targetUserId);
                if (user == null)
                {
                    Console.WriteLine($"[WARN] Target user {targetUserId} not found");
                    return false;
                }

                // Kiểm tra đã là bạn bè
                if (_friendRepository.IsFriend(senderId, targetUserId))
                {
                    Console.WriteLine($"[WARN] Users {senderId} and {targetUserId} are already friends");
                    return false;
                }

                return _friendRepository.SendFriendRequest(senderId, targetUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendFriendRequest failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Chấp nhận yêu cầu kết bạn
        /// </summary>
        public bool AcceptFriendRequest(int userId, int friendUserId)
        {
            try
            {
                return _friendRepository.AcceptFriendRequest(userId, friendUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AcceptFriendRequest failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Từ chối yêu cầu kết bạn
        /// </summary>
        public bool RejectFriendRequest(int userId, int friendUserId)
        {
            try
            {
                return _friendRepository.RejectFriendRequest(userId, friendUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RejectFriendRequest failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Xóa bạn
        /// </summary>
        public bool RemoveFriend(int userId, int friendUserId)
        {
            try
            {
                return _friendRepository.RemoveFriend(userId, friendUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RemoveFriend failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem hai user có phải bạn không
        /// </summary>
        public bool IsFriend(int userId1, int userId2)
        {
            try
            {
                return _friendRepository.IsFriend(userId1, userId2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IsFriend failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tìm kiếm user theo username
        /// </summary>
        public User SearchUser(string username)
        {
            try
            {
                return _userRepository.GetUserByUsername(username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SearchUser failed: {ex.Message}");
                return null;
            }
        }
    }
}
