using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Devices.Pwm;
using Windows.UI.Xaml;
using WinDrone.Networking;
using WinDrone.Sensors;

namespace WinDrone
{
    public static class Startup
    {
        private static Listener Server { get; } = new Listener(true);
        private static List<Client> Clients { get; } = new List<Client>();
        private static L3GD20H Gyro { get; set; }
        private static BMP180 Temp { get; set; }
        private static LSM303DLHC.Accelerometer Accel { get; set; }
        private static LSM303DLHC.Magnetometer Magnet { get; set; }
        private static Dictionary<int, GPIO> Pins { get; } = new Dictionary<int, GPIO>();
        private static Dictionary<int, PwmPin> ThrottlePins { get; } = new Dictionary<int, PwmPin>();
        private static Queue<LSM303DLHC.Data> AccelBuffer { get; } = new Queue<LSM303DLHC.Data>();

        private const int AccelBufferLength = 5;
        private const double DegreesInRad = 180.0 / Math.PI;

        public static void Run()
        {
            if (LightningProvider.IsLightningEnabled)
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();

            InitPwm();
            InitPins();
            InitAccel();

            var timer = new DispatcherTimer();
            timer.Tick += (s, e) =>
            {
                //var tempData = await Temp.Read();
                //var gyroData = Gyro.Read();
                var accelData = Accel.Read();

                AccelBuffer.Enqueue(accelData);
                if (AccelBuffer.Count > AccelBufferLength)
                    AccelBuffer.Dequeue();

                CheckLevel();
            };
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();
        }

        private static bool CheckLevel()
        {
            if (AccelBuffer.Count < AccelBufferLength)
                return false;

            int
                x = AccelBuffer.Sum(d => d.X) / AccelBufferLength,
                y = AccelBuffer.Sum(d => d.Y) / AccelBufferLength,
                z = AccelBuffer.Sum(d => d.Z) / AccelBufferLength;

            double
                accXnorm = x / Math.Sqrt(x * x + y * y + z * z),
                accYnorm = y / Math.Sqrt(x * x + y * y + z * z),
                pitch = Math.Asin(accXnorm),
                roll = -Math.Asin(accYnorm / Math.Cos(accXnorm));

            pitch *= DegreesInRad;
            roll *= DegreesInRad;

            bool level = (roll > -2 && roll < 2) && (pitch > -2 && pitch < 2);
            Pins[1].Set(level ? GpioPinValue.Low : GpioPinValue.High);
            Pins[2].Set(!level ? GpioPinValue.Low : GpioPinValue.High);
            return level;
        }

        private static async void InitTemp()
        {
            var device = await GetDevice(BMP180.ADDRESS);
            Temp = new BMP180(ref device);
            if (!Temp.Begin())
                Debug.WriteLine("Something went wrong while initializing the temp sensor");
        }

        private static async void InitGyro()
        {
            var device = await GetDevice(L3GD20H.ADDRESS);
            Gyro = new L3GD20H(ref device);
            if (!Gyro.Begin())
                Debug.WriteLine("Something went wrong while initializing the Gyro");
        }

        private static async void InitAccel()
        {
            var device = await GetDevice(LSM303DLHC.Accelerometer.ADDRESS);
            Accel = new LSM303DLHC.Accelerometer(ref device);
            if (!Accel.Begin())
                Debug.WriteLine("Something went wrong while initializing the Accel");
        }

        private static async void InitMagnet()
        {
            var device = await GetDevice(LSM303DLHC.Magnetometer.ADDRESS);
            Magnet = new LSM303DLHC.Magnetometer(ref device);
            if (!Magnet.Begin())
                Debug.WriteLine("Something went wrong while initializing the Magnet");
        }

        private static void InitPins()
        {
            Pins.Add(1, new GPIO(GPIO.Numbers.GPIO17));
            Pins.Add(2, new GPIO(GPIO.Numbers.GPIO27));
            Pins.Add(3, new GPIO(GPIO.Numbers.GPIO22));
        }

        private static async void InitPwm()
        {
            var controllers = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());
            PwmController controller = controllers[1];
            controller.SetDesiredFrequency(50);
            
            PwmPin pin = controller.OpenPin((int)GPIO.Numbers.GPIO4);
            ThrottlePins.Add(1, pin);
            Throttle(1, 0);
            pin.Start();
       
        }

        /// <summary>
        /// Sets the throttle of an engine
        /// </summary>
        public static void Throttle(int engineId, int amount)
        {
            const double min = 0.061; // 1.2ms and a bit
            const double max = 0.070; // 1.4ms
            const double off = 0.050; // 1ms
            const double gap = max - min;

            if (!ThrottlePins.ContainsKey(engineId))
                return;
            PwmPin pin = ThrottlePins[engineId];

            if (amount <= 0 || amount > 100)
            {
                pin.SetActiveDutyCyclePercentage(off);
                return;
            }

            double throttle = gap * (amount / 100) + min;
            if (throttle > max)
                throttle = max;
            pin.SetActiveDutyCyclePercentage(throttle);
        }

        private static async Task<I2cDevice> GetDevice(int address)
        {
            var controller = await I2cController.GetDefaultAsync();
            var settings = new I2cConnectionSettings(address)
            {
                BusSpeed = I2cBusSpeed.FastMode,
                SharingMode = I2cSharingMode.Shared
            };
            return controller.GetDevice(settings);
        }

        private static void InitServer()
        {
            Server.OnSocket += (socket) => Clients.Add(new Client(socket));
            Server.OnInitFinished += () =>
            {
                Pins[3].Set(GpioPinValue.High);
                Debug.WriteLine("Server Listening");
            };
            Server.Init();
        }
    }
}
