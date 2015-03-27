using System.Windows;

namespace CardioMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Core.Settings.Settings.LoadFromFile();
        }
    }
}
