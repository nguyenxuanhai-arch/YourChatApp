using System;
using System.Collections.Generic;
using System.Windows.Forms;
using YourChatApp.Client.Network;
using YourChatApp.Shared.Models;

namespace YourChatApp.Client.Forms
{
    public partial class ChatWindowForm : Form
    {
        private ClientSocket _clientSocket;
        private string _friendUsername;
        private int _friendUserId;

        public ChatWindowForm(ClientSocket clientSocket, string friendUsername, int friendUserId = 0)
        {
            _clientSocket = clientSocket;
            _friendUsername = friendUsername;
            _friendUserId = friendUserId;
            InitializeComponent();
            titleLabel.Text = $"Chat with {_friendUsername}";
            this.Text = $"Chat with {_friendUsername}";
            _clientSocket.OnPacketReceived += HandleServerMessage;
            LoadChatHistory();
        }

        private void LoadChatHistory()
        {
            if (_friendUserId <= 0)
            {
                statusLabel.Text = "Cannot load chat history - friend ID unknown";
                return;
            }

            // Send GET_MESSAGES command to server
            var messageData = new Dictionary<string, object>
            {
                { "fromUserId", _friendUserId }
            };
            CommandPacket packet = PacketProcessor.CreateCommand(CommandType.GET_MESSAGES, messageData);
            _clientSocket.SendPacket(packet);

            statusLabel.Text = "Loading chat history...";
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            string message = messageInputTextBox.Text.Trim();

            if (string.IsNullOrEmpty(message))
                return;

            // Hiển thị tin nhắn của mình
            chatDisplayTextBox.AppendText($"You: {message}\n");
            chatDisplayTextBox.ScrollToCaret();

            // Gửi tin nhắn qua socket
            var messageData = new Dictionary<string, object>
            {
                { "receiverId", _friendUserId },
                { "content", message }
            };

            CommandPacket packet = PacketProcessor.CreateCommand(CommandType.CHAT_MESSAGE, messageData);
            bool sent = _clientSocket.SendPacket(packet);

            if (sent)
            {
                statusLabel.Text = "Message sent";
                messageInputTextBox.Text = "";
            }
            else
            {
                statusLabel.Text = "Failed to send message";
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && !e.Shift)
            {
                SendButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void HandleServerMessage(CommandPacket packet)
        {
            this.Invoke(new Action(() =>
            {
                if (packet.Command == CommandType.CHAT_MESSAGE)
                {
                    if (packet.Data.ContainsKey("content"))
                    {
                        string fromUsername = packet.Data.ContainsKey("fromUsername")
                            ? packet.Data["fromUsername"].ToString()
                            : _friendUsername;
                        string content = packet.Data["content"].ToString();
                        chatDisplayTextBox.AppendText($"{fromUsername}: {content}\n");
                        chatDisplayTextBox.ScrollToCaret();
                    }
                }
                else if (packet.Command == CommandType.GET_MESSAGES)
                {
                    if (packet.Data.ContainsKey("messages"))
                    {
                        chatDisplayTextBox.Clear();
                        var messagesList = packet.Data["messages"];

                        if (messagesList is System.Collections.IEnumerable enumerable)
                        {
                            int count = 0;
                            foreach (var msgObj in enumerable)
                            {
                                count++;
                                if (msgObj is Dictionary<string, object> msgDict)
                                {
                                    int senderId = Convert.ToInt32(msgDict["senderId"]);
                                    string content = msgDict["content"].ToString();

                                    // Format: "You: message" if sender, "Friend: message" if receiver
                                    // We don't have current user ID here, so just use the friend username
                                    string senderDisplay = _friendUsername;

                                    chatDisplayTextBox.AppendText($"{senderDisplay}: {content}\n");
                                }
                            }

                            if (count == 0)
                                chatDisplayTextBox.AppendText("[No messages yet]\n");
                        }

                        chatDisplayTextBox.ScrollToCaret();
                        statusLabel.Text = "Chat ready";
                    }
                }
            }));
        }
    }
}
