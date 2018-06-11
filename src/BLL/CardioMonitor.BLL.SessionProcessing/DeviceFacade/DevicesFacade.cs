using System;
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
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;
using Markeli.Utils.Logging;

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
        [CanBeNull] private readonly ILogger _logger;
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

        private bool _isReverseAlreadyRequested;

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
            [NotNull] IWorkerController workerController,
            [CanBeNull] ILogger logger = null)
        {
            if (workerController == null) throw new ArgumentNullException(nameof(workerController));
            _monitorController = monitorController ?? throw new ArgumentNullException(nameof(monitorController));
            _logger = logger;
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
            _isReverseAlreadyRequested = false;
        }

        private void CreatePipeline(
            [NotNull] IBedController bedController,
            [NotNull] IMonitorController monitorController)
        {
            _logger?.Trace($"{GetType().Name}: начато создание pipeline...");
            _logger?.Debug($"{GetType().Name}: таймаут операций с инверсионным столом - {_startParams.BedControllerConfig.Timeout}");
            _logger?.Debug($"{GetType().Name}: таймаут операций с кардиомонитором - {_startParams.MonitorControllerConfig.Timeout}");

            var sessionInfoProvider = new SessionProcessingInfoProvider(
                _bedController, 
                _startParams.BedControllerConfig.Timeout);
            sessionInfoProvider.SetLogger(_logger);
            var sessionInfoProvidingBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    sessionInfoProvider.ProcessAsync(context));

            var iterationProvider = new IterationParamsProvider(
                _bedController,
                _startParams.BedControllerConfig.Timeout);
            iterationProvider.SetLogger(_logger);
            var iterationProvidingBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    iterationProvider.ProcessAsync(context));

            var angleReciever = new AngleReciever(
                bedController,
                _startParams.BedControllerConfig.Timeout);
            angleReciever.SetLogger(_logger);
            var anlgeRecieveBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    angleReciever.ProcessAsync(context));

            var checkPointChecker = new IterationBasedCheckPointChecker();
            checkPointChecker.SetLogger(_logger);
            var checkPointCheckBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context =>
                    checkPointChecker.ProcessAsync(context));

            var mainBroadcastBlock = new BroadcastBlock<CycleProcessingContext>(context => context);

            var pumpingManager = new PumpingManager(
                _monitorController,
                _startParams.MonitorControllerConfig.Timeout);
            pumpingManager.SetLogger(_logger);
            var pupmingManagerBlock = new TransformBlock<CycleProcessingContext, CycleProcessingContext>(
                context => pumpingManager.ProcessAsync(context));
            var forcedCommandPupmingManagerBlock = new TransformBlock<CycleProcessingContext, CycleProcessingContext>(
                context => pumpingManager.ProcessAsync(context));

            var pressureParamsProvider = new PatientPressureParamsProvider(
                monitorController,
                _startParams.MonitorControllerConfig.Timeout);
            pressureParamsProvider.SetLogger(_logger);
            var pressureParamsProviderBlock = new TransformBlock<CycleProcessingContext, CycleProcessingContext>(
                context => pressureParamsProvider.ProcessAsync(context));

            var commonParamsProvider = new CommonPatientParamsProvider(
                monitorController, 
                _startParams.MonitorControllerConfig.Timeout);
            commonParamsProvider.SetLogger(_logger);
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

            _logger?.Trace($"{GetType().Name}: создание pipeline завершено");
        }


        private Task CollectDataFromPipeline([NotNull] CycleProcessingContext context)
        {
            try
            {
                _logger?.Trace($"{GetType().Name}: агрегация данных...");

                var isForcedCollectionRequested = context.TryGetForcedDataCollectionRequest()?.IsRequested ?? false;
                
                var exceptionParams = context.TryGetExceptionContextParams();
                if (exceptionParams != null)
                {
                    if (exceptionParams.Exception.ErrorCode == SessionProcessingErrorCodes.PumpingError
                        || exceptionParams.Exception.ErrorCode == SessionProcessingErrorCodes.PumpingTimeout)
                    {
                        _isFailedOnPumping = true;
                    }
                    _logger?.Trace($"{GetType().Name}: ошибка в Pipeline: " +
                                   $"итерация {exceptionParams.Exception.IterationNumber}, " +
                                   $"сенас {exceptionParams.Exception.CycleNumber}, " +
                                   $"код ошибки {exceptionParams.Exception.ErrorCode}, " +
                                   $"причина: {exceptionParams.Exception.Message}",
                        exceptionParams.Exception);
                    RiseOnce(exceptionParams, () => OnException?.Invoke(this, exceptionParams.Exception));
                    return HandleConnectionErrorsOnDemandAsync(exceptionParams);
                }

                var sessionProcessingInfo = context.TryGetSessionProcessingInfo();
                if (sessionProcessingInfo == null)
                {

                    _logger?.Warning($"{GetType().Name}: не удалось получить информацию о сеансе");
                    return Task.CompletedTask;
                }

                var iterationsInfo = context.TryGetIterationParams();
                if (iterationsInfo == null)
                {
                    _logger?.Warning($"{GetType().Name}: не удалось получить информацию об итерациях");
                    return Task.CompletedTask;
                }

                var angleParams = context.TryGetAngleParam();


                var pressureParams = context.TryGetPressureParams();
                if (pressureParams != null)
                {
                    _logger?.Trace($"{GetType().Name}: поступили новые данных о давлении пациента");
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
                    _logger?.Trace($"{GetType().Name}: поступили новые общие данные пациента");
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
                    _logger?.Trace($"{GetType().Name}: текущий угол наклона кровати по оси Х - {angleParams.CurrentAngle}");
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
                        _logger?.Trace($"{GetType().Name}: остановка синхронизатора сессии");
                        _cycleProcessingSynchronizer.Stop();
                    }
                    
                    if (_previouslyKnownCycleNumber != sessionProcessingInfo.CurrentCycleNumber ||
                          isSessionCompleted)
                    {
                        await InnerForceDataCollectionRequestAsync().ConfigureAwait(false);
                        _previouslyKnownCycleNumber = sessionProcessingInfo.CurrentCycleNumber; // по идеи, 
                        OnCycleCompleted?.Invoke(this, _previouslyKnownCycleNumber);
                        _logger?.Info($"{GetType().Name}: закончился цикл {sessionProcessingInfo.CurrentCycleNumber}");
                    }

                    if (isSessionCompleted)
                    {
                        _isStandartProcessingInProgress = false;
                        OnSessionCompleted?.Invoke(this, EventArgs.Empty);
                        _logger?.Info($"{GetType().Name}: сеанс завершился");
                    }
                });

            }
            catch (Exception e)
            {
                _logger?.Error($"{GetType().Name}: Необработанная ошибка в процессе выполнения сеанса. Причина: {e.Message}", e);
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
            if (_startParams.BedControllerConfig.DeviceReconnectionTimeout == null) return;
            
            if (Monitor.TryEnter(_monitorRecconectionSyncObject))
            {
                _logger?.Info($"{GetType().Name}: повторное подключение к инверсионному столу...");
                try
                {
                    if (_monitorController.IsConnected) return;
                    await _monitorController.DisconnectAsync().ConfigureAwait(false);
                    await Task.Delay(_startParams.BedControllerConfig.DeviceReconnectionTimeout.Value).ConfigureAwait(false);
                    await _monitorController.ConnectAsync().ConfigureAwait(false);
                    _logger?.Info($"{GetType().Name}: установлено подключение к инверсионному столу");
                }
                catch (Exception ex)
                {
                    _logger?.Error($"{GetType().Name}: ошибка повторного подключения к инверсионному столу. Причина: {ex.Message}", ex);
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
            if (_startParams.MonitorControllerConfig.DeviceReconnectionTimeout == null) return;
            
            if (Monitor.TryEnter(_inversionTableRecconectionSyncObject))
            {
                try
                {
                    _logger?.Info($"{GetType().Name}: повторное подключение к кардиомонитору...");
                    if (_bedController.IsConnected) return;
                    await _bedController.DisconnectAsync().ConfigureAwait(false);
                    await Task.Delay(_startParams.MonitorControllerConfig.DeviceReconnectionTimeout.Value).ConfigureAwait(false);
                    await _bedController.ConnectAsync().ConfigureAwait(false);
                    _logger?.Info($"{GetType().Name}: установлено подключение к кардиомонитору");
                }
                catch (Exception ex)
                {
                    _logger?.Error($"{GetType().Name}: ошибка повторного подключения к кардиомонитору. Причина: {ex.Message}", ex);
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
        /// Вызывает действия для указанного параметра только один раз
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
                    _logger?.Info($"{GetType().Name}: запрос экстренной остановки с инверсионного стола...");
                    await InnerEmeregencyStopAsync(true)
                        .ConfigureAwait(false);
                    OnEmeregencyStoppedFromDevice?.Invoke(this, EventArgs.Empty);
                    _logger?.Info($"{GetType().Name}: запрос экстренной остановки с инверсионного стола выполнен");
                }
                catch (SessionProcessingException e)
                {
                    _logger?.Error($"{GetType().Name}: ошибка экстренной остановки, запущенной с инверсионное стола. Причина: {e.Message}", e);
                    OnException?.Invoke(
                        this,
                        e);
                }
                catch (Exception e)
                {
                    _logger?.Error($"{GetType().Name}: непредвиденная ошибка экстренной остановки, запущенной с инверсионное стола. Причина: {e.Message}", e);
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
                    _logger?.Info($"{GetType().Name}: запрос реверса с инверсионного стола...");
                    await InnerProcessReverseRequestAsync(true)
                        .ConfigureAwait(false);
                    OnReversedFromDevice?.Invoke(this, EventArgs.Empty);
                    _logger?.Info($"{GetType().Name}: запрос реверса с инверсионного стола выполнен");
                }
                catch (SessionProcessingException e)
                {
                    _logger?.Error($"{GetType().Name}: ошибка реверса, запущенного с инверсионного стола. Причина: {e.Message}", e);
                    OnException?.Invoke(
                        this,
                        e);
                }
                catch (Exception e)
                {
                    _logger?.Error($"{GetType().Name}: непредвиденаня ошибка реверса, запущенного с инверсионного стола. Причина: {e.Message}", e);
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
                    _logger?.Info($"{GetType().Name}: запрос продолжения сеанса с инверсионного стола...");
                    await InnerStartAsync(true)
                        .ConfigureAwait(false);
                    OnResumedFromDevice?.Invoke(this, EventArgs.Empty);
                    _logger?.Info($"{GetType().Name}: запрос продолжения сеанса с инверсионного стола выполнен");
                }
                catch (SessionProcessingException e)
                {
                    _logger?.Error($"{GetType().Name}: ошибка продолжения сеанса, запущенного с инверсионного стола. Причина: {e.Message}", e);
                    OnException?.Invoke(
                        this,
                        e);
                }
                catch (Exception e)
                {
                    _logger?.Error($"{GetType().Name}: непредвиденная ошибка продолжения сеанса, запущенного с инверсионного стола. Причина: {e.Message}", e);
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
                    _logger?.Info($"{GetType().Name}: запрос приостановки сеанса с инверсионного стола...");
                    await InnerPauseAsync(true)
                        .ConfigureAwait(false);
                    OnPausedFromDevice?.Invoke(this, EventArgs.Empty);
                    _logger?.Info($"{GetType().Name}: запрос приостановки сеанса с инверсионного стола выполнен");
                }
                catch (Exception e)
                {
                    _logger?.Error($"{GetType().Name}: ошибка приостановки сеанса с инверсионного стола. Причина: {e.Message}", e);
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
            _logger?.Info($"{GetType().Name}: автонакачка включена");
        }

        public void DisableAutoPumping()
        {
            _isAutoPumpingEnabled = false;
            _cycleProcessingSynchronizer.DisableAutoPumping();
            _logger?.Info($"{GetType().Name}: автонакачка выключена");
        }

        public Task StartAsync()
        {
            try
            {
                _logger?.Info($"{GetType().Name}: запуск/продолжение сеанса");
                return InnerStartAsync(false);
            }
            catch (SessionProcessingException e)
            {
                _logger?.Error($"{GetType().Name}: ошибка запуска/продолжения сеанса. Причина: {e.Message}", e);
                OnException?.Invoke(
                    this, 
                    e);
            }
            catch (Exception e)
            {
                _logger?.Error($"{GetType().Name}: непредвиденная ошибка запуска/продолжения сеанса. Причина: {e.Message}", e);
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
                    _logger?.Info($"{GetType().Name}: вызвано продолжение сеанса");
                    await _bedController
                        .ExecuteCommandAsync(BedControlCommand.Start)
                        .ConfigureAwait(false);

                    _logger?.Info($"{GetType().Name}: сеанс продолжен");
                }
            }
            else
            {
                _logger?.Info($"{GetType().Name}: вызван старт сеанса");
                _logger?.Trace($"{GetType().Name}: инициализация контроллера инверсионного стола");
                _bedController.Init(_startParams.BedControllerConfig);
                _logger?.Trace($"{GetType().Name}: подключение к инверсионному столу");
                await _bedController
                    .ConnectAsync()
                    .ConfigureAwait(false);
               

                _logger?.Trace($"{GetType().Name}: инициализация контроллера кардиомонитора");
                _monitorController.Init(_startParams.MonitorControllerConfig);
                _logger?.Trace($"{GetType().Name}: подключение к кардиомонитору");
                await _monitorController
                    .ConnectAsync()
                    .ConfigureAwait(false);

                // измерим перед стартом
                _logger?.Trace($"{GetType().Name}: начальный запрос данных");
                await InnerForceDataCollectionRequestAsync()
                    .ConfigureAwait(false);
                // уведомление об ошибке уже сформировалось выше, просто не стартуем
                if (_isFailedOnPumping)
                {
                    _logger?.Trace($"{GetType().Name}: ошибка накачки давления.");
                    OnException?.Invoke(
                        this, 
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.StartFailed, 
                            "Сеанс не будет запущен из-за ошибки накачки манжеты"));
                    return;
                }
                // запускаем кровать

                _logger?.Trace($"{GetType().Name}: старт сеанса");
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.Start)
                    .ConfigureAwait(false);
                // запускаем обработку
                _logger?.Trace($"{GetType().Name}: инициализация сервиса синхронизации");
                _cycleProcessingSynchronizer.Init(_startParams.UpdateDataPeriod);
                _logger?.Trace($"{GetType().Name}: старт сервиса синхронизации");
                _cycleProcessingSynchronizer.Start();
                _isStandartProcessingInProgress = true;

                _logger?.Info($"{GetType().Name}: сеанс запущен");
            }
        }

        public Task EmergencyStopAsync()
        {
            try
            {
                _logger?.Trace($"{GetType().Name}: вызвана экстренная остановка");
                return InnerEmeregencyStopAsync(false);
            }
            catch (SessionProcessingException e)
            {

                _logger?.Error($"{GetType().Name}: ошибка экстренной остановки. Причина: {e.Message}", e);
                OnException?.Invoke(
                    this, 
                    e);
            }
            catch (Exception e)
            {
                _logger?.Error($"{GetType().Name}: непредвиденная ошибка экстренной остановки. Причина: {e.Message}", e);
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
            _logger?.Trace($"{GetType().Name}: остановка сервиса синхронизации");
            _cycleProcessingSynchronizer.Stop();
            _isStandartProcessingInProgress = false;
            if (!isCalledFromDevice)
            {
                _logger?.Trace($"{GetType().Name}: отправка команды экстренной остановки инверсионному столу");
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.EmergencyStop)
                    .ConfigureAwait(false);
            }
            _logger?.Trace($"{GetType().Name}: отключение от инверсионного стола");
            await _bedController
                .DisconnectAsync()
                .ConfigureAwait(false);
            _logger?.Trace($"{GetType().Name}: отключение от кардиомонитора");
            await _monitorController
                .DisconnectAsync()
                .ConfigureAwait(false);
            //tdo synchonizer
        }

        public Task PauseAsync()
        {
            try
            {
                _logger?.Info($"{GetType().Name}: запрос приостановки сеанса");
                return InnerPauseAsync(false);
            }
            catch (SessionProcessingException e)
            {
                _logger?.Error($"{GetType().Name}: ошибка приостановки сеанса. Причина: {e.Message}", e);
                OnException?.Invoke(
                    this, 
                    e);
            }
            catch (Exception e)
            {
                _logger?.Error($"{GetType().Name}: непредвиденная ошибка приостановки сеанса. Причина: {e.Message}", e);
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
            _logger?.Trace($"{GetType().Name}: пауза сервиса синхронизации");
            _cycleProcessingSynchronizer.Pause();
            if (!isCalledFromDevice)
            {
                _logger?.Trace($"{GetType().Name}: отправка команды паузы инверсионному столу");
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.Pause)
                    .ConfigureAwait(false);
            }
        }


        public Task ProcessReverseRequestAsync()
        {
            if (_isReverseAlreadyRequested)
            {
                throw new InvalidOperationException("Реверс уже был запрошен ранее");
            }

            try
            {
                _logger?.Info($"{GetType().Name}: запрос реверса");
                return InnerProcessReverseRequestAsync(false);

            }
            catch (SessionProcessingException e)
            {
                _logger?.Error($"{GetType().Name}: ошибка реверса. Причина: {e.Message}", e);
                OnException?.Invoke(
                    this,
                    e);
            }
            catch (Exception e)
            {
                _logger?.Error($"{GetType().Name}: непредвиденная ошибка реверса. Причина: {e.Message}", e);
                OnException?.Invoke(
                    this,
                    new SessionProcessingException(
                        SessionProcessingErrorCodes.UnhandledException,
                        e.Message,
                        e));
            }
            return Task.CompletedTask;
        }

        private async Task InnerProcessReverseRequestAsync(bool isCalledFromDevice)
        {
            if (_isReverseAlreadyRequested)
            {
                throw new InvalidOperationException("Реверс уже был запрошен ранее");
            }

            if (!isCalledFromDevice)
            {
                _logger?.Trace($"{GetType().Name}: отправка команды реверса инверсионному столу");
                await _bedController
                    .ExecuteCommandAsync(BedControlCommand.Reverse)
                    .ConfigureAwait(false);
            }

            _isReverseAlreadyRequested = true;
        }

        public Task ForceDataCollectionRequestAsync()
        {
            if (_isStandartProcessingInProgress)
            {
                throw new InvalidOperationException("Нельзя вызывать перезамер показателей во время выполнения сеанса");
            }
            
            try
            {
                _logger?.Info($"{GetType().Name}: ручной запуск сбора показателей пациента");
                return InnerForceDataCollectionRequestAsync();
            }
            catch (SessionProcessingException e)
            {
                _logger?.Error($"{GetType().Name}: ошибка ручного запуска сбора показателей пациента. Причина: {e.Message}", e);
                OnException?.Invoke(
                    this,
                    e);
            }
            catch (Exception e)
            {
                _logger?.Error($"{GetType().Name}: непредвиденная ошибка ручного запуска сбора показателей пациента. Причина: {e.Message}", e);
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

            _logger?.Trace($"{GetType().Name}: запуск pipeline для ручного сбора показателей пациента");
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