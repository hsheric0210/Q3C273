using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Q3C273.Shared.Video.Compression
{
    public class JpgCompression : IDisposable
    {
        private readonly ImageCodecInfo _encoderInfo;
        private readonly EncoderParameters _encoderParams;

        public JpgCompression(long quality)
        {
            var parameter = new EncoderParameter(Encoder.Quality, quality);
            _encoderInfo = GetEncoderInfo("image/jpeg");
            _encoderParams = new EncoderParameters(2);
            _encoderParams.Param[0] = parameter;
            _encoderParams.Param[1] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionRle);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_encoderParams != null)
                    _encoderParams.Dispose();
            }
        }

        public byte[] Compress(Bitmap bmp)
        {
            using (var stream = new MemoryStream())
            {
                bmp.Save(stream, _encoderInfo, _encoderParams);
                return stream.ToArray();
            }
        }

        public void Compress(Bitmap bmp, ref Stream targetStream)
        {
            bmp.Save(targetStream, _encoderInfo, _encoderParams);
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            var imageEncoders = ImageCodecInfo.GetImageEncoders();
            var num2 = imageEncoders.Length - 1;
            for (var i = 0; i <= num2; i++)
            {
                if (imageEncoders[i].MimeType == mimeType)
                    return imageEncoders[i];
            }
            return null;
        }
    }
}