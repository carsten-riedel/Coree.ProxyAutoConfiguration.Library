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
            var result = Coree.ProxyAutoConfiguration.Library.Class1.Foo();

            Assert.IsNotNull(result);
            Assert.AreEqual(result, "123");
        }
    }
}