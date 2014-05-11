namespace StealME.Server
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    using StealME.Server.Core;
    using StealME.Server.Networking.EventArgs;

    /// <summary>
    /// Interaction logic for ClientStatusDisplay.xaml
    /// </summary>
    public partial class ClientStatusDisplay : UserControl
    {
        private ClientHandler _client;
        public ClientStatusDisplay(ClientHandler client)
        {
            InitializeComponent();
            this._client = client;
            this.Client.Authenticated += new EventHandler(this.client_Authenticated);
            this.Client.Disconnected += new EventHandler(this.client_Disconnected);
            this.Client.MessageReceived += new EventHandler<MessageEventArgs>(this.client_MessageReceived);

            DisableControls();
            txtClientAddress.Text = this.Client.GetIPAddress();
        }

        public ClientHandler Client
        {
            get
            {
                return this._client;
            }
        }

        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            lstMessageBox.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                lstMessageBox.Items.Add(e.Message);
            }));
        }

        void DisableControls()
        {
            lstMessageBox.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                new Action(
                    () =>
                    {
                        btnActivate.IsEnabled = false;
                        btnDeactivate.IsEnabled = false;
                        btnGetStatus.IsEnabled = false;
                        btnSignal.IsEnabled = false;
                    }));
        }

        void EnableControls()
        {
            lstMessageBox.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                new Action(
                    () =>
                    {
                        btnActivate.IsEnabled = true;
                        btnDeactivate.IsEnabled = true;
                        btnGetStatus.IsEnabled = true;
                        btnSignal.IsEnabled = true;
                    }));
        }

        void client_Disconnected(object sender, EventArgs e)
        {
            DisableControls();
        }

        void client_Authenticated(object sender, EventArgs e)
        {
            EnableControls();
            lstMessageBox.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                lstMessageBox.Items.Add("Client authenticated successfully.");
                this.DataContext = this.Client.Tracker;
            }));
        }

        private void btnActivate_Click(object sender, RoutedEventArgs e)
        {
            this.Client.EnqueueMessage("CMD.ACTIVATE");
        }

        private void btnDeactivate_Click(object sender, RoutedEventArgs e)
        {
            this.Client.EnqueueMessage("CMD.DEACTIVATE");
        }

        private void btnGetStatus_Click(object sender, RoutedEventArgs e)
        {
            this.Client.EnqueueMessage("GET.STATUS");
        }

        private void btnSignal_Click(object sender, RoutedEventArgs e)
        {
            this.Client.EnqueueMessage("CMD.SIGNAL");
        }
    }
}
