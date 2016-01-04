using RemotingInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Windows;

namespace Client
{
    public partial class MainWindow : Window
    {
        private IRemotingInterface _interface;
        public ObservableCollection<Message> _messages { get; set; }
        public ObservableCollection<string> _users { get; set; }
        private string _username;
        private string _address;
        private int _port;
        private bool _nameDup;
        private TcpChannel channel;

        public MainWindow(string username, string address, int port)
        {
            InitializeComponent();

            this.Closed += HandleCloseWindow;

            _username = username;
            _address = address;
            _port = port;

            DataContext = this;
            _messages = new ObservableCollection<Message>();
            _users = new ObservableCollection<string>();

            try
            {
                channel = new TcpChannel();
                ChannelServices.RegisterChannel(channel, true);
                _interface = (IRemotingInterface)Activator.GetObject(typeof(IRemotingInterface), "tcp://" + _address + ":" + _port + "/Server");
            } catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

        }

        public bool CanLogin()
        {
            if (_interface == null)
            {
                MessageBox.Show("IP or port not availabe, please change and try again", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ChannelServices.UnregisterChannel(channel);
                return false;
            }

            if (PingHost(_address, _port))
            {
                if (!_interface.Login(_username))
                {
                    MessageBox.Show("Username has already been used", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _nameDup = true;
                    this.Close();
                    ChannelServices.UnregisterChannel(channel);
                    return false;
                }
                else
                {
                    Work work = new Work();
                    work.username = _username;
                    work.address = _address;
                    work.port = _port;
                    work.ListenToNewMessages += HandleNewMessages;
                    work.ListenToUsers += HandleUsers;
                    Thread thread = new Thread(work.CheckForUpdate);
                    thread.Start();
                    return true;
                }
            } else
            {
                MessageBox.Show("Server is not running or listening on this port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ChannelServices.UnregisterChannel(channel);
                return false;
            }

        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Message message = new Message(_username, this.InputTextBox.Text);
            string result = _interface.SendNewMessage(message);
            MessageBox.Show(result, "Result");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _interface.Logout(_username);
            ChannelServices.UnregisterChannel(channel);
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        public static bool PingHost(string _HostURI, int _PortNumber)
        {
            try
            {
                TcpClient client = new TcpClient(_HostURI, _PortNumber);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return false;
            }
        }

        private void HandleNewMessages(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                (Action)delegate ()
                {
                    _messages.Add(e as Message);
                });
        }

        private void HandleUsers(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                (Action)delegate ()
                {
                    _users.Clear();
                    List<string> names = (e as Users).Names;
                    for (int i = 0; i < names.Count; i++)
                    {
                        _users.Add(names[i]);
                    }
                });
        }

        private void HandleCloseWindow(object sender, EventArgs e)
        {
            if (_interface != null)
            {
                bool nameDup = (sender as MainWindow)._nameDup;
                if (!nameDup)
                {
                    _interface.Logout(_username);
                }
            }
            //ChannelServices.UnregisterChannel(channel);
        }

    }

    public class Work
    {
        public EventHandler ListenToNewMessages;
        public EventHandler ListenToUsers;
        private IRemotingInterface _interface;
        public string username;
        public string address;
        public int port;
        private double lastTimestamp = 0;

        public void CheckForUpdate()
        {
            _interface = (IRemotingInterface)Activator.GetObject(typeof(IRemotingInterface), "tcp://" + address + ":" + port + "/Server");

            while (_interface != null)
            {
                if (ListenToNewMessages != null)
                {
                    List<Message> messages = _interface.GetHistoryMessages(lastTimestamp);
                    if (messages == null)
                        continue;
                    for (int i = 0; i < messages.Count; i++)
                    {
                        ListenToNewMessages(this, messages[i]);
                    }
                    lastTimestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    Thread.Sleep(1000);
                }

                if (ListenToUsers != null)
                {
                    Users users = _interface.GetUsers();
                    ListenToUsers(this, users);
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
