using System;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace YourChatApp.Client.VideoAudio
{
    /// <summary>
    /// Xử lý thu âm từ Microphone và phát âm thanh sử dụng NAudio
    /// </summary>
    public class AudioCapturePlayback
    {
        private WaveInEvent _waveIn;
        private BufferedWaveProvider _bufferedWaveProvider;
        private WaveOutEvent _waveOut;
        private bool _isRecording = false;
        private bool _isPlaying = false;
        private WaveFormat _waveFormat;

        // Delegate cho dữ liệu âm thanh
        public delegate void AudioDataHandler(byte[] audioData);
        public event AudioDataHandler OnAudioDataCaptured;

        public delegate void ErrorHandler(string errorMessage);
        public event ErrorHandler OnError;

        public AudioCapturePlayback()
        {
            // Định dạng audio: 8000Hz, 16-bit, Mono (tối ưu cho voice chat)
            _waveFormat = new WaveFormat(8000, 16, 1);
        }

        /// <summary>
        /// Bắt đầu ghi âm
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording)
                return;

            try
            {
                _waveIn = new WaveInEvent
                {
                    WaveFormat = _waveFormat,
                    BufferMilliseconds = 100 // 100ms buffer
                };

                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.RecordingStopped += OnRecordingStopped;

                _waveIn.StartRecording();
                _isRecording = true;
                Console.WriteLine("[+] Audio recording started (NAudio)");
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Audio recording error: {ex.Message}");
                Console.WriteLine($"[ERROR] Audio recording failed: {ex.Message}");
                _isRecording = false;
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (e.BytesRecorded > 0)
                {
                    // Gửi dữ liệu audio đã thu được
                    byte[] audioData = new byte[e.BytesRecorded];
                    Buffer.BlockCopy(e.Buffer, 0, audioData, 0, e.BytesRecorded);
                    OnAudioDataCaptured?.Invoke(audioData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Audio data processing failed: {ex.Message}");
            }
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                Console.WriteLine($"[ERROR] Recording stopped with error: {e.Exception.Message}");
                OnError?.Invoke($"Recording error: {e.Exception.Message}");
            }
        }

        /// <summary>
        /// Dừng ghi âm
        /// </summary>
        public void StopRecording()
        {
            try
            {
                if (_waveIn != null)
                {
                    _waveIn.StopRecording();
                    _waveIn.DataAvailable -= OnDataAvailable;
                    _waveIn.RecordingStopped -= OnRecordingStopped;
                    _waveIn.Dispose();
                    _waveIn = null;
                }
                _isRecording = false;
                Console.WriteLine("[-] Audio recording stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Stop recording failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Phát âm thanh
        /// </summary>
        public void PlayAudio(byte[] audioData)
        {
            try
            {
                // Khởi tạo wave out nếu chưa có
                if (_waveOut == null)
                {
                    _bufferedWaveProvider = new BufferedWaveProvider(_waveFormat)
                    {
                        BufferDuration = TimeSpan.FromSeconds(2),
                        DiscardOnBufferOverflow = true
                    };

                    _waveOut = new WaveOutEvent
                    {
                        DesiredLatency = 100 // 100ms latency
                    };
                    _waveOut.Init(_bufferedWaveProvider);
                    _waveOut.Play();
                    _isPlaying = true;
                }

                // Thêm dữ liệu vào buffer để phát
                _bufferedWaveProvider.AddSamples(audioData, 0, audioData.Length);
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
            try
            {
                if (_waveOut != null)
                {
                    _waveOut.Stop();
                    _waveOut.Dispose();
                    _waveOut = null;
                }
                if (_bufferedWaveProvider != null)
                {
                    _bufferedWaveProvider.ClearBuffer();
                    _bufferedWaveProvider = null;
                }
                _isPlaying = false;
                Console.WriteLine("[-] Audio playback stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Stop playback failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Kiểm tra có đang ghi âm hay không
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// Kiểm tra có đang phát hay không
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            StopRecording();
            StopPlayback();
        }
    }
}