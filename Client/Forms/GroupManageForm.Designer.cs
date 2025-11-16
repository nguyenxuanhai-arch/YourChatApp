namespace YourChatApp.Client.Forms
{
    partial class GroupManageForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label groupLabel;
        private System.Windows.Forms.ListBox membersListBox;
        private System.Windows.Forms.ComboBox availableComboBox;
        private System.Windows.Forms.Button addMemberButton;
        private System.Windows.Forms.Button removeMemberButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button closeButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.groupLabel = new System.Windows.Forms.Label();
            this.membersListBox = new System.Windows.Forms.ListBox();
            this.availableComboBox = new System.Windows.Forms.ComboBox();
            this.addMemberButton = new System.Windows.Forms.Button();
            this.removeMemberButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // groupLabel
            // 
            this.groupLabel.AutoSize = false;
            this.groupLabel.Location = new System.Drawing.Point(12, 8);
            this.groupLabel.Name = "groupLabel";
            this.groupLabel.Size = new System.Drawing.Size(360, 28);
            this.groupLabel.TabIndex = 0;
            this.groupLabel.Text = "Group...";
            this.groupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.groupLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            // 
            // membersListBox
            // 
            this.membersListBox.FormattingEnabled = true;
            this.membersListBox.ItemHeight = 25;
            this.membersListBox.Location = new System.Drawing.Point(12, 44);
            this.membersListBox.Name = "membersListBox";
            this.membersListBox.Size = new System.Drawing.Size(360, 200);
            this.membersListBox.TabIndex = 1;
            this.membersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // availableComboBox
            // 
            this.availableComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.availableComboBox.Location = new System.Drawing.Point(12, 256);
            this.availableComboBox.Name = "availableComboBox";
            this.availableComboBox.Size = new System.Drawing.Size(240, 33);
            this.availableComboBox.TabIndex = 2;
            this.availableComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // addMemberButton
            // 
            this.addMemberButton.Location = new System.Drawing.Point(260, 252);
            this.addMemberButton.Name = "addMemberButton";
            this.addMemberButton.Size = new System.Drawing.Size(112, 36);
            this.addMemberButton.TabIndex = 3;
            this.addMemberButton.Text = "Add";
            this.addMemberButton.UseVisualStyleBackColor = true;
            this.addMemberButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top)));
            this.addMemberButton.Click += new System.EventHandler(this.addMemberButton_Click);
            // 
            // removeMemberButton
            // 
            this.removeMemberButton.Location = new System.Drawing.Point(12, 296);
            this.removeMemberButton.Name = "removeMemberButton";
            this.removeMemberButton.Size = new System.Drawing.Size(360, 36);
            this.removeMemberButton.TabIndex = 4;
            this.removeMemberButton.Text = "Remove Selected Member";
            this.removeMemberButton.UseVisualStyleBackColor = true;
            this.removeMemberButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right) 
            | System.Windows.Forms.AnchorStyles.Top)));
            this.removeMemberButton.Click += new System.EventHandler(this.removeMemberButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(12, 340);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(160, 36);
            this.deleteButton.TabIndex = 5;
            this.deleteButton.Text = "Delete Group";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(252, 340);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(120, 36);
            this.closeButton.TabIndex = 6;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // GroupManageForm
            // 
            this.ClientSize = new System.Drawing.Size(384, 392);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.removeMemberButton);
            this.Controls.Add(this.addMemberButton);
            this.Controls.Add(this.availableComboBox);
            this.Controls.Add(this.membersListBox);
            this.Controls.Add(this.groupLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GroupManageForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage Group";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
