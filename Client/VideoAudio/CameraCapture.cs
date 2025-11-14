using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace YourChatApp.Client.VideoAudio
{
    /// <summary>
    /// Xử lý Capture camera và hiển thị video
    /// Ghi chú: Để đơn giản cho môn Lập trình Mạng, chúng ta sẽ dùng GDI+ và các API Windows cơ bản
    /// Trong thực tế sản phẩm, nên dùng DirectShow hoặc thư viện khác specialized
    /// </summary>
    public class CameraCapture
    {
        private bool _isCapturing = false;
        private int _frameRate = 30; // 30 FPS
        private int _frameWidth = 640;
        private int _frameHeight = 480;

        // Delegate cho các frame mới
        public delegate void FrameCapturedHandler(Bitmap frame);
        public event FrameCapturedHandler OnFrameCaptured;

        public delegate void ErrorHandler(string errorMessage);
        public event ErrorHandler OnError;

        /// <summary>
        /// Bắt đầu capture camera
        /// </summary>
        public void StartCapture()
        {
            if (_isCapturing)
                return;

            _isCapturing = true;

            // Chạy capture trong background thread
            Task.Run(() =>
            {
                try
                {
                    Console.WriteLine("[+] Camera capture started");

                    // Simulate camera capture (vì không có DirectShow/camera thực)
                    int interval = 1000 / _frameRate; // Milliseconds per frame

                    while (_isCapturing)
                    {
                        // Tạo một bitmap mẫu (trong thực tế sẽ từ camera)
                        Bitmap frame = CreateBlankFrame();

                        // Phát event
                        OnFrameCaptured?.Invoke(frame);

                        Thread.Sleep(interval);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Camera capture error: {ex.Message}");
                    Console.WriteLine($"[ERROR] Camera capture failed: {ex.Message}");
                }
                finally
                {
                    _isCapturing = false;
                }
            });
        }

        /// <summary>
        /// Dừng capture camera
        /// </summary>
        public void StopCapture()
        {
            _isCapturing = false;
            Console.WriteLine("[-] Camera capture stopped");
        }

        /// <summary>
        /// Tạo bitmap frame mẫu
        /// </summary>
        private Bitmap CreateBlankFrame()
        {
            Bitmap bitmap = new Bitmap(_frameWidth, _frameHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
                g.DrawString("Video Stream", new Font("Arial", 20), Brushes.White, 10, 10);
                g.DrawString(DateTime.Now.ToString(), new Font("Arial", 12), Brushes.Yellow, 10, 40);
            }

            return bitmap;
        }

        /// <summary>
        /// Đặt độ phân giải frame
        /// </summary>
        public void SetFrameSize(int width, int height)
        {
            _frameWidth = width;
            _frameHeight = height;
        }

        /// <summary>
        /// Kiểm tra có đang capture hay không
        /// </summary>
        public bool IsCapturing => _isCapturing;
    }
}
