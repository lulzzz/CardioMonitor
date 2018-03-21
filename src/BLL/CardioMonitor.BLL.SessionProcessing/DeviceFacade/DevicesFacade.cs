﻿using System;
using System.Runtime.Caching;
using System.Threading;
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
using CardioMonitor.Devices;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    /// <summary>
    /// Фасад взаимодействия с внешними устройствами
    /// </summary>
    /// <remarks>
    /// Управляет и запрашивает данные с устроств согласно бизнес-логике
    /// </remarks>
    internal class DevicesFacade : 
        IDevicesFacade,
        IDisposable
    {
        private const short StartCycleNumber = 1;
        
        #region Private fields
 
        private readonly TimeSpan _cachedEventLifetime = TimeSpan.FromSeconds(30);

        private bool _isFailedOnPumping;
        
        [NotNull] private readonly IMonitorController _monitorController;
        [NotNull] private readonly IBedController _bedController;
        [NotNull] private readonly CycleProcessingSynchronizer _cycleProcessingSynchronizer;

        [NotNull]
        private readonly SessionParams _startParams;

        [NotNull]
        private readonly BroadcastBlock<CycleProcessingContext> _pipelineOnTimeStartBlock;
        [NotNull]
        private readonly ActionBlock<CycleProcessingContext> _pipelineFinishCollectorBlock;
        [NotNull]
        private readonly BroadcastBlock<CycleProcessingContext> _forcedRequestBlock;

        private bool _isStandartProcessingInProgress;
        private short _previouslyKnownCycleNumber;
        private bool _isAutoPumpingEnabled;

        private readonly MemoryCache _processedEventsCached;

        #endregion

        #region Events
        
        /// <summary>
        /// Событие обновления времени, прошедшего с начала сеанса
        /// </summary>
        public event EventHandler<TimeSpan> OnElapsedTimeChanged;
        
        /// <summary>
        /// Событие обновления времени, оставшегося до конца сеанса
        /// </summary>
        public event EventHandler<TimeSpan> OnRemainingTimeChanged;
        
        /// <summary>
        /// Событие изменения текущего угла наклона кровати по оси Х
        /// </summary>
        public event EventHandler<float> OnCurrentAngleXRecieved;
        
        /// <summary>
        /// Событие получения новых данных давления пациента
        /// </summary>
        public event EventHandler<PatientPressureParams> OnPatientPressureParamsRecieved;
        
        /// <summary>
        /// Событие получения новых общих данных пациента
        /// </summary>
        public event EventHandler<CommonPatientParams> OnCommonPatientParamsRecieved;

        /// <summary>
        /// Событие возникновения исключения в ходе выполнения сеанса 
        /// </summary>
        public event EventHandler<SessionProcessingException> OnException;
        
        /// <summary>
        /// Событие возникновения критической ошибки, не позволяющей продолжить сенас
        /// </summary>
        public event EventHandler<Exception> OnSessionErrorStop;
        
        /// <summary>
        /// Событие заврешения цикла
        /// </summary>
        public event EventHandler<short> OnCycleCompleted;

        /// <summary>
        /// Событие завершения сеанса
        /// </summary>
        public event EventHandler OnSessionCompleted;
        
        /// <summary>
        /// Событие постановки сеанса на паузу, выполненное с кровати
        /// </summary>
        public event EventHandler OnPausedFromDevice;
        
        /// <summary>
        /// Событие продолжения сеанса после паузы, выполненное с кровати
        /// </summary>
        public event EventHandler OnResumedFromDevice;
        
        /// <summary>
        /// Событие экстренной, выполненное с кровати
        /// </summary>
        public event EventHandler OnEmeregencyStoppedFromDevice;
        
        /// <summary>
        /// Событие реверса, выполненное с кровати
        /// </summary>
        public event EventHandler OnReversedFromDevice;
        
        #endregion
        
        private readonly object _inversionTableRecconectionSyncObject = new object();
        private readonly object _monitorRecconectionSyncObject = new object();
        
        public DevicesFacade(
            [NotNull] SessionParams startParams,
            [NotNull] IBedController bedController,
            [NotNull] IMonitorController monitorController,
            [NotNull] IWorkerController workerController)
        {
            if (workerController == null) throw new ArgumentNullException(nameof(workerController));
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            _bedController = bedController ?? throw new ArgumentNullException(nameof(bedController));

            _startParams = startParams ?? throw new ArgumentNullException(nameof(startParams));
            _isAutoPumpingEnabled = true;

            _pipelineOnTimeStartBlock = new BroadcastBlock<CycleProcessingContext>(context => context);
            _pipelineFinishCollectorBlock = new ActionBlock<CycleProcessingContext>(CollectDataFromPipeline);
            _forcedRequestBlock = new BroadcastBlock<CycleProcessingContext>(context => context);

            CreatePipeline(bedController, monitorController);
            
            _cycleProcessingSynchronizer = new CycleProcessingSynchronizer(
                _pipelineOnTimeStartBlock, 
                workerController,
                _isAutoPumpingEnabled,
                startParams.PumpingNumberOfAttemptsOnProcessing);
            _isStandartProcessingInProgress = false;
            _previouslyKnownCycleNumber = StartCycleNumber;
            
            _bedController.OnPauseFromDeviceRequested += BedControllerOnPauseFromDeviceRequested;
            _bedController.OnResumeFromDeviceRequested += BedControllerOnResumeFromDeviceRequested;
            _bedController.OnReverseFromDeviceRequested += BedControllerOnReverseFromDeviceRequested;
            _bedController.OnEmeregencyStopFromDeviceRequested += BedControllerOnEmeregencyStopFromDeviceRequested;
            
            _processedEventsCached = new MemoryCache(GetType().Name);
            _isFailedOnPumping = false;
        }

        private void CreatePipeline(
            [NotNull] IBedController bedController,
            [NotNull] IMonitorController monitorController)
        {
            var sessionInfoProvider = new SessionProcessingInfoProvider(
                _bedController, 
                _startParams.BedControllerInitParams.Timeout);
            var sessionInfoProvidingBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    sessionInfoProvider.ProcessAsync(context));

            var iterationProvider = new IterationParamsProvider(
                _bedController,
                _startParams.BedControllerInitParams.Timeout);
            var iterationProvidingBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    iterationProvider.ProcessAsync(context));

            var angleReciever = new AngleReciever(
                bedController,
                _startParams.BedControllerInitParams.Timeout);
            var anlgeRecieveBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    angleReciever.ProcessAsync(context));

            var checkPointChecker = new IterationBasedCheckPointChecker();
            var checkPointCheckBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    checkPointChecker.ProcessAsync(context));

            var mainBroadcastBlock = new BroadcastBlock<CycleProcessingContext>(context => context);

            var pumpingManager = new PumpingManager(_monitorController);
            var pupmingManagerBlock = new TransformBlock<CycleProcessingContext, CycleProcessingContext>(
                context => pumpingManager.ProcessAsync(context));
            var forcedCommandPupmingManagerBlock = new TransformBlock<CycleProcessingContext, CycleProcessingContext>(
                context => pumpingManager.ProcessAsync(context));

            var pressureParamsProvider = new PatientPressureParamsProvider(monitorController);
            var pressureParamsProviderBlock = new TransformBlock<CycleProcessingContext, CycleProcessingContext>(
                context => pressureParamsProvider.ProcessAsync(context));

            var commonParamsProvider = new CommonPatientParamsProvider(monitorController, _startParams.MonitorControllerInitParams.Timeout);
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
                forcedCommandPupmingManagerBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                }, context =>
                {
                    var forcedRequested = context?.TryGetForcedDataCollectionRequest()?.IsRequested ?? false;
                    return pumpingManager.CanProcess(context) && forcedRequested;
                });
            checkPointCheckBlock.LinkTo(
                mainBroadcastBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                }, context =>
                {
                    var forcedRequested = context?.TryGetForcedDataCollectionRequest()?.IsRequested ?? false;
                    return !forcedRequested;
                });

            forcedCommandPupmingManagerBlock.LinkTo(
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
                },
                context => commonParamsProvider.CanProcess(context));


            mainBroadcastBlock.LinkTo(
                pupmingManagerBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                },
                context =>
                {
                    var forcedRequested = context?.TryGetForcedDataCollectionRequest()?.IsRequested ?? false;
                    return pumpingManager.CanProcess(context) && !forcedRequested;
                });
            mainBroadcastBlock.LinkTo(
                pressureParamsProviderBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                },
                context =>
                {
                    var forcedRequested = context?.TryGetForcedDataCollectionRequest()?.IsRequested ?? false;
                    var wasPumpingCompleted = context?.TryGetAutoPumpingResultParams()?.WasPumpingCompleted ?? false;
                    return forcedRequested && wasPumpingCompleted;
                });

            pupmingManagerBlock.LinkTo(
                pressureParamsProviderBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                },
                context => pressureParamsProvider.CanProcess(context));

            commonParamsProviderBlock.LinkTo(
                _pipelineFinishCollectorBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });

            pressureParamsProviderBlock.LinkTo(
                _pipelineFinishCollectorBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });
        }


        private Task CollectDataFromPipeline([NotNull] CycleProcessingContext context)
        {
            try
            {
                
                var isForcedCollectionRequested = context.TryGetForcedDataCollectionRequest()?.IsRequested ?? false;
                
                var exceptionParams = context.TryGetExceptionContextParams();
                if (exceptionParams != null)
                {
                    if (exceptionParams.Exception.ErrorCode == SessionProcessingErrorCodes.PumpingError
                        || exceptionParams.Exception.ErrorCode == SessionProcessingErrorCodes.PumpingTimeout)
                    {
                        _isFailedOnPumping = true;
                    }
                    RiseOnce(exceptionParams, () => OnException?.Invoke(this, exceptionParams.Exception));
                    return HandleConnectionErrorsOnDemandAsync(exceptionParams);
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


                var pressureParams = context.TryGetPressureParams();
                if (pressureParams != null)
                {
                    RiseOnce(pressureParams, () => OnPatientPressureParamsRecieved?.Invoke(
                        this,
                        new PatientPressureParams(
                            angleParams?.CurrentAngle ?? 0,
                            pressureParams.SystolicArterialPressure,
                            pressureParams.DiastolicArterialPressure,
                            pressureParams.AverageArterialPressure,
                            iterationsInfo.CurrentIteration,
                            sessionProcessingInfo.CurrentCycleNumber)));
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
                            commonParams.Spo2,
                            iterationsInfo.CurrentIteration,
                            sessionProcessingInfo.CurrentCycleNumber)));
                }

                // чтобы не было рекурсии: форсированный сбор нужен для получения параметров, а не обновления данных о сеансе
                if (isForcedCollectionRequested) return Task.CompletedTask;
                
                if (angleParams != null)
                {
                    RiseOnce(angleParams, () => OnCurrentAngleXRecieved?.Invoke(this, angleParams.CurrentAngle));
                }

                RiseOnce(sessionProcessingInfo, async () =>
                {
                    OnElapsedTimeChanged?.Invoke(this, sessionProcessingInfo.ElapsedTime);
                    OnRemainingTimeChanged?.Invoke(this, sessionProcessingInfo.RemainingTime);

                    // считаем, что сессия завершилась, когда оставшееся время равно 0
                    var isSessionCompleted = sessionProcessingInfo.RemainingTime <= TimeSpan.Zero;
                    // чтобы не было лишних вызовов в случае долгого обновления в последней фазе
                    if (isSessionCompleted)
                    {
                        _cycleProcessingSynchronizer.Stop();
                    }
                    //todo придумать, как определять, что закончился цикл, что надо снять показатели в последнем 0
                    
                    if (_previouslyKnownCycleNumber != sessionProcessingInfo.CurrentCycleNumber ||
                          isSessionCompleted)
                    {
                        await InnerForceDataCollectionRequestAsync().ConfigureAwait(false);
                        _previouslyKnownCycleNumber = sessionProcessingInfo.CurrentCycleNumber; // по идеи, 
                        OnCycleCompleted?.Invoke(this, _previouslyKnownCycleNumber);
                    }

                    if (isSessionCompleted)
                    {
                        _isStandartProcessingInProgress = false;
                        OnSessionCompleted?.Invoke(this, EventArgs.Empty);
                    }
                });

            }
            catch (Exception e)
            {
                OnException?.Invoke(
                    this,
                    new SessionProcessingException(
                        SessionProcessingErrorCodes.UnhandledException,
                        "Необработанная ошибка в процессе выполнения сеанса",
                        e));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Попытается переподключиться к устройствам в случае ошибки соединения
        /// </summary>
        /// <param name="exceptionParams">Ошибки, возникшие в ходе сеанса</param>
        /// <returns></returns>
        [NotNull]
        private Task HandleConnectionErrorsOnDemandAsync([NotNull] ExceptionCycleProcessingContextParams exceptionParams)
        {
            var exception = exceptionParams.Exception;
            switch (exception.ErrorCode)
            {
                case SessionProcessingErrorCodes.InversionTableConnectionError:
                    return ReconnectToInversionTableAsync();
                case SessionProcessingErrorCodes.MonitorConnectionError:
                    return ReconnectToMonitorAsync();
                default:
                    return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Устанавливает повторное соединение с инверсионным столом
        /// </summary>
        /// <returns></returns>
        private async Task ReconnectToInversionTableAsync()
        {
            if (_monitorController.IsConnected) return;
            if (_startParams.DeviceReconnectionTimeout == null) return;
            
            if (Monitor.TryEnter(_monitorRecconectionSyncObject))
            {
                try
                {
                    if (_monitorController.IsConnected) return;
                    await _monitorController.DisconnectAsync().ConfigureAwait(false);
                    await Task.Delay(_startParams.DeviceReconnectionTimeout.Value).ConfigureAwait(false);
                    await _monitorController.ConnectAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    OnSessionErrorStop?.Invoke(this, ex);
                }
                finally
                {
                    // Ensure that the lock is released.
                    Monitor.Exit(_monitorRecconectionSyncObject);
                }
            }
        }

        /// <summary>
        /// Устанавливает повторное соединение с кардиомонитором
        /// </summary>
        /// <returns></returns>
        private async Task ReconnectToMonitorAsync()
        { 
            if (_bedController.IsConnected) return;
            if (_startParams.DeviceReconnectionTimeout == null) return;
            
            if (Monitor.TryEnter(_inversionTableRecconectionSyncObject))
            {
                try
                {
                    if (_bedController.IsConnected) return;
                    await _bedController.DisconnectAsync().ConfigureAwait(false);
                    await Task.Delay(_startParams.DeviceReconnectionTimeout.Value).ConfigureAwait(false);
                    await _bedController.ConnectAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    OnSessionErrorStop?.Invoke(this, ex);
                }
                finally
                {
                    // Ensure that the lock is released.
                    Monitor.Exit(_inversionTableRecconectionSyncObject);
                }
            }
            
        }

        /// <summary>
        /// Вызывает действия для указанного параметр только один раз
        /// </summary>
        /// <param name="contextParams"></param>
        /// <param name="action"></param>
        /// <remarks>
        /// Крайне ползено, т.к. в Pipeline есть пара параллельных участков, параметры будут дублирвоаться
        /// </remarks>
        private void RiseOnce([NotNull] ICycleProcessingContextParams contextParams, [NotNull] Action action)
        {
            if (_processedEventsCached.ContainsEvent(contextParams)) return;
            
            _processedEventsCached.AddEventToCache(contextParams, _cachedEventLifetime);
            
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
                            SessionProcessingErrorCodes.UnhandledException, 
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
                            SessionProcessingErrorCodes.UnhandledException,
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
                            SessionProcessingErrorCodes.UnhandledException,
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
                            SessionProcessingErrorCodes.UnhandledException,
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
            try
            {
                return InnerStartAsync(false);
            }
            catch (SessionProcessingException e)
            {
                OnException?.Invoke(
                    this, 
                    e);
            }
            catch (Exception e)
            {
                OnException?.Invoke(
                    this,
                    new SessionProcessingException(
                        SessionProcessingErrorCodes.UnhandledException,
                        e.Message, 
                        e));
            }
            return Task.CompletedTask;
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
                
                _monitorController.Init(_startParams.MonitorControllerInitParams);
                await _monitorController
                    .ConnectAsync()
                    .ConfigureAwait(false);
                
                // измерим перед стартом
                await InnerForceDataCollectionRequestAsync()
                    .ConfigureAwait(false);
                // уведомление об ошибке уже сформировалось выше, просто не стартуем
                if (_isFailedOnPumping)
                {
                    OnException?.Invoke(
                        this, 
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.StartFailed, 
                            "Сеанс не будет запущен из-за ошибки накачки манжеты"));
                    return;
                }
                // запускаем кровать
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.Start)
                    .ConfigureAwait(false);
                // запускаем обработку
                _cycleProcessingSynchronizer.Init(_startParams.UpdateDatePeriod);
                _cycleProcessingSynchronizer.Start();
                _isStandartProcessingInProgress = true;
            }
        }

        public Task EmergencyStopAsync()
        {
            try
            {
                return InnerEmeregencyStopAsync(false);
            }
            catch (SessionProcessingException e)
            {
                OnException?.Invoke(
                    this, 
                    e);
            }
            catch (Exception e)
            {
                OnException?.Invoke(
                    this,
                    new SessionProcessingException(
                        SessionProcessingErrorCodes.UnhandledException,
                        e.Message, 
                        e));
            }
            return Task.CompletedTask;
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
            try
            {
                return InnerPauseAsync(false);
            }
            catch (SessionProcessingException e)
            {
                OnException?.Invoke(
                    this, 
                    e);
            }
            catch (Exception e)
            {
                OnException?.Invoke(
                    this,
                    new SessionProcessingException(
                        SessionProcessingErrorCodes.UnhandledException,
                        e.Message, 
                        e));
            }
            return Task.CompletedTask;
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
            try
            {
                return InnerProcessReverseRequestAsync(false);
            }
            catch (SessionProcessingException e)
            {
                OnException?.Invoke(
                    this,
                    e);
            }
            catch (Exception e)
            {
                OnException?.Invoke(
                    this,
                    new SessionProcessingException(
                        SessionProcessingErrorCodes.UnhandledException,
                        e.Message,
                        e));
            }
            return Task.CompletedTask;
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
        
        public Task ForceDataCollectionRequestAsync()
        {
            if (_isStandartProcessingInProgress)
            {
                throw new InvalidOperationException("Нельзя вызывать перезамер показателей во время выполнения сеанса");
            }
            
            try
            {
                return InnerForceDataCollectionRequestAsync();
            }
            catch (SessionProcessingException e)
            {
                OnException?.Invoke(
                    this,
                    e);
            }
            catch (Exception e)
            {
                OnException?.Invoke(
                    this,
                    new SessionProcessingException(
                        SessionProcessingErrorCodes.UnhandledException,
                        e.Message,
                        e));
            }
            return Task.CompletedTask;
        }

        private async Task InnerForceDataCollectionRequestAsync()
        {
            //todo может, вручную задавать номер итерации и цикла?
            var context = new CycleProcessingContext();
            context.AddOrUpdate(new ForcedDataCollectionRequestCycleProcessingContextParams(true));
            context.AddOrUpdate(
                new PumpingRequestContextParams(
                    _isAutoPumpingEnabled, 
                    _startParams.PumpingNumberOfAttemptsOnStartAndFinish));
            await _forcedRequestBlock
                .SendAsync(context)
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            _cycleProcessingSynchronizer.Stop();
            _cycleProcessingSynchronizer.Dispose();
            
            _bedController.OnPauseFromDeviceRequested -= BedControllerOnPauseFromDeviceRequested;
            _bedController.OnResumeFromDeviceRequested -= BedControllerOnResumeFromDeviceRequested;
            _bedController.OnReverseFromDeviceRequested -= BedControllerOnReverseFromDeviceRequested;
            _bedController.OnEmeregencyStopFromDeviceRequested -= BedControllerOnEmeregencyStopFromDeviceRequested;
            
            _bedController.Dispose();
            _monitorController.Dispose();
            _pipelineOnTimeStartBlock.Complete();
            _pipelineFinishCollectorBlock.Completion.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}