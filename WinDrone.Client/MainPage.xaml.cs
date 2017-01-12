using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinDrone.Networking;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WinDrone.Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Socket Client { get; set; }

        public MainPage()
        {
            InitClient();
            this.InitializeComponent();
        }

        private async void InitClient()
        {
            var socket = new StreamSocket();
            var server = new HostName("windrone");
            await socket.ConnectAsync(server, Listener.Port);
            Client = new Socket(socket);
            Client.OnPacket += (packet) => PacketHandler.HandlePacket(Client, packet);
        }
    }
}
