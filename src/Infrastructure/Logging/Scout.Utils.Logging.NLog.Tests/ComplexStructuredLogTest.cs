using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Scout.Utils.Logging.NLog.Tests
{
    /// <summary>
    /// Проверяет структурного логирование комплексных типов данных
    /// </summary>
    /// <remarks>
    /// Никаких авто проверок нет, сделано для просмотра лога
    /// </remarks>
    [TestClass]
    public class ComplexStructuredLogTest
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
            _logger.Info("One complex arg: {@arg}", new OneComplexArg { Name = name});
        }

        [TestMethod]
        public void TwoSimpleArg_Test()
        {
            var name = "Max";
            _logger.Info("Simple and complex args from {Name}: {@complexArg}", name, new TwoComplexArg
            {
                DateTime = DateTime.UtcNow,
                Name = name
            });
        }
        
        public class OneComplexArg
        {
            public string Name { get; set; }
        }

        public class TwoComplexArg
        {
            public string Name { get; set; }

            public DateTime DateTime { get; set; }
        }
    }
}
