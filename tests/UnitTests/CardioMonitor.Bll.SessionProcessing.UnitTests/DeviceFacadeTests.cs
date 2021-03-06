using System;
using System.Threading.Tasks;
using CardioMonitor.BLL.SessionProcessing;
using CardioMonitor.BLL.SessionProcessing.DeviceFacade;
using CardioMonitor.Devices.Bed.Infrastructure;
using CardioMonitor.Devices.Monitor.Infrastructure;
using CardioMonitor.Infrastructure.Workers;
using Moq;
using Xunit;

namespace CardioMonitor.Bll.SessionProcessing.UnitTests
{
    public class DeviceFacadeTests
    {
        private readonly SessionParams _startParams;

        public DeviceFacadeTests()
        {
            var bedInitParams = new Mock<IBedControllerConfig>();
            bedInitParams.Setup(x => x.Timeout).Returns(TimeSpan.FromSeconds(30));
            var monitorInitParams = new Mock<IMonitorControllerConfig>();
            bedInitParams.Setup(x => x.Timeout).Returns(TimeSpan.FromSeconds(30));
            _startParams = 
                new SessionParams(
                    2,
                    TimeSpan.FromMilliseconds(500),
                    bedInitParams.Object,
                    monitorInitParams.Object,
                    3,
                    2);
        }
        
        [Fact]
        public async Task DeviceFacade_TimeChanged_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            var elapsedTimeRequestsCount = 0;
            bedController
                .Setup(x => x.GetElapsedTimeAsync())
                .Returns(async () =>
                {
                    elapsedTimeRequestsCount++;
                    await Task.Yield();
                    return TimeSpan.FromSeconds(1);
                });
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(10);
                });
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            TimeSpan? elapsedTime = null;
            TimeSpan? remainingTime = null;
            facade.OnElapsedTimeChanged += (sender, span) => elapsedTime = span;
            facade.OnRemainingTimeChanged += (sender, span) => remainingTime = span;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(1.5)).ConfigureAwait(false);
            await facade.EmergencyStopAsync().ConfigureAwait(false);
            
            Assert.True(elapsedTimeRequestsCount > 1);
            Assert.NotNull(elapsedTime);
            Assert.Equal(TimeSpan.FromSeconds(1), elapsedTime.Value);
            
            Assert.NotNull(remainingTime);
            Assert.Equal(TimeSpan.FromMinutes(10), remainingTime.Value);
        }
        
        [Fact]
        public async Task DeviceFacade_OneEventOneRise_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            bedController
                .SetupSequence(x => x.GetElapsedTimeAsync())
                .Returns(Task.FromResult(TimeSpan.FromSeconds(0)))
                .Returns(Task.FromResult(TimeSpan.FromSeconds(1)));
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(10);
                });
            bedController
                .Setup(x => x.GetCurrentCycleNumberAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return 1;
                });
            bedController
                .Setup(x => x.GetNextIterationNumberForCommonParamsMeasuringAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return 1;
                });
            bedController
                .Setup(x => x.GetNextIterationNumberForEcgMeasuringAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return 1;
                });
            bedController
                .Setup(x => x.GetNextIterationNumberForPressureMeasuringAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return 1;
                });
            bedController
                .Setup(x => x.GetCurrentIterationAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return 1;
                });
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var timeEventRiseCalls = 0;
            facade.OnElapsedTimeChanged += (sender, span) =>
            {
                timeEventRiseCalls++;
            };
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(1.5));
            await facade.EmergencyStopAsync();
            
            Assert.Equal(1, timeEventRiseCalls);
        }
        
        [Fact]
        public async Task DeviceFacade_SessionCompleted_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(0);
                });
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var isSessionCompleted = false;
            facade.OnSessionCompleted += (sender, span) => isSessionCompleted = true;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(3));
            await facade.EmergencyStopAsync();
            
            Assert.True(isSessionCompleted);
        }
        
        [Fact]
        public async Task DeviceFacade_CycleCompleted_OnTimeElapsedAsLastCycle_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(0);
                });
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var isCycleCompleted = false;
            facade.OnCycleCompleted += (sender, span) => isCycleCompleted = true;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(3));
            await facade.EmergencyStopAsync();
            
            Assert.True(isCycleCompleted);
        }
        
        [Fact]
        public async Task DeviceFacade_CycleCompleted_OnCycleNumbers_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(1);
                });
            bedController
                .SetupSequence(x => x.GetCurrentIterationAsync())
                .Returns(Task.FromResult((short) 1))
                .Returns(Task.FromResult((short) 2));
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var isCycleCompleted = false;
            facade.OnCycleCompleted += (sender, span) => isCycleCompleted = true;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(3));
            await facade.EmergencyStopAsync();
            
            Assert.True(isCycleCompleted);
        }
        
        [Fact]
        public async Task DeviceFacade_CollectCommonParams_OnDesiredIteration_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(1);
                });
            bedController
                .Setup(x => x.GetCurrentIterationAsync())
                .Returns(Task.FromResult((short) 1));
            
            bedController
                .Setup(x => x.GetNextIterationNumberForCommonParamsMeasuringAsync())
                .Returns(Task.FromResult((short) 1));
            
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var isPatientParamsRecieved = false;
            facade.OnCommonPatientParamsRecieved += (sender, span) => isPatientParamsRecieved = true;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(3));
            await facade.EmergencyStopAsync();
            
            Assert.True(isPatientParamsRecieved);
        }
        
        [Fact]
        public async Task DeviceFacade_CollectCommonParams_OnForcedRequest_Ok()
        {
            var remainingTime = TimeSpan.FromMinutes(1);
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return remainingTime;
                });
            bedController
                .Setup(x => x.GetCurrentIterationAsync())
                .Returns(Task.FromResult((short) 1));
            
            bedController
                .Setup(x => x.GetNextIterationNumberForPressureMeasuringAsync())
                .Returns(Task.FromResult((short) 1));
            
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var patientCommonCallsCount = 0;
            facade.OnCommonPatientParamsRecieved += (sender, span) => patientCommonCallsCount++;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            remainingTime = TimeSpan.Zero;
            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
            await facade.ForceDataCollectionRequestAsync().ConfigureAwait(false);
            
            Assert.Equal(3, patientCommonCallsCount);
        }
        
        [Fact]
        public async Task DeviceFacade_DontCollectCommonParams_OnNotDesiredIteration_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(1);
                });
            bedController
                .Setup(x => x.GetCurrentCycleNumberAsync())
                .Returns(Task.FromResult((short)1));
            bedController
                .Setup(x => x.GetCurrentIterationAsync())
                .Returns(Task.FromResult((short) 1));
            
            bedController
                .Setup(x => x.GetNextIterationNumberForCommonParamsMeasuringAsync())
                .Returns(Task.FromResult((short)2));
            
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var patientParamsRecievedCount = 0;
            facade.OnCommonPatientParamsRecieved += (sender, span) => patientParamsRecievedCount++;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(5));
            await facade.EmergencyStopAsync();
            
            // 1 запрос всегда при старте работает
            Assert.Equal(1, patientParamsRecievedCount);
        }
        
        [Fact]
        public async Task DeviceFacade_CollectPressureParams_OnDesiredIteration_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(1);
                });
            bedController
                .Setup(x => x.GetCurrentIterationAsync())
                .Returns(Task.FromResult((short) 1));
            
            bedController
                .Setup(x => x.GetNextIterationNumberForPressureMeasuringAsync())
                .Returns(Task.FromResult((short) 1));
            
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var isPatientPressureParamsRecieved = false;
            facade.OnPatientPressureParamsRecieved += (sender, span) => isPatientPressureParamsRecieved = true;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(3));
            await facade.EmergencyStopAsync();
            
            Assert.True(isPatientPressureParamsRecieved);
        }
        
        [Fact]
        public async Task DeviceFacade_CollectPressureParams_OnForcedRequest_Ok()
        {
            var remainingTime = TimeSpan.FromMinutes(1);
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return remainingTime;
                });
            bedController
                .Setup(x => x.GetCurrentIterationAsync())
                .Returns(Task.FromResult((short) 1));
            
            bedController
                .Setup(x => x.GetNextIterationNumberForPressureMeasuringAsync())
                .Returns(Task.FromResult((short) 1));
            
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var patientParamsCallsCount = 0;
            facade.OnPatientPressureParamsRecieved += (sender, span) => patientParamsCallsCount++;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            remainingTime = TimeSpan.Zero;
            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);
            await facade.ForceDataCollectionRequestAsync().ConfigureAwait(false);
            
            Assert.Equal(3, patientParamsCallsCount);
        }
        
        [Fact]
        public async Task DeviceFacade_DontCollectPressureParams_OnNotDesiredIteration_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(1);
                });
            bedController
                .Setup(x => x.GetCurrentCycleNumberAsync())
                .Returns(Task.FromResult((short)1));
            bedController
                .Setup(x => x.GetCurrentIterationAsync())
                .Returns(Task.FromResult((short) 1));
            
            bedController
                .Setup(x => x.GetNextIterationNumberForPressureMeasuringAsync())
                .Returns(Task.FromResult((short)2));
            
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            var patientPressureParamsRecievedCount = 0;
            facade.OnPatientPressureParamsRecieved += (sender, span) => patientPressureParamsRecievedCount++;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(5));
            await facade.EmergencyStopAsync();
            
            // 1 запрос всегда при старте работает
            Assert.Equal(1, patientPressureParamsRecievedCount);
        }
        
        [Fact]
        public async Task DeviceFacade_OnException_Ok()
        {
            var bedController =
                new Mock<IBedController>();
            bedController
                .Setup(x => x.GetRemainingTimeAsync())
                .Returns(async () =>
                {
                    await Task.Yield();
                    return TimeSpan.FromMinutes(1);
                });
            bedController
                .Setup(x => x.GetCurrentCycleNumberAsync())
                .Returns(Task.FromResult((short)1));
            bedController
                .Setup(x => x.GetCurrentIterationAsync())
                .Returns(Task.FromResult((short) 1));

            var risedException = new Exception();
            bedController
                .Setup(x => x.GetNextIterationNumberForPressureMeasuringAsync())
                .Throws(risedException);
            
            var monitorController =
                Mock.Of<IMonitorController>();
            
            var facade = new DevicesFacade(_startParams,
                bedController.Object,
                monitorController,
                new WorkerController());

            Exception handledException = null;
            facade.OnException += (sender, ex) => handledException = ex;
            await facade.StartAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(1.5));
            
            Assert.Equal(risedException, handledException.InnerException);
        }
    }
}