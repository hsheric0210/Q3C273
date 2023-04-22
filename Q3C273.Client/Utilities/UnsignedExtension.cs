using System;
using Ton618.Utilities.PE;

namespace Ton618.Utilities
{
    internal static class UnsignedExtension
    {
        // uint
        public static uint uplus(this int a, uint b) => (uint)a + b;
        public static uint uplus(this uint a, int b) => a + (uint)b;
        public static uint uminus(this int a, uint b) => (uint)a - b;
        public static uint uminus(this uint a, int b) => a - (uint)b;

        // ulong
        public static ulong uplus(this long a, ulong b) => (ulong)a + b;
        public static ulong uplus(this ulong a, long b) => a + (ulong)b;
        public static ulong uminus(this long a, ulong b) => (ulong)a - b;
        public static ulong uminus(this ulong a, long b) => a - (ulong)b;
    }

    internal static class IntPtrExtension
    {
        public static IntPtr uplusptr(this IntPtr a, uint b) => (IntPtr)(a.ToUInt64() + b);
        public static IntPtr uminusptr(this IntPtr a, uint b) => (IntPtr)(a.ToUInt64() - b);

        public static UIntPtr uplusptr(this UIntPtr a, uint b) => (UIntPtr)(a.ToUInt64() + b);
        public static UIntPtr uminusptr(this UIntPtr a, uint b) => (UIntPtr)(a.ToUInt64() - b);

        public static IntPtr uplusptr(this IntPtr a, ulong b) => (IntPtr)(a.ToUInt64() + b);
        public static IntPtr uminusptr(this IntPtr a, ulong b) => (IntPtr)(a.ToUInt64() - b);

        public static UIntPtr uplusptr(this UIntPtr a, ulong b) => (UIntPtr)(a.ToUInt64() + b);
        public static UIntPtr uminusptr(this UIntPtr a, ulong b) => (UIntPtr)(a.ToUInt64() - b);

        public static IntPtr uplusptr(this IntPtr a, IntPtr b) => (IntPtr)(a.ToUInt64() + b.ToUInt64());
        public static IntPtr uminusptr(this IntPtr a, IntPtr b) => (IntPtr)(a.ToUInt64() - b.ToUInt64());

        public static UIntPtr uplusptr(this UIntPtr a, IntPtr b) => (UIntPtr)(a.ToUInt64() + b.ToUInt64());
        public static UIntPtr uminusptr(this UIntPtr a, IntPtr b) => (UIntPtr)(a.ToUInt64() - b.ToUInt64());

        public static IntPtr uplusptr(this IntPtr a, UIntPtr b) => (IntPtr)(a.ToUInt64() + b.ToUInt64());
        public static IntPtr uminusptr(this IntPtr a, UIntPtr b) => (IntPtr)(a.ToUInt64() - b.ToUInt64());

        public static UIntPtr uplusptr(this UIntPtr a, UIntPtr b) => (UIntPtr)(a.ToUInt64() + b.ToUInt64());
        public static UIntPtr uminusptr(this UIntPtr a, UIntPtr b) => (UIntPtr)(a.ToUInt64() - b.ToUInt64());
    }
}
