namespace YourChatApp.Client.Forms
{
    partial class MainChatForm
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
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            logoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            friendsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addFriendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewFriendListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            callToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            videoCallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            friendsLabel = new System.Windows.Forms.Label();
            friendsListBox = new System.Windows.Forms.ListBox();
            chatLabel = new System.Windows.Forms.Label();
            videoCallButton = new System.Windows.Forms.Button();
            chatTextBox = new System.Windows.Forms.RichTextBox();
            messageInputTextBox = new System.Windows.Forms.TextBox();
            sendButton = new System.Windows.Forms.Button();
            notificationPanel = new System.Windows.Forms.Panel();
            notificationListBox = new System.Windows.Forms.ListBox();
            notificationLabel = new System.Windows.Forms.Label();
            statusLabel = new System.Windows.Forms.Label();
            mainTabControl = new System.Windows.Forms.TabControl();
            tabChats = new System.Windows.Forms.TabPage();
            lableChats = new System.Windows.Forms.Label();
            tabGroups = new System.Windows.Forms.TabPage();
            groupsLabel = new System.Windows.Forms.Label();
            createGroupButton = new System.Windows.Forms.Button();
            groupChatTextBox = new System.Windows.Forms.RichTextBox();
            groupMessageInput = new System.Windows.Forms.TextBox();
            sendGroupMessageButton = new System.Windows.Forms.Button();
            viewMembersButton = new System.Windows.Forms.Button();
            manageGroupButton = new System.Windows.Forms.Button();
            groupsListBox = new System.Windows.Forms.ListBox();
            menuStrip1.SuspendLayout();
            notificationPanel.SuspendLayout();
            mainTabControl.SuspendLayout();
            tabChats.SuspendLayout();
            tabGroups.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, friendsToolStripMenuItem, callToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(10, 4, 0, 4);
            menuStrip1.Size = new System.Drawing.Size(1490, 37);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { logoutToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            fileToolStripMenuItem.Text = "&File";
            // 
            // logoutToolStripMenuItem
            // 
            logoutToolStripMenuItem.Name = "logoutToolStripMenuItem";
            logoutToolStripMenuItem.Size = new System.Drawing.Size(171, 34);
            logoutToolStripMenuItem.Text = "&Logout";
            logoutToolStripMenuItem.Click += Logout_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(171, 34);
            exitToolStripMenuItem.Text = "E&xit";
            exitToolStripMenuItem.Click += Exit_Click;
            // 
            // friendsToolStripMenuItem
            // 
            friendsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { addFriendToolStripMenuItem, viewFriendListToolStripMenuItem });
            friendsToolStripMenuItem.Name = "friendsToolStripMenuItem";
            friendsToolStripMenuItem.Size = new System.Drawing.Size(85, 29);
            friendsToolStripMenuItem.Text = "&Friends";
            // 
            // addFriendToolStripMenuItem
            // 
            addFriendToolStripMenuItem.Name = "addFriendToolStripMenuItem";
            addFriendToolStripMenuItem.Size = new System.Drawing.Size(236, 34);
            addFriendToolStripMenuItem.Text = "&Add Friend";
            addFriendToolStripMenuItem.Click += AddFriend_Click;
            // 
            // viewFriendListToolStripMenuItem
            // 
            viewFriendListToolStripMenuItem.Name = "viewFriendListToolStripMenuItem";
            viewFriendListToolStripMenuItem.Size = new System.Drawing.Size(236, 34);
            viewFriendListToolStripMenuItem.Text = "&View Friend List";
            viewFriendListToolStripMenuItem.Click += ViewFriends_Click;
            // 
            // callToolStripMenuItem
            // 
            callToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { videoCallToolStripMenuItem });
            callToolStripMenuItem.Name = "callToolStripMenuItem";
            callToolStripMenuItem.Size = new System.Drawing.Size(56, 29);
            callToolStripMenuItem.Text = "&Call";
            // 
            // videoCallToolStripMenuItem
            // 
            videoCallToolStripMenuItem.Name = "videoCallToolStripMenuItem";
            videoCallToolStripMenuItem.Size = new System.Drawing.Size(193, 34);
            videoCallToolStripMenuItem.Text = "&Video Call";
            videoCallToolStripMenuItem.Click += VideoCall_Click;
            // 
            // friendsLabel
            // 
            friendsLabel.AutoSize = true;
            friendsLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            friendsLabel.Location = new System.Drawing.Point(15, 59);
            friendsLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            friendsLabel.Name = "friendsLabel";
            friendsLabel.Size = new System.Drawing.Size(100, 29);
            friendsLabel.TabIndex = 1;
            friendsLabel.Text = "Friends";
            // 
            // friendsListBox
            // 
            friendsListBox.FormattingEnabled = true;
            friendsListBox.ItemHeight = 25;
            friendsListBox.Location = new System.Drawing.Point(15, 118);
            friendsListBox.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            friendsListBox.Name = "friendsListBox";
            friendsListBox.Size = new System.Drawing.Size(170, 929);
            friendsListBox.TabIndex = 2;
            friendsListBox.SelectedIndexChanged += FriendsList_SelectedIndexChanged;
            friendsListBox.DoubleClick += FriendsList_DoubleClick;
            // 
            // chatLabel
            // 
            chatLabel.AutoSize = true;
            chatLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            chatLabel.Location = new System.Drawing.Point(195, 69);
            chatLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            chatLabel.Name = "chatLabel";
            chatLabel.Size = new System.Drawing.Size(127, 29);
            chatLabel.TabIndex = 3;
            chatLabel.Text = "Messages";
            // 
            // videoCallButton
            // 
            videoCallButton.Location = new System.Drawing.Point(923, 6);
            videoCallButton.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            videoCallButton.Name = "videoCallButton";
            videoCallButton.Size = new System.Drawing.Size(57, 49);
            videoCallButton.TabIndex = 4;
            videoCallButton.Text = "📹";
            videoCallButton.UseVisualStyleBackColor = true;
            videoCallButton.Click += VideoCall_Click;
            // 
            // chatTextBox
            // 
            chatTextBox.Location = new System.Drawing.Point(10, 54);
            chatTextBox.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            chatTextBox.Name = "chatTextBox";
            chatTextBox.ReadOnly = true;
            chatTextBox.Size = new System.Drawing.Size(970, 834);
            chatTextBox.TabIndex = 5;
            chatTextBox.Text = "";
            // 
            // messageInputTextBox
            // 
            messageInputTextBox.Location = new System.Drawing.Point(5, 890);
            messageInputTextBox.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            messageInputTextBox.Multiline = true;
            messageInputTextBox.Name = "messageInputTextBox";
            messageInputTextBox.Size = new System.Drawing.Size(862, 31);
            messageInputTextBox.TabIndex = 6;
            messageInputTextBox.KeyDown += MessageInput_KeyDown;
            // 
            // sendButton
            // 
            sendButton.Location = new System.Drawing.Point(868, 887);
            sendButton.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            sendButton.Name = "sendButton";
            sendButton.Size = new System.Drawing.Size(120, 34);
            sendButton.TabIndex = 7;
            sendButton.Text = "Send";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += SendButton_Click;
            // 
            // notificationPanel
            // 
            notificationPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            notificationPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            notificationPanel.Controls.Add(notificationListBox);
            notificationPanel.Controls.Add(notificationLabel);
            notificationPanel.Location = new System.Drawing.Point(1206, 59);
            notificationPanel.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            notificationPanel.Name = "notificationPanel";
            notificationPanel.Size = new System.Drawing.Size(274, 988);
            notificationPanel.TabIndex = 8;
            // 
            // notificationListBox
            // 
            notificationListBox.BackColor = System.Drawing.Color.White;
            notificationListBox.FormattingEnabled = true;
            notificationListBox.ItemHeight = 25;
            notificationListBox.Location = new System.Drawing.Point(-1, 58);
            notificationListBox.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            notificationListBox.Name = "notificationListBox";
            notificationListBox.Size = new System.Drawing.Size(274, 929);
            notificationListBox.TabIndex = 1;
            notificationListBox.DoubleClick += NotificationListBox_DoubleClick;
            // 
            // notificationLabel
            // 
            notificationLabel.AutoSize = true;
            notificationLabel.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            notificationLabel.Location = new System.Drawing.Point(15, 20);
            notificationLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            notificationLabel.Name = "notificationLabel";
            notificationLabel.Size = new System.Drawing.Size(211, 26);
            notificationLabel.TabIndex = 0;
            notificationLabel.Text = "🔔 Friend Requests";
            // 
            // statusLabel
            // 
            statusLabel.ForeColor = System.Drawing.Color.Green;
            statusLabel.Location = new System.Drawing.Point(42, 989);
            statusLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new System.Drawing.Size(111, 39);
            statusLabel.TabIndex = 9;
            statusLabel.Text = "Connected";
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(tabChats);
            mainTabControl.Controls.Add(tabGroups);
            mainTabControl.Location = new System.Drawing.Point(195, 59);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new System.Drawing.Size(1001, 986);
            mainTabControl.TabIndex = 20;
            mainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
            // 
            // tabChats
            // 
            tabChats.Controls.Add(lableChats);
            tabChats.Controls.Add(sendButton);
            tabChats.Controls.Add(messageInputTextBox);
            tabChats.Controls.Add(chatTextBox);
            tabChats.Controls.Add(videoCallButton);
            tabChats.Location = new System.Drawing.Point(4, 34);
            tabChats.Name = "tabChats";
            tabChats.Size = new System.Drawing.Size(993, 948);
            tabChats.TabIndex = 0;
            tabChats.Text = "Chats";
            tabChats.UseVisualStyleBackColor = true;
            // 
            // lableChats
            // 
            lableChats.AutoSize = true;
            lableChats.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            lableChats.Location = new System.Drawing.Point(10, 10);
            lableChats.MaximumSize = new System.Drawing.Size(100, 100);
            lableChats.Name = "lableChats";
            lableChats.Size = new System.Drawing.Size(79, 29);
            lableChats.TabIndex = 8;
            lableChats.Text = "Chats";
            // 
            // tabGroups
            // 
            tabGroups.Controls.Add(groupsLabel);
            tabGroups.Controls.Add(createGroupButton);
            tabGroups.Controls.Add(groupChatTextBox);
            tabGroups.Controls.Add(groupMessageInput);
            tabGroups.Controls.Add(sendGroupMessageButton);
            tabGroups.Controls.Add(viewMembersButton);
            tabGroups.Controls.Add(manageGroupButton);
            tabGroups.Location = new System.Drawing.Point(4, 34);
            tabGroups.Name = "tabGroups";
            tabGroups.Size = new System.Drawing.Size(993, 948);
            tabGroups.TabIndex = 1;
            tabGroups.Text = "Groups";
            tabGroups.UseVisualStyleBackColor = true;
            // 
            // groupsLabel
            // 
            groupsLabel.AutoSize = true;
            groupsLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            groupsLabel.Location = new System.Drawing.Point(10, 10);
            groupsLabel.Name = "groupsLabel";
            groupsLabel.Size = new System.Drawing.Size(99, 29);
            groupsLabel.TabIndex = 0;
            groupsLabel.Text = "Groups";
            // 
            // createGroupButton
            // 
            createGroupButton.Location = new System.Drawing.Point(115, 8);
            createGroupButton.Name = "createGroupButton";
            createGroupButton.Size = new System.Drawing.Size(120, 34);
            createGroupButton.TabIndex = 4;
            createGroupButton.Text = "Create Group";
            createGroupButton.UseVisualStyleBackColor = true;
            createGroupButton.Visible = false;
            createGroupButton.Click += OpenCreateGroupForm_Click;
            // 
            // groupChatTextBox
            // 
            groupChatTextBox.Location = new System.Drawing.Point(10, 54);
            groupChatTextBox.Name = "groupChatTextBox";
            groupChatTextBox.ReadOnly = true;
            groupChatTextBox.Size = new System.Drawing.Size(970, 834);
            groupChatTextBox.TabIndex = 15;
            groupChatTextBox.Text = "";
            // 
            // groupMessageInput
            // 
            groupMessageInput.Location = new System.Drawing.Point(10, 904);
            groupMessageInput.Name = "groupMessageInput";
            groupMessageInput.Size = new System.Drawing.Size(862, 31);
            groupMessageInput.TabIndex = 16;
            // 
            // sendGroupMessageButton
            // 
            sendGroupMessageButton.Location = new System.Drawing.Point(873, 904);
            sendGroupMessageButton.Name = "sendGroupMessageButton";
            sendGroupMessageButton.Size = new System.Drawing.Size(120, 34);
            sendGroupMessageButton.TabIndex = 17;
            sendGroupMessageButton.Text = "Send";
            sendGroupMessageButton.UseVisualStyleBackColor = true;
            sendGroupMessageButton.Click += SendGroupMessageButton_Click;
            // 
            // viewMembersButton
            // 
            viewMembersButton.Location = new System.Drawing.Point(810, 8);
            viewMembersButton.Name = "viewMembersButton";
            viewMembersButton.Size = new System.Drawing.Size(170, 34);
            viewMembersButton.TabIndex = 18;
            viewMembersButton.Text = "View Members";
            viewMembersButton.UseVisualStyleBackColor = true;
            viewMembersButton.Visible = false;
            viewMembersButton.Click += ViewMembersButton_Click;
            // 
            // manageGroupButton
            // 
            manageGroupButton.Location = new System.Drawing.Point(684, 8);
            manageGroupButton.Name = "manageGroupButton";
            manageGroupButton.Size = new System.Drawing.Size(120, 34);
            manageGroupButton.TabIndex = 19;
            manageGroupButton.Text = "Manage Group";
            manageGroupButton.UseVisualStyleBackColor = true;
            manageGroupButton.Visible = false;
            manageGroupButton.Click += ManageGroupButton_Click;
            // 
            // groupsListBox
            // 
            groupsListBox.ItemHeight = 25;
            groupsListBox.Location = new System.Drawing.Point(15, 118);
            groupsListBox.Name = "groupsListBox";
            groupsListBox.Size = new System.Drawing.Size(170, 929);
            groupsListBox.TabIndex = 1;
            groupsListBox.Visible = false;
            groupsListBox.SelectedIndexChanged += GroupsListBox_SelectedIndexChanged;
            // 
            // MainChatForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1490, 1074);
            Controls.Add(statusLabel);
            Controls.Add(notificationPanel);
            Controls.Add(mainTabControl);
            Controls.Add(groupsListBox);
            Controls.Add(chatLabel);
            Controls.Add(friendsListBox);
            Controls.Add(friendsLabel);
            Controls.Add(menuStrip1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(1500, 1000);
            Name = "MainChatForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "YourChatApp - Main Chat";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            notificationPanel.ResumeLayout(false);
            notificationPanel.PerformLayout();
            mainTabControl.ResumeLayout(false);
            tabChats.ResumeLayout(false);
            tabChats.PerformLayout();
            tabGroups.ResumeLayout(false);
            tabGroups.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem friendsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFriendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewFriendListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem callToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem videoCallToolStripMenuItem;
        private System.Windows.Forms.Label friendsLabel;
        private System.Windows.Forms.ListBox friendsListBox;
        // Other controls...
        private System.Windows.Forms.Label chatLabel;
        private System.Windows.Forms.Button videoCallButton;
        private System.Windows.Forms.RichTextBox chatTextBox;
        private System.Windows.Forms.TextBox messageInputTextBox;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Panel notificationPanel;
        private System.Windows.Forms.ListBox notificationListBox;
        private System.Windows.Forms.Label notificationLabel;
        private System.Windows.Forms.Label statusLabel;

        // Groups tab controls
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage tabChats;
        private System.Windows.Forms.TabPage tabGroups;
        private System.Windows.Forms.Label groupsLabel;
        private System.Windows.Forms.ListBox groupsListBox;
        private System.Windows.Forms.Button manageGroupButton;
        private System.Windows.Forms.Button createGroupButton;
        private System.Windows.Forms.RichTextBox groupChatTextBox;
        private System.Windows.Forms.TextBox groupMessageInput;
        private System.Windows.Forms.Button sendGroupMessageButton;
        private System.Windows.Forms.Button viewMembersButton;
        private System.Windows.Forms.Label lableChats;
    }
}
