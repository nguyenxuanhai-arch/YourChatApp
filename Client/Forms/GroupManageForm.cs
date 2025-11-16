using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using YourChatApp.Client.Network;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Forms
{
    public partial class GroupManageForm : Form
    {
        private int _groupId;
        private YourChatApp.Client.Network.ClientSocket _clientSocket;
        private int _currentUserId;
        private List<YourChatApp.Shared.Models.User> _members = new List<YourChatApp.Shared.Models.User>();
        private List<YourChatApp.Shared.Models.User> _available = new List<YourChatApp.Shared.Models.User>();
        private List<YourChatApp.Shared.Models.User> _allFriends = new List<YourChatApp.Shared.Models.User>();

        public bool DeleteRequested { get; private set; } = false;

        public GroupManageForm(int groupId, string groupName, List<YourChatApp.Shared.Models.User> members, List<YourChatApp.Shared.Models.User> friends, YourChatApp.Client.Network.ClientSocket clientSocket, int currentUserId)
        {
            InitializeComponent();
            _groupId = groupId;
            this.Text = groupName;
            groupLabel.Text = groupName;
            _clientSocket = clientSocket;
            _currentUserId = currentUserId;

            // Initialize member list
            if (members != null)
            {
                _members.Clear();
                membersListBox.Items.Clear();
                var seen = new HashSet<int>();
                foreach (var m in members)
                {
                    if (m == null) continue;
                    if (seen.Contains(m.UserId)) continue;
                    seen.Add(m.UserId);
                    _members.Add(m);
                    membersListBox.Items.Add($"{m.DisplayName} (@{m.Username})");
                }
            }

            // Build available list from friends that are not already members
            if (friends != null)
            {
                // keep original friends list for recomputing availability after refresh
                _allFriends = new List<YourChatApp.Shared.Models.User>(friends);
                _available = _allFriends.Where(f => !_members.Any(m => m.UserId == f.UserId)).ToList();
                foreach (var a in _available)
                    availableComboBox.Items.Add($"{a.DisplayName} (@{a.Username})");
                if (availableComboBox.Items.Count > 0)
                    availableComboBox.SelectedIndex = 0;
            }

            _clientSocket.OnPacketReceived += HandleServerResponse;
        }

        private void HandleServerResponse(YourChatApp.Shared.Models.CommandPacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<YourChatApp.Shared.Models.CommandPacket>(HandleServerResponse), packet);
                return;
            }

            switch (packet.Command)
            {
                case YourChatApp.Shared.Models.CommandType.INVITE_TO_GROUP:
                    if (packet.StatusCode >= 200 && packet.StatusCode < 300)
                    {
                        MessageBox.Show("Member added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshMembersFromServer();
                    }
                    else
                    {
                        MessageBox.Show(packet.Message ?? "Failed to add member", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case YourChatApp.Shared.Models.CommandType.LEAVE_GROUP:
                    if (packet.StatusCode >= 200 && packet.StatusCode < 300)
                    {
                        MessageBox.Show("Member removed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshMembersFromServer();
                    }
                    else
                    {
                        MessageBox.Show(packet.Message ?? "Failed to remove member", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case YourChatApp.Shared.Models.CommandType.GET_GROUPS:
                    // When GET_GROUPS returns members for a specific group, update the lists
                    try
                    {
                        if (packet.Data != null && packet.Data.ContainsKey("members"))
                        {
                            // parse members
                            _members.Clear();
                            membersListBox.Items.Clear();

                            var membersObj = packet.Data["members"];
                            if (membersObj is System.Collections.IEnumerable menEnum)
                            {
                                var seenServer = new HashSet<int>();
                                foreach (var m in menEnum)
                                {
                                    try
                                    {
                                        int uid = 0;
                                        string uname = "";
                                        string dname = "";
                                        if (m is System.Collections.Generic.IDictionary<string, object> md)
                                        {
                                            uid = md.ContainsKey("userId") ? Convert.ToInt32(md["userId"]) : 0;
                                            uname = md.ContainsKey("username") ? md["username"]?.ToString() : "";
                                            dname = md.ContainsKey("displayName") ? md["displayName"]?.ToString() : uname;
                                        }
                                        else
                                        {
                                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(m);
                                            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<YourChatApp.Shared.Models.User>(json);
                                            if (user != null)
                                            {
                                                uid = user.UserId;
                                                uname = user.Username;
                                                dname = user.DisplayName;
                                            }
                                        }

                                        if (uid == 0 && string.IsNullOrEmpty(uname))
                                            continue;

                                        // skip duplicates from server payload
                                        if (seenServer.Contains(uid))
                                            continue;
                                        seenServer.Add(uid);

                                        var u = new YourChatApp.Shared.Models.User { UserId = uid, Username = uname, DisplayName = dname };
                                        _members.Add(u);
                                        membersListBox.Items.Add($"{dname} (@{uname})");
                                    }
                                    catch { }
                                }
                            }

                            // Recompute available friends (those in _allFriends but not in _members)
                            availableComboBox.Items.Clear();
                            _available = _allFriends.Where(f => !_members.Any(m => m.UserId == f.UserId)).ToList();
                            foreach (var a in _available)
                                availableComboBox.Items.Add($"{a.DisplayName} (@{a.Username})");
                            if (availableComboBox.Items.Count > 0)
                                availableComboBox.SelectedIndex = 0;
                        }
                    }
                    catch { }
                    break;
            }
        }

        private void RefreshMembersFromServer()
        {
            // Request updated members for this group
            var data = new Dictionary<string, object> { { "groupId", _groupId } };
            var packet = new YourChatApp.Shared.Models.CommandPacket(YourChatApp.Shared.Models.CommandType.GET_GROUPS, data);
            _clientSocket.SendPacket(packet);
        }

        private void addMemberButton_Click(object sender, EventArgs e)
        {
            if (availableComboBox.SelectedIndex < 0 || availableComboBox.SelectedIndex >= _available.Count)
            {
                MessageBox.Show("Select a user to add.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userToAdd = _available[availableComboBox.SelectedIndex];
            var data = new Dictionary<string, object>
            {
                { "groupId", _groupId },
                { "userId", userToAdd.UserId }
            };
            var packet = new YourChatApp.Shared.Models.CommandPacket(YourChatApp.Shared.Models.CommandType.INVITE_TO_GROUP, data);
            _clientSocket.SendPacket(packet);
        }

        private void removeMemberButton_Click(object sender, EventArgs e)
        {
            if (membersListBox.SelectedIndex < 0 || membersListBox.SelectedIndex >= _members.Count)
            {
                MessageBox.Show("Select a member to remove.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var member = _members[membersListBox.SelectedIndex];
            var data = new Dictionary<string, object>
            {
                { "groupId", _groupId },
                { "userId", member.UserId }
            };
            var packet = new YourChatApp.Shared.Models.CommandPacket(YourChatApp.Shared.Models.CommandType.LEAVE_GROUP, data);
            _clientSocket.SendPacket(packet);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("Delete this group? This action cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r == DialogResult.Yes)
            {
                DeleteRequested = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            _clientSocket.OnPacketReceived -= HandleServerResponse;
            this.Close();
        }
    }
}
