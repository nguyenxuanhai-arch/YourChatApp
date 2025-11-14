using System;
using System.Threading;
using System.Threading.Tasks;

namespace YourChatApp.Client.VideoAudio
{
    /// <summary>
    /// Xử lý thu âm từ Microphone và phát âm thanh
    /// Ghi chú: Để đơn giản, chúng ta sẽ mock các chức năng này
    /// Trong thực tế, nên dùng NAudio hoặc WASAPI
    /// </summary>
    public class AudioCapturePlayback
    {
        private bool _isRecording = false;
        private bool _isPlaying = false;
        private int _sampleRate = 44100; // Hz
        private int _bitRate = 16; // bits

        // Delegate cho dữ liệu âm thanh
        public delegate void AudioDataHandler(byte[] audioData);
        public event AudioDataHandler OnAudioDataCaptured;

        public delegate void ErrorHandler(string errorMessage);
        public event ErrorHandler OnError;

        /// <summary>
        /// Bắt đầu ghi âm
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording)
                return;

            _isRecording = true;

            Task.Run(() =>
            {
                try
                {
                    Console.WriteLine("[+] Audio recording started");

                    int chunkSize = _sampleRate * _bitRate / 8 / 10; // 100ms chunks
                    int interval = 100; // Milliseconds

                    while (_isRecording)
                    {
                        // Tạo dữ liệu âm thanh mẫu (silence)
                        byte[] audioChunk = new byte[chunkSize];
                        for (int i = 0; i < audioChunk.Length; i++)
                        {
                            audioChunk[i] = 128; // Silent (mid-point for signed audio)
                        }

                        OnAudioDataCaptured?.Invoke(audioChunk);

                        Thread.Sleep(interval);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Audio recording error: {ex.Message}");
                    Console.WriteLine($"[ERROR] Audio recording failed: {ex.Message}");
                }
                finally
                {
                    _isRecording = false;
                }
            });
        }

        /// <summary>
        /// Dừng ghi âm
        /// </summary>
        public void StopRecording()
        {
            _isRecording = false;
            Console.WriteLine("[-] Audio recording stopped");
        }

        /// <summary>
        /// Phát âm thanh
        /// </summary>
        public void PlayAudio(byte[] audioData)
        {
            try
            {
                if (_isPlaying)
                {
                    Console.WriteLine("[WARN] Already playing audio");
                    return;
                }

                _isPlaying = true;

                Task.Run(() =>
                {
                    try
                    {
                        // Trong thực tế, dữ liệu âm thanh sẽ được chuyển tới speaker
                        Console.WriteLine($"[*] Playing audio: {audioData.Length} bytes");
                        
                        // Simulate playback delay
                        Thread.Sleep(100);

                        _isPlaying = false;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke($"Audio playback error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Audio playback error: {ex.Message}");
                Console.WriteLine($"[ERROR] Audio playback failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Dừng phát âm thanh
        /// </summary>
        public void StopPlayback()
        {
            _isPlaying = false;
            Console.WriteLine("[-] Audio playback stopped");
        }

        /// <summary>
        /// Kiểm tra có đang ghi âm hay không
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// Kiểm tra có đang phát hay không
        /// </summary>
        public bool IsPlaying => _isPlaying;
    }
}
