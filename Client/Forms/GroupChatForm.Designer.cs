namespace YourChatApp.Client.Forms
{
    partial class GroupChatForm
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
            panelLeft = new System.Windows.Forms.Panel();
            lstGroups = new System.Windows.Forms.ListView();
            colGroupName = new System.Windows.Forms.ColumnHeader();
            colCreatedAt = new System.Windows.Forms.ColumnHeader();
            panelCreateGroup = new System.Windows.Forms.Panel();
            btnCreateGroup = new System.Windows.Forms.Button();
            txtGroupName = new System.Windows.Forms.TextBox();
            lblCreateGroup = new System.Windows.Forms.Label();
            panelRight = new System.Windows.Forms.Panel();
            panelMembers = new System.Windows.Forms.Panel();
            lstMembers = new System.Windows.Forms.ListView();
            colMemberUsername = new System.Windows.Forms.ColumnHeader();
            colMemberName = new System.Windows.Forms.ColumnHeader();
            btnRemoveMember = new System.Windows.Forms.Button();
            lblMembers = new System.Windows.Forms.Label();
            panelAddMember = new System.Windows.Forms.Panel();
            lstAvailableUsers = new System.Windows.Forms.ListBox();
            btnAddMember = new System.Windows.Forms.Button();
            lblAddMember = new System.Windows.Forms.Label();
            panelCenter = new System.Windows.Forms.Panel();
            txtMessages = new System.Windows.Forms.RichTextBox();
            panelMessageInput = new System.Windows.Forms.Panel();
            btnSendMessage = new System.Windows.Forms.Button();
            txtMessageInput = new System.Windows.Forms.TextBox();
            lblGroupTitle = new System.Windows.Forms.Label();
            panelLeft.SuspendLayout();
            panelCreateGroup.SuspendLayout();
            panelRight.SuspendLayout();
            panelMembers.SuspendLayout();
            panelAddMember.SuspendLayout();
            panelCenter.SuspendLayout();
            panelMessageInput.SuspendLayout();
            SuspendLayout();
            // 
            // panelLeft
            // 
            panelLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelLeft.Controls.Add(lstGroups);
            panelLeft.Controls.Add(panelCreateGroup);
            panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            panelLeft.Location = new System.Drawing.Point(0, 0);
            panelLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new System.Drawing.Size(350, 1033);
            panelLeft.TabIndex = 0;
            // 
            // lstGroups
            // 
            lstGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colGroupName, colCreatedAt });
            lstGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            lstGroups.FullRowSelect = true;
            lstGroups.GridLines = true;
            lstGroups.Location = new System.Drawing.Point(0, 188);
            lstGroups.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            lstGroups.Name = "lstGroups";
            lstGroups.Size = new System.Drawing.Size(348, 843);
            lstGroups.TabIndex = 1;
            lstGroups.UseCompatibleStateImageBehavior = false;
            lstGroups.View = System.Windows.Forms.View.Details;
            lstGroups.SelectedIndexChanged += lstGroups_SelectedIndexChanged;
            // 
            // colGroupName
            // 
            colGroupName.Text = "Group Name";
            colGroupName.Width = 150;
            // 
            // colCreatedAt
            // 
            colCreatedAt.Text = "Created";
            colCreatedAt.Width = 100;
            // 
            // panelCreateGroup
            // 
            panelCreateGroup.Controls.Add(btnCreateGroup);
            panelCreateGroup.Controls.Add(txtGroupName);
            panelCreateGroup.Controls.Add(lblCreateGroup);
            panelCreateGroup.Dock = System.Windows.Forms.DockStyle.Top;
            panelCreateGroup.Location = new System.Drawing.Point(0, 0);
            panelCreateGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            panelCreateGroup.Name = "panelCreateGroup";
            panelCreateGroup.Size = new System.Drawing.Size(348, 188);
            panelCreateGroup.TabIndex = 0;
            // 
            // btnCreateGroup
            // 
            btnCreateGroup.Location = new System.Drawing.Point(19, 125);
            btnCreateGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnCreateGroup.Name = "btnCreateGroup";
            btnCreateGroup.Size = new System.Drawing.Size(185, 47);
            btnCreateGroup.TabIndex = 2;
            btnCreateGroup.Text = "Create Group";
            btnCreateGroup.UseVisualStyleBackColor = true;
            btnCreateGroup.Click += btnCreateGroup_Click;
            // 
            // txtGroupName
            // 
            txtGroupName.Location = new System.Drawing.Point(19, 70);
            txtGroupName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtGroupName.Name = "txtGroupName";
            txtGroupName.Size = new System.Drawing.Size(185, 31);
            txtGroupName.TabIndex = 1;
            // 
            // lblCreateGroup
            // 
            lblCreateGroup.AutoSize = true;
            lblCreateGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblCreateGroup.Location = new System.Drawing.Point(15, 23);
            lblCreateGroup.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblCreateGroup.Name = "lblCreateGroup";
            lblCreateGroup.Size = new System.Drawing.Size(142, 25);
            lblCreateGroup.TabIndex = 0;
            lblCreateGroup.Text = "Create Group";
            // 
            // panelRight
            // 
            panelRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelRight.Controls.Add(panelMembers);
            panelRight.Controls.Add(panelAddMember);
            panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            panelRight.Location = new System.Drawing.Point(1100, 0);
            panelRight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            panelRight.Name = "panelRight";
            panelRight.Size = new System.Drawing.Size(350, 1033);
            panelRight.TabIndex = 1;
            // 
            // panelMembers
            // 
            panelMembers.Controls.Add(lstMembers);
            panelMembers.Controls.Add(btnRemoveMember);
            panelMembers.Controls.Add(lblMembers);
            panelMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            panelMembers.Location = new System.Drawing.Point(0, 0);
            panelMembers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            panelMembers.Name = "panelMembers";
            panelMembers.Size = new System.Drawing.Size(348, 562);
            panelMembers.TabIndex = 1;
            // 
            // lstMembers
            // 
            lstMembers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colMemberUsername, colMemberName });
            lstMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            lstMembers.FullRowSelect = true;
            lstMembers.GridLines = true;
            lstMembers.Location = new System.Drawing.Point(0, 39);
            lstMembers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            lstMembers.Name = "lstMembers";
            lstMembers.Size = new System.Drawing.Size(348, 461);
            lstMembers.TabIndex = 2;
            lstMembers.UseCompatibleStateImageBehavior = false;
            lstMembers.View = System.Windows.Forms.View.Details;
            // 
            // colMemberUsername
            // 
            colMemberUsername.Text = "Username";
            colMemberUsername.Width = 120;
            // 
            // colMemberName
            // 
            colMemberName.Text = "Display Name";
            colMemberName.Width = 130;
            // 
            // btnRemoveMember
            // 
            btnRemoveMember.Dock = System.Windows.Forms.DockStyle.Bottom;
            btnRemoveMember.Location = new System.Drawing.Point(0, 500);
            btnRemoveMember.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnRemoveMember.Name = "btnRemoveMember";
            btnRemoveMember.Size = new System.Drawing.Size(348, 62);
            btnRemoveMember.TabIndex = 1;
            btnRemoveMember.Text = "Remove Member";
            btnRemoveMember.UseVisualStyleBackColor = true;
            btnRemoveMember.Click += btnRemoveMember_Click;
            // 
            // lblMembers
            // 
            lblMembers.BackColor = System.Drawing.SystemColors.ControlLight;
            lblMembers.Dock = System.Windows.Forms.DockStyle.Top;
            lblMembers.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblMembers.Location = new System.Drawing.Point(0, 0);
            lblMembers.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblMembers.Name = "lblMembers";
            lblMembers.Size = new System.Drawing.Size(348, 39);
            lblMembers.TabIndex = 0;
            lblMembers.Text = "Group Members";
            lblMembers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelAddMember
            // 
            panelAddMember.Controls.Add(lstAvailableUsers);
            panelAddMember.Controls.Add(btnAddMember);
            panelAddMember.Controls.Add(lblAddMember);
            panelAddMember.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelAddMember.Location = new System.Drawing.Point(0, 562);
            panelAddMember.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            panelAddMember.Name = "panelAddMember";
            panelAddMember.Size = new System.Drawing.Size(348, 469);
            panelAddMember.TabIndex = 0;
            // 
            // lstAvailableUsers
            // 
            lstAvailableUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            lstAvailableUsers.FormattingEnabled = true;
            lstAvailableUsers.ItemHeight = 25;
            lstAvailableUsers.Location = new System.Drawing.Point(0, 39);
            lstAvailableUsers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            lstAvailableUsers.Name = "lstAvailableUsers";
            lstAvailableUsers.Size = new System.Drawing.Size(348, 368);
            lstAvailableUsers.TabIndex = 2;
            // 
            // btnAddMember
            // 
            btnAddMember.Dock = System.Windows.Forms.DockStyle.Bottom;
            btnAddMember.Location = new System.Drawing.Point(0, 407);
            btnAddMember.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnAddMember.Name = "btnAddMember";
            btnAddMember.Size = new System.Drawing.Size(348, 62);
            btnAddMember.TabIndex = 1;
            btnAddMember.Text = "Add Member";
            btnAddMember.UseVisualStyleBackColor = true;
            btnAddMember.Click += btnAddMember_Click;
            // 
            // lblAddMember
            // 
            lblAddMember.BackColor = System.Drawing.SystemColors.ControlLight;
            lblAddMember.Dock = System.Windows.Forms.DockStyle.Top;
            lblAddMember.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblAddMember.Location = new System.Drawing.Point(0, 0);
            lblAddMember.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblAddMember.Name = "lblAddMember";
            lblAddMember.Size = new System.Drawing.Size(348, 39);
            lblAddMember.TabIndex = 0;
            lblAddMember.Text = "Available Users";
            lblAddMember.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelCenter
            // 
            panelCenter.Controls.Add(txtMessages);
            panelCenter.Controls.Add(panelMessageInput);
            panelCenter.Controls.Add(lblGroupTitle);
            panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            panelCenter.Location = new System.Drawing.Point(350, 0);
            panelCenter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            panelCenter.Name = "panelCenter";
            panelCenter.Size = new System.Drawing.Size(750, 1033);
            panelCenter.TabIndex = 2;
            // 
            // txtMessages
            // 
            txtMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            txtMessages.Location = new System.Drawing.Point(0, 62);
            txtMessages.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtMessages.Name = "txtMessages";
            txtMessages.ReadOnly = true;
            txtMessages.Size = new System.Drawing.Size(750, 846);
            txtMessages.TabIndex = 2;
            txtMessages.Text = "";
            // 
            // panelMessageInput
            // 
            panelMessageInput.Controls.Add(btnSendMessage);
            panelMessageInput.Controls.Add(txtMessageInput);
            panelMessageInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelMessageInput.Location = new System.Drawing.Point(0, 908);
            panelMessageInput.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            panelMessageInput.Name = "panelMessageInput";
            panelMessageInput.Size = new System.Drawing.Size(750, 125);
            panelMessageInput.TabIndex = 1;
            // 
            // btnSendMessage
            // 
            btnSendMessage.Location = new System.Drawing.Point(612, 31);
            btnSendMessage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnSendMessage.Name = "btnSendMessage";
            btnSendMessage.Size = new System.Drawing.Size(125, 62);
            btnSendMessage.TabIndex = 1;
            btnSendMessage.Text = "Send";
            btnSendMessage.UseVisualStyleBackColor = true;
            btnSendMessage.Click += btnSendMessage_Click;
            // 
            // txtMessageInput
            // 
            txtMessageInput.Location = new System.Drawing.Point(12, 31);
            txtMessageInput.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtMessageInput.Multiline = true;
            txtMessageInput.Name = "txtMessageInput";
            txtMessageInput.Size = new System.Drawing.Size(586, 60);
            txtMessageInput.TabIndex = 0;
            txtMessageInput.KeyPress += txtMessageInput_KeyPress;
            // 
            // lblGroupTitle
            // 
            lblGroupTitle.BackColor = System.Drawing.SystemColors.ActiveCaption;
            lblGroupTitle.Dock = System.Windows.Forms.DockStyle.Top;
            lblGroupTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblGroupTitle.Location = new System.Drawing.Point(0, 0);
            lblGroupTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblGroupTitle.Name = "lblGroupTitle";
            lblGroupTitle.Size = new System.Drawing.Size(750, 62);
            lblGroupTitle.TabIndex = 0;
            lblGroupTitle.Text = "Select a Group";
            lblGroupTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GroupChatForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1450, 1033);
            Controls.Add(panelCenter);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            Name = "GroupChatForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Group Chat - YourChatApp";
            panelLeft.ResumeLayout(false);
            panelCreateGroup.ResumeLayout(false);
            panelCreateGroup.PerformLayout();
            panelRight.ResumeLayout(false);
            panelMembers.ResumeLayout(false);
            panelAddMember.ResumeLayout(false);
            panelCenter.ResumeLayout(false);
            panelMessageInput.ResumeLayout(false);
            panelMessageInput.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.ListView lstGroups;
        private System.Windows.Forms.ColumnHeader colGroupName;
        private System.Windows.Forms.ColumnHeader colCreatedAt;
        private System.Windows.Forms.Panel panelCreateGroup;
        private System.Windows.Forms.Button btnCreateGroup;
        private System.Windows.Forms.TextBox txtGroupName;
        private System.Windows.Forms.Label lblCreateGroup;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelMembers;
        private System.Windows.Forms.ListView lstMembers;
        private System.Windows.Forms.ColumnHeader colMemberUsername;
        private System.Windows.Forms.ColumnHeader colMemberName;
        private System.Windows.Forms.Button btnRemoveMember;
        private System.Windows.Forms.Label lblMembers;
        private System.Windows.Forms.Panel panelAddMember;
        private System.Windows.Forms.ListBox lstAvailableUsers;
        private System.Windows.Forms.Button btnAddMember;
        private System.Windows.Forms.Label lblAddMember;
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.RichTextBox txtMessages;
        private System.Windows.Forms.Panel panelMessageInput;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.TextBox txtMessageInput;
        private System.Windows.Forms.Label lblGroupTitle;
    }
}
