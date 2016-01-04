using RemotingInterface;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Server
{
    class Server : MarshalByRefObject, IRemotingInterface
    {
        private List<Message> _historyMessages = new List<Message>();
        private Users _users = new Users();

        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(12346);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(Server), "Server", WellKnownObjectMode.Singleton);

            Console.WriteLine("Server start running");
            Console.ReadLine();
        }

        // Pour laisser le serveur fonctionner sans time out
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public string SendNewMessage(Message message)
        {
            _historyMessages.Add(message);
            return "message sent with success";

        }

        public List<Message> GetHistoryMessages(double lastTimestamp)
        {

            return _historyMessages.FindAll(message => message.Timestamp > lastTimestamp);
        }

        public Users GetUsers()
        {
            return _users;
        }

        public bool Login(string username)
        {
            if (_users.Names.Contains(username))
            {
                return false;
            }
            else
            {
                _users.Names.Add(username);
                return true;
            }
        }

        public bool Logout(string username)
        {
            _users.Names.Remove(username);
            return true;
        }
    }
}
