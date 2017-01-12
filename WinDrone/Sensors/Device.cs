using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace WinDrone.Sensors
{
    public abstract class Device
    {
        private I2cDevice I2c { get; set; }

        public Device(ref I2cDevice device)
        {
            I2c = device;
        }

        protected byte[] ReadBytes(byte register, int length)
        {
            var data = new byte[length];
            I2c.WriteRead(new byte[] { register }, data);
            return data;
        }

        protected byte ReadByte(byte register) => 
            ReadBytes(register, 1)[0];

        protected void WriteByte(byte register, byte data) =>
            I2c.Write(new byte[] { register, data });

        protected short ToShort(byte[] data, int offset = 0) => 
            (short)((data[0 + offset] << 8) | data[1 + offset]);

        protected ushort ToUShort(byte[] data) =>
            (ushort)((data[0] << 8) | data[1]);

        public abstract bool Begin();
    }
}
