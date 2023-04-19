using Q3C273.Shared.Video.Compression;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Q3C273.Shared.Video.Codecs
{
    public class UnsafeStreamCodec : IDisposable
    {
        public int Monitor { get; private set; }
        public Resolution Resolution { get; private set; }
        public Size CheckBlock { get; private set; }
        public int ImageQuality
        {
            get { return _imageQuality; }
            private set
            {
                lock (_imageProcessLock)
                {
                    _imageQuality = value;

                    if (_jpgCompression != null)
                        _jpgCompression.Dispose();

                    _jpgCompression = new JpgCompression(_imageQuality);
                }
            }
        }

        private int _imageQuality;
        private byte[] _encodeBuffer;
        private Bitmap _decodedBitmap;
        private PixelFormat _encodedFormat;
        private int _encodedWidth;
        private int _encodedHeight;
        private readonly object _imageProcessLock = new object();
        private JpgCompression _jpgCompression;

        /// <summary>
        /// Initialize a new instance of UnsafeStreamCodec class.
        /// </summary>
        /// <param name="imageQuality">The quality to use between 0-100.</param>
        /// <param name="monitor">The monitor used for the images.</param>
        /// <param name="resolution">The resolution of the monitor.</param>
        public UnsafeStreamCodec(int imageQuality, int monitor, Resolution resolution)
        {
            ImageQuality = imageQuality;
            Monitor = monitor;
            Resolution = resolution;
            CheckBlock = new Size(50, 1);
        }

        public void Dispose()
        {
            Dispose(true);

            // Tell the Garbage Collector to not waste time finalizing this object
            // since we took care of it.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_decodedBitmap != null)
                    _decodedBitmap.Dispose();

                if (_jpgCompression != null)
                    _jpgCompression.Dispose();
            }
        }

        public unsafe void CodeImage(IntPtr scan0, Rectangle scanArea, Size imageSize, PixelFormat format,
            Stream outStream)
        {
            lock (_imageProcessLock)
            {
                byte* pScan0;
                if (IntPtr.Size == 8)
                    // 64 bit process
                    pScan0 = (byte*)scan0.ToInt64();
                else
                {
                    // 32 bit process
                    pScan0 = (byte*)scan0.ToInt32();
                }

                if (!outStream.CanWrite)
                    throw new Exception("Must have access to Write in the Stream");

                var stride = 0;
                var rawLength = 0;
                var pixelSize = 0;

                switch (format)
                {
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        pixelSize = 3;
                        break;
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                        pixelSize = 4;
                        break;
                    default:
                        throw new NotSupportedException(format.ToString());
                }

                stride = imageSize.Width * pixelSize;
                rawLength = stride * imageSize.Height;

                if (_encodeBuffer == null)
                {
                    _encodedFormat = format;
                    _encodedWidth = imageSize.Width;
                    _encodedHeight = imageSize.Height;
                    _encodeBuffer = new byte[rawLength];

                    fixed (byte* ptr = _encodeBuffer)
                    {
                        byte[] temp = null;
                        using (var tmpBmp = new Bitmap(imageSize.Width, imageSize.Height, stride, format, scan0))
                        {
                            temp = _jpgCompression.Compress(tmpBmp);
                        }

                        outStream.Write(BitConverter.GetBytes(temp.Length), 0, 4);
                        outStream.Write(temp, 0, temp.Length);
                        SharedNatives.memcpy(new IntPtr(ptr), scan0, (uint)rawLength);
                    }
                    return;
                }

                if (_encodedFormat != format)
                    throw new Exception("PixelFormat is not equal to previous Bitmap");
                else if (_encodedWidth != imageSize.Width || _encodedHeight != imageSize.Height)
                {
                    throw new Exception("Bitmap width/height are not equal to previous bitmap");
                }

                var oldPos = outStream.Position;
                outStream.Write(new byte[4], 0, 4);
                long totalDataLength = 0;

                var blocks = new List<Rectangle>();

                var s = new Size(scanArea.Width, CheckBlock.Height);
                var lastSize = new Size(scanArea.Width % CheckBlock.Width, scanArea.Height % CheckBlock.Height);

                var lasty = scanArea.Height - lastSize.Height;
                var lastx = scanArea.Width - lastSize.Width;

                var cBlock = new Rectangle();
                var finalUpdates = new List<Rectangle>();

                s = new Size(scanArea.Width, s.Height);

                fixed (byte* encBuffer = _encodeBuffer)
                {
                    var index = 0;

                    for (var y = scanArea.Y; y != scanArea.Height; y += s.Height)
                    {
                        if (y == lasty)
                            s = new Size(scanArea.Width, lastSize.Height);

                        cBlock = new Rectangle(scanArea.X, y, scanArea.Width, s.Height);

                        var offset = y * stride + scanArea.X * pixelSize;

                        if (SharedNatives.memcmp(encBuffer + offset, pScan0 + offset, (uint)stride) != 0)
                        {
                            index = blocks.Count - 1;

                            if (blocks.Count != 0 && blocks[index].Y + blocks[index].Height == cBlock.Y)
                            {
                                cBlock = new Rectangle(blocks[index].X, blocks[index].Y, blocks[index].Width,
                                    blocks[index].Height + cBlock.Height);
                                blocks[index] = cBlock;
                            }
                            else
                            {
                                blocks.Add(cBlock);
                            }
                        }
                    }

                    for (var i = 0; i < blocks.Count; i++)
                    {
                        s = new Size(CheckBlock.Width, blocks[i].Height);

                        for (var x = scanArea.X; x != scanArea.Width; x += s.Width)
                        {
                            if (x == lastx)
                                s = new Size(lastSize.Width, blocks[i].Height);

                            cBlock = new Rectangle(x, blocks[i].Y, s.Width, blocks[i].Height);
                            var foundChanges = false;
                            var blockStride = (uint)(pixelSize * cBlock.Width);

                            for (var j = 0; j < cBlock.Height; j++)
                            {
                                var blockOffset = stride * (cBlock.Y + j) + pixelSize * cBlock.X;

                                if (SharedNatives.memcmp(encBuffer + blockOffset, pScan0 + blockOffset, blockStride) != 0)
                                    foundChanges = true;

                                SharedNatives.memcpy(encBuffer + blockOffset, pScan0 + blockOffset, blockStride);
                                //copy-changes
                            }

                            if (foundChanges)
                            {
                                index = finalUpdates.Count - 1;

                                if (finalUpdates.Count > 0 &&
                                    finalUpdates[index].X + finalUpdates[index].Width == cBlock.X)
                                {
                                    var rect = finalUpdates[index];
                                    var newWidth = cBlock.Width + rect.Width;
                                    cBlock = new Rectangle(rect.X, rect.Y, newWidth, rect.Height);
                                    finalUpdates[index] = cBlock;
                                }
                                else
                                {
                                    finalUpdates.Add(cBlock);
                                }
                            }
                        }
                    }
                }

                for (var i = 0; i < finalUpdates.Count; i++)
                {
                    var rect = finalUpdates[i];
                    var blockStride = pixelSize * rect.Width;

                    Bitmap tmpBmp = null;
                    BitmapData tmpData = null;
                    long length;

                    try
                    {
                        tmpBmp = new Bitmap(rect.Width, rect.Height, format);
                        tmpData = tmpBmp.LockBits(new Rectangle(0, 0, tmpBmp.Width, tmpBmp.Height),
                            ImageLockMode.ReadWrite, tmpBmp.PixelFormat);

                        for (int j = 0, offset = 0; j < rect.Height; j++)
                        {
                            var blockOffset = stride * (rect.Y + j) + pixelSize * rect.X;
                            SharedNatives.memcpy((byte*)tmpData.Scan0.ToPointer() + offset, pScan0 + blockOffset, (uint)blockStride);
                            //copy-changes
                            offset += blockStride;
                        }

                        outStream.Write(BitConverter.GetBytes(rect.X), 0, 4);
                        outStream.Write(BitConverter.GetBytes(rect.Y), 0, 4);
                        outStream.Write(BitConverter.GetBytes(rect.Width), 0, 4);
                        outStream.Write(BitConverter.GetBytes(rect.Height), 0, 4);
                        outStream.Write(new byte[4], 0, 4);

                        length = outStream.Length;
                        var old = outStream.Position;

                        _jpgCompression.Compress(tmpBmp, ref outStream);

                        length = outStream.Position - length;

                        outStream.Position = old - 4;
                        outStream.Write(BitConverter.GetBytes(length), 0, 4);
                        outStream.Position += length;
                    }
                    finally
                    {
                        tmpBmp.UnlockBits(tmpData);
                        tmpBmp.Dispose();
                    }

                    totalDataLength += length + 4 * 5;
                }

                outStream.Position = oldPos;
                outStream.Write(BitConverter.GetBytes(totalDataLength), 0, 4);
            }
        }

        public unsafe Bitmap DecodeData(IntPtr codecBuffer, uint length)
        {
            if (length < 4)
                return _decodedBitmap;

            var dataSize = *(int*)codecBuffer;

            if (_decodedBitmap == null)
            {
                var temp = new byte[dataSize];

                fixed (byte* tempPtr = temp)
                {
                    SharedNatives.memcpy(new IntPtr(tempPtr), new IntPtr(codecBuffer.ToInt32() + 4), (uint)dataSize);
                }

                _decodedBitmap = (Bitmap)Image.FromStream(new MemoryStream(temp));

                return _decodedBitmap;
            }
            else
            {
                return _decodedBitmap;
            }
        }

        public Bitmap DecodeData(Stream inStream)
        {
            var temp = new byte[4];
            inStream.Read(temp, 0, 4);
            var dataSize = BitConverter.ToInt32(temp, 0);

            if (_decodedBitmap == null)
            {
                temp = new byte[dataSize];
                inStream.Read(temp, 0, temp.Length);
                _decodedBitmap = (Bitmap)Image.FromStream(new MemoryStream(temp));

                return _decodedBitmap;
            }

            using (var g = Graphics.FromImage(_decodedBitmap))
            {
                while (dataSize > 0)
                {
                    var tempData = new byte[4 * 5];
                    inStream.Read(tempData, 0, tempData.Length);

                    var rect = new Rectangle(BitConverter.ToInt32(tempData, 0), BitConverter.ToInt32(tempData, 4),
                        BitConverter.ToInt32(tempData, 8), BitConverter.ToInt32(tempData, 12));
                    var updateLen = BitConverter.ToInt32(tempData, 16);

                    var buffer = new byte[updateLen];
                    inStream.Read(buffer, 0, buffer.Length);

                    using (var m = new MemoryStream(buffer))
                    {
                        using (var tmp = (Bitmap)Image.FromStream(m))
                        {
                            g.DrawImage(tmp, rect.Location);
                        }
                    }

                    dataSize -= updateLen + 4 * 5;
                }
            }

            return _decodedBitmap;
        }
    }
}
