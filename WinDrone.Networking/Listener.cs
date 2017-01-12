using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace WinDrone.Networking
{
    public class Listener
    {
        private StreamSocketListener Server { get; } = new StreamSocketListener();
        public const string Port = "1337";

        public Action<Socket> OnSocket;
        public Action OnInitFinished;

        public Listener(bool suppressInit = false)
        {
            if (!suppressInit)
                Init();
        }

        public async void Init()
        {
            Server.ConnectionReceived += Server_ConnectionReceived;
            await Server.BindServiceNameAsync(Port);
            OnInitFinished?.Invoke();
        }

        private void Server_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args) =>
            OnSocket?.Invoke(new Socket(sender, args));
    }
}
