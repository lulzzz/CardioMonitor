using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Scout.Utils.Logging.Serilog;
using Serilog;
using Serilog.Events;

namespace Scout.Utils.Logging.Seriglog.Tests
{
    [TestClass]
    public class SeriLoggerTest
    {
        [TestMethod]
        public void LogText_WithCorrelationId_Test()
        {
            LogEvent logEvent = null;
            
            var testGuid = Guid.NewGuid().ToString();
            const string loggerName = "TestLogger";
            
            var configurator = new Mock<ISerilogConfigurator>();
            configurator.Setup(c => c.ConfigPath).Returns(String.Empty);
            configurator.Setup(c => c.ConfigureSerilog(It.IsAny<LoggerConfiguration>())).Callback((LoggerConfiguration c) =>
            {
                c.WriteTo.Sink(new FakeSink(e => logEvent = e));
            });
            
            var loggerFactory = new SeriloggerFactory(configurator.Object);
            var logger = loggerFactory.CreateLogger(loggerName, testGuid);

            var testMessage = "Test message";
            logger.Info(testMessage);
            
            Assert.IsNotNull(logEvent);
            Assert.IsTrue(logEvent.Properties.ContainsKey("CorrelationId"));
            //Так нужно, т.к. "Serilog renders string values in double quotes to more transparently indicate the underlying data type"
            Assert.AreEqual($"\"{testGuid}\"", logEvent.Properties["CorrelationId"].ToString());
            Assert.IsTrue(logEvent.Properties.ContainsKey("Name"));
            Assert.AreEqual($"\"{loggerName}\"", logEvent.Properties["Name"].ToString());
            Assert.AreEqual(testMessage, logEvent.MessageTemplate.Text);
        }
        
        [TestMethod]
        public void LogText_NoCorrelationId_Test()
        {
            LogEvent logEvent = null;
            
            string loggerName = "TestLogger";
            var testMessage = "Test message";
            
            var configurator = new Mock<ISerilogConfigurator>();
            configurator.Setup(c => c.ConfigPath).Returns(String.Empty);
            configurator.Setup(c => c.ConfigureSerilog(It.IsAny<LoggerConfiguration>())).Callback((LoggerConfiguration c) =>
            {
                c.WriteTo.Sink(new FakeSink(e => logEvent = e));
            });
            
            var loggerFactory = new SeriloggerFactory(configurator.Object);
            var logger = loggerFactory.CreateLogger(loggerName);
            
            logger.Info(testMessage);

            Assert.IsNotNull(logEvent);
            Assert.IsFalse(logEvent.Properties.ContainsKey("CorrelationId"));
            Assert.IsTrue(logEvent.Properties.ContainsKey("Name"));
            //Так нужно, т.к. "Serilog renders string values in double quotes to more transparently indicate the underlying data type"
            Assert.AreEqual($"\"{loggerName}\"", logEvent.Properties["Name"].ToString());
            Assert.AreEqual(testMessage, logEvent.MessageTemplate.Text);
        }
        
        [TestMethod]
        public void LogObject_WithCorrelationId_Test()
        {
            LogEvent logEvent = null;
            
            var testGuid = Guid.NewGuid().ToString();
            var loggerName = "TestLogger";
            var testMessageTemplate = "template {@message}";
            var testMessageObject = new {Message = "test", Id = 1};
            
            var configurator = new Mock<ISerilogConfigurator>();
            configurator.Setup(c => c.ConfigPath).Returns(String.Empty);
            configurator.Setup(c => c.ConfigureSerilog(It.IsAny<LoggerConfiguration>())).Callback((LoggerConfiguration c) =>
            {
                c.WriteTo.Sink(new FakeSink(e => logEvent = e));
            });
            
            var loggerFactory = new SeriloggerFactory(configurator.Object);
            var logger = loggerFactory.CreateLogger(loggerName, testGuid);
           
            logger.Info(testMessageTemplate, testMessageObject);
            
            Assert.IsNotNull(logEvent);
            Assert.IsTrue(logEvent.Properties.ContainsKey("CorrelationId"));
            //Так нужно, т.к. "Serilog renders string values in double quotes to more transparently indicate the underlying data type"
            Assert.AreEqual($"\"{testGuid}\"", logEvent.Properties["CorrelationId"].ToString());
            Assert.IsTrue(logEvent.Properties.ContainsKey("Name"));
            Assert.AreEqual($"\"{loggerName}\"", logEvent.Properties["Name"].ToString());
            Assert.AreEqual(testMessageTemplate, logEvent.MessageTemplate.Text);
            Assert.IsTrue(logEvent.Properties.ContainsKey("message"));
            Assert.IsNotNull(logEvent.Properties["message"]);
        }
    }
}