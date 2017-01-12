using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDrone.Networking;

namespace WinDrone
{
    public class Client
    {
        private Socket Socket { get; set; }

        public Client(Socket socket)
        {
            Socket = socket;
            Socket.OnPacket += HandlePacket;

            Debug.WriteLine("Client Connected");

            var pw = new PacketWriter();
            pw.Write(Header.ClientConnected);
            Socket.SendPacket(pw);
        }

        private void HandlePacket(byte[] packet)
        {
            var pr = new PacketReader(packet);
            Header header = pr.ReadHeader();
            switch (header)
            {
                default:
                    Debug.WriteLine($"Unhandled packet: {header}");
                    break;
            }
        }
    }
}
