﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.Devices.Bed.Fake;
using Xunit;

namespace CardioMonitor.Devices.Bed.UnitTests
{
    public class FakeBedControllerConfigBuilderTests
    {
        [Fact]
        public void Build_JsonWithoutAllParams_Ok()
        {
            var updateDataPeriodText = "01:02:03";
            var timeoutText = "01:02:03";
            var timeout = TimeSpan.Parse(timeoutText);
            var updatePeriod = TimeSpan.Parse(updateDataPeriodText);
            var jsonConfig = "{	\"UpdateDataPeriod\": " +
                             $"\"{updateDataPeriodText}\",	" +
                             "\"Timeout\": " +
                             $"\"{timeoutText}" +
                             "\"}";

            var builder = new FakeBedControllerConfigBuilder();

            var config = builder.Build(0, 0, 0, jsonConfig);

            Assert.Equal(timeout, config.Timeout);
            Assert.Equal(updatePeriod, config.UpdateDataPeriod);
        }

    }
}
