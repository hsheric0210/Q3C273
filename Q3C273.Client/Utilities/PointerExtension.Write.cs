using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    public static partial class PointerExtension
    {
        public unsafe static void WriteBytes(this UnmanagedMemoryStream stream, byte[] buffer) => stream.Write(buffer, 0, buffer.Length);
        public unsafe static void WriteObject(this UnmanagedMemoryStream stream, object obj)
        {
            Marshal.StructureToPtr(obj, new IntPtr(stream.PositionPointer), false);
            stream.Position += Marshal.SizeOf(obj);
        }

        public static unsafe void WriteBytes(this IntPtr ptr, byte[] buf) => new UnmanagedMemoryStream((byte*)ptr.ToPointer(), buf.Length, buf.Length, FileAccess.Write).Write(buf, 0, buf.Length);

        public static void WriteByte(this IntPtr ptr, int offset, byte value) => Marshal.WriteByte(ptr, offset, value);

        public unsafe static void WriteValue<T>(this IntPtr ptr, T value) where T : unmanaged
        {
            var pValue = (T*)ptr.ToPointer();
            *pValue = value;
        }

        public static void WriteInt64(this IntPtr ptr, long value) => Marshal.WriteInt64(ptr, value);

        public static void WriteInt32(this IntPtr ptr, int value) => Marshal.WriteInt32(ptr, value);
    }
}
