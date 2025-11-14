using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using YourChatApp.Shared.Models;
using YourChatApp.Server.Database;

namespace YourChatApp.Server.Services
{
    /// <summary>
    /// Xử lý logic xác thực người dùng
    /// </summary>
    public class AuthenticationService
    {
        private UserRepository _userRepository = new UserRepository();

        /// <summary>
        /// Mã hóa mật khẩu sử dụng SHA256
        /// </summary>
        public string HashPassword(string password)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return Convert.ToBase64String(hashedBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Password hashing failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra mật khẩu
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                var hashOfInput = HashPassword(password);
                return hashOfInput == hash;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Đăng nhập user
        /// </summary>
        public User Login(string username, string password)
        {
            try
            {
                var user = _userRepository.GetUserByUsername(username);
                if (user == null)
                {
                    Console.WriteLine($"[WARN] Login failed: user '{username}' not found");
                    return null;
                }

                // Kiểm tra mật khẩu
                // TODO: Thêm logic so sánh mật khẩu từ database

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Login failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Đăng ký user mới
        /// </summary>
        public bool Register(string username, string email, string displayName, string password)
        {
            try
            {
                // Kiểm tra user đã tồn tại
                if (_userRepository.GetUserByUsername(username) != null)
                {
                    Console.WriteLine($"[WARN] Registration failed: username '{username}' already exists");
                    return false;
                }

                // Mã hóa mật khẩu
                string passwordHash = HashPassword(password);
                if (passwordHash == null)
                {
                    return false;
                }

                // Tạo user mới
                return _userRepository.CreateUser(username, email, displayName, passwordHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Registration failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra token/session (mở rộng)
        /// </summary>
        public bool ValidateSession(string token)
        {
            // TODO: Implement session/token validation
            return true;
        }
    }
}
