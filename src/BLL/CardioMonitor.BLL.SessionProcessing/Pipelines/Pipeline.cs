using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CardioMonitor.BLL.SessionProcessing.Pipelines.ActionBlocks;
using CardioMonitor.BLL.SessionProcessing.Pipelines.Angle;
using CardioMonitor.BLL.SessionProcessing.Pipelines.CheckPoints;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.SessionProcessing;
using JetBrains.Annotations;

namespace CardioMonitor.BLL.SessionProcessing.Pipelines
{
    public class Pipeline : 
        IPipeline,
        IDisposable
    {
        //todo reverse
        [NotNull] private readonly ICheckPointResolver _checkPointResolver;
        private readonly CycleTimeController _cycleTimeController;

        private readonly PipelineStartParams _startParams;

        private readonly BroadcastBlock<PipelineContext> _timeBroadcastBlock;
        private readonly ActionBlock<PipelineContext> _collectorBlock;
        
        /// <summary>
        /// Внутренние блоки Pipeline, которые будут между получением времени и агрегацией данных
        /// </summary>
        private readonly List<IPipelineElement> _pipelineInnerBlocks;

        public event EventHandler<TimeSpan> OnElapsedTimeChanged;
        
        public event EventHandler<double> OnCurrentAngleChanged;

        public Pipeline(
            [NotNull] PipelineStartParams startParams,
            [NotNull] IBedController bedController,
            [NotNull] ICheckPointResolver checkPointResolver)
        {
            if (bedController == null) throw new ArgumentNullException(nameof(bedController));
            _checkPointResolver = checkPointResolver ?? throw new ArgumentNullException(nameof(checkPointResolver));
            
            _startParams = startParams ?? throw new ArgumentNullException(nameof(startParams));

            _pipelineInnerBlocks = new List<IPipelineElement>();

            _timeBroadcastBlock = new BroadcastBlock<PipelineContext>(context => context);
            _collectorBlock = new ActionBlock<PipelineContext>(CollectDataFromPipeline);



            var angleReciever = new AngleReciever(bedController);
            _pipelineInnerBlocks.Add(angleReciever);
            var anlgeRecieveBlock =
                new TransformBlock<PipelineContext, PipelineContext>(context => angleReciever.ProcessAsync(context));

            var checkPointChecker = new CheckPointChecker(checkPointResolver);
            var checkPointCheckBlock =
                new TransformBlock<PipelineContext, PipelineContext>(context =>
                    checkPointChecker.ProcessAsync(context));

            var mainBroadcastBlock = new BroadcastBlock<PipelineContext>(context => context);

            _timeBroadcastBlock.LinkTo(
                _collectorBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });


            _timeBroadcastBlock.LinkTo(
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
                _collectorBlock,
                new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });

            _cycleTimeController = new CycleTimeController(_timeBroadcastBlock);
        }

        private async Task CollectDataFromPipeline([NotNull] PipelineContext context)
        {
            await Task.Yield();
            var timeParams = context.TryGetTimeParams();
            if (timeParams != null)
            {
                OnElapsedTimeChanged?.Invoke(this, timeParams.ElapsedTime);
            }
            var angleParams = context.TryGetAngleParam();
            if (angleParams != null)
            {
                OnCurrentAngleChanged?.Invoke(this, angleParams.CurrentAngle);
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
            }
        }

        public async Task EmergencyStopAsync()
        {
            await Task.Yield();
            //throw new System.NotImplementedException();
        }

        public async Task PauseAsync()
        {
            await Task.Yield();
            _cycleTimeController.Start();
        }

        public async Task ResetAsync()
        {
            await Task.Yield();
            _cycleTimeController.Init(_startParams.CycleDuration, _startParams.CycleTickDuration);
        }

        public void Dispose()
        {
            _timeBroadcastBlock.Complete();
            _collectorBlock.Completion.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}