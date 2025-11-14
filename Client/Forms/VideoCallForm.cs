using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using YourChatApp.Client.Network;
using YourChatApp.Client.VideoAudio;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Forms
{
    public class VideoCallForm : Form
    {
        private ClientSocket _clientSocket;
        private CameraCapture _cameraCapture;
        private AudioCapturePlayback _audioPlayback;
        private string _callId = Guid.NewGuid().ToString();
        private bool _isInCall = false;
        private int _friendId;
        private string _friendName;
        private int _currentUserId;
        private DateTime _lastStatsUpdate = DateTime.MinValue;
        private bool _isSendingAudio = false;
        private DateTime _lastAudioSend = DateTime.MinValue;
        private bool _isSendingVideo = false;
        private DateTime _lastVideoSend = DateTime.MinValue;

        public VideoCallForm(ClientSocket clientSocket, int friendId = 0, string friendName = "", int currentUserId = 0)
        {
            _clientSocket = clientSocket;
            _friendId = friendId;
            _friendName = friendName;
            _currentUserId = currentUserId;
            try
            {
                InitializeComponent();
                // Initialize with stubs to avoid null reference errors
                try
                {
                    _cameraCapture = new CameraCapture();
                }
                catch (Exception cameraEx)
                {
                    Console.WriteLine($"[VideoCallForm] Camera init failed: {cameraEx.Message}, using stub");
                    _cameraCapture = null;
                }

                try
                {
                    _audioPlayback = new AudioCapturePlayback();
                }
                catch (Exception audioEx)
                {
                    Console.WriteLine($"[VideoCallForm] Audio init failed: {audioEx.Message}, using stub");
                    _audioPlayback = null;
                }

                Console.WriteLine($"[VideoCallForm] Video call form initialized for {_friendName} (ID: {_friendId})");
                if (_clientSocket != null)
                    _clientSocket.OnPacketReceived += HandleServerMessage;
                
                // Auto-start call when form opens
                this.Load += (s, e) => AutoStartCall();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VideoCallForm] Error initializing: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error initializing video call form: {ex.Message}");
            }
        }

        private void AutoStartCall()
        {
            try
            {
                // Wait a bit for form to fully load
                System.Threading.Tasks.Task.Delay(500).ContinueWith(t =>
                {
                    this.Invoke(new Action(() =>
                    {
                        Button startCallButton = (Button)this.Controls["startCallButton"];
                        if (startCallButton != null)
                        {
                            StartCallButton_Click(startCallButton, EventArgs.Empty);
                        }
                    }));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VideoCallForm] Auto-start failed: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            try
            {
                this.Text = string.IsNullOrEmpty(_friendName) ? "Video Call" : $"Video Call - {_friendName}";
                this.Size = new System.Drawing.Size(900, 650);
                this.StartPosition = FormStartPosition.CenterScreen;

                // Local Video Display (Trái)
                PictureBox localVideoPictureBox = new PictureBox();
                localVideoPictureBox.Name = "localVideoPictureBox";
                localVideoPictureBox.Location = new System.Drawing.Point(10, 10);
                localVideoPictureBox.Size = new System.Drawing.Size(420, 380);
                localVideoPictureBox.BorderStyle = BorderStyle.FixedSingle;
                localVideoPictureBox.BackColor = Color.Black;
                this.Controls.Add(localVideoPictureBox);

                Label localLabel = new Label();
                localLabel.Text = "Local Video";
                localLabel.Location = new System.Drawing.Point(10, 395);
                localLabel.Size = new System.Drawing.Size(420, 25);
                this.Controls.Add(localLabel);

            // Remote Video Display (Phải)
            PictureBox remoteVideoPictureBox = new PictureBox();
            remoteVideoPictureBox.Name = "remoteVideoPictureBox";
            remoteVideoPictureBox.Location = new System.Drawing.Point(470, 10);
            remoteVideoPictureBox.Size = new System.Drawing.Size(420, 380);
            remoteVideoPictureBox.BorderStyle = BorderStyle.FixedSingle;
            remoteVideoPictureBox.BackColor = Color.Black;
            this.Controls.Add(remoteVideoPictureBox);

            Label remoteLabel = new Label();
            remoteLabel.Text = "Remote Video";
            remoteLabel.Location = new System.Drawing.Point(470, 395);
            remoteLabel.Size = new System.Drawing.Size(420, 25);
            this.Controls.Add(remoteLabel);

            // Controls
            Label controlsLabel = new Label();
            controlsLabel.Text = "Call Controls";
            controlsLabel.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            controlsLabel.Location = new System.Drawing.Point(10, 430);
            controlsLabel.Size = new System.Drawing.Size(200, 25);
            this.Controls.Add(controlsLabel);

            // Start Call Button
            Button startCallButton = new Button();
            startCallButton.Name = "startCallButton";
            startCallButton.Text = "Start Call";
            startCallButton.Location = new System.Drawing.Point(10, 460);
            startCallButton.Size = new System.Drawing.Size(100, 40);
            startCallButton.Click += StartCallButton_Click;
            this.Controls.Add(startCallButton);

            // Microphone Toggle
            CheckBox microphoneCheckBox = new CheckBox();
            microphoneCheckBox.Name = "microphoneCheckBox";
            microphoneCheckBox.Text = "Microphone";
            microphoneCheckBox.Checked = true;
            microphoneCheckBox.Location = new System.Drawing.Point(120, 465);
            microphoneCheckBox.Size = new System.Drawing.Size(120, 25);
            microphoneCheckBox.CheckedChanged += MicrophoneCheckBox_CheckedChanged;
            this.Controls.Add(microphoneCheckBox);

            // Camera Toggle
            CheckBox cameraCheckBox = new CheckBox();
            cameraCheckBox.Name = "cameraCheckBox";
            cameraCheckBox.Text = "Camera";
            cameraCheckBox.Checked = true;
            cameraCheckBox.Location = new System.Drawing.Point(250, 465);
            cameraCheckBox.Size = new System.Drawing.Size(100, 25);
            cameraCheckBox.CheckedChanged += CameraCheckBox_CheckedChanged;
            this.Controls.Add(cameraCheckBox);

            // End Call Button
            Button endCallButton = new Button();
            endCallButton.Name = "endCallButton";
            endCallButton.Text = "End Call";
            endCallButton.Location = new System.Drawing.Point(360, 460);
            endCallButton.Size = new System.Drawing.Size(100, 40);
            endCallButton.Click += EndCallButton_Click;
            endCallButton.Enabled = false;
            this.Controls.Add(endCallButton);

            // Statistics/Status
            RichTextBox statsTextBox = new RichTextBox();
            statsTextBox.Name = "statsTextBox";
            statsTextBox.Location = new System.Drawing.Point(10, 510);
            statsTextBox.Size = new System.Drawing.Size(880, 105);
            statsTextBox.ReadOnly = true;
            this.Controls.Add(statsTextBox);

            UpdateStats();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VideoCallForm] Error in InitializeComponent: {ex.Message}");
                MessageBox.Show($"Error initializing video call UI: {ex.Message}");
            }
        }

        private void StartCallButton_Click(object sender, EventArgs e)
        {
            try
            {
                _isInCall = true;

                CheckBox cameraCheckBox = (CheckBox)this.Controls["cameraCheckBox"];
                CheckBox microphoneCheckBox = (CheckBox)this.Controls["microphoneCheckBox"];
                Button startCallButton = (Button)this.Controls["startCallButton"];
                Button endCallButton = (Button)this.Controls["endCallButton"];

                // Bắt đầu capture camera
                if (cameraCheckBox.Checked && _cameraCapture != null)
                {
                    try
                    {
                        _cameraCapture.OnFrameCaptured += DisplayLocalFrame;
                        _cameraCapture.OnFrameCaptured += SendVideoFrame; // Send to remote
                        _cameraCapture.StartCapture();
                    }
                    catch (Exception cameraEx)
                    {
                        Console.WriteLine($"[VideoCallForm] Camera capture error: {cameraEx.Message}");
                        cameraCheckBox.Checked = false;
                    }
                }

                // Bắt đầu ghi âm
                if (microphoneCheckBox.Checked && _audioPlayback != null)
                {
                    try
                    {
                        _audioPlayback.OnAudioDataCaptured += SendAudioData;
                        _audioPlayback.StartRecording();
                    }
                    catch (Exception audioEx)
                    {
                        Console.WriteLine($"[VideoCallForm] Audio recording error: {audioEx.Message}");
                        microphoneCheckBox.Checked = false;
                    }
                }

                startCallButton.Enabled = false;
                endCallButton.Enabled = true;

                Console.WriteLine("[VideoCallForm] Video call started successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting call: {ex.Message}");
            }
        }

        private void EndCallButton_Click(object sender, EventArgs e)
        {
            try
            {
                _isInCall = false;

                if (_cameraCapture != null)
                    _cameraCapture.StopCapture();
                if (_audioPlayback != null)
                    _audioPlayback.StopRecording();

                Button startCallButton = (Button)this.Controls["startCallButton"];
                Button endCallButton = (Button)this.Controls["endCallButton"];

                startCallButton.Enabled = true;
                endCallButton.Enabled = false;

                PictureBox localVideoPictureBox = (PictureBox)this.Controls["localVideoPictureBox"];
                localVideoPictureBox.Image = null;

                MessageBox.Show("Video call ended");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error ending call: {ex.Message}");
            }
        }

        private void MicrophoneCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_audioPlayback == null)
                return;

            CheckBox microphoneCheckBox = (CheckBox)sender;

            if (microphoneCheckBox.Checked && _isInCall)
            {
                try
                {
                    _audioPlayback.StartRecording();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VideoCallForm] Microphone start error: {ex.Message}");
                }
            }
            else if (!microphoneCheckBox.Checked && _isInCall)
            {
                try
                {
                    _audioPlayback.StopRecording();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VideoCallForm] Microphone stop error: {ex.Message}");
                }
            }
        }

        private void CameraCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_cameraCapture == null)
                return;

            CheckBox cameraCheckBox = (CheckBox)sender;

            if (cameraCheckBox.Checked && _isInCall)
            {
                try
                {
                    _cameraCapture.StartCapture();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VideoCallForm] Camera start error: {ex.Message}");
                }
            }
            else if (!cameraCheckBox.Checked && _isInCall)
            {
                try
                {
                    _cameraCapture.StopCapture();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VideoCallForm] Camera stop error: {ex.Message}");
                }
            }
        }

        private void DisplayLocalFrame(Bitmap frame)
        {
            if (this.IsDisposed || this.Disposing)
                return;

            try
            {
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        PictureBox localVideoPictureBox = (PictureBox)this.Controls["localVideoPictureBox"];
                        if (localVideoPictureBox != null && !localVideoPictureBox.IsDisposed)
                        {
                            var oldImage = localVideoPictureBox.Image;
                            localVideoPictureBox.Image = new Bitmap(frame);
                            oldImage?.Dispose();
                            UpdateStats();
                        }
                    }
                    catch { }
                }));
            }
            catch { }
        }

        private async void SendVideoFrame(Bitmap frame)
        {
            if (!_isInCall || frame == null || _isSendingVideo)
                return;

            // Rate limiting: Send at most 3 frames per second (333ms between frames)
            if ((DateTime.Now - _lastVideoSend).TotalMilliseconds < 333)
                return;

            try
            {
                _isSendingVideo = true;
                _lastVideoSend = DateTime.Now;

                // Compress frame to JPEG for network transmission
                using (var ms = new System.IO.MemoryStream())
                {
                    // Reduce quality to 30% for better network performance
                    var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                        .FirstOrDefault(e => e.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                    var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                    encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                        System.Drawing.Imaging.Encoder.Quality, 30L);
                    
                    frame.Save(ms, encoder, encoderParams);
                    byte[] frameData = ms.ToArray();

                    // Send video frame packet
                    var videoPacket = new Dictionary<string, object>
                    {
                        { "callId", _callId },
                        { "receiverId", 0 },
                        { "videoData", Convert.ToBase64String(frameData) }
                    };

                    CommandPacket packet = PacketProcessor.CreateCommand(CommandType.VIDEO_AUDIO_DATA, videoPacket);
                    await System.Threading.Tasks.Task.Run(() => _clientSocket.SendPacket(packet));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Send video frame failed: {ex.Message}");
            }
            finally
            {
                _isSendingVideo = false;
            }
        }

        private async void SendAudioData(byte[] audioData)
        {
            if (!_isInCall || _isSendingAudio)
                return;

            // Rate limiting: only send once every 200ms (5 packets/sec)
            if ((DateTime.Now - _lastAudioSend).TotalMilliseconds < 200)
                return;

            try
            {
                _isSendingAudio = true;
                _lastAudioSend = DateTime.Now;
                
                // Add delay to prevent network buffer overflow
                await System.Threading.Tasks.Task.Delay(100);
                
                // Gửi audio data qua socket
                var audioPacket = new Dictionary<string, object>
                {
                    { "callId", _callId },
                    { "receiverId", 0 }, // TODO: Set actual receiver
                    { "audioData", Convert.ToBase64String(audioData) }
                };

                CommandPacket packet = PacketProcessor.CreateCommand(CommandType.VIDEO_AUDIO_DATA, audioPacket);
                _clientSocket.SendPacket(packet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Send audio failed: {ex.Message}");
            }
            finally
            {
                _isSendingAudio = false;
            }
        }

        private void DisplayRemoteFrame(byte[] frameData)
        {
            if (this.IsDisposed || this.Disposing || frameData == null)
                return;

            try
            {
                using (var ms = new System.IO.MemoryStream(frameData))
                {
                    Bitmap frame = new Bitmap(ms);
                    
                    this.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            PictureBox remoteVideoPictureBox = (PictureBox)this.Controls["remoteVideoPictureBox"];
                            if (remoteVideoPictureBox != null && !remoteVideoPictureBox.IsDisposed)
                            {
                                var oldImage = remoteVideoPictureBox.Image;
                                remoteVideoPictureBox.Image = frame;
                                oldImage?.Dispose();
                            }
                        }
                        catch { }
                    }));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Display remote frame failed: {ex.Message}");
            }
        }

        private void HandleServerMessage(CommandPacket packet)
        {
            if (packet.Command == CommandType.VIDEO_AUDIO_DATA)
            {
                // Handle audio data
                if (_audioPlayback != null && packet.Data.ContainsKey("audioData"))
                {
                    try
                    {
                        string base64Audio = packet.Data["audioData"].ToString();
                        byte[] audioData = Convert.FromBase64String(base64Audio);
                        _audioPlayback.PlayAudio(audioData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[VideoCallForm] Error playing audio: {ex.Message}");
                    }
                }
                
                // Handle video data
                if (packet.Data.ContainsKey("videoData"))
                {
                    try
                    {
                        string base64Video = packet.Data["videoData"].ToString();
                        byte[] videoData = Convert.FromBase64String(base64Video);
                        DisplayRemoteFrame(videoData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[VideoCallForm] Error displaying video: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateStats()
        {
            // Throttle updates to once per second to avoid performance issues
            if ((DateTime.Now - _lastStatsUpdate).TotalMilliseconds < 1000)
                return;

            _lastStatsUpdate = DateTime.Now;

            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(new Action(UpdateStats));
                }
                catch
                {
                    // Form may be closing
                }
                return;
            }

            try
            {
                RichTextBox statsTextBox = (RichTextBox)this.Controls["statsTextBox"];
                if (statsTextBox != null && !statsTextBox.IsDisposed)
                {
                    statsTextBox.Clear();
                    statsTextBox.AppendText($"Call Status: {(_isInCall ? "In Call" : "Idle")}\n");
                    statsTextBox.AppendText($"Camera: {(_cameraCapture != null && _cameraCapture.IsCapturing ? "ON" : "OFF")}\n");
                    statsTextBox.AppendText($"Microphone: {(_audioPlayback != null && _audioPlayback.IsRecording ? "ON" : "OFF")}\n");
                    statsTextBox.AppendText($"Connection: {(_clientSocket != null && _clientSocket.IsConnected ? "Connected" : "Disconnected")}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VideoCallForm] Error updating stats: {ex.Message}");
            }
        }
    }
}
