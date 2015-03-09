using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CardioMonitor.Settings;
using MahApps.Metro;

namespace CardioMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //need to fix this problem
            //seems this bug_ apper after adding MessageHelper
           // ShutdownMode = ShutdownMode.OnMainWindowClose;
            Settings.Settings.LoadFromFile();

        }
    }
}
