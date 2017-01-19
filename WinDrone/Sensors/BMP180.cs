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
    /// https://cdn-shop.adafruit.com/datasheets/BST-BMP180-DS000-09.pdf
    /// </remarks>
    public class BMP180 : Device
    {
        private CalibrationData Calibration { get; set; }
        private Mode Oversampling { get; set; }

        public BMP180(ref I2cDevice device, Mode mode = Mode.ULTRA_HIGH_RESOLUTION)
            : base(ref device)
        {
            Oversampling = mode;
        }

        public override bool Begin()
        {
            try
            {
                var id = ReadByte((byte)Register.CHIP_ID);
                if (id != ID)
                    return false;

                // Read calibration data
                Calibration = new CalibrationData()
                {
                    AC1 = ReadShort((byte)Register.AC1),
                    AC2 = ReadShort((byte)Register.AC2),
                    AC3 = ReadShort((byte)Register.AC3),
                    AC4 = ReadUShort((byte)Register.AC4),
                    AC5 = ReadUShort((byte)Register.AC5),
                    AC6 = ReadUShort((byte)Register.AC6),
                    B1 = ReadShort((byte)Register.B1),
                    B2 = ReadShort((byte)Register.B2),
                    MB = ReadShort((byte)Register.MB),
                    MC = ReadShort((byte)Register.MC),
                    MD = ReadShort((byte)Register.MD),
                };
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<Data> Read()
        {
            byte[] tData = await ReadControl(Register.TEMP_CTRL, 2);
            byte[] pData = await ReadControl(Register.PRES_CTRL + ((byte)Oversampling << 6), 3);
            return Calc(tData, pData);
        }

        private Data Calc(byte[] tData, byte[] pData)
        {
            double
                x1, x2, x3,
                b3, b4, b5, b6, b7;

            x1 = (ToShort(tData) - Calibration.AC6) * Calibration.AC5 / Math.Pow(2, 15);
            x2 = Calibration.MC * Math.Pow(2, 11) / (x1 + Calibration.MD);
            b5 = x1 + x2;

            long up = (pData[0] << 16) | (pData[1] << 8) | pData[0];
            up >>= 8 - (byte)Oversampling;
            b6 = b5 - 4000;
            x1 = (Calibration.B2 * (b6 * b6 / Math.Pow(2, 12))) / Math.Pow(2, 11);
            x2 = Calibration.AC2 * b6 / Math.Pow(2, 11);
            x3 = x1 + x2;
            b3 = (((long)(Calibration.AC1 * 4 + x3) << (byte)Oversampling) + 2) / 4;
            x1 = Calibration.AC3 * b6 / Math.Pow(2, 13);
            x2 = (Calibration.B1 * (b6 * b6 / Math.Pow(2, 12))) / Math.Pow(2, 16);
            x3 = ((x1 + x2) + 2) / 4;
            b4 = Calibration.AC4 * (ulong)(x3 + 0x8000) / Math.Pow(2, 15);
            b7 = ((ulong)(up - b3) * (50000ul >> (byte)Oversampling));

            long p;
            if (b7 < 0x80000000)
                p = (long)((b7 * 2) / b4);
            else
                p = (long)((b7 / b4) * 2);
            x1 = Math.Pow((p / Math.Pow(2, 8)), 2);
            x1 = (x1 * 3038) / Math.Pow(2, 16);
            x2 = (-7357 * p) / Math.Pow(2, 16);
            p = (long)(p + (x1 + x2 + 3791) / Math.Pow(2, 4));

            return new Data
            {
                Temperature = (int)((b5 + 8) / Math.Pow(2, 4)),
                Pressure = p
            };
        }

        private async Task<byte[]> ReadControl(Register register, int length)
        {
            // Request read
            WriteByte((byte)Register.CONTROL, (byte)register);
            var initial = (int)register & 0xF0;
            // Wait for Sco
            while ((ReadByte((byte)Register.CONTROL) & 0xF0) == initial)
                await Task.Delay(5);
            // Read data
            return ReadBytes((byte)Register.MSB, length);
        }

        #region
        struct CalibrationData
        {
            public short AC1;
            public short AC2;
            public short AC3;
            public ushort AC4;
            public ushort AC5;
            public ushort AC6;
            public short B1;
            public short B2;
            public short MB;
            public short MC;
            public short MD;
        }

        public struct Data
        {
            public int Temperature;
            public long Pressure;
        }
        #endregion

        #region Enums 
        public enum Register
        {
            TEMP_CTRL = 0x2E,
            PRES_CTRL = 0x34,
            AC1 = 0xAA,
            AC2 = 0xAC,
            AC3 = 0xAE,
            AC4 = 0xB0,
            AC5 = 0xB2,
            AC6 = 0xB4,
            B1 = 0xB6,
            B2 = 0xB8,
            MB = 0xBA,
            MC = 0xBC,
            MD = 0xBE,
            CHIP_ID = 0xD0,
            CONTROL = 0xF4,
            MSB = 0xF6,
            LSB = 0xF7,
            XLSB = 0xF8
        }

        public enum Mode
        {
            ULTRA_LOW_POWER = 0,
            STANDARD = 1,
            HIGH_RESOLUTION = 2,
            ULTRA_HIGH_RESOLUTION = 3
        }
        #endregion

        #region Constants
        public const byte ADDRESS = 0x77;
        public const byte ID = 0x55;
        #endregion
    }
}
