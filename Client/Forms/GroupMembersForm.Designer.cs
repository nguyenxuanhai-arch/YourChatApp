namespace YourChatApp.Client.Forms
{
    partial class GroupMembersForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox membersListBox;
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
            this.membersListBox = new System.Windows.Forms.ListBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // membersListBox
            // 
            this.membersListBox.FormattingEnabled = true;
            this.membersListBox.ItemHeight = 25;
            this.membersListBox.Location = new System.Drawing.Point(12, 12);
            this.membersListBox.Name = "membersListBox";
            this.membersListBox.Size = new System.Drawing.Size(360, 304);
            this.membersListBox.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(252, 330);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(120, 34);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // GroupMembersForm
            // 
            this.ClientSize = new System.Drawing.Size(384, 376);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.membersListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GroupMembersForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Group Members";
            this.ResumeLayout(false);
        }
    }
}
