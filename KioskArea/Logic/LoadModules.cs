using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IKiosk;
using KComponents;

namespace KioskArea.Logic
{
    public class LoadModules
    {
        const string modulesPath = "{0}\\Modules";
        public static void Load(List<ApplicatoinService> appServiceContainer, List<WebAppPlugin> plugins)
        {
            if (appServiceContainer != null && plugins != null)
            {
                string rootPath = System.Environment.CurrentDirectory;
                string moduleAddress = string.Format(modulesPath, rootPath);
                System.Console.WriteLine(moduleAddress);

                if (Directory.Exists(moduleAddress) == false)
                {
                    Directory.CreateDirectory(moduleAddress);
                }

                string[] files = Directory.GetFiles(moduleAddress);
                foreach (string file in files)
                {
                    System.Console.WriteLine(file);
                    if (file.EndsWith(".dll"))
                    {
                        try
                        {
                            Assembly lib = Assembly.LoadFile(file);

                            System.Console.WriteLine(lib.FullName);
                            foreach (Type ty in lib.GetExportedTypes())
                            {
                                if (ty.IsSubclassOf(typeof(ApplicatoinService)))
                                {
                                    try
                                    {
                                        ApplicatoinService app = (ApplicatoinService)lib.CreateInstance(ty.FullName);
                                        UIElement ui = app.GetViewer() as UIElement;
                                        if (ui != null)
                                        {
                                            appServiceContainer.Add(app);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Console.WriteLine(ex.StackTrace);
                                        KioskLog.Instance().Error("LoadModules", ex.Message + ex.StackTrace);
                                    }

                                    break;
                                }
                                else if (ty.IsSubclassOf(typeof(WebAppPlugin)))
                                {
                                    WebAppPlugin plugin = (WebAppPlugin)lib.CreateInstance(ty.FullName);
                                    plugins.Add(plugin);
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine(ex.Message);
                            KioskLog.Instance().Error("LoadModules", ex.Message + ex.StackTrace);
                        }
                    }

                }
            }

        }
    }
}
