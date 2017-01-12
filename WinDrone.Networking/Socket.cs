using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace WinDrone.Networking
{
    public class Socket
    {
        public Action OnDisconnect;
        public Action<byte[]> OnPacket;

        private Stream InputStream { get; set; }
        private Stream OutputStream { get; set; }
        private StreamSocket StreamSocket { get; set; }

        public Socket(StreamSocket socket)
        {
            Init(socket);
            Listen();
        }

        internal Socket(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
            : this(args.Socket) { }

        private void Init(StreamSocket socket)
        {
            StreamSocket = socket;
            InputStream = socket.InputStream.AsStreamForRead();
            OutputStream = socket.OutputStream.AsStreamForWrite();

            OnDisconnect += () => StreamSocket.Dispose();
        }

        private async void Listen()
        {
            try
            {
                byte[] length = new byte[1];
                while (true)
                {
                    await InputStream.ReadAsync(length, 0, 1);
                    if (length[0] == 0)
                    {
                        Disconnect();
                        return;
                    }

                    byte[] packet = new byte[length[0]];
                    await InputStream.ReadAsync(packet, 0, length[0]);
                    OnPacket?.Invoke(packet);
                }
            }
            catch
            {
                Disconnect();
            }
        }

        public async void SendPacket(PacketWriter packet)
        {
            byte[] data = packet.ToArray();
            await OutputStream.WriteAsync(data, 0, data.Length);
            await OutputStream.FlushAsync();
        }

        public void Disconnect() =>
            OnDisconnect?.Invoke();
    }
}
