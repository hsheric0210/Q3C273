using System;

namespace Q3C273.Shared.Utilities
{
    /// <summary>
    /// Original source available at: https://catfrom.top/140
    /// </summary>
    public static partial class Qase64
    {
        private static byte LookupByte(char c)
        {
            if (c == table[64])
                return 0;

            for (var x = 0; x < 64; x++)
            {
                if (table[x] == c)
                    return (byte)x;
            }

            return 0;
        }

        public static byte[] Decode(string source)
        {
            var length = source.Length;

            //find how many padding are there
            var paddingCount = 0;
            for (var x = 0; x < 2; x++)
            {
                if (source[length - x - 1] == table[64])
                    paddingCount++;
            }

            //calculate the blockCount; assuming all whitespace and carriage returns/newline were removed.
            var blockCount = length / 4;
            var length2 = blockCount * 3;
            var buffer = new byte[length];//first conversion result
            var buffer2 = new byte[length2];//decoded array with padding

            for (var x = 0; x < length; x++)
                buffer[x] = LookupByte(source[x]);

            byte b, b1, b2, b3;
            byte temp1, temp2, temp3, temp4;

            for (var x = 0; x < blockCount; x++)
            {
                temp1 = buffer[x * 4];
                temp2 = buffer[x * 4 + 1];
                temp3 = buffer[x * 4 + 2];
                temp4 = buffer[x * 4 + 3];

                b = (byte)(temp1 << 2);
                b1 = (byte)((temp2 & 48) >> 4);
                b1 += b;

                b = (byte)((temp2 & 15) << 4);
                b2 = (byte)((temp3 & 60) >> 2);
                b2 += b;

                b = (byte)((temp3 & 3) << 6);
                b3 = temp4;
                b3 += b;

                buffer2[x * 3] = b1;
                buffer2[x * 3 + 1] = b2;
                buffer2[x * 3 + 2] = b3;
            }

            //remove paddings
            var length3 = length2 - paddingCount;
            var result = new byte[length3];

            for (var x = 0; x < length3; x++)
                result[x] = buffer2[x];

            return result;
        }
    }
}
