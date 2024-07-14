using Microsoft.Maui.Controls;

namespace AsyncClientMAUI
{
    public partial class MainPage : ContentPage
    {
        private ClientNetworkModule networkModule;

        public MainPage()
        {
            InitializeComponent();
            networkModule = new ClientNetworkModule(AddLog, MessageReceived);
        }

        private void OnConnectClicked(object sender, EventArgs e)
        {
            string ipAddress = IpEntry.Text;
            int port = int.Parse(PortEntry.Text);
            networkModule.ConnectToServer(ipAddress, port);
        }

        private void OnSendClicked(object sender, EventArgs e)
        {
            string message = MessageEntry.Text;
            networkModule.SendMessage(message);
        }

        private void MessageReceived(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AddLog($"Received from server: {message}");
            });
        }

        private void AddLog(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LogEditor.Text += $"{message}{Environment.NewLine}";
            });
        }
    }
}
