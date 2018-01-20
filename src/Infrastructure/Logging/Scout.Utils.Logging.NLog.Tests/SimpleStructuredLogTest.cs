using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Scout.Utils.Logging.NLog.Tests
{
    /// <summary>
    /// Проверяет структурного логирование простых типов данных
    /// </summary>
    /// <remarks>
    /// Никаких авто проверок нет, сделано для просмотра лога
    /// </remarks>
    [TestClass]
    public class SimpleStructuredLogTest
    {
        private static ILogger _logger;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            //CleanUp нельзя делать, нужно самому смотреть, что все ок.
            CleanUp();

              var factory = new NLoggerFactory("NLog.config");
            _logger = factory.CreateLogger("main");
        }

        private static void CleanUp()
        {
            try
            {
                var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "logs"));
                foreach (var file in di.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                foreach (var dir in di.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [TestMethod]
        public void OneSimpleArg_Test()
        {
            var name = "Max";
            _logger.Info("One simple arg from {Name}", name);
        }

        [TestMethod]
        public void TwoSimpleArg_Test()
        {
            var name = "Max";
            _logger.Info("Two simple args from {Name} at {Date}", name, DateTime.Now);
        }

        [TestMethod]
        public void ThreeSimpleArg_Test()
        {
            var name = "Max";
            _logger.Info("Three simple args from {Name} at {Date} per {count} test", name, DateTime.Now, 1);
        }

        [TestMethod]
        public void FourSimpleArg_Test()
        {
            var name = "Max";
            _logger.Info("Four simple args from {Name} at {Date} per {count} test with tolerance {tolerance}", name, DateTime.Now, 1, 0.5);
        }

        [TestMethod]
        public void SixSimpleArg_Test()
        {
            var name = "Max";
            _logger.Info("Six simple args from {Name} at {Date} per {count} test with tolerance {tolerance}, always {position} with mark {mark}", name, DateTime.Now, 1, 0.5, true, 'A');
        }
    }
}
