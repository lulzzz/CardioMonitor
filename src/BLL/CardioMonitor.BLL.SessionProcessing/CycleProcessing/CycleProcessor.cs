using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CardioMonitor.BLL.SessionProcessing.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.Angle;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.CommonParams;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.Exceptions;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.PressureParams;
using CardioMonitor.BLL.SessionProcessing.CycleProcessing.Time;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Threading;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.CycleProcessing
{
    internal class CycleProcessor : 
        ICycleProcessor,
        IDisposable
    {
        [NotNull] private readonly ICheckPointResolver _checkPointResolver;
        [NotNull] private readonly CycleProcessingSynchroniaztionController _cycleProcessingSynchroniaztionController;

        private readonly PipelineStartParams _startParams;

        private readonly BroadcastBlock<CycleProcessingContext> _pipelineOnTimeStartBlock;
        private readonly ActionBlock<CycleProcessingContext> _pipelineFinishCollectorBlock;
        private readonly BroadcastBlock<CycleProcessingContext> _forcedRequestBlock;

        private bool _isStandartProcessingInProgress;
        
        
        public event EventHandler<TimeSpan> OnElapsedTimeChanged;
        
        public event EventHandler<double> OnCurrentAngleRecieved;
        
        public event EventHandler<PatientPressureParams> OnPatientPressureParamsRecieved;
        
        public event EventHandler<CommonPatientParams> OnCommonPatientParamsRecieved;

        public event EventHandler<SessionProcessingException> OnException; 
        
        
        public CycleProcessor(
            [NotNull] PipelineStartParams startParams,
            [NotNull] IBedController bedController,
            [NotNull] ICheckPointResolver checkPointResolver,
            [NotNull] IMonitorController monitorController,
            [NotNull] TaskHelper taskHelper)
        {
            if (bedController == null) throw new ArgumentNullException(nameof(bedController));
            if (monitorController == null) throw new ArgumentNullException(nameof(monitorController));
            if (taskHelper == null) throw new ArgumentNullException(nameof(taskHelper));
            _checkPointResolver = checkPointResolver ?? throw new ArgumentNullException(nameof(checkPointResolver));
            
            _startParams = startParams ?? throw new ArgumentNullException(nameof(startParams));

            _pipelineOnTimeStartBlock = new BroadcastBlock<CycleProcessingContext>(context => context);
            _pipelineFinishCollectorBlock = new ActionBlock<CycleProcessingContext>(CollectDataFromPipeline);
            _forcedRequestBlock = new BroadcastBlock<CycleProcessingContext>(context => context);

            var angleReciever = new AngleReciever(bedController);
            var anlgeRecieveBlock =
                new TransformBlock<CycleProcessingContext, CycleProcessingContext>(context => angleReciever.ProcessAsync(context));

            var checkPointChecker = new CheckPointChecker(checkPointResolver);
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
            
            _cycleProcessingSynchroniaztionController = new CycleProcessingSynchroniaztionController(_pipelineOnTimeStartBlock);
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
            
            var timeParams = context.TryGetTimeParams();
            if (timeParams != null)
            {
                OnElapsedTimeChanged?.Invoke(this, timeParams.ElapsedTime);
            }
            var angleParams = context.TryGetAngleParam();
            if (angleParams != null)
            {
                OnCurrentAngleRecieved?.Invoke(this, angleParams.CurrentAngle);
            }
            var pressureParams = context.TryGetPressureParams();
            if (pressureParams != null)
            {
                OnPatientPressureParamsRecieved?.Invoke(
                    this, 
                    new PatientPressureParams(
                        pressureParams.InclinationAngle,
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
                        commonParams.InclinationAngle,
                        commonParams.HeartRate,
                        commonParams.RepsirationRate,
                        commonParams.Spo2));
            }
        }

        public async Task StartAsync()
        {
            await Task.Yield();
            
            if (_cycleProcessingSynchroniaztionController.IsPaused)
            {
                _cycleProcessingSynchroniaztionController.Resume();
            }
            else
            {
                _cycleProcessingSynchroniaztionController.Init(_startParams.CycleDuration, _startParams.CycleTickDuration);
                _cycleProcessingSynchroniaztionController.Start();
                _isStandartProcessingInProgress = true;
            }
        }

        public async Task StopAsync()
        {
            await Task.Yield();
            _cycleProcessingSynchroniaztionController.Stop();
            _isStandartProcessingInProgress = false;
        }

        public async Task PauseAsync()
        {
            await Task.Yield();
            _cycleProcessingSynchroniaztionController.Puase();
        }

        public async Task ResetAsync()
        {
            await Task.Yield();
            _cycleProcessingSynchroniaztionController.Init(_startParams.CycleDuration, _startParams.CycleTickDuration);
            _isStandartProcessingInProgress = false; 
        }

        public void ProcessReverseRequest()
        {
            _checkPointResolver.ConsiderReversing();
        }

        public void Dispose()
        {
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