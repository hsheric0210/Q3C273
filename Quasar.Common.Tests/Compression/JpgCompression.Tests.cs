using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q3C273.Shared.Video.Compression;
using System;
using System.Drawing;

namespace Q3C273.Shared.Tests.Compression
{
    [TestClass]
    public class JpgCompressionTests
    {
        [TestMethod, TestCategory("Compression")]
        public void CompressionTest()
        {
            var quality = long.MaxValue;
            var jpg = new JpgCompression(quality);
            var bitmap = new Bitmap(200, 200);

            var result = jpg.Compress(bitmap);

            Assert.IsNotNull(result);
            CollectionAssert.AllItemsAreNotNull(result);
        }
    }
}
