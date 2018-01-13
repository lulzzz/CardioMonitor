using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing.CheckPoints;
using CardioMonitor.BLL.SessionProcessing.Pipelines;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Logs;
using CardioMonitor.Infrastructure.Threading;
using Moq;
using Xunit;

namespace CardioMonitor.Bll.SessionProcessing.UnitTests
{
    public class PipelineTests
    {
        private readonly PipelineStartParams _startParams = 
            new PipelineStartParams(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1));
        
        [Fact]
        public async Task TimeController_ElapsedTimeChanged_Ok()
        {
            var pipeline = new Pipeline(_startParams,
                Mock.Of<IBedController>(),
                Mock.Of<ICheckPointResolver>(),
                Mock.Of<IMonitorController>(),
                new TaskHelper(Mock.Of<ILogger>()));

            TimeSpan? elapsedTime = null;
            pipeline.OnElapsedTimeChanged += (sender, span) => elapsedTime = span;
            await pipeline.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(1.5));
            await pipeline.EmergencyStopAsync().ConfigureAwait(false);
            
            Assert.NotNull(elapsedTime);
            Assert.Equal(TimeSpan.FromSeconds(1), elapsedTime.Value);
        }
    }
}