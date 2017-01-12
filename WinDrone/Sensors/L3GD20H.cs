using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace WinDrone.Sensors
{
    /// <remarks>
    /// https://cdn-shop.adafruit.com/datasheets/L3GD20H.pdf
    /// </remarks>
    public class L3GD20H : Device
    {
        private Range GyroRange { get; set; }

        public L3GD20H(ref I2cDevice device, Range range = Range.DPS_2000)
            : base(ref device)
        {
            GyroRange = range;
        }

        public Data Read()
        {
            byte register = (byte)Register.OUT_X_L | 0x80;
            var data = ReadBytes(register, 6);

            return new Data()
            {
                X = (short)(data[0] | (data[1] << 8)) * DPS_TO_RADS,
                Y = (short)(data[2] | (data[3] << 8)) * DPS_TO_RADS,
                Z = (short)(data[4] | (data[5] << 8)) * DPS_TO_RADS,
            };
        }

        public override bool Begin()
        {
            try
            {
                var whoAmI = ReadByte((byte)Register.WHO_AM_I);
                if (whoAmI != ID)
                    return false;

                // Disable all
                WriteByte((byte)Register.CTRL1, 0x00);
                // Set to normal and open 3 channels
                WriteByte((byte)Register.CTRL1, 0x0F);

                // Set resolution
                switch (GyroRange)
                {
                    case Range.DPS_250:
                        WriteByte((byte)Register.CTRL4, 0x00);
                        break;
                    case Range.DPS_500:
                        WriteByte((byte)Register.CTRL4, 0x10);
                        break;
                    case Range.DPS_2000:
                        WriteByte((byte)Register.CTRL4, 0x20);
                        break;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        #region Structs
        public struct Data
        {
            public double X;
            public double Y;
            public double Z;
        }
        #endregion

        #region Enums
        public enum Range
        {
            DPS_250 = 250,
            DPS_500 = 500,
            DPS_2000 = 2000
        }

        public enum Register
        {
            WHO_AM_I = 0x0F,
            CTRL1 = 0x20,
            CTRL2 = 0x21,
            CTRL3 = 0x22,
            CTRL4 = 0x23,
            CTRL5 = 0x24,
            OUT_X_L = 0x28,
            OUT_X_H = 0x29,
            OUT_Y_L = 0x2A,
            OUT_Y_H = 0x2B,
            OUT_Z_L = 0x2C,
            OUT_Z_H = 0x2D,
        }
        #endregion

        #region Constants
        public const byte ADDRESS = 0x6B;
        public const byte ID = 0xD7;
        public const double DPS_TO_RADS = .017453293;
        #endregion
    }
}
