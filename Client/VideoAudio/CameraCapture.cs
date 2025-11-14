using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;

namespace YourChatApp.Client.VideoAudio
{
    /// <summary>
    /// Xử lý Capture camera thật sử dụng AForge.NET
    /// Fallback về simulated camera nếu không có camera thật
    /// </summary>
    public class CameraCapture
    {
        private VideoCaptureDevice _videoDevice;
        private FilterInfoCollection _videoDevices;
        private bool _isCapturing = false;
        private bool _useSimulatedCamera = false;
        private int _frameWidth = 640;
        private int _frameHeight = 480;
        private CancellationTokenSource _simulationCancellation;

        // Delegate cho các frame mới
        public delegate void FrameCapturedHandler(Bitmap frame);
        public event FrameCapturedHandler OnFrameCaptured;

        public delegate void ErrorHandler(string errorMessage);
        public event ErrorHandler OnError;

        public CameraCapture()
        {
            try
            {
                // Lấy danh sách camera devices
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                
                if (_videoDevices.Count == 0)
                {
                    Console.WriteLine("[WARN] No camera devices found. Using simulated camera.");
                }
                else
                {
                    Console.WriteLine($"[+] Found {_videoDevices.Count} camera device(s)");
                    for (int i = 0; i < _videoDevices.Count; i++)
                    {
                        Console.WriteLine($"    [{i}] {_videoDevices[i].Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to enumerate cameras: {ex.Message}");
                _videoDevices = null;
            }
        }

        /// <summary>
        /// Bắt đầu capture camera
        /// </summary>
        public void StartCapture()
        {
            if (_isCapturing)
                return;

            try
            {
                // Thử dùng camera thật trước
                if (_videoDevices != null && _videoDevices.Count > 0)
                {
                    try
                    {
                        // Sử dụng camera đầu tiên
                        _videoDevice = new VideoCaptureDevice(_videoDevices[0].MonikerString);

                        // Set resolution nếu có thể
                        if (_videoDevice.VideoCapabilities != null && _videoDevice.VideoCapabilities.Length > 0)
                        {
                            // Tìm resolution gần nhất với 640x480
                            var capability = _videoDevice.VideoCapabilities
                                .OrderBy(c => Math.Abs(c.FrameSize.Width - _frameWidth) + Math.Abs(c.FrameSize.Height - _frameHeight))
                                .FirstOrDefault();
                            
                            if (capability != null)
                            {
                                _videoDevice.VideoResolution = capability;
                                Console.WriteLine($"[+] Camera resolution set to {capability.FrameSize.Width}x{capability.FrameSize.Height} @ {capability.AverageFrameRate} FPS");
                            }
                        }

                        // Hook vào event khi có frame mới
                        _videoDevice.NewFrame += VideoDevice_NewFrame;

                        // Bắt đầu capture
                        _videoDevice.Start();
                        _isCapturing = true;
                        _useSimulatedCamera = false;
                        
                        Console.WriteLine($"[+] Real camera capture started: {_videoDevices[0].Name}");
                        return;
                    }
                    catch (Exception cameraEx)
                    {
                        Console.WriteLine($"[WARN] Real camera failed: {cameraEx.Message}, falling back to simulated camera");
                        OnError?.Invoke($"Camera error: {cameraEx.Message}. Using simulated camera.");
                    }
                }
                
                // Fallback: Dùng simulated camera
                Console.WriteLine("[+] Starting simulated camera");
                _useSimulatedCamera = true;
                _isCapturing = true;
                _simulationCancellation = new CancellationTokenSource();
                
                Task.Run(() => SimulateCameraCapture(_simulationCancellation.Token));
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Failed to start camera: {ex.Message}");
                Console.WriteLine($"[ERROR] Camera start failed: {ex.Message}");
                _isCapturing = false;
            }
        }

        /// <summary>
        /// Simulate camera khi không có camera thật
        /// </summary>
        private void SimulateCameraCapture(CancellationToken cancellationToken)
        {
            Console.WriteLine("[+] Simulated camera running");
            int frameCount = 0;
            
            while (!cancellationToken.IsCancellationRequested && _isCapturing)
            {
                try
                {
                    Bitmap frame = CreateSimulatedFrame(frameCount++);
                    OnFrameCaptured?.Invoke(frame);
                    Thread.Sleep(100); // 10 FPS
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] Simulated camera error: {ex.Message}");
                }
            }
            
            Console.WriteLine("[-] Simulated camera stopped");
        }

        /// <summary>
        /// Tạo frame giả lập
        /// </summary>
        private Bitmap CreateSimulatedFrame(int frameNumber)
        {
            Bitmap bitmap = new Bitmap(_frameWidth, _frameHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Background gradient
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, _frameWidth, _frameHeight),
                    Color.FromArgb(40, 40, 80),
                    Color.FromArgb(20, 20, 40),
                    45f))
                {
                    g.FillRectangle(brush, 0, 0, _frameWidth, _frameHeight);
                }
                
                // Camera icon simulation
                g.DrawString("📹 SIMULATED CAMERA", new Font("Arial", 24, FontStyle.Bold), Brushes.White, 120, 180);
                g.DrawString("No real camera detected", new Font("Arial", 14), Brushes.LightGray, 180, 230);
                g.DrawString($"Frame: {frameNumber}", new Font("Arial", 12), Brushes.Yellow, 10, 10);
                g.DrawString(DateTime.Now.ToString("HH:mm:ss.fff"), new Font("Arial", 12), Brushes.Yellow, 10, 450);
            }
            
            return bitmap;
        }

        /// <summary>
        /// Event handler khi có frame mới từ camera
        /// </summary>
        private void VideoDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Clone bitmap vì eventArgs.Frame sẽ bị dispose sau khi event xong
                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
                
                // Resize nếu cần
                if (frame.Width != _frameWidth || frame.Height != _frameHeight)
                {
                    Bitmap resized = new Bitmap(_frameWidth, _frameHeight);
                    using (Graphics g = Graphics.FromImage(resized))
                    {
                        g.DrawImage(frame, 0, 0, _frameWidth, _frameHeight);
                    }
                    frame.Dispose();
                    frame = resized;
                }

                // Phát event với frame mới
                OnFrameCaptured?.Invoke(frame);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Error processing frame: {ex.Message}");
            }
        }

        /// <summary>
        /// Dừng capture camera
        /// </summary>
        public void StopCapture()
        {
            if (!_isCapturing)
                return;

            try
            {
                _isCapturing = false;
                
                // Stop real camera
                if (_videoDevice != null)
                {
                    if (_videoDevice.IsRunning)
                    {
                        _videoDevice.SignalToStop();
                        _videoDevice.WaitForStop();
                    }
                    _videoDevice.NewFrame -= VideoDevice_NewFrame;
                    _videoDevice = null;
                }
                
                // Stop simulated camera
                if (_simulationCancellation != null)
                {
                    _simulationCancellation.Cancel();
                    _simulationCancellation.Dispose();
                    _simulationCancellation = null;
                }
                
                Console.WriteLine("[-] Camera capture stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Error stopping camera: {ex.Message}");
            }
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

        /// <summary>
        /// Cleanup
        /// </summary>
        ~CameraCapture()
        {
            StopCapture();
        }
    }
}
