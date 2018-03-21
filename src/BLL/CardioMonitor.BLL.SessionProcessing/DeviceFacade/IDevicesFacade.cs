using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.Exceptions;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    /// <summary>
    /// Фасад для всей подсистемы взаимодействия с оборудованием
    /// </summary>
    internal interface IDevicesFacade
    {
        event EventHandler<TimeSpan> OnElapsedTimeChanged;
        event EventHandler<TimeSpan> OnRemainingTimeChanged;
        
        event EventHandler<float> OnCurrentAngleXRecieved;
        
        event EventHandler<PatientPressureParams> OnPatientPressureParamsRecieved;
        
        event EventHandler<CommonPatientParams> OnCommonPatientParamsRecieved;

        event EventHandler<SessionProcessingException> OnException;
        
        event EventHandler<Exception> OnSessionErrorStop;
        
        event EventHandler<short> OnCycleCompleted;

        event EventHandler OnSessionCompleted;
        
        event EventHandler OnPausedFromDevice;
        
        event EventHandler OnResumedFromDevice;
        
        event EventHandler OnEmeregencyStoppedFromDevice;
        
        event EventHandler OnReversedFromDevice;

        void EnableAutoPumping();

        void DisableAutoPumping();
        
        Task StartAsync();

        Task EmergencyStopAsync();

        Task PauseAsync();

        Task ProcessReverseRequestAsync();

        Task ForceDataCollectionRequestAsync();
    }
}