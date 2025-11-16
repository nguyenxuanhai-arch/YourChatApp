using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YourChatApp.Client.Forms
{
    public partial class GroupMembersForm : Form
    {
        public GroupMembersForm(List<string> members, string groupName = "Members")
        {
            InitializeComponent();
            this.Text = groupName;
            if (members != null)
            {
                foreach (var m in members)
                    membersListBox.Items.Add(m);
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
