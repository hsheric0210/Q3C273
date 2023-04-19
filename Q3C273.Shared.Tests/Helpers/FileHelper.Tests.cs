using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q3C273.Shared.Helpers;

namespace Q3C273.Shared.Tests.Helpers
{
    [TestClass]
    public class FileHelperTests
    {
        [TestMethod, TestCategory("Helpers")]
        public void RandomFilenameTest()
        {
            var length = 100;
            var name = FileHelper.GetRandomFilename(length);

            Assert.IsNotNull(name);
            Assert.IsTrue(name.Length == length);
        }

        [TestMethod, TestCategory("Helpers")]
        public void ValidateExecutableTest()
        {
            var bytes = new byte[] { 77, 90 };

            Assert.IsTrue(FileHelper.HasExecutableIdentifier(bytes));
        }

        [TestMethod, TestCategory("Helpers")]
        public void ValidateExecutableTest2()
        {
            var bytes = new byte[] { 22, 93 };

            Assert.IsFalse(FileHelper.HasExecutableIdentifier(bytes));
        }
    }
}
