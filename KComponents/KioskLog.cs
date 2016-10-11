using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KComponents
{
    public class KioskLog : LogHelper.Log
    {
        static object syncObj = new object();
        static KioskLog current;
        public static LogHelper.ILog Instance()
        {
            if (current == null)
            {
                lock (syncObj)
                {
                    if (current == null)
                    {
                        try
                        {
                            string strAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            string logPath = System.IO.Path.Combine(strAppDataFolder, @"Kiosk\log");
                            current = new KioskLog(logPath);
                            current.Level = 3;

                        }
                        catch (Exception ex)
                        {
                            throw new SystemException("Init log instance Error: " + ex.Message);
                        }
                    }
                }
            }
            return current;

        }


        private KioskLog(string driectory)
            : base(driectory)
        {

        }
    }
}
