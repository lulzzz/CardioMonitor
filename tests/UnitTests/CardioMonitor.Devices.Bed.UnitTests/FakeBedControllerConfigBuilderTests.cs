using Xunit;

namespace CardioMonitor.Devices.Bed.UnitTests
{
    public class FakeBedControllerConfigBuilderTests
    {
        [Fact]
        public void Build_JsonWithoutAllParams_Ok()
        {
            //todo fix later
            /*
            var updateDataPeriodText = "01:02:03";
            var timeoutText = "01:02:03";
            var timeout = TimeSpan.Parse(timeoutText);
            var updatePeriod = TimeSpan.Parse(updateDataPeriodText);
            var jsonConfig = "{	\"UpdateDataPeriod\": " +
                             $"\"{updateDataPeriodText}\",	" +
                             "\"Timeout\": " +
                             $"\"{timeoutText}" +
                             "\"}";

            var maxAngle = 1f;
            short cyclesCount = 2;
            var movementFrequency = 3f;

            var builder = new FakeBedControllerConfigBuilder();

            var config = builder.Build(maxAngle, cyclesCount, ;movementFrequency, jsonConfig) as FakeBedControllerConfig;

            Assert.Equal(maxAngle, config.MaxAngleX);
            Assert.Equal(cyclesCount, config.CyclesCount);
            Assert.Equal(movementFrequency, config.MovementFrequency);
            Assert.Equal(timeout, config.Timeout);
            Assert.Equal(updatePeriod, config.UpdateDataPeriod);*/
        }

    }
}
