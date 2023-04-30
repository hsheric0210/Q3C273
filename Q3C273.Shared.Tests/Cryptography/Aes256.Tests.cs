using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q3C273.Shared.Helpers;
using Q3C273.Shared.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Linq;
using Q3C273.Shared.Utilities;
using System;

namespace Q3C273.Shared.Tests.Cryptography
{
    [TestClass]
    public class Aes128Tests
    {
        [TestMethod, TestCategory("Cryptography")]
        public void EncryptAndDecryptStringTest()
        {
            var password = StringHelper.GetRandomString(50);

            var aes = new Aes256(password);

            for (var i = 0; i < 1000; i++)
            {
                var input = StringHelper.GetRandomString(1000);
                var encrypted = aes.Encrypt(input);

                Assert.IsNotNull(encrypted);
                Assert.AreNotEqual(encrypted, input);

                var decrypted = aes.Decrypt(encrypted);

                Assert.AreEqual(input, decrypted);
            }
        }

        [TestMethod, TestCategory("Cryptography")]
        public void EncryptAndDecryptByteArrayTest()
        {
            var password = StringHelper.GetRandomString(50);

            var aes = new Aes256(password);

            for (var i = 0; i < 1000; i++)
            {
                var input = StringHelper.GetRandomString(1000);
                var inputByte = Encoding.UTF8.GetBytes(input);
                var encryptedByte = aes.Encrypt(inputByte);

                Assert.IsNotNull(encryptedByte);
                CollectionAssert.AllItemsAreNotNull(encryptedByte);
                CollectionAssert.AreNotEqual(encryptedByte, inputByte);

                var decryptedByte = aes.Decrypt(encryptedByte);

                CollectionAssert.AreEqual(inputByte, decryptedByte);
            }
        }
    }
}
