using System.Threading.Tasks;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    /// <summary>
    /// Фасад для всей подсистемы взаимодействия с оборудованием
    /// </summary>
    internal interface IDevicesFacade
    {
        Task StartAsync();

        Task StopAsync();

        Task PauseAsync();

        Task ProcessReverseRequestAsync();

        Task ForceDataCollectionRequestAsync();
    }
}