using System;

namespace Q3C273.Shared.Utilities
{
    public static partial class Qase64
    {
        /// <summary>
        /// The lookup table of Qase64, a modified version of Base64. Feel free to change, but if you change you must update both the client and the server.
        /// Original Base64 alphabet is 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/='
        /// </summary>
        internal static readonly char[] table = "*Aac2b 1Q6gRofHZT;PEx<zO@FUd{KS`D$/5W_&3r7sjw^Y%+JV=\\8[|0M>-iCLhq".ToCharArray();
    }
}
