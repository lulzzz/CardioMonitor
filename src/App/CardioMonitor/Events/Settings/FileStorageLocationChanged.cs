using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enexure.MicroBus;

namespace CardioMonitor.Events.Settings
{
    public class FileStorageLocationChanged : IEvent
    {
        public FileStorageLocationChanged(string storageLocation)
        {
            StorageLocation = storageLocation;
        }

        public string StorageLocation { get; }
    }
}
