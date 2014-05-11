using System.Windows;

namespace StealME.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows.Controls;
    using System.Windows.Threading;

    using StealME.Server.Core;
    using StealME.Server.Networking.EventArgs;
    using StealME.Server.Networking.Tcp;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SynchronousSocketListener _socketListener;

        public MainWindow()
        {
            InitializeComponent();

            _socketListener = new SynchronousSocketListener();
            _socketListener.ClientConnected += new System.EventHandler<ClientEventArgs>(this.socketListener_ClientConnected);
            _socketListener.ClientLeft += new System.EventHandler<ClientEventArgs>(this.socketListener_ClientLeft);

            Thread listenerThread = new Thread(new ThreadStart(_socketListener.StartListening));
            listenerThread.IsBackground = true;
            listenerThread.Start();

            statConnections.Text = _connections.ToString();
        }

        private Dictionary<ConnectionHandler, ClientStatusDisplay> _tabContents =
            new Dictionary<ConnectionHandler, ClientStatusDisplay>();

        private int _connections = 0;

        private void socketListener_ClientLeft(object sender, ClientEventArgs e)
        {
            ClientTabs.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    TabItem[] tabs = new TabItem[ClientTabs.Items.Count];
                    ClientTabs.Items.CopyTo(tabs, 0);
                    foreach (TabItem tab in tabs)
                    {
                        if (tab.Content is ClientStatusDisplay && ((ClientStatusDisplay)tab.Content) == _tabContents[e.Connection]) ClientTabs.Items.Remove(tab);
                    }

                    _connections--;
                    statConnections.Text = _connections.ToString();
                }));
        }

        void socketListener_ClientConnected(object sender, ClientEventArgs e)
        {
            ClientTabs.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                  {
                      ClientStatusDisplay csd = new ClientStatusDisplay(new ClientHandler(e.Connection));
                      _tabContents[e.Connection] = csd;
                      TabItem tab = new TabItem();
                      tab.Content = csd;
                      tab.Header = e.Connection.ClientSocket.Client.RemoteEndPoint.ToString();
                      ClientTabs.Items.Add(tab);

                      _connections++;
                      statConnections.Text = _connections.ToString();
                  }));
        }
    }
}
