using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q3C273.Shared.Helpers;
using Q3C273.Shared.Cryptography;
using System.Text;

namespace Q3C273.Shared.Tests.Cryptography
{
    [TestClass]
    public class Aes128Tests
    {
        [TestMethod, TestCategory("Cryptography")]
        public void EncryptAndDecryptStringTest()
        {
            var input = StringHelper.GetRandomString(100);
            var password = StringHelper.GetRandomString(50);

            var aes = new Aes256(password);

            var encrypted = aes.Encrypt(input);

            Assert.IsNotNull(encrypted);
            Assert.AreNotEqual(encrypted, input);

            var decrypted = aes.Decrypt(encrypted);

            Assert.AreEqual(input, decrypted);
        }

        [TestMethod, TestCategory("Cryptography")]
        public void EncryptAndDecryptByteArrayTest()
        {
            var input = StringHelper.GetRandomString(100);
            var inputByte = Encoding.UTF8.GetBytes(input);
            var password = StringHelper.GetRandomString(50);

            var aes = new Aes256(password);

            var encryptedByte = aes.Encrypt(inputByte);

            Assert.IsNotNull(encryptedByte);
            CollectionAssert.AllItemsAreNotNull(encryptedByte);
            CollectionAssert.AreNotEqual(encryptedByte, inputByte);

            var decryptedByte = aes.Decrypt(encryptedByte);

            CollectionAssert.AreEqual(inputByte, decryptedByte);
        }
    }
}
