using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using KComponents;

namespace KioskArea
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            int startIndex = 0;
            if (e.Args.Length > 0)
            {
                int.TryParse(e.Args[0], out startIndex);
            }

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            this.MainWindow = new Shell() { StartIndex = startIndex };
            this.MainWindow.Show();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            KioskLog.Instance().Error("All", e.Exception.StackTrace);
            e.Handled = true;
        }
    }
}
