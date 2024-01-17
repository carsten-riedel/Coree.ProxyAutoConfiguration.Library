using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Coree.ProxyAutoConfiguration.Library.MSTest
{
    [TestClass]
    public class FunctionalTests
    {

        [TestInitialize()]
        public void Startup()
        {

        }

        [TestMethod]
        public void TestFooMethod()
        {
            var result = new Coree.ProxyAutoConfiguration.Library.ProxyAutoConfiguration();

            //Assert.IsNotNull(result);
            //Assert.AreEqual(result, "123");
        }
    }
}