using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace YourChatApp.Client.Forms
{
    public partial class CreateGroupForm : Form
    {
        public string GroupName => groupNameTextBox.Text.Trim();
        public List<int> SelectedUserIds { get; private set; } = new List<int>();

        private List<Tuple<int, string>> _friends;

        public CreateGroupForm(List<Tuple<int, string>> friends)
        {
            InitializeComponent();
            _friends = friends ?? new List<Tuple<int, string>>();
            LoadFriends();
        }

        private void LoadFriends()
        {
            friendsCheckedListBox.Items.Clear();
            foreach (var f in _friends)
            {
                friendsCheckedListBox.Items.Add(f.Item2);
            }
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            SelectedUserIds.Clear();
            foreach (int i in friendsCheckedListBox.CheckedIndices)
            {
                if (i >= 0 && i < _friends.Count)
                    SelectedUserIds.Add(_friends[i].Item1);
            }

            if (string.IsNullOrEmpty(GroupName))
            {
                MessageBox.Show("Please enter a group name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
