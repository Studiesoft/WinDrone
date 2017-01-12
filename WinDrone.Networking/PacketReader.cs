using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDrone.Networking
{
    public class PacketReader
    {
        private byte[] Buffer { get; set; }
        private int Position { get; set; } = 0;

        public int Length
        {
            get { return Buffer.Length; }
        }

        public int Available
        {
            get { return Length - Position; }
        }

        public PacketReader(byte[] packet)
        {
            Buffer = packet;
        }

        public byte ReadByte() =>
            Buffer[StartRead(1)];

        public Header ReadHeader() =>
            (Header)ReadByte();

        public byte[] ReadBytes(int length)
        {
            byte[] toRead = new byte[length];
            System.Buffer.BlockCopy(Buffer, StartRead(length), toRead, 0, length);
            return toRead;
        }

        public short ReadShort() =>
            BitConverter.ToInt16(Buffer, StartRead(2));

        public ushort ReadUShort() =>
            BitConverter.ToUInt16(Buffer, StartRead(2));

        public int ReadInt() =>
            BitConverter.ToInt32(Buffer, StartRead(4));

        public uint ReadUInt() =>
            BitConverter.ToUInt32(Buffer, StartRead(4));

        private int StartRead(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length", "Length cannot be zero or negative");

            int sPosition = Position;
            Position += length;
            if (Available < 0)
            {
                Position = sPosition; //restore old
                throw new OutOfMemoryException("Not enough data in buffer");
            }

            return sPosition;
        }
    }
}
