using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using YourChatApp.Shared.Models;

namespace YourChatApp.Server.Database
{
    public class UserRepository
    {
        private DbConnection _dbConnection = DbConnection.Instance;

        public User GetUserById(int userId)
        {
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT UserId, Username, Email, DisplayName, Status, CreatedAt, LastSeen FROM Users WHERE UserId = @UserId";
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapReaderToUser(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetUserById failed: {ex.Message}");
            }
            return null;
        }

        public User GetUserByUsername(string username)
        {
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT UserId, Username, Email, DisplayName, Status, CreatedAt, LastSeen FROM Users WHERE Username = @Username";
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapReaderToUser(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetUserByUsername failed: {ex.Message}");
            }
            return null;
        }

        public bool AuthenticateUser(string username, string passwordHash)
        {
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT PasswordHash FROM Users WHERE Username = @Username";
                    command.Parameters.AddWithValue("@Username", username);

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        string storedHash = result.ToString();
                        return storedHash == passwordHash;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AuthenticateUser failed: {ex.Message}");
            }
            return false;
        }

        public bool CreateUser(string username, string email, string displayName, string passwordHash)
        {
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Users (Username, Email, DisplayName, PasswordHash, Status) VALUES (@Username, @Email, @DisplayName, @PasswordHash, 0)";
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@DisplayName", displayName);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] CreateUser failed: {ex.Message}");
                return false;
            }
        }

        public bool UpdateUserStatus(int userId, UserStatus status)
        {
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Users SET Status = @Status, LastSeen = NOW() WHERE UserId = @UserId";
                    command.Parameters.AddWithValue("@Status", (int)status);
                    command.Parameters.AddWithValue("@UserId", userId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateUserStatus failed: {ex.Message}");
                return false;
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            try
            {
                using (var connection = _dbConnection.OpenConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT UserId, Username, Email, DisplayName, Status, CreatedAt, LastSeen FROM Users";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            users.Add(MapReaderToUser(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetAllUsers failed: {ex.Message}");
            }
            return users;
        }

        private User MapReaderToUser(MySqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.GetString(2),
                DisplayName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Status = (UserStatus)reader.GetInt32(4),
                CreatedAt = reader.GetDateTime(5),
                LastSeen = reader.GetDateTime(6)
            };
        }
    }
}
