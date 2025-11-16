namespace YourChatApp.Client.Forms
{
    partial class ChatWindowForm
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
            this.titleLabel = new System.Windows.Forms.Label();
            this.chatDisplayTextBox = new System.Windows.Forms.RichTextBox();
            this.messageInputTextBox = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(10, 10);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(115, 22);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Chat with...";
            // 
            // chatDisplayTextBox
            // 
            this.chatDisplayTextBox.Location = new System.Drawing.Point(10, 45);
            this.chatDisplayTextBox.Name = "chatDisplayTextBox";
            this.chatDisplayTextBox.ReadOnly = true;
            this.chatDisplayTextBox.Size = new System.Drawing.Size(565, 330);
            this.chatDisplayTextBox.TabIndex = 1;
            this.chatDisplayTextBox.Text = "";
            // 
            // messageInputTextBox
            // 
            this.messageInputTextBox.Location = new System.Drawing.Point(10, 385);
            this.messageInputTextBox.Multiline = true;
            this.messageInputTextBox.Name = "messageInputTextBox";
            this.messageInputTextBox.Size = new System.Drawing.Size(500, 70);
            this.messageInputTextBox.TabIndex = 2;
            this.messageInputTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MessageInput_KeyDown);
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(515, 385);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(60, 70);
            this.sendButton.TabIndex = 3;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.Location = new System.Drawing.Point(10, 460);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(565, 20);
            this.statusLabel.TabIndex = 4;
            this.statusLabel.Text = "Ready";
            // 
            // ChatWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 500);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.messageInputTextBox);
            this.Controls.Add(this.chatDisplayTextBox);
            this.Controls.Add(this.titleLabel);
            this.Name = "ChatWindowForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chat Window";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.RichTextBox chatDisplayTextBox;
        private System.Windows.Forms.TextBox messageInputTextBox;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Label statusLabel;
    }
}
