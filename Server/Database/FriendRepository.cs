using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using YourChatApp.Shared.Models;

namespace YourChatApp.Server.Database
{
    /// <summary>
    /// Xử lý các thao tác Friend với database
    /// </summary>
    public class FriendRepository
    {
        private DbConnection _dbConnection = DbConnection.Instance;

        /// <summary>
        /// Lấy danh sách bạn bè của user
        /// </summary>
        public List<User> GetFriends(int userId)
        {
            var friends = new List<User>();
            var userRepo = new UserRepository();

            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT FriendUserId FROM Friendships
                        WHERE UserId = @UserId AND Status = 1";
                    
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int friendId = reader.GetInt32("FriendUserId");
                            var friend = userRepo.GetUserById(friendId);
                            if (friend != null)
                            {
                                friends.Add(friend);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetFriends failed: {ex.Message}");
            }

            return friends;
        }

        /// <summary>
        /// Gửi yêu cầu kết bạn
        /// </summary>
        public bool SendFriendRequest(int userId, int targetUserId)
        {
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO Friendships (UserId, FriendUserId, Status)
                        VALUES (@UserId, @FriendUserId, 0)";
                    
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@FriendUserId", targetUserId);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
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
                using (var connection = _dbConnection.OpenConnection())
                {
                    var transaction = connection.BeginTransaction();
                    
                    try
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        
                        // Cập nhật yêu cầu hiện tại (friendUserId gửi yêu cầu cho userId)
                        // So record is: UserId = friendUserId, FriendUserId = userId, Status = 0
                        command.CommandText = @"
                            UPDATE Friendships SET Status = 1
                            WHERE UserId = @FriendUserId AND FriendUserId = @UserId";
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@FriendUserId", friendUserId);
                        command.ExecuteNonQuery();

                        // Tạo yêu cầu ngược lại (userId chỉ theo friendUserId)
                        command.Parameters.Clear();
                        command.CommandText = @"
                            INSERT INTO Friendships (UserId, FriendUserId, Status)
                            VALUES (@UserId, @FriendUserId, 1)";
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@FriendUserId", friendUserId);
                        command.ExecuteNonQuery();

                        transaction.Commit();
                        Console.WriteLine($"[DB] Accepted friendship: UserId={friendUserId} -> UserId={userId}");
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
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
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    // The pending request is: UserId = friendUserId, FriendUserId = userId
                    command.CommandText = @"
                        DELETE FROM Friendships
                        WHERE UserId = @FriendUserId AND FriendUserId = @UserId";
                    
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@FriendUserId", friendUserId);

                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                        Console.WriteLine($"[DB] Rejected friendship request from UserId={friendUserId}");
                    return result > 0;
                }
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
                using (var connection = _dbConnection.OpenConnection())
                {
                    var transaction = connection.BeginTransaction();
                    
                    try
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        
                        command.CommandText = @"
                            DELETE FROM Friendships
                            WHERE (UserId = @UserId AND FriendUserId = @FriendUserId)
                               OR (UserId = @FriendUserId AND FriendUserId = @UserId)";
                        
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@FriendUserId", friendUserId);
                        command.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
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
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT COUNT(*) FROM Friendships
                        WHERE (UserId = @UserId1 AND FriendUserId = @UserId2 AND Status = 1)
                           OR (UserId = @UserId2 AND FriendUserId = @UserId1 AND Status = 1)";
                    
                    command.Parameters.AddWithValue("@UserId1", userId1);
                    command.Parameters.AddWithValue("@UserId2", userId2);

                    var result = command.ExecuteScalar();
                    return (long)result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IsFriend failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách các yêu cầu kết bạn chưa được chấp nhận
        /// </summary>
        public List<User> GetPendingFriendRequests(int userId)
        {
            var pendingRequests = new List<User>();
            var userRepo = new UserRepository();

            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT UserId FROM Friendships
                        WHERE FriendUserId = @UserId AND Status = 0";
                    
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int requesterId = reader.GetInt32("UserId");
                            var requester = userRepo.GetUserById(requesterId);
                            if (requester != null)
                            {
                                pendingRequests.Add(requester);
                                Console.WriteLine($"[FRIEND] Found pending request from UserId {requesterId} to UserId {userId}");
                            }
                        }
                    }
                }
                Console.WriteLine($"[FRIEND] Total pending requests for UserId {userId}: {pendingRequests.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetPendingFriendRequests failed: {ex.Message}");
            }

            return pendingRequests;
        }
    }
}
