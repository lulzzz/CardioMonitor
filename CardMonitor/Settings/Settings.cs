using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.Core;

namespace CardioMonitor.Settings
{
    [Serializable]
    public class Settings
    {
        private const string SettingsName = "Settings.bin";
        
        private static volatile Settings _instance;
        private static readonly object _syncObject = new object();


        public string SeletedAppThemeName { get; set; }
        public string SelectedAcentColorName { get; set; }
        
        public static Settings Instance
        {
            get
            {
                if (null != _instance)
                {
                    return _instance;
                }
                lock (_syncObject)
                {
                    if (null == _instance)
                    {
                        LoadFromFile();
                    }
                }
                return _instance;
            }
        }

        public static void LoadFromFile() 
        {
            try
            {
                using (var loadingStream = new FileStream(SettingsName, FileMode.Open))
                {
                    var bf = new BinaryFormatter();
                    _instance  = (Settings)bf.Deserialize(loadingStream);
                    loadingStream.Close();
                }
            }
            catch (Exception ex)
            {
                _instance = new Settings();
                SaveToFile();
            }
        }

        public static void SaveToFile() 
        {
            try
            {
                using (var savingStream = new FileStream(SettingsName, FileMode.Create))
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(savingStream, _instance);
                    savingStream.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
