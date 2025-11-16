using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using YourChatApp.Shared.Models;

namespace YourChatApp.Server.Database
{
    public class MessageRepository
    {
        private DbConnection _dbConnection = DbConnection.Instance;

        public bool SaveMessage(Message message)
        {
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Messages (SenderId, ReceiverId, GroupId, Content, MessageType, SentAt) VALUES (@SenderId, @ReceiverId, @GroupId, @Content, @MessageType, @SentAt)";
                    command.Parameters.AddWithValue("@SenderId", message.SenderId);
                    command.Parameters.AddWithValue("@ReceiverId", message.ReceiverId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@GroupId", message.GroupId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Content", message.Content);
                    command.Parameters.AddWithValue("@MessageType", (int)message.MessageType);
                    command.Parameters.AddWithValue("@SentAt", message.SentAt);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SaveMessage failed: {ex.Message}");
                return false;
            }
        }

        public List<Message> GetMessages(int userId1, int userId2, int limit = 50)
        {
            var messages = new List<Message>();
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT MessageId, SenderId, ReceiverId, GroupId, Content, MessageType, SentAt, ReadAt FROM Messages WHERE (SenderId = @UserId1 AND ReceiverId = @UserId2) OR (SenderId = @UserId2 AND ReceiverId = @UserId1) ORDER BY SentAt DESC LIMIT @Limit";
                    command.Parameters.AddWithValue("@UserId1", userId1);
                    command.Parameters.AddWithValue("@UserId2", userId2);
                    command.Parameters.AddWithValue("@Limit", limit);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            messages.Add(MapReaderToMessage(reader));
                    }
                }
                messages.Reverse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetMessages failed: {ex.Message}");
            }
            return messages;
        }

        public List<Message> GetGroupMessages(int groupId, int limit = 100)
        {
            var messages = new List<Message>();
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT MessageId, SenderId, ReceiverId, GroupId, Content, MessageType, SentAt, ReadAt FROM Messages WHERE GroupId = @GroupId ORDER BY SentAt DESC LIMIT @Limit";
                    command.Parameters.AddWithValue("@GroupId", groupId);
                    command.Parameters.AddWithValue("@Limit", limit);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            messages.Add(MapReaderToMessage(reader));
                    }
                }
                messages.Reverse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetGroupMessages failed: {ex.Message}");
            }
            return messages;
        }

        public List<Message> GetUnreadMessages(int userId)
        {
            var messages = new List<Message>();
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT MessageId, SenderId, ReceiverId, GroupId, Content, MessageType, SentAt, ReadAt FROM Messages WHERE ReceiverId = @ReceiverId AND ReadAt IS NULL ORDER BY SentAt DESC";
                    command.Parameters.AddWithValue("@ReceiverId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            messages.Add(MapReaderToMessage(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetUnreadMessages failed: {ex.Message}");
            }
            return messages;
        }

        public bool MarkMessageAsRead(int messageId)
        {
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Messages SET ReadAt = NOW() WHERE MessageId = @MessageId";
                    command.Parameters.AddWithValue("@MessageId", messageId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MarkMessageAsRead failed: {ex.Message}");
                return false;
            }
        }

        public bool DeleteMessage(int messageId)
        {
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Messages WHERE MessageId = @MessageId";
                    command.Parameters.AddWithValue("@MessageId", messageId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DeleteMessage failed: {ex.Message}");
                return false;
            }
        }

        private Message MapReaderToMessage(MySqlDataReader reader)
        {
            return new Message
            {
                MessageId = reader.GetInt32(0),
                SenderId = reader.GetInt32(1),
                ReceiverId = reader.IsDBNull(2) ? null : (int?)reader.GetInt32(2),
                GroupId = reader.IsDBNull(3) ? null : (int?)reader.GetInt32(3),
                Content = reader.GetString(4),
                MessageType = (MessageType)reader.GetInt32(5),
                SentAt = reader.GetDateTime(6),
                ReadAt = reader.IsDBNull(7) ? null : (DateTime?)reader.GetDateTime(7)
            };
        }
    }
}
