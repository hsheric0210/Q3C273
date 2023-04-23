using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    public static partial class PointerExtension
    {
        public static ulong ToUInt64(this IntPtr ptr) => (ulong)ptr.ToInt64();

        public static uint ToUInt32(this IntPtr ptr) => (uint)ptr.ToInt32();

        public static IntPtr Add(this IntPtr address, IntPtr offset) => new IntPtr(address.ToInt64() + offset.ToInt64());

        public static IntPtr Add(this IntPtr address, uint offset) => new IntPtr(address.ToInt64() + offset);
    }
}
