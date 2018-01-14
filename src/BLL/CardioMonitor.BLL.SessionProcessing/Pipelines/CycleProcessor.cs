using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CardioMonitor.BLL.SessionProcessing.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.Exceptions;
using CardioMonitor.BLL.SessionProcessing.Pipelines.Angle;
using CardioMonitor.BLL.SessionProcessing.Pipelines.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.Pipelines.CommonParams;
using CardioMonitor.BLL.SessionProcessing.Pipelines.ForcedDataCollectionRequest;
using CardioMonitor.BLL.SessionProcessing.Pipelines.PressureParams;
using CardioMonitor.BLL.SessionProcessing.Pipelines.Time;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Threading;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    internal class CycleProcessor : 
        ICycleProcessor,
        IDisposable
    {
        [NotNull] private readonly ICheckPointResolver _checkPointResolver;
        [NotNull] private readonly CycleTimeController _cycleTimeController;

        private readonly PipelineStartParams _startParams;

        private readonly BroadcastBlock<PipelineContext> _pipelineOnTimeStartBlock;
        private readonly ActionBlock<PipelineContext> _pipelineFinishCollectorBlock;
        private readonly BroadcastBlock<PipelineContext> _forcedRequestBlock;

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

            _pipelineOnTimeStartBlock = new BroadcastBlock<PipelineContext>(context => context);
            _pipelineFinishCollectorBlock = new ActionBlock<PipelineContext>(CollectDataFromPipeline);
            _forcedRequestBlock = new BroadcastBlock<PipelineContext>(context => context);

            var angleReciever = new AngleReciever(bedController);
            var anlgeRecieveBlock =
                new TransformBlock<PipelineContext, PipelineContext>(context => angleReciever.ProcessAsync(context));

            var checkPointChecker = new CheckPointChecker(checkPointResolver);
            var checkPointCheckBlock =
                new TransformBlock<PipelineContext, PipelineContext>(context =>
                    checkPointChecker.ProcessAsync(context));

            var mainBroadcastBlock = new BroadcastBlock<PipelineContext>(context => context);

            var pressureParamsProvider = new PatientPressureParamsProvider(monitorController, taskHelper);
            var pressureParamsProviderBlock = new TransformBlock<PipelineContext, PipelineContext>(
                context => pressureParamsProvider.ProcessAsync(context));
            
            var commonParamsProvider = new CommonPatientParamsProvider(monitorController, taskHelper);
            var commonParamsProviderBlock = new TransformBlock<PipelineContext, PipelineContext>(
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
            
            _cycleTimeController = new CycleTimeController(_pipelineOnTimeStartBlock);
            _isStandartProcessingInProgress = false;
        }

        private async Task CollectDataFromPipeline([NotNull] PipelineContext context)
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
            
            if (_cycleTimeController.IsPaused)
            {
                _cycleTimeController.Resume();
            }
            else
            {
                _cycleTimeController.Init(_startParams.CycleDuration, _startParams.CycleTickDuration);
                _cycleTimeController.Start();
                _isStandartProcessingInProgress = true;
            }
        }

        public async Task StopAsync()
        {
            await Task.Yield();
            _cycleTimeController.Stop();
            _isStandartProcessingInProgress = false;
        }

        public async Task PauseAsync()
        {
            await Task.Yield();
            _cycleTimeController.Puase();
        }

        public async Task ResetAsync()
        {
            await Task.Yield();
            _cycleTimeController.Init(_startParams.CycleDuration, _startParams.CycleTickDuration);
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
            
            var context = new PipelineContext();
            context.AddOrUpdate(new ForcedDataCollectionRequestContextParams(true));
            await _forcedRequestBlock
                .SendAsync(context)
                .ConfigureAwait(false);
        }
    }
}