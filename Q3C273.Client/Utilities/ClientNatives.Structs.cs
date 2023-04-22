using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    internal static partial class ClientNatives
    {
        public enum NT_STATUS
        {
            STATUS_SUCCESS = 0x00000000,
            STATUS_BUFFER_OVERFLOW = unchecked((int)0x80000005L),
            STATUS_INFO_LENGTH_MISMATCH = unchecked((int)0xC0000004L),
            STATUS_INVALID_HANDLE = unchecked((int)0xC0000008L),
            STATUS_INVALID_PARAMETER = unchecked((int)0xC000000DL),
            STATUS_ACCESS_DENIED = unchecked((int)0xC0000022L),
            STATUS_BUFFER_TOO_SMALL = unchecked((int)0xC0000023L),
            STATUS_NOT_SUPPORTED = unchecked((int)0xC00000BBL),
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct _UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;

            public string GetText()
            {
                if (Buffer == IntPtr.Zero || MaximumLength == 0)
                    return "";

                return Marshal.PtrToStringUni(Buffer, Length / 2);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct _LIST_ENTRY
        {
            public IntPtr Flink;
            public IntPtr Blink;

            internal unsafe IntPtr Unlink()
            {
                var pNext = (_LIST_ENTRY*)Flink.ToPointer();
                var pPrev = (_LIST_ENTRY*)Blink.ToPointer();

                var thisLink = pNext->Blink;

                pNext->Blink = Blink;
                pPrev->Flink = Flink;

                var thisItem = (_LIST_ENTRY*)thisLink.ToPointer();

                thisItem->Blink = IntPtr.Zero;
                thisItem->Flink = IntPtr.Zero;

                return thisLink;
            }

            internal unsafe void LinkTo(IntPtr hiddenModuleLink)
            {
                if (hiddenModuleLink == IntPtr.Zero)
                    return;

                var nextItem = (_LIST_ENTRY*)Flink.ToPointer();
                var thisItem = (_LIST_ENTRY*)nextItem->Blink.ToPointer();

                var newItem = (_LIST_ENTRY*)hiddenModuleLink.ToPointer();
                newItem->Flink = Flink;
                newItem->Blink = new IntPtr(thisItem);

                thisItem->Flink = hiddenModuleLink;
                nextItem->Blink = hiddenModuleLink;
            }
        }
    }
}
