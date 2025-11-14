using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using YourChatApp.Server.Database;

namespace YourChatApp.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            int port = Server.DEFAULT_PORT;

            // Kiểm tra argument dòng lệnh
            if (args.Length > 0 && int.TryParse(args[0], out int customPort))
            {
                port = customPort;
            }

            try
            {
                // Initialize database first
                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║     DATABASE INITIALIZATION            ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");
                var dbConnection = DbConnection.Instance;
                if (!dbConnection.InitializeDatabase())
                {
                    Console.WriteLine("[✗] Fatal: Database initialization failed. Exiting...");
                    return;
                }

                // Create test users
                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║     CREATING TEST USERS                ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");
                CreateTestUsers();
                Console.WriteLine();

                var server = new Server(port);
                await server.StartAsync();

                // Chặn cho đến khi user nhấn Enter
                Console.WriteLine("\n[*] Press Enter to stop the server...");
                Console.ReadLine();

                server.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        static void CreateTestUsers()
        {
            try
            {
                var userRepo = new YourChatApp.Server.Database.UserRepository();

                // Test user 1: testuser / password123
                var user1 = userRepo.GetUserByUsername("testuser");
                if (user1 == null)
                {
                    if (userRepo.CreateUser("testuser", "test@example.com", "Test User", HashPassword("password123")))
                    {
                        Console.WriteLine("[✓] Created test user: testuser / password123");
                    }
                }
                else
                {
                    Console.WriteLine("[*] Test user 'testuser' already exists");
                }

                // Test user 2: user1 / 123456
                var user2 = userRepo.GetUserByUsername("user1");
                if (user2 == null)
                {
                    if (userRepo.CreateUser("user1", "user1@example.com", "User One", HashPassword("123456")))
                    {
                        Console.WriteLine("[✓] Created test user: user1 / 123456");
                    }
                }
                else
                {
                    Console.WriteLine("[*] Test user 'user1' already exists");
                }

                // Test user 3: user2 / 123456
                var user3 = userRepo.GetUserByUsername("user2");
                if (user3 == null)
                {
                    if (userRepo.CreateUser("user2", "user2@example.com", "User Two", HashPassword("123456")))
                    {
                        Console.WriteLine("[✓] Created test user: user2 / 123456");
                    }
                }
                else
                {
                    Console.WriteLine("[*] Test user 'user2' already exists");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create test users: {ex.Message}");
            }
        }
    }
}