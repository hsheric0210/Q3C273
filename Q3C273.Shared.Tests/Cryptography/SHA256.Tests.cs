using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q3C273.Shared.Cryptography;
using Q3C273.Shared.Helpers;

namespace Q3C273.Shared.Tests.Cryptography
{
    [TestClass]
    public class Sha256Tests
    {
        [TestMethod, TestCategory("Cryptography")]
        public void ComputeHashTest()
        {
            var input = StringHelper.GetRandomString(100);
            var result = Sha256.ComputeHash(input);

            Assert.IsNotNull(result);
            Assert.AreNotEqual(result, input);
        }
    }
}
