using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.CommonParams;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Threading;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    internal class DevicesFacade : 
        IDevicesFacade,
        IDisposable
    {
        [NotNull] private readonly IMonitorController _monitorController;
        [NotNull] private readonly IBedController _bedController;
        [NotNull] private readonly ICheckPointResolver _checkPointResolver;
        [NotNull] private readonly CycleProcessingSynchronizer _cycleProcessingSynchronizer;

        private readonly SessionParams _startParams;

        private readonly BroadcastBlock<CycleProcessingContext> _pipelineOnTimeStartBlock;
        private readonly ActionBlock<CycleProcessingContext> _pipelineFinishCollectorBlock;
        private readonly BroadcastBlock<CycleProcessingContext> _forcedRequestBlock;

        private bool _isStandartProcessingInProgress;
        
        public event EventHandler<TimeSpan> OnElapsedTimeChanged;
        
        public event EventHandler<float> OnCurrentAngleXRecieved;
        
        public event EventHandler<PatientPressureParams> OnPatientPressureParamsRecieved;
        
        public event EventHandler<CommonPatientParams> OnCommonPatientParamsRecieved;

        public event EventHandler<SessionProcessingException> OnException;
        
        public event EventHandler<short> OnCycleCompleted;

        public event EventHandler OnSessionCompleted;

        public event EventHandler OnStartedFromDevice;
        
        public event EventHandler OnPausedFromDevice;
        
        public event EventHandler OnResumedFromDevice;
        
        public event EventHandler OnEmeregencyStoppedFromDevice;
        
        public event EventHandler OnReversedFromDevice;
        
        public DevicesFacade(
            [NotNull] SessionParams startParams,
            [NotNull] IBedController bedController,
            [NotNull] IMonitorController monitorController,
            [NotNull] TaskHelper taskHelper,
            [NotNull] IWorkerController workerController)
        {
            if (taskHelper == null) throw new ArgumentNullException(nameof(taskHelper));
            if (workerController == null) throw new ArgumentNullException(nameof(workerController));
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            _bedController = bedController ?? throw new ArgumentNullException(nameof(bedController));

            _startParams = startParams ?? throw new ArgumentNullException(nameof(startParams));

            _pipelineOnTimeStartBlock = new BroadcastBlock<CycleProcessingContext>(context => context);
            _pipelineFinishCollectorBlock = new ActionBlock<CycleProcessingContext>(CollectDataFromPipeline);
            _forcedRequestBlock = new BroadcastBlock<CycleProcessingContext>(context => context);

            var angleReciever = new AngleReciever(bedController);
            var anlgeRecieveBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context => angleReciever.ProcessAsync(context));

            var checkPointChecker = new IterationBasedCheckPointChecker();
            var checkPointCheckBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    checkPointChecker.ProcessAsync(context));

            var mainBroadcastBlock = new BroadcastBlock<CycleProcessingContext>(context => context);

            var pressureParamsProvider = new PatientPressureParamsProvider(monitorController, taskHelper);
            var pressureParamsProviderBlock = new TransformBlock<CycleProcessingContext, CycleProcessingContext>(
                context => pressureParamsProvider.ProcessAsync(context));
            
            var commonParamsProvider = new CommonPatientParamsProvider(monitorController, taskHelper);
            var commonParamsProviderBlock = new TransformBlock<CycleProcessingContext, CycleProcessingContext>(
                context => commonParamsProvider.ProcessAsync(context));
            
            _pipelineOnTimeStartBlock.LinkTo(
                _pipelineFinishCollectorBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });
           
            _pipelineOnTimeStartBlock.LinkTo(
                anlgeRecieveBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });
            
            _forcedRequestBlock.LinkTo(
                anlgeRecieveBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });
            anlgeRecieveBlock.LinkTo(
                checkPointCheckBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });
            checkPointCheckBlock.LinkTo(
                mainBroadcastBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });

            mainBroadcastBlock.LinkTo(
                _pipelineFinishCollectorBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });
            
            mainBroadcastBlock.LinkTo(
                commonParamsProviderBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });
            
            mainBroadcastBlock.LinkTo(
                pressureParamsProviderBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });

            commonParamsProviderBlock.LinkTo(
                _pipelineFinishCollectorBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                },
                context => commonParamsProvider.CanProcess(context));
            
            pressureParamsProviderBlock.LinkTo(
                _pipelineFinishCollectorBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                },
                context => pressureParamsProvider.CanProcess(context));
            
            _cycleProcessingSynchronizer = new CycleProcessingSynchronizer(_pipelineOnTimeStartBlock, workerController);
            _isStandartProcessingInProgress = false;
        }

        private async Task CollectDataFromPipeline([NotNull] CycleProcessingContext context)
        {
            await Task.Yield();

            var exceptionParams = context.TryGetExceptionContextParams();
            if (exceptionParams != null)
            {
                OnException?.Invoke(this, exceptionParams.Exception);
            }
            
            var timeParams = context.TryGetSessionProcessingInfo();
            if (timeParams != null)
            {
                OnElapsedTimeChanged?.Invoke(this, timeParams.ElapsedTime);
            }
            var angleParams = context.TryGetAngleParam();
            if (angleParams != null)
            {
                OnCurrentAngleXRecieved?.Invoke(this, angleParams.CurrentAngle);
            }
            var pressureParams = context.TryGetPressureParams();
            if (pressureParams != null)
            {
                OnPatientPressureParamsRecieved?.Invoke(
                    this, 
                    new PatientPressureParams(
                        angleParams?.CurrentAngle ?? 0,
                        pressureParams.SystolicArterialPressure,
                        pressureParams.DiastolicArterialPressure,
                        pressureParams.AverageArterialPressure));
            }
            var commonParams = context.TryGetCommonPatientParams();
            if (commonParams != null)
            {
                OnCommonPatientParamsRecieved?.Invoke(
                    this, 
                    new CommonPatientParams(
                        angleParams?.CurrentAngle ?? 0,
                        commonParams.HeartRate,
                        commonParams.RepsirationRate,
                        commonParams.Spo2));
            }
        }

        public async Task StartAsync()
        {
            
            if (_cycleProcessingSynchronizer.IsPaused)
            {
                _cycleProcessingSynchronizer.Resume();
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.Start)
                    .ConfigureAwait(false);
            }
            else
            {
                _bedController.Init(_startParams.BedControllerInitParams);
                await _bedController
                    .ConnectAsync()
                    .ConfigureAwait(false);
                // выполнить калиборвку относительно горизонта
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.Callibrate)
                    .ConfigureAwait(false);
                // запускаем кровать
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.Start)
                    .ConfigureAwait(false);
                // измерим перед стартом
                await ForceDataCollectionRequestAsync()
                    .ConfigureAwait(false);
                // запускаем обработку
                _cycleProcessingSynchronizer.Init(_startParams.UpdateDatePeriod);
                _monitorController.Init(_startParams.MonitorControllerInitParams);
                await _monitorController
                    .ConnectAsync()
                    .ConfigureAwait(false);
                _cycleProcessingSynchronizer.Start();
                _isStandartProcessingInProgress = true;
            }
        }

        public async Task StopAsync()
        {
            _cycleProcessingSynchronizer.Stop();
            _isStandartProcessingInProgress = false;
            await _bedController
                .ExecuteCommandAsync(BedControlCommand.EmergencyStop)
                .ConfigureAwait(false);
            await _bedController
                .DisconnectAsync()
                .ConfigureAwait(false);
            await _monitorController
                .DisconnectAsync()
                .ConfigureAwait(false);
        }

        public Task PauseAsync()
        {
            _cycleProcessingSynchronizer.Pause();
            return _bedController
                .ExecuteCommandAsync(BedControlCommand.Pause);
        }


        public Task ProcessReverseRequestAsync()
        {
            return _bedController
                .ExecuteCommandAsync(BedControlCommand.Reverse);
        }

        public void Dispose()
        {
            _cycleProcessingSynchronizer.Stop();
            _cycleProcessingSynchronizer.Dispose();
            _bedController.Dispose();
            _monitorController.Dispose();
            _pipelineOnTimeStartBlock.Complete();
            _pipelineFinishCollectorBlock.Completion.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task ForceDataCollectionRequestAsync()
        {
            if (_isStandartProcessingInProgress)
            {
                throw new InvalidOperationException("Can not execute force request while cycle on progress");
            }
            
            var context = new CycleProcessingContext();
            context.AddOrUpdate(new ForcedDataCollectionRequestCycleProcessingContextParams(true));
            await _forcedRequestBlock
                .SendAsync(context)
                .ConfigureAwait(false);
        }
    }
}