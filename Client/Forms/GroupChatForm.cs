using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YourChatApp.Client.Network;
using YourChatApp.Shared.Models;
using MessageBox = System.Windows.Forms.MessageBox;

namespace YourChatApp.Client.Forms
{
    public partial class GroupChatForm : Form
    {
        private ClientSocket _clientSocket;
        private User _currentUser;
        private List<User> _allUsers;
        private Group _currentGroup;

        public GroupChatForm(ClientSocket clientSocket, User currentUser)
        {
            InitializeComponent();
            _clientSocket = clientSocket;
            _currentUser = currentUser;
            _allUsers = new List<User>();
            
            _clientSocket.OnPacketReceived += HandleServerResponse;
            LoadGroups();
        }

        private void LoadGroups()
        {
            // Request list of groups from server
            var packet = new CommandPacket(CommandType.GET_GROUPS);
            _clientSocket.SendPacket(packet);
        }

        private void LoadGroupMembers(int groupId)
        {
            var data = new Dictionary<string, object>
            {
                { "groupId", groupId }
            };
            var packet = new CommandPacket(CommandType.GET_GROUPS, data);
            _clientSocket.SendPacket(packet);
        }

        private void LoadAvailableUsers()
        {
            var packet = new CommandPacket(CommandType.GET_FRIENDS);
            _clientSocket.SendPacket(packet);
        }

        private void HandleServerResponse(CommandPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<CommandPacket>(HandleServerResponse), packet);
                return;
            }

            switch (packet.Command)
            {
                case CommandType.GET_GROUPS:
                    if (packet.Data.ContainsKey("groups"))
                        UpdateGroupsList(packet.Data["groups"].ToString());
                    break;
                case CommandType.CREATE_GROUP:
                    LoadGroups();
                    MessageBox.Show("Group created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case CommandType.GET_FRIENDS:
                    if (packet.Data.ContainsKey("friends"))
                        UpdateAvailableUsers(packet.Data["friends"].ToString());
                    break;
                case CommandType.GROUP_MESSAGE:
                    DisplayGroupMessage(packet);
                    break;
            }
        }

        private void UpdateGroupsList(string data)
        {
            lstGroups.Items.Clear();
            if (string.IsNullOrEmpty(data)) return;

            var groups = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Group>>(data);
            foreach (var group in groups)
            {
                var item = new ListViewItem(group.GroupName);
                item.SubItems.Add(group.CreatedAt.ToString("dd/MM/yyyy"));
                item.Tag = group;
                lstGroups.Items.Add(item);
            }
        }

        private void UpdateMembersList(string data)
        {
            lstMembers.Items.Clear();
            if (string.IsNullOrEmpty(data)) return;

            var members = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(data);
            foreach (var member in members)
            {
                var item = new ListViewItem(member.Username);
                item.SubItems.Add(member.DisplayName);
                item.Tag = member;
                lstMembers.Items.Add(item);
            }
        }

        private void UpdateAvailableUsers(string data)
        {
            lstAvailableUsers.Items.Clear();
            if (string.IsNullOrEmpty(data)) return;

            _allUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(data);
            foreach (var user in _allUsers)
            {
                if (user.UserId != _currentUser.UserId)
                {
                    lstAvailableUsers.Items.Add(user.Username);
                }
            }
        }

        private void DisplayGroupMessage(CommandPacket packet)
        {
            if (packet.Data.ContainsKey("message"))
            {
                var messageJson = packet.Data["message"].ToString();
                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Shared.Models.Message>(messageJson);
                txtMessages.AppendText($"[{message.Timestamp:HH:mm}] {message.SenderName}: {message.Content}\r\n");
            }
        }

        private void btnCreateGroup_Click(object sender, EventArgs e)
        {
            string groupName = txtGroupName.Text.Trim();
            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("Please enter a group name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var data = new Dictionary<string, object>
            {
                { "groupName", groupName },
                { "createdBy", _currentUser.UserId }
            };

            var packet = new CommandPacket(CommandType.CREATE_GROUP, data);
            _clientSocket.SendPacket(packet);
            txtGroupName.Clear();
        }

        private void lstGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGroups.SelectedItems.Count > 0)
            {
                _currentGroup = (Group)lstGroups.SelectedItems[0].Tag;
                lblGroupTitle.Text = $"Group: {_currentGroup.GroupName}";
                LoadGroupMembers(_currentGroup.GroupId);
                LoadAvailableUsers();
                
                // Load group messages
                var data = new Dictionary<string, object>
                {
                    { "groupId", _currentGroup.GroupId }
                };
                var packet = new CommandPacket(CommandType.GROUP_MESSAGE, data);
                _clientSocket.SendPacket(packet);
            }
        }

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            if (_currentGroup == null)
            {
                MessageBox.Show("Please select a group first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lstAvailableUsers.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to add.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedUsername = lstAvailableUsers.SelectedItem.ToString();
            var userToAdd = _allUsers.FirstOrDefault(u => u.Username == selectedUsername);

            if (userToAdd != null)
            {
                var data = new Dictionary<string, object>
                {
                    { "groupId", _currentGroup.GroupId },
                    { "userId", userToAdd.UserId }
                };

                var packet = new CommandPacket(CommandType.INVITE_TO_GROUP, data);
                _clientSocket.SendPacket(packet);
            }
        }

        private void btnRemoveMember_Click(object sender, EventArgs e)
        {
            if (_currentGroup == null || lstMembers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a group and a member to remove.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var memberToRemove = (User)lstMembers.SelectedItems[0].Tag;

            var data = new Dictionary<string, object>
            {
                { "groupId", _currentGroup.GroupId },
                { "userId", memberToRemove.UserId }
            };

            var packet = new CommandPacket(CommandType.LEAVE_GROUP, data);
            _clientSocket.SendPacket(packet);
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (_currentGroup == null)
            {
                MessageBox.Show("Please select a group first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string messageText = txtMessageInput.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            var data = new Dictionary<string, object>
            {
                { "groupId", _currentGroup.GroupId },
                { "content", messageText }
            };

            var packet = new CommandPacket(CommandType.GROUP_MESSAGE, data);
            _clientSocket.SendPacket(packet);
            txtMessageInput.Clear();
        }

        private void txtMessageInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnSendMessage_Click(sender, e);
                e.Handled = true;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _clientSocket.OnPacketReceived -= HandleServerResponse;
            base.OnFormClosing(e);
        }
    }
}
