using System;
using System.IO;
using System.Runtime.InteropServices;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Utilities
{
    internal static class NativeClipboard
    {
        // Better than System.Windows.Forms.Clipboard :)
        public unsafe static string GetClipboardContent()
        {
            var data = "";
            do
            {
                if (!OpenClipboard(IntPtr.Zero))
                    break;

                var formatListSize = sizeof(StandardClipboardFormats) * 2;
                var formatList = Marshal.AllocHGlobal(formatListSize);
                var formatListStream = new UnmanagedMemoryStream((byte*)formatList.ToPointer(), formatListSize, formatListSize, FileAccess.ReadWrite);
                formatListStream.WriteObject(StandardClipboardFormats.CF_UNICODETEXT);
                formatListStream.WriteObject(StandardClipboardFormats.CF_TEXT);

                var availFormat = GetPriorityClipboardFormat(formatList, 2);
                if (availFormat == 0)
                    break; // None of supplied formats are available

                var clipHandle = GetClipboardData(availFormat);
                if (clipHandle == IntPtr.Zero)
                    break;

                var clipData = GlobalLock(clipHandle);
                data = (availFormat == (uint)StandardClipboardFormats.CF_UNICODETEXT) ? Marshal.PtrToStringUni(clipData) : Marshal.PtrToStringAnsi(clipData);
                GlobalUnlock(clipHandle);
            } while (false);

            CloseClipboard();
            return data;
        }
    }
}
