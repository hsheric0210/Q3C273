using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q3C273.Shared.Cryptography;
using Q3C273.Shared.Helpers;
using Q3C273.Shared.Utilities;
using System.Text;
using System;

namespace Q3C273.Shared.Tests.Cryptography
{
    [TestClass]
    public class Qase64Tests
    {
        [TestMethod, TestCategory("Helpers")]
        public void Qase64EncodingAndDecoding()
        {
            for (var i = 0; i < 10000; i++)
            {
                var input = StringHelper.GetRandomString(1000);
                var inputByte = Encoding.UTF8.GetBytes(input);
                var encoded = Qase64.Encode(inputByte);

                Assert.IsNotNull(encoded);

                var decoded = Encoding.UTF8.GetString(Qase64.Decode(encoded));

                Assert.AreEqual(input, decoded);
            }
        }
    }
}
