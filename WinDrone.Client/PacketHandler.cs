using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDrone.Networking;

namespace WinDrone.Client
{
    public static class PacketHandler
    {
        public static void HandlePacket(Socket client, byte[] packet)
        {
            var pr = new PacketReader(packet);
            Header header = pr.ReadHeader();
            switch (header)
            {
                case Header.ClientConnected:
                    break;
                default:
                    Debug.WriteLine($"Unhandled packet: {header}");
                    break;
            }
        }
    }
}
