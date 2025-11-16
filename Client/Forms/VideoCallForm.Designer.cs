namespace YourChatApp.Client.Forms
{
    partial class VideoCallForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            localVideoPictureBox = new System.Windows.Forms.PictureBox();
            remoteVideoPictureBox = new System.Windows.Forms.PictureBox();
            startCallButton = new System.Windows.Forms.Button();
            microphoneCheckBox = new System.Windows.Forms.CheckBox();
            cameraCheckBox = new System.Windows.Forms.CheckBox();
            endCallButton = new System.Windows.Forms.Button();
            statsTextBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)localVideoPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)remoteVideoPictureBox).BeginInit();
            SuspendLayout();
            // 
            // localVideoPictureBox
            // 
            localVideoPictureBox.BackColor = System.Drawing.Color.Black;
            localVideoPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            localVideoPictureBox.Location = new System.Drawing.Point(861, 16);
            localVideoPictureBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            localVideoPictureBox.Name = "localVideoPictureBox";
            localVideoPictureBox.Size = new System.Drawing.Size(250, 180);
            localVideoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            localVideoPictureBox.TabIndex = 0;
            localVideoPictureBox.TabStop = false;
            // 
            // remoteVideoPictureBox
            // 
            remoteVideoPictureBox.BackColor = System.Drawing.Color.Black;
            remoteVideoPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            remoteVideoPictureBox.Location = new System.Drawing.Point(12, 16);
            remoteVideoPictureBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            remoteVideoPictureBox.Name = "remoteVideoPictureBox";
            remoteVideoPictureBox.Size = new System.Drawing.Size(1099, 711);
            remoteVideoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            remoteVideoPictureBox.TabIndex = 2;
            remoteVideoPictureBox.TabStop = false;
            // 
            // startCallButton
            // 
            startCallButton.Location = new System.Drawing.Point(642, 647);
            startCallButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            startCallButton.Name = "startCallButton";
            startCallButton.Size = new System.Drawing.Size(125, 62);
            startCallButton.TabIndex = 5;
            startCallButton.Text = "Start Call";
            startCallButton.UseVisualStyleBackColor = true;
            startCallButton.Click += StartCallButton_Click;
            // 
            // microphoneCheckBox
            // 
            microphoneCheckBox.AutoSize = true;
            microphoneCheckBox.Checked = true;
            microphoneCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            microphoneCheckBox.Location = new System.Drawing.Point(323, 680);
            microphoneCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            microphoneCheckBox.Name = "microphoneCheckBox";
            microphoneCheckBox.Size = new System.Drawing.Size(134, 29);
            microphoneCheckBox.TabIndex = 6;
            microphoneCheckBox.Text = "Microphone";
            microphoneCheckBox.UseVisualStyleBackColor = true;
            microphoneCheckBox.CheckedChanged += MicrophoneCheckBox_CheckedChanged;
            // 
            // cameraCheckBox
            // 
            cameraCheckBox.AutoSize = true;
            cameraCheckBox.Checked = true;
            cameraCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            cameraCheckBox.Location = new System.Drawing.Point(201, 680);
            cameraCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cameraCheckBox.Name = "cameraCheckBox";
            cameraCheckBox.Size = new System.Drawing.Size(98, 29);
            cameraCheckBox.TabIndex = 7;
            cameraCheckBox.Text = "Camera";
            cameraCheckBox.UseVisualStyleBackColor = true;
            cameraCheckBox.CheckedChanged += CameraCheckBox_CheckedChanged;
            // 
            // endCallButton
            // 
            endCallButton.Enabled = false;
            endCallButton.Location = new System.Drawing.Point(477, 647);
            endCallButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            endCallButton.Name = "endCallButton";
            endCallButton.Size = new System.Drawing.Size(125, 62);
            endCallButton.TabIndex = 8;
            endCallButton.Text = "End Call";
            endCallButton.UseVisualStyleBackColor = true;
            endCallButton.Click += EndCallButton_Click;
            // 
            // statsTextBox
            // 
            statsTextBox.Location = new System.Drawing.Point(13, 737);
            statsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            statsTextBox.Name = "statsTextBox";
            statsTextBox.ReadOnly = true;
            statsTextBox.Size = new System.Drawing.Size(1099, 151);
            statsTextBox.TabIndex = 9;
            statsTextBox.Text = "";
            // 
            // VideoCallForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1125, 902);
            Controls.Add(statsTextBox);
            Controls.Add(endCallButton);
            Controls.Add(cameraCheckBox);
            Controls.Add(microphoneCheckBox);
            Controls.Add(startCallButton);
            Controls.Add(localVideoPictureBox);
            Controls.Add(remoteVideoPictureBox);
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            Name = "VideoCallForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Video Call";
            ((System.ComponentModel.ISupportInitialize)localVideoPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)remoteVideoPictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox localVideoPictureBox;
        private System.Windows.Forms.PictureBox remoteVideoPictureBox;
        private System.Windows.Forms.Button startCallButton;
        private System.Windows.Forms.CheckBox microphoneCheckBox;
        private System.Windows.Forms.CheckBox cameraCheckBox;
        private System.Windows.Forms.Button endCallButton;
        private System.Windows.Forms.RichTextBox statsTextBox;
    }
}
