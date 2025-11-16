namespace YourChatApp.Client.Forms
{
    partial class CreateGroupForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox groupNameTextBox;
        private System.Windows.Forms.CheckedListBox friendsCheckedListBox;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button cancelButton;

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
            this.groupNameTextBox = new System.Windows.Forms.TextBox();
            this.friendsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.createButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // groupNameTextBox
            // 
            this.groupNameTextBox.Location = new System.Drawing.Point(12, 12);
            this.groupNameTextBox.Name = "groupNameTextBox";
            this.groupNameTextBox.PlaceholderText = "Group name";
            this.groupNameTextBox.Size = new System.Drawing.Size(360, 31);
            this.groupNameTextBox.TabIndex = 0;
            // 
            // friendsCheckedListBox
            // 
            this.friendsCheckedListBox.Location = new System.Drawing.Point(12, 50);
            this.friendsCheckedListBox.Name = "friendsCheckedListBox";
            this.friendsCheckedListBox.Size = new System.Drawing.Size(360, 260);
            this.friendsCheckedListBox.TabIndex = 1;
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(12, 320);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(120, 34);
            this.createButton.TabIndex = 2;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(252, 320);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(120, 34);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // CreateGroupForm
            // 
            this.ClientSize = new System.Drawing.Size(384, 370);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.friendsCheckedListBox);
            this.Controls.Add(this.groupNameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateGroupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Group";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
