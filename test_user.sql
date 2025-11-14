-- Insert test user into MySQL
-- Password hash for "password123": sha256("password123") in Base64

INSERT INTO yourchatapp.Users (Username, Email, PasswordHash, DisplayName, Status) 
VALUES ('testuser', 'test@example.com', 'PMWkjZYHYa4xEJRjWrsGSmXbKxO7VspZqmFLvwUf+OI=', 'Test User', 1)
ON DUPLICATE KEY UPDATE Status = 1;

-- If you want to insert with a specific password hash, use:
-- Hash of "123456": /97ujZYhSZrL8hngUgXEMUhhFwhjN6lhO4aJG5LSx5s=
INSERT INTO yourchatapp.Users (Username, Email, PasswordHash, DisplayName, Status) 
VALUES ('user1', 'user1@example.com', '/97ujZYhSZrL8hngUgXEMUhhFwhjN6lhO4aJG5LSx5s=', 'User One', 0)
ON DUPLICATE KEY UPDATE Status = 0;

SELECT * FROM yourchatapp.Users;
