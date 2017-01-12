using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDrone.Networking
{
    public class PacketWriter
    {
        private byte[] Buffer { get; } = new byte[0xFF];
        private int Position { get; set; } = 0;

        public unsafe void Write(byte val)
        {
            const byte size = 1;
            Check(size);

            fixed(byte* buffer = Buffer)
                *(buffer + Position) = val;
            Position += size;
        }

        public void Write(Header val) =>
            Write((byte)val);

        public void Write(byte[] val)
        {
            Check((byte)val.Length);

            Array.Copy(val, 0, Buffer, Position, val.Length);
            Position += val.Length;
        }

        public unsafe void Write(short val)
        {
            const byte size = 2;
            Check(size);

            fixed (byte* buffer = Buffer)
                *(short*)(buffer + Position) = val;
            Position += size;
        }

        public unsafe void Write(ushort val)
        {
            const byte size = 2;
            Check(size);

            fixed (byte* buffer = Buffer)
                *(ushort*)(buffer + Position) = val;
            Position += size;
        }

        public unsafe void Write(int val)
        {
            const byte size = 4;
            Check(size);

            fixed (byte* buffer = Buffer)
                *(int*)(buffer + Position) = val;
            Position += size;
        }

        public unsafe void Write(uint val)
        {
            const byte size = 4;
            Check(size);

            fixed (byte* buffer = Buffer)
                *(uint*)(buffer + Position) = val;
            Position += size;
        }

        private void Check(byte size)
        {
            if (Position + size > Buffer.Length)
                throw new OutOfMemoryException("Exceeded buffer size");
        }

        public byte[] ToArray()
        {
            var data = new byte[Position + 1];
            Array.Copy(Buffer, 0, data, 1, Position);
            data[0] = (byte)Position;
            return data;
        }
    }
}
