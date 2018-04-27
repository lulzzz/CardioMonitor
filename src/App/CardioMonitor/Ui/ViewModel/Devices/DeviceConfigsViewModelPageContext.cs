using System;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Devices
{
    public class DeviceConfigsViewModelPageContext : IStoryboardPageContext
    {
        public bool IsAdded { get; set; }

        public Guid DeviceId { get; set; }

        public Guid DeviceTypeId { get; set; }

        public string ConfigName { get; set; }
    }
}