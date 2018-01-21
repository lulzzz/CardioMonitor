using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Angle;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.CommonParams;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Exceptions;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.Iterations;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.PressureParams;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade.SessionProcessingInfo;
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
        [NotNull] private readonly CycleProcessingSynchronizer _cycleProcessingSynchronizer;

        private readonly SessionParams _startParams;

        private readonly BroadcastBlock<CycleProcessingContext> _pipelineOnTimeStartBlock;
        private readonly ActionBlock<CycleProcessingContext> _pipelineFinishCollectorBlock;
        private readonly BroadcastBlock<CycleProcessingContext> _forcedRequestBlock;

        private bool _isStandartProcessingInProgress;
        private short _previouslyKnownCycleNumber;
        private bool _isAutoPumpingEnabled;

        private readonly MemoryCache _processedEventsCached;
        
        public event EventHandler<TimeSpan> OnElapsedTimeChanged;
        public event EventHandler<TimeSpan> OnRemainingTimeChanged;
        
        public event EventHandler<float> OnCurrentAngleXRecieved;
        
        public event EventHandler<PatientPressureParams> OnPatientPressureParamsRecieved;
        
        public event EventHandler<CommonPatientParams> OnCommonPatientParamsRecieved;

        public event EventHandler<SessionProcessingException> OnException;
        
        public event EventHandler<short> OnCycleCompleted;

        public event EventHandler OnSessionCompleted;
        
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
            _isAutoPumpingEnabled = true;

            _pipelineOnTimeStartBlock = new BroadcastBlock<CycleProcessingContext>(context => context);
            _pipelineFinishCollectorBlock = new ActionBlock<CycleProcessingContext>(CollectDataFromPipeline);
            _forcedRequestBlock = new BroadcastBlock<CycleProcessingContext>(context => context);

            CreatePipeline(bedController, monitorController, taskHelper);
            
            _cycleProcessingSynchronizer = new CycleProcessingSynchronizer(
                _pipelineOnTimeStartBlock, 
                workerController,
                _isAutoPumpingEnabled);
            _isStandartProcessingInProgress = false;
            _previouslyKnownCycleNumber = 1;
            
            _bedController.OnPauseFromDeviceRequested += BedControllerOnPauseFromDeviceRequested;
            _bedController.OnResumeFromDeviceRequested += BedControllerOnResumeFromDeviceRequested;
            _bedController.OnReverseFromDeviceRequested += BedControllerOnReverseFromDeviceRequested;
            _bedController.OnEmeregencyStopFromDeviceRequested += BedControllerOnEmeregencyStopFromDeviceRequested;
            
            _processedEventsCached = new MemoryCache(GetType().Name);
        }
       
        private void CreatePipeline(
            [NotNull] IBedController bedController,
            [NotNull] IMonitorController monitorController,
            [NotNull] TaskHelper taskHelper)
        {
            var sessionInfoProvider = new SessionProcessingInfoProvider(_bedController);
            var sessionInfoProvidingBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    sessionInfoProvider.ProcessAsync(context));

            var iterationProvider = new IterationParamsProvider(_bedController);
            var iterationProvidingBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    iterationProvider.ProcessAsync(context));

            var angleReciever = new AngleReciever(bedController);
            var anlgeRecieveBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    angleReciever.ProcessAsync(context));

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
                sessionInfoProvidingBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });

            _forcedRequestBlock.LinkTo(
                sessionInfoProvidingBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });

            sessionInfoProvidingBlock.LinkTo(
                iterationProvidingBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });

            iterationProvidingBlock.LinkTo(
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
        }

        
        private Task CollectDataFromPipeline([NotNull] CycleProcessingContext context)
        {
            var exceptionParams = context.TryGetExceptionContextParams();
            if (exceptionParams != null)
            {
                RiseOnce(exceptionParams, () => OnException?.Invoke(this, exceptionParams.Exception));
            }
            
            var sessionProcessingInfo = context.TryGetSessionProcessingInfo();
            if (sessionProcessingInfo == null)
            {
                return Task.CompletedTask;
            }
            var iterationsInfo = context.TryGetIterationParams();
            if (iterationsInfo == null)
            {
                return Task.CompletedTask;
            }
            
            var angleParams = context.TryGetAngleParam();
            if (angleParams != null)
            {
                RiseOnce(angleParams, () => OnCurrentAngleXRecieved?.Invoke(this, angleParams.CurrentAngle));
            }
            
            var pressureParams = context.TryGetPressureParams();
            if (pressureParams != null)
            {
                RiseOnce(pressureParams, () => OnPatientPressureParamsRecieved?.Invoke(
                    this, 
                    new PatientPressureParams(
                        angleParams?.CurrentAngle ?? 0,
                        pressureParams.SystolicArterialPressure,
                        pressureParams.DiastolicArterialPressure,
                        pressureParams.AverageArterialPressure)));
            }
            var commonParams = context.TryGetCommonPatientParams();
            if (commonParams != null)
            {
                RiseOnce(commonParams, () => OnCommonPatientParamsRecieved?.Invoke(
                    this, 
                    new CommonPatientParams(
                        angleParams?.CurrentAngle ?? 0,
                        commonParams.HeartRate,
                        commonParams.RepsirationRate,
                        commonParams.Spo2)));
            }
            
            RiseOnce(sessionProcessingInfo, async () =>
            {
                OnElapsedTimeChanged?.Invoke(this, sessionProcessingInfo.ElapsedTime);
                OnRemainingTimeChanged?.Invoke(this, sessionProcessingInfo.RemainingTime);
                
                // считаем, что сессия завершилась, когда оставшееся время равно 0
                var isSessionCompleted = sessionProcessingInfo.RemainingTime <= TimeSpan.Zero;
         
                //todo придумать, как определять, что закончился цикл, что надо снять показатели в последнем 0
                if (_previouslyKnownCycleNumber != sessionProcessingInfo.CurrentCycleNumber || isSessionCompleted)
                {
                    _previouslyKnownCycleNumber = sessionProcessingInfo.CurrentCycleNumber;// по идеи, 
                    await ForceDataCollectionRequestAsync();
                    OnCycleCompleted?.Invoke(this, _previouslyKnownCycleNumber);
                }
                if (isSessionCompleted)
                {
                    _cycleProcessingSynchronizer.Stop();
                    OnSessionCompleted?.Invoke(this, EventArgs.Empty);
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Вызывает действия для указанного параметр только один раз
        /// </summary>
        /// <param name="contextParams"></param>
        /// <param name="action"></param>
        /// <remarks>
        /// Крайне ползено, т.к. в Pipeline есть пара параллельных участков, параметры будут дублирвоаться
        /// </remarks>
        private void RiseOnce(ICycleProcessingContextParams contextParams, Action action)
        {
            if (_processedEventsCached.ContainsEvent(contextParams)) return;
            
            _processedEventsCached.AddEventToCache(contextParams, TimeSpan.FromMinutes(1));
            
            action.Invoke();
            
        }
        
        private void BedControllerOnEmeregencyStopFromDeviceRequested(object sender, EventArgs eventArgs)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await InnerEmeregencyStopAsync(true)
                        .ConfigureAwait(false);
                    OnEmeregencyStoppedFromDevice?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    OnException?.Invoke(
                        this, 
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.UnhandlerException, 
                            "Ошибка обработки команды экстренной остановки кровати, запрошенной с пульта", 
                            e));
                }
            });
        }

        private void BedControllerOnReverseFromDeviceRequested(object sender, EventArgs eventArgs)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await InnerProcessReverseRequestAsync(true)
                        .ConfigureAwait(false);
                    OnReversedFromDevice?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    OnException?.Invoke(
                        this,
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.UnhandlerException,
                            "Ошибка обработки команды реверса, запрошенной с пульта",
                            e));
                }
            });
        }

        private void BedControllerOnResumeFromDeviceRequested(object sender, EventArgs eventArgs)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await InnerStartAsync(true)
                        .ConfigureAwait(false);
                    OnResumedFromDevice?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    OnException?.Invoke(
                        this,
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.UnhandlerException,
                            "Ошибка обработки команды продолжения сеанса, запрошенной с пульта",
                            e));
                }
            });
        }

        private void BedControllerOnPauseFromDeviceRequested(object sender, EventArgs eventArgs)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await InnerPauseAsync(true)
                        .ConfigureAwait(false);
                    OnPausedFromDevice?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    OnException?.Invoke(
                        this,
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.UnhandlerException,
                            "Ошибка обработки команды паузы сеанса, запрошенной с пульта",
                            e));
                }
            });
        }

        public void EnableAutoPumping()
        {
            _isAutoPumpingEnabled = true;
            _cycleProcessingSynchronizer.EnableAutoPumping();
        }

        public void DisableAutoPumping()
        {
            _isAutoPumpingEnabled = false;
            _cycleProcessingSynchronizer.DisableAutoPumping();
        }

        public Task StartAsync()
        {
            return InnerStartAsync(false);
        }

        private async Task InnerStartAsync(bool isCalledFromDevice)
        {
            if (_cycleProcessingSynchronizer.IsPaused)
            {
                _cycleProcessingSynchronizer.Resume();
                if (!isCalledFromDevice)
                {
                    await _bedController
                        .ExecuteCommandAsync(BedControlCommand.Start)
                        .ConfigureAwait(false);
                }
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
                // измерим перед стартом
                await ForceDataCollectionRequestAsync()
                    .ConfigureAwait(false);
                // запускаем кровать
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.Start)
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

        public Task EmergencyStopAsync()
        {
            return InnerEmeregencyStopAsync(false);
        }

        private async Task InnerEmeregencyStopAsync(bool isCalledFromDevice)
        {
            _cycleProcessingSynchronizer.Stop();
            _isStandartProcessingInProgress = false;
            if (!isCalledFromDevice)
            {
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.EmergencyStop)
                    .ConfigureAwait(false);
            }
            await _bedController
                .DisconnectAsync()
                .ConfigureAwait(false);
            await _monitorController
                .DisconnectAsync()
                .ConfigureAwait(false);
        }

        public Task PauseAsync()
        {
            return InnerPauseAsync(false);
        }

        private async Task InnerPauseAsync(bool isCalledFromDevice)
        {
            _cycleProcessingSynchronizer.Pause();
            if (!isCalledFromDevice)
            {
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.Pause)
                    .ConfigureAwait(false);
            }
        }


        public Task ProcessReverseRequestAsync()
        {
            return InnerProcessReverseRequestAsync(false);
        }

        private Task InnerProcessReverseRequestAsync(bool isCalledFromDevice)
        {
            if (!isCalledFromDevice)
            {
                return _bedController
                    .ExecuteCommandAsync(BedControlCommand.Reverse);
            }
            return Task.CompletedTask;
        }
        
        public async Task ForceDataCollectionRequestAsync()
        {
            if (_isStandartProcessingInProgress)
            {
                throw new InvalidOperationException("Can not execute force request while cycle on progress");
            }
     
            //todo может, вручную задавать номер итерации и цикла?
            var context = new CycleProcessingContext();
            context.AddOrUpdate(new ForcedDataCollectionRequestCycleProcessingContextParams(true));
            context.AddOrUpdate(new AutoPumpingContextParams(_isAutoPumpingEnabled));
            await _forcedRequestBlock
                .SendAsync(context)
                .ConfigureAwait(false);
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
    }
}