using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace Client
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanLogin())
            {
                MainWindow main = new MainWindow(this.Username.Text.ToString(), this.IPAddress.Text.ToString(), Int32.Parse(this.Port.Text.ToString()));
                if (main.CanLogin())
                {
                    main.Show();
                    this.Close();
                }
            }
        }

        private bool CanLogin()
        {
            if (this.Username.Text.Length == 0)
            {
                MessageBox.Show("Username cannot be empty", "Error", MessageBoxButton.OK,MessageBoxImage.Error);
                return false;
            }
            if (this.IPAddress.Text.Length == 0)
            {
                MessageBox.Show("IP address cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (this.Port.Text.Length == 0)
            {
                MessageBox.Show("Port number cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void Port_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }
    }
}
