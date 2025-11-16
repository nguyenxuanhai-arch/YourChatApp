using System;
using MySql.Data.MySqlClient;

namespace YourChatApp.Server.Database
{
    /// <summary>
    /// Quản lý kết nối tới MySQL database
    /// </summary>
    public class DbConnection
    {
        private static DbConnection _instance;
        private string _connectionString;
        private string _connectionStringNoDb;
        private static readonly object _lockObj = new object();

        // Cấu hình MySQL - Thay đổi theo máy của bạn
        private const string DEFAULT_HOST = "127.0.0.1";
        private const string DEFAULT_USER = "root";
        private const string DEFAULT_PASSWORD = "123456";
        private const string DEFAULT_DATABASE = "yourchatapp";
        private const int DEFAULT_PORT = 3306;

        // runtime values (may be overridden from environment variable ConnectionStrings__Default)
        private string _host = DEFAULT_HOST;
        private string _user = DEFAULT_USER;
        private string _password = DEFAULT_PASSWORD;
        private string _database = DEFAULT_DATABASE;
        private int _port = DEFAULT_PORT;

        private DbConnection()
        {
            // Allow overriding connection string via environment variable
            string envConn = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                             ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            if (!string.IsNullOrEmpty(envConn))
            {
                try
                {
                    var builder = new MySqlConnectionStringBuilder(envConn);
                    _connectionString = builder.ConnectionString;

                    // copy and remove database for no-db connection
                    var noDbBuilder = new MySqlConnectionStringBuilder(builder.ConnectionString);
                    noDbBuilder.Remove("Database");
                    _connectionStringNoDb = noDbBuilder.ConnectionString;

                    // set runtime fields for logging
                    _host = builder.Server;
                    _user = builder.UserID;
                    _password = builder.Password;
                    _database = builder.Database;
                    _port = (int)builder.Port;
                }
                catch (Exception ex)
                {
                    // fallback to defaults on parse error
                    Console.WriteLine($"[WARN] Failed to parse connection string from environment: {ex.Message}");
                    _connectionString = $"Server={DEFAULT_HOST};Port={DEFAULT_PORT};Uid={DEFAULT_USER};Pwd={DEFAULT_PASSWORD};Database={DEFAULT_DATABASE};";
                    _connectionStringNoDb = $"Server={DEFAULT_HOST};Port={DEFAULT_PORT};Uid={DEFAULT_USER};Pwd={DEFAULT_PASSWORD};";
                }
            }
            else
            {
                _connectionString = $"Server={DEFAULT_HOST};Port={DEFAULT_PORT};Uid={DEFAULT_USER};Pwd={DEFAULT_PASSWORD};Database={DEFAULT_DATABASE};";
                _connectionStringNoDb = $"Server={DEFAULT_HOST};Port={DEFAULT_PORT};Uid={DEFAULT_USER};Pwd={DEFAULT_PASSWORD};";
            }
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static DbConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new DbConnection();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Mở kết nối mới
        /// </summary>
        public MySqlConnection OpenConnection()
        {
            try
            {
                Console.WriteLine($"[DB] Attempting connection to {_host}:{_port} as {_user}...");
                var connection = new MySqlConnection(_connectionString);
                connection.Open();
                Console.WriteLine($"[✓] Database connection successful!");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[✗] ERROR - Failed to open database connection: {ex.Message}");
                Console.WriteLine($"[!] Connection String: {_connectionString}");
                return null;
            }
        }

        /// <summary>
        /// Mở kết nối không chỉ định database (để tạo database)
        /// </summary>
        private MySqlConnection OpenConnectionNoDb()
        {
            try
            {
                var connection = new MySqlConnection(_connectionStringNoDb);
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[✗] ERROR - Failed to open database connection (no db): {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tạo database nếu chưa tồn tại
        /// </summary>
        private void CreateDatabaseIfNotExists()
        {
            try
            {
                Console.WriteLine($"[DB] Checking if database '{DEFAULT_DATABASE}' exists...");
                using (var connection = OpenConnectionNoDb())
                {
                    if (connection == null)
                    {
                        Console.WriteLine("[✗] Cannot check database - connection failed");
                        return;
                    }

                    var command = connection.CreateCommand();
                    command.CommandText = $"CREATE DATABASE IF NOT EXISTS {DEFAULT_DATABASE}";
                    command.ExecuteNonQuery();
                    Console.WriteLine($"[✓] Database '{DEFAULT_DATABASE}' created or already exists");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[✗] ERROR - Failed to create database: {ex.Message}");
            }
        }

        /// <summary>
        /// Khởi tạo database và tables
        /// </summary>
        public bool InitializeDatabase()
        {
            try
            {
                Console.WriteLine("[DB] Starting database initialization...");
                
                // Tạo database nếu chưa tồn tại
                CreateDatabaseIfNotExists();

                using (var connection = OpenConnection())
                {
                    if (connection == null)
                    {
                        Console.WriteLine("[✗] Cannot initialize database - connection failed");
                        return false;
                    }

                    var command = connection.CreateCommand();
                    
                    // Tạo bảng Users
                    Console.WriteLine("[DB] Creating table: Users...");
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            UserId INT PRIMARY KEY AUTO_INCREMENT,
                            Username VARCHAR(50) UNIQUE NOT NULL,
                            Email VARCHAR(100) UNIQUE NOT NULL,
                            PasswordHash VARCHAR(255) NOT NULL,
                            DisplayName VARCHAR(100),
                            Status INT DEFAULT 0,
                            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            LastSeen TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                        )";
                    command.ExecuteNonQuery();
                    Console.WriteLine("[✓] Table created: Users");

                    // Tạo bảng Messages
                    Console.WriteLine("[DB] Creating table: Messages...");
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Messages (
                            MessageId INT PRIMARY KEY AUTO_INCREMENT,
                            SenderId INT NOT NULL,
                            ReceiverId INT,
                            GroupId INT,
                            Content LONGTEXT NOT NULL,
                            MessageType INT DEFAULT 0,
                            SentAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            ReadAt TIMESTAMP NULL,
                            FOREIGN KEY (SenderId) REFERENCES Users(UserId),
                            FOREIGN KEY (ReceiverId) REFERENCES Users(UserId),
                            INDEX idx_receiver (ReceiverId),
                            INDEX idx_sender (SenderId)
                        )";
                    command.ExecuteNonQuery();
                    Console.WriteLine("[✓] Table created: Messages");

                    // Tạo bảng Friendships
                    Console.WriteLine("[DB] Creating table: Friendships...");
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Friendships (
                            FriendshipId INT PRIMARY KEY AUTO_INCREMENT,
                            UserId INT NOT NULL,
                            FriendUserId INT NOT NULL,
                            Status INT DEFAULT 0,
                            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (UserId) REFERENCES Users(UserId),
                            FOREIGN KEY (FriendUserId) REFERENCES Users(UserId),
                            UNIQUE KEY unique_friendship (UserId, FriendUserId)
                        )";
                    command.ExecuteNonQuery();
                    Console.WriteLine("[✓] Table created: Friendships");

                    // Tạo bảng Groups
                    Console.WriteLine("[DB] Creating table: Groups...");
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS `Groups` (
                            GroupId INT PRIMARY KEY AUTO_INCREMENT,
                            GroupName VARCHAR(100) NOT NULL,
                            Description TEXT,
                            CreatedBy INT NOT NULL,
                            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
                        )";
                    command.ExecuteNonQuery();
                    Console.WriteLine("[✓] Table created: Groups");

                    // Tạo bảng GroupMembers
                    Console.WriteLine("[DB] Creating table: GroupMembers...");
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS `GroupMembers` (
                            GroupMemberId INT PRIMARY KEY AUTO_INCREMENT,
                            GroupId INT NOT NULL,
                            UserId INT NOT NULL,
                            JoinedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (GroupId) REFERENCES `Groups`(GroupId),
                            FOREIGN KEY (UserId) REFERENCES Users(UserId),
                            UNIQUE KEY unique_member (GroupId, UserId)
                        )";
                    command.ExecuteNonQuery();
                    Console.WriteLine("[✓] Table created: GroupMembers");

                    // Tạo bảng GroupInvites
                    Console.WriteLine("[DB] Creating table: GroupInvites...");
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS `GroupInvites` (
                            InviteId INT PRIMARY KEY AUTO_INCREMENT,
                            GroupId INT NOT NULL,
                            InviterId INT NOT NULL,
                            InvitedUserId INT NOT NULL,
                            Status INT DEFAULT 0,
                            InvitedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            RespondedAt TIMESTAMP NULL,
                            FOREIGN KEY (GroupId) REFERENCES `Groups`(GroupId),
                            FOREIGN KEY (InviterId) REFERENCES Users(UserId),
                            FOREIGN KEY (InvitedUserId) REFERENCES Users(UserId),
                            UNIQUE KEY unique_invite (GroupId, InvitedUserId)
                        )";
                    command.ExecuteNonQuery();
                    Console.WriteLine("[✓] Table created: GroupInvites");

                    // Tạo bảng VideoCallRequests
                    Console.WriteLine("[DB] Creating table: VideoCallRequests...");
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS VideoCallRequests (
                            CallId VARCHAR(36) PRIMARY KEY,
                            CallerId INT NOT NULL,
                            CallerName VARCHAR(100) NOT NULL,
                            ReceiverId INT NOT NULL,
                            Status VARCHAR(20) DEFAULT 'pending',
                            InitiatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            AcceptedAt TIMESTAMP NULL,
                            RejectedAt TIMESTAMP NULL,
                            FOREIGN KEY (CallerId) REFERENCES Users(UserId),
                            FOREIGN KEY (ReceiverId) REFERENCES Users(UserId)
                        )";
                    command.ExecuteNonQuery();
                    Console.WriteLine("[✓] Table created: VideoCallRequests");

                    Console.WriteLine("[✓✓✓] Database initialized successfully - All tables created!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[✗✗✗] ERROR - Database initialization failed: {ex.Message}");
                Console.WriteLine($"[!] Details: {ex.InnerException?.Message}");
                return false;
            }
        }
    }
}
