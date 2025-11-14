using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using YourChatApp.Client.Network;
using YourChatApp.Client.VideoAudio;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Forms
{
    public partial class VideoCallForm : Form
    {
        private ClientSocket _clientSocket;
        private CameraCapture _cameraCapture;
        private AudioCapturePlayback _audioPlayback;
        private string _callId;
        private bool _isInCall = false;

        public VideoCallForm(ClientSocket clientSocket)
        {
            _clientSocket = clientSocket;
            InitializeComponent();
            _cameraCapture = new CameraCapture();
            _audioPlayback = new AudioCapturePlayback();
            _clientSocket.OnPacketReceived += HandleServerMessage;
        }

        private void InitializeComponent()
        {
            this.Text = "Video Call";
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
                if (cameraCheckBox.Checked)
                {
                    _cameraCapture.OnFrameCaptured += DisplayLocalFrame;
                    _cameraCapture.StartCapture();
                }

                // Bắt đầu ghi âm
                if (microphoneCheckBox.Checked)
                {
                    _audioPlayback.OnAudioDataCaptured += SendAudioData;
                    _audioPlayback.StartRecording();
                }

                startCallButton.Enabled = false;
                endCallButton.Enabled = true;

                MessageBox.Show("Video call started");
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

                _cameraCapture.StopCapture();
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
            CheckBox microphoneCheckBox = (CheckBox)sender;

            if (microphoneCheckBox.Checked && _isInCall)
            {
                _audioPlayback.StartRecording();
            }
            else if (!microphoneCheckBox.Checked && _isInCall)
            {
                _audioPlayback.StopRecording();
            }
        }

        private void CameraCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cameraCheckBox = (CheckBox)sender;

            if (cameraCheckBox.Checked && _isInCall)
            {
                _cameraCapture.StartCapture();
            }
            else if (!cameraCheckBox.Checked && _isInCall)
            {
                _cameraCapture.StopCapture();
            }
        }

        private void DisplayLocalFrame(Bitmap frame)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    PictureBox localVideoPictureBox = (PictureBox)this.Controls["localVideoPictureBox"];
                    if (localVideoPictureBox.Image != null)
                    {
                        localVideoPictureBox.Image.Dispose();
                    }
                    localVideoPictureBox.Image = new Bitmap(frame);
                    UpdateStats();
                }));
            }
            catch { }
        }

        private void SendAudioData(byte[] audioData)
        {
            if (!_isInCall)
                return;

            try
            {
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
        }

        private void HandleServerMessage(CommandPacket packet)
        {
            if (packet.Command == CommandType.VIDEO_AUDIO_DATA)
            {
                if (packet.Data.ContainsKey("audioData"))
                {
                    string base64Audio = packet.Data["audioData"].ToString();
                    byte[] audioData = Convert.FromBase64String(base64Audio);
                    _audioPlayback.PlayAudio(audioData);
                }
            }
        }

        private void UpdateStats()
        {
            RichTextBox statsTextBox = (RichTextBox)this.Controls["statsTextBox"];
            statsTextBox.Clear();
            statsTextBox.AppendText($"Call Status: {(_isInCall ? "In Call" : "Idle")}\n");
            statsTextBox.AppendText($"Camera: {(_cameraCapture.IsCapturing ? "ON" : "OFF")}\n");
            statsTextBox.AppendText($"Microphone: {(_audioPlayback.IsRecording ? "ON" : "OFF")}\n");
            statsTextBox.AppendText($"Connection: {(_clientSocket.IsConnected ? "Connected" : "Disconnected")}\n");
        }
    }
}
