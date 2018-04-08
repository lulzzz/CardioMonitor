

namespace CardioMonitor.Events.Settings
{
    public class FileStorageLocationChanged 
    {
        public FileStorageLocationChanged(string storageLocation)
        {
            StorageLocation = storageLocation;
        }

        public string StorageLocation { get; }
    }
}
