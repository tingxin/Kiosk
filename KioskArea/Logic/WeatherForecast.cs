using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace KioskArea.Logic
{
    class WeatherForecastEventArgs : EventArgs
    {
        public string Weather { get; private set; }

        public WeatherForecastEventArgs(string w)
        {
            Weather = w;
        }
    }

    class WeatherForecast
    {
        public event EventHandler<WeatherForecastEventArgs> WeatherChanged;

        #region fields
        public string _curWeather;
        private string[] _dw;
        private DateTime _startDate;
        private DispatcherTimer _timer = new DispatcherTimer();
        #endregion

        #region public methods
        public WeatherForecast()
        {
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += Timer_Tick;

            LoadWeather();
        }

        public string Start()
        {
            _timer.Start();
            return GetTodayWeather();
        }

        public void Stop()
        {
            _timer.Stop();
        }
        
        public BitmapImage LoadWeatherIcon(string weather)
        {
            try
            {
                var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", weather + ".png");
                return LoadImageFile(fileName);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.Write(ex.Message);
                return null;
            }
        }

        public string GetWeatherImageAddress(string weather) {
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", weather + ".png");
            return fileName;
        }
        #endregion

        #region private methods
        void Timer_Tick(object sender, EventArgs e)
        {
            string w = GetTodayWeather();
            if (w != _curWeather)
            {
                _curWeather = w;
                RaiseWeatherChanged(w);
            }
        }

        private void RaiseWeatherChanged(string weather)
        {
            if (WeatherChanged != null)
            {
                WeatherChanged(this, new WeatherForecastEventArgs(weather));
            }
        }

        private void LoadWeather()
        {
            NameValueCollection section = ConfigurationManager.GetSection("CustomerConfig/WeatherInfo") as NameValueCollection;
            _startDate = DateTime.Today;
            DateTime.TryParse(section["StartDate"], out _startDate);
            var dw = section["Weather"].Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            _dw = dw;
        }
        
        private string GetTodayWeather()
        {
            int days = (int)((DateTime.Today - _startDate).TotalDays);
            try
            {
                return _dw[days];
            }
            catch
            {
                return "";
            }
        }

        private BitmapImage LoadImageFile(string fileName)
        {
            byte[] bytes = null;
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                BinaryReader binReader = new BinaryReader(fs);
                FileInfo fileInfo = new FileInfo(fileName);
                bytes = binReader.ReadBytes((int)fileInfo.Length);
            }

            // Init bitmap
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(bytes);
            bitmap.EndInit();

            return bitmap;
        }
        #endregion
    }
}
