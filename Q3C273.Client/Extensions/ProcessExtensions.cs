using Ton618.Utilities;
using System.Diagnostics;
using System.Text;

namespace Ton618.Extensions
{
    public static class ProcessExtensions
    {
        public static string GetMainModuleFileName(this Process proc)
        {
            uint nChars = 260;
            var buffer = new StringBuilder((int)nChars);

            var success = ClientNatives.QueryFullProcessImageName(proc.Handle, 0, buffer, ref nChars);

            return success ? buffer.ToString() : null;
        }
    }
}
