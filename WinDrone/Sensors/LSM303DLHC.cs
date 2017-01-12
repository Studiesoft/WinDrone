using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace WinDrone.Sensors
{
    /// <remarks>
    /// https://cdn-shop.adafruit.com/datasheets/LSM303DLHC.PDF
    /// </remarks>
    public class LSM303DLHC
    {
        public class Accelerometer : Device
        {
            public const byte ADDRESS = 0x19;
            private PowerMode Mode { get; set; }
            private ForceScale Scale { get; set; }

            public Accelerometer(ref I2cDevice device, PowerMode mode = PowerMode.Hz400, ForceScale scale = ForceScale.G2)
                : base(ref device)
            {
                Mode = mode;
                Scale = scale;
            }

            public Data Read()
            {
                byte register = (byte)Register.OUT_X_L_A | 0x80;
                var data = ReadBytes(register, 6);

                int
                    x = (short)(data[0] | (data[1] << 8)) >> 4,
                    y = (short)(data[2] | (data[3] << 8)) >> 4,
                    z = (short)(data[4] | (data[5] << 8)) >> 4;

                return new Data()
                {
                    X = x,
                    Y = y,
                    Z = z,
                };
            }

            public override bool Begin()
            {
                try
                {
                    // Enable and set mode
                    byte val = (byte)(((byte)Mode << 4) | 7);
                    WriteByte((byte)Register.CTRL_REG1_A, val);

                    // Scale
                    val = (byte)((byte)Scale << 4);
                    WriteByte((byte)Register.CTRL_REG4_A, val);
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        public class Magnetometer : Device
        {
            public const byte ADDRESS = 0x1E;
            private Gain Gain { get; set; }

            public Magnetometer(ref I2cDevice device, Gain gain = Gain.L7)
                : base(ref device)
            {
                Gain = gain;
            }

            public Data Read()
            {
                byte register = (byte)Register.OUT_X_H_M;
                var data = ReadBytes(register, 6);

                short
                    x = (short)(data[0] | (data[1] << 8)),
                    y = (short)(data[4] | (data[5] << 8)),
                    z = (short)(data[2] | (data[3] << 8));

                return new Data()
                {
                    X = x,
                    Y = y,
                    Z = z,
                };
            }

            public override bool Begin()
            {
                try
                {
                    // Enable
                    WriteByte((byte)Register.MR_REG_M, 0x00);

                    // Set sampling rate
                    WriteByte((byte)Register.CRA_REG_M, 0x10);

                    // Gain
                    byte val = (byte)((byte)Gain << 5);
                    WriteByte((byte)Register.CRB_REG_M, val);
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        #region Structs
        public struct Data
        {
            public int X;
            public int Y;
            public int Z;
        }
        #endregion

        #region Enums
        public enum Register
        {
            CRA_REG_M = 0x00,
            CRB_REG_M = 0x01,
            MR_REG_M = 0x02,
            OUT_X_H_M = 0x03,
            OUT_X_L_M = 0x04,
            OUT_Z_H_M = 0x05,
            OUT_Z_L_M = 0x06,
            OUT_Y_H_M = 0x07,
            OUT_Y_L_M = 0x08,
            CTRL_REG1_A = 0x20,
            CTRL_REG2_A = 0x21,
            CTRL_REG3_A = 0x22,
            CTRL_REG4_A = 0x23,
            CTRL_REG5_A = 0x24,
            CTRL_REG6_A = 0x25,
            OUT_X_L_A = 0x28,
            OUT_X_H_A = 0x29,
            OUT_Y_L_A = 0x2A,
            OUT_Y_H_A = 0x2B,
            OUT_Z_L_A = 0x2C,
            OUT_Z_H_A = 0x2D,
        }

        public enum PowerMode
        {
            Down = 0x00,
            Hz1 = 0x01,
            Hz10 = 0x02,
            Hz25 = 0x03,
            Hz50 = 0x04,
            Hz100 = 0x05,
            Hz200 = 0x06,
            Hz400 = 0x07
        }

        public enum ForceScale
        {
            G2 = 0,
            G4 = 2,
            G8 = 3,
            G16 = 4
        }

        public enum Gain
        {
            L0, L1, L2, L3, L4, L5, L6, L7
        }
        #endregion
    }
}
