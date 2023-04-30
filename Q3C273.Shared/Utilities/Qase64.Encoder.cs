using System;

namespace Q3C273.Shared.Utilities
{
    /// <summary>
    /// Original source available at: https://catfrom.top/140
    /// </summary>
    public static partial class Qase64
    {
        private static char LookupChar(byte b) => b > 63 ? throw new FormatException("Invalid index") : table[b];

        public static string Encode(byte[] input)
        {
            var inputLength = input.Length;
            int paddingCount, blockCount;
            if (inputLength % 3 == 0)
            {
                paddingCount = 0;
                blockCount = inputLength / 3;
            }
            else
            {
                paddingCount = 3 - inputLength % 3;//need to add padding
                blockCount = (inputLength + paddingCount) / 3;
            }

            var length2 = inputLength + paddingCount;//or blockCount *3
            var source2 = new byte[length2];
            //copy data over insert padding
            for (var x = 0; x < length2; x++)
                source2[x] = x < inputLength ? input[x] : (byte)0;

            byte b1, b2, b3;
            byte temp, temp1, temp2, temp3, temp4;
            var buffer = new byte[blockCount * 4];
            var result = new char[blockCount * 4];
            for (var x = 0; x < blockCount; x++)
            {
                b1 = source2[x * 3];
                b2 = source2[x * 3 + 1];
                b3 = source2[x * 3 + 2];

                temp1 = (byte)((b1 & 252) >> 2);//first

                temp = (byte)((b1 & 3) << 4);
                temp2 = (byte)((b2 & 240) >> 4);
                temp2 += temp; //second

                temp = (byte)((b2 & 15) << 2);
                temp3 = (byte)((b3 & 192) >> 6);
                temp3 += temp; //third

                temp4 = (byte)(b3 & 63); //fourth

                buffer[x * 4] = temp1;
                buffer[x * 4 + 1] = temp2;
                buffer[x * 4 + 2] = temp3;
                buffer[x * 4 + 3] = temp4;
            }

            for (var x = 0; x < blockCount * 4; x++)
                result[x] = LookupChar(buffer[x]);

            //covert last "A"s to "=", based on paddingCount
            switch (paddingCount)
            {
                case 0:
                    break;
                case 1:
                    result[blockCount * 4 - 1] = table[64];
                    break;
                case 2:
                    result[blockCount * 4 - 1] = table[64];
                    result[blockCount * 4 - 2] = table[64];
                    break;
                default:
                    break;
            }

            return new string(result);
        }
    }
}
