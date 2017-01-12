using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;


namespace WinDrone.Sensors
{
    public class GPIO
    {
        private static GpioController Controller { get; } = GpioController.GetDefault();

        public Numbers Number { get; set; }
        private GpioPinValue Value { get; set; }
        private GpioPin Pin { get; set; }

        public GPIO(Numbers number, GpioPinValue value = GpioPinValue.Low)
        {
            Number = number;
            Value = value;

            Pin = Controller.OpenPin((int)number);
            Pin.Write(Value);
            Pin.SetDriveMode(GpioPinDriveMode.Output);
        }

        public void Switch()
        {
            if (Value == GpioPinValue.Low)
                Value = GpioPinValue.High;
            else
                Value = GpioPinValue.Low;
            Pin.Write(Value);
        }

        public void Set(GpioPinValue value)
        {
            Value = value;
            Pin.Write(Value);
        }

        public enum Numbers
        {
            GPIO4 = 4,
            GPIO5 = 5,
            GPIO6 = 6,
            GPIO13 = 13,
            GPIO16 = 16,
            GPIO17 = 17,
            GPIO19 = 19,
            GPIO20 = 20,
            GPIO21 = 21,
            GPIO22 = 22,
            GPIO26 = 26,
            GPIO27 = 27,
        }
    }
}
