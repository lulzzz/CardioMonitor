using System;
using System.Collections.Generic;
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
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using JetBrains.Annotations;
using Markeli.Utils.Logging;
using Polly;

namespace CardioMonitor.BLL.SessionProcessing.DeviceFacade
{
    /// <summary>
    /// Фасад взаимодействия с внешними устройствами
    /// </summary>
    /// <remarks>
    /// Управляет и запрашивает данные с устройств согласно бизнес-логике
    /// </remarks>
    internal class DevicesFacade : 
        IDevicesFacade
    {
        private const short StartCycleNumber = 1;
        private readonly TimeSpan ReconnectionWaitingTimeout = TimeSpan.FromMilliseconds(2);
        
        #region Private fields
 
        private readonly TimeSpan _cachedEventLifetime = TimeSpan.FromSeconds(30);
        
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

        private readonly HashSet<int> _completedCycles;

        /// <summary>
        /// Признак глобального завершения сессия в рамках всего пайплайна
        /// </summary>
        /// <remarks>
        /// Небольшой костыль, чтобы дважды не срабатывало информирование о завершении сеанса
        /// </remarks>
        private bool _isSessionGlobalyCompleted;

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

        public event EventHandler<ReconnectionEventArgs> OnInversionTableReconnectionStarted;
        public event EventHandler<ReconnectionEventArgs> OnInversionTableReconnectionWaiting;
        public event EventHandler OnInversionTableReconnected;
        public event EventHandler OnInversionTableReconnectionFailed;
        
        public event EventHandler<ReconnectionEventArgs> OnMonitorReconnectionStarted;
        public event EventHandler<ReconnectionEventArgs> OnMonitorReconnectionWaiting;
        public event EventHandler OnMonitorReconnected;
        public event EventHandler OnMonitorReconnectionFailed;

        #endregion
        
        private readonly SemaphoreSlim _inversionTableRecconectionMutex;
        private readonly SemaphoreSlim _monitorRecconectionMutex;
        
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
            _isReverseAlreadyRequested = false;
            _isSessionGlobalyCompleted = false;
            _completedCycles = new HashSet<int>();
            
            _monitorRecconectionMutex = new SemaphoreSlim(1,1);
            _inversionTableRecconectionMutex = new SemaphoreSlim(1,1);
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
                context => pumpingManager.CanProcess(context));
            
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
                
                var isForcedCollectionRequested = context.TryGetForcedDataCollectionRequest() != null;
                var exceptionParams = context.TryGetExceptionContextParams();
                if (exceptionParams != null)
                {
                    _logger?.Trace($"{GetType().Name}: ошибка в Pipeline: " +
                                   $"итерация {exceptionParams.Exception.IterationNumber}, " +
                                   $"сеанс {exceptionParams.Exception.CycleNumber}, " +
                                   $"код ошибки {exceptionParams.Exception.ErrorCode}, " +
                                   $"причина: {exceptionParams.Exception.Message}",
                        exceptionParams.Exception);
                    if (IsFatalErrorOccured(exceptionParams))
                    {
                        _cycleProcessingSynchronizer.Stop();
                        OnSessionErrorStop?.Invoke(this, exceptionParams.Exception);
                        return Task.CompletedTask;
                    }

                    var canReconnectToMonitor = CanReconnectToMonitor(exceptionParams.Exception);
                    var canReconnectoToInversionTable = CanReconnectToInversionTable(exceptionParams.Exception);
                    
                    
                    RiseOnce(exceptionParams, () =>
                    {
                        if (canReconnectToMonitor || canReconnectoToInversionTable)
                        {
                            _cycleProcessingSynchronizer.Pause();
                        }
                        OnException?.Invoke(this, exceptionParams.Exception);
                    });
                    ReconnectToDeviceAndRestorePipelineSaveAsync(
                        exceptionParams.Exception,
                        canReconnectoToInversionTable,
                        canReconnectToMonitor);
                    
                    return Task.CompletedTask;
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
                var angleX = angleParams?.CurrentAngle == null
                    ? 0
                    : (float)(Math.Round(angleParams.CurrentAngle * 2, MidpointRounding.AwayFromZero) / 2);


                var pressureParams = context.TryGetPressureParams();
                if (pressureParams != null)
                {
                    _logger?.Trace($"{GetType().Name}: поступили новые данные о давлении пациента");
                    RiseOnce(pressureParams, () => OnPatientPressureParamsRecieved?.Invoke(
                        this,
                        new PatientPressureParams(
                            angleX,
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
                            angleX,
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
                    _logger?.Trace($"{GetType().Name}: текущий угол наклона кровати по оси Х - {angleParams}");
                    RiseOnce(angleParams, () => OnCurrentAngleXRecieved?.Invoke(this, angleParams.CurrentAngle));
                }

                RiseOnce(sessionProcessingInfo, async () =>
                {
                    OnElapsedTimeChanged?.Invoke(this, sessionProcessingInfo.ElapsedTime);
                    OnRemainingTimeChanged?.Invoke(this, sessionProcessingInfo.RemainingTime);

                    // считаем, что сессия завершилась, когда оставшееся время равно 0
                    var isSessionCompleted = sessionProcessingInfo.RemainingTime <= TimeSpan.Zero 
                                             && sessionProcessingInfo.ElapsedTime > TimeSpan.Zero
                                             && sessionProcessingInfo.CurrentCycleNumber == sessionProcessingInfo.CyclesCount;
                    // чтобы не было лишних вызовов в случае долгого обновления в последней фазе
                    if (isSessionCompleted)
                    {
                        _logger?.Trace($"{GetType().Name}: остановка синхронизатора сессии");
                        _cycleProcessingSynchronizer.Stop();
                    }

                    var wasLastCycleDataCollected = false;
                    if ((_previouslyKnownCycleNumber != sessionProcessingInfo.CurrentCycleNumber 
                         || isSessionCompleted) 
                        && !_completedCycles.Contains(_previouslyKnownCycleNumber))
                    {
                        _completedCycles.Add(_previouslyKnownCycleNumber);
                        await InnerForceDataCollectionRequestAsync().ConfigureAwait(false);
                        OnCycleCompleted?.Invoke(this, _previouslyKnownCycleNumber);
                        _logger?.Info($"{GetType().Name}: закончился цикл {_previouslyKnownCycleNumber}");
                        _previouslyKnownCycleNumber = sessionProcessingInfo.CurrentCycleNumber; // по идеи, 
                        wasLastCycleDataCollected = true;
                    }

                    if (isSessionCompleted 
                        && wasLastCycleDataCollected)
                    {
                        _isSessionGlobalyCompleted = true;
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

        private bool IsFatalErrorOccured([NotNull] ExceptionCycleProcessingContextParams exceptionParams)
        {
            var exception = exceptionParams.Exception;
            switch (exception.ErrorCode)
            {
                case SessionProcessingErrorCodes.Unknown:
                case SessionProcessingErrorCodes.UnhandledException:
                    return true;
                default:
                    return false;
            }
        }

       
        private bool CanReconnectToInversionTable([NotNull] SessionProcessingException exception)
        {
            switch (exception.ErrorCode)
            {
                case SessionProcessingErrorCodes.InversionTableConnectionError:
                case SessionProcessingErrorCodes.InversionTableTimeout:
                    return true;
                default:
                    return false;
            }
        }
        
        private bool CanReconnectToMonitor([NotNull] SessionProcessingException exception)
        {
            switch (exception.ErrorCode)
            {
                case SessionProcessingErrorCodes.MonitorConnectionError:
                case SessionProcessingErrorCodes.PatientCommonParamsRequestTimeout:
                case SessionProcessingErrorCodes.PatientPressureParamsRequestTimeout:
                case SessionProcessingErrorCodes.PumpingTimeout:
                    return true;
                default:
                    return false;
            }
        }

        private async Task ReconnectToDeviceAndRestorePipelineSaveAsync(
            [NotNull] Exception pipelineException,
            bool canReconnectoToInversionTable, 
            bool canReconnectToMonior)
        {
            if (!canReconnectoToInversionTable && !canReconnectToMonior) return;
            var canRestorePipeline = true;
            if (canReconnectoToInversionTable)
            {
                canRestorePipeline = await ReconnectToInversionTableSaveAsync()
                    .ConfigureAwait(false);
            }

            if (canRestorePipeline && canReconnectToMonior)
            {
                canRestorePipeline = await ReconnectToMonitorSafeAsync();
            }

            if (!canRestorePipeline)
            {
                _cycleProcessingSynchronizer.Stop();
                OnSessionErrorStop?.Invoke(this, pipelineException);
            }
            else
            {
                _cycleProcessingSynchronizer.Resume();
            }
        }

        /// <summary>
        /// Устанавливает повторное соединение с инверсионным столом
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ReconnectToInversionTableSaveAsync()
        {
            if (_bedController.IsConnected) return true;
            if (_startParams.BedControllerConfig.DeviceReconnectionTimeout == null
                || _startParams.BedControllerConfig.DeviceReconectionsRetriesCount == null)
            {
                return false;
            }

            var reconnectionTimeout = _startParams.BedControllerConfig.DeviceReconnectionTimeout.Value;
            var reconnectionsCount = _startParams.BedControllerConfig.DeviceReconectionsRetriesCount.Value;
            if (reconnectionsCount > 0)
            {
                reconnectionsCount = reconnectionsCount - 1;
            }

            var isFree = await _inversionTableRecconectionMutex
                .WaitAsync(ReconnectionWaitingTimeout)
                .ConfigureAwait(false);
            // кто-то другой уже переподключается
            if (!isFree) return true;
            if (_bedController.IsConnected) return true;

            _logger?.Info($"{GetType().Name}: восстановление подключения к инверсионному столу...");
            try
            {
                var reconnectionEventArgs = new ReconnectionEventArgs(reconnectionTimeout, 1);
                var recilencePolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(
                        reconnectionsCount,
                        retryAttemp => reconnectionTimeout,
                        (exception, timeSpan, localContext) =>
                        {
                            _logger?.Error($"{GetType().Name}: попытка восстановления соединения с инверсионным столом " +
                                           $"номер {reconnectionEventArgs.ReconnectionRetryNumber} завершилась не удачно. Причина: {exception.Message}",
                                exception);
                            reconnectionEventArgs = new ReconnectionEventArgs(
                                reconnectionTimeout, 
                                reconnectionEventArgs.ReconnectionRetryNumber+1);
                            OnInversionTableReconnectionWaiting?.Invoke(this, reconnectionEventArgs);
                        });
                return await recilencePolicy.ExecuteAsync(async () =>
                {
                    if (_bedController.IsConnected) return true;
                    OnInversionTableReconnectionStarted?.Invoke(this, reconnectionEventArgs);
                    await _bedController
                        .ConnectAsync()
                        .ConfigureAwait(false);
                    _logger?.Info($"{GetType().Name}: восстановлено подключение к инверсионному столу");
                    OnInversionTableReconnected?.Invoke(this, EventArgs.Empty);
                    return true;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.Error($"{GetType().Name}: ошибка восстановления подключения к инверсионному столу. " +
                               $"Причина: {ex.Message}", ex);
                OnInversionTableReconnectionFailed?.Invoke(this, EventArgs.Empty);
                return false;
            }
            finally
            {
                _inversionTableRecconectionMutex.Release();
            }
        }
        
        /// <summary>
        /// Устанавливает повторное соединение с кардиомонитором
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ReconnectToMonitorSafeAsync()
        { 
            if (_monitorController.IsConnected) return true;
            if (_startParams.MonitorControllerConfig.DeviceReconnectionTimeout == null
                || _startParams.MonitorControllerConfig.DeviceReconectionsRetriesCount == null)
            {
                return false;
            }

            var reconnectionTimeout = _startParams.MonitorControllerConfig.DeviceReconnectionTimeout.Value;
            var reconnectionsCount = _startParams.MonitorControllerConfig.DeviceReconectionsRetriesCount.Value;
            if (reconnectionsCount > 0)
            {
                reconnectionsCount = reconnectionsCount - 1;
            }

            var isFree = await _monitorRecconectionMutex
                .WaitAsync(ReconnectionWaitingTimeout)
                .ConfigureAwait(false);
            // кто-то другой уже переподключается
            if (!isFree) return true;
            if (_monitorController.IsConnected) return true;

            _logger?.Info($"{GetType().Name}: восстановление подключения к кардиомонитору...");
            try
            {
                var reconnectionEventArgs = new ReconnectionEventArgs(reconnectionTimeout, 1);
                var recilencePolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(
                        reconnectionsCount,
                        retryAttemp => reconnectionTimeout,
                        (exception, timeSpan, localContext) =>
                        {
                            _logger?.Error($"{GetType().Name}: попытка восстановления соединения с кардиомонитором номер " +
                                           $"{reconnectionEventArgs.ReconnectionRetryNumber} завершилась не удачно. Причина: {exception.Message}",
                                exception);
                            reconnectionEventArgs = new ReconnectionEventArgs(reconnectionTimeout, 
                                reconnectionEventArgs.ReconnectionRetryNumber+1);
                            OnMonitorReconnectionWaiting?.Invoke(this, reconnectionEventArgs);
                        });
                return await recilencePolicy.ExecuteAsync(async () =>
                {
                    if (_monitorController.IsConnected) return true;
                    OnMonitorReconnectionStarted?.Invoke(this, reconnectionEventArgs);
                    await _monitorController
                        .ConnectAsync()
                        .ConfigureAwait(false);
                    _logger?.Info($"{GetType().Name}: восстановлено подключение к кардиомонитору");
                    OnMonitorReconnected?.Invoke(this, EventArgs.Empty);
                    return true;
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.Error($"{GetType().Name}: ошибка восстановления подключения к кардиомонитору. " +
                               $"Причина: {ex.Message}", ex);
                OnMonitorReconnectionFailed?.Invoke(this, EventArgs.Empty);
                return false;
            }
            finally
            {
                _monitorRecconectionMutex.Release();
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

        public Task<bool> StartAsync()
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
                return Task.FromResult(false);
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
                return Task.FromResult(false);
            }
        }

        private async Task<bool> InnerStartAsync(bool isCalledFromDevice)
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
                _bedController.InitController(_startParams.BedControllerConfig);
                _logger?.Trace($"{GetType().Name}: подключение к инверсионному столу");
                try
                {
                    await _bedController
                        .ConnectAsync()
                        .ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await _bedController
                        .DisconnectAsync()
                        .ConfigureAwait(false);
                    return false;
                }
               
                // запускаем кровать
                var bedStatus = await _bedController
                    .GetBedStatusAsync()
                    .ConfigureAwait(false);
                if (bedStatus != BedStatus.Ready)
                {
                    _logger?.Trace($"{GetType().Name}: ошибка готовности кровати");
                    OnException?.Invoke(
                        this,
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.StartFailed,
                            "Сеанс не будет запущен, так как кровать не готова к старту"));
                    await _bedController
                        .DisconnectAsync()
                        .ConfigureAwait(false);
                    return false;
                }

                _logger?.Trace($"{GetType().Name}: инициализация контроллера кардиомонитора");
                _monitorController.Init(_startParams.MonitorControllerConfig);
                _logger?.Trace($"{GetType().Name}: подключение к кардиомонитору");
                try
                {
                    await _monitorController
                        .ConnectAsync()
                        .ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await _monitorController
                        .DisconnectAsync()
                        .ConfigureAwait(false);
                    return false;
                }

                // измерим перед стартом
                _logger?.Trace($"{GetType().Name}: начальный запрос данных");
                var isForcedDataCollected = await InnerForceDataCollectionRequestAsync()
                    .ConfigureAwait(false);
                // если ошибки измерения в первой точке - сеанс не начинаем
                if (!isForcedDataCollected)
                {
                    _logger?.Trace($"{GetType().Name}: ошибка сбора начальных данных");
                    OnException?.Invoke(
                        this, 
                        new SessionProcessingException(
                            SessionProcessingErrorCodes.StartFailed, 
                            "Сеанс не будет запущен из-за ошибки в получении начальных показателей пациента"));
                    await _bedController
                        .DisconnectAsync()
                        .ConfigureAwait(false);
                    await _monitorController
                        .DisconnectAsync()
                        .ConfigureAwait(false);
                    return false;
                    
                }
                _logger?.Trace($"{GetType().Name}: подготовка инверсионного стола к сеансу");
                await _bedController
                    .PrepareDeviceForSessionAsync()
                    .ConfigureAwait(false);    
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

            return true;
        }

        public Task<bool> EmergencyStopAsync()
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
                return Task.FromResult(false);
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
                return Task.FromResult(false);
            }
        }

        private async Task<bool> InnerEmeregencyStopAsync(bool isCalledFromDevice)
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
            //todo synchonizer

            return true;
        }

        public Task<bool> PauseAsync()
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
                return Task.FromResult(false);
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
                return Task.FromResult(false);
            }          
        }

        private async Task<bool> InnerPauseAsync(bool isCalledFromDevice)
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

            return true;
        }


        public Task<bool> ProcessReverseRequestAsync()
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
                return Task.FromResult(false);
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
                return Task.FromResult(false);
            }
        }

        private async Task<bool> InnerProcessReverseRequestAsync(bool isCalledFromDevice)
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
            return true;
        }

        public Task<bool> ForceDataCollectionRequestAsync()
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
                return Task.FromResult(false);
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
                return Task.FromResult(false);
            }
        }

        private async Task<bool> InnerForceDataCollectionRequestAsync()
        {
            var context = new CycleProcessingContext();
            var forcedRequest = new ForcedDataCollectionRequestCycleProcessingContextParams();
            context.AddOrUpdate(forcedRequest);
            context.AddOrUpdate(
                new PumpingRequestContextParams(
                    _isAutoPumpingEnabled, 
                    _startParams.PumpingNumberOfAttemptsOnStartAndFinish));

            _logger?.Trace($"{GetType().Name}: запуск pipeline для ручного сбора показателей пациента");
            await _forcedRequestBlock
                .SendAsync(context)
                .ConfigureAwait(false);

            _logger?.Trace($"{GetType().Name}: ожидание завершения ручного сбора показателей пациента"); 
            await forcedRequest.ResultingTask
                .ConfigureAwait(false);
            _logger?.Trace($"{GetType().Name}: ручной сбор показателей пациента завершен"); 
            
            var pumpingResult = context.TryGetAutoPumpingResultParams();
            if (pumpingResult == null) return false;

            if (!pumpingResult.WasPumpingCompleted) return false;

            var exceptions = context.TryGetExceptionContextParams();
            return exceptions == null;
        }

        public void Dispose()
        {
            if (_cycleProcessingSynchronizer != null)
            {
                _cycleProcessingSynchronizer.Stop();
                _cycleProcessingSynchronizer.Dispose();
            }

            if (_bedController != null)
            {
                _bedController.OnPauseFromDeviceRequested -= BedControllerOnPauseFromDeviceRequested;
                _bedController.OnResumeFromDeviceRequested -= BedControllerOnResumeFromDeviceRequested;
                _bedController.OnReverseFromDeviceRequested -= BedControllerOnReverseFromDeviceRequested;
                _bedController.OnEmeregencyStopFromDeviceRequested -= BedControllerOnEmeregencyStopFromDeviceRequested;
            
                _bedController.Dispose();
                
            }

            if (_monitorController != null)
            {
                _monitorController.Dispose();
            }
            
            _pipelineOnTimeStartBlock.Complete();
            _pipelineFinishCollectorBlock.Completion.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}