using System;
using System.Collections.Generic;

namespace RemotingInterface
{
    public interface IRemotingInterface
    {
        bool Login(string username);
        bool Logout(string username);
        string SendNewMessage(Message message);
        List<Message> GetHistoryMessages(double lastTimestamp);
        Users GetUsers();
    }

    [Serializable]
    public class Message : EventArgs
    {
        public string Username { get; set; }
        public string Text { get; set; }
        public double Timestamp { get; set; }

        public Message(string Username, string Text)
        {
            this.Username = Username;
            this.Text = Text;
            this.Timestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }

    [Serializable]
    public class Users : EventArgs
    {
        public List<string> Names = new List<string>();
    }
}
