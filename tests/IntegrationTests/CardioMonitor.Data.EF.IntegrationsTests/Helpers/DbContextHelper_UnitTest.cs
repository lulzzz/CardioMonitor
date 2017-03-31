using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Security.X509.Extensions;

namespace CardioMonitor.Dal.Ef.UnitTests.Helpers
{
    [TestClass]
    public class DbContextHelper_UnitTest
    {
        [TestMethod]
        public void CreateContext_Test()
        {
            var context = DbContextHelper.GetInitializeContext();
            var a = context.Patients.FirstOrDefault();
        }
    }
}
