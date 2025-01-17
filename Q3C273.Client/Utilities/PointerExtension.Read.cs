﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    public static partial class PointerExtension
    {
        public static ushort PeekUInt16(this BinaryReader reader)
        {
            var oldPosition = reader.BaseStream.Position;
            var result = reader.ReadUInt16();
            reader.BaseStream.Position = oldPosition;

            return result;
        }

        public unsafe static T Read<T>(this BinaryReader reader) where T : new()
        {
            var obj = new T();
            var typeSize = Marshal.SizeOf(obj);

            var buffer = new byte[typeSize];
            reader.Read(buffer, 0, typeSize);

            fixed (byte* p = buffer)
            {
                var ptr = new IntPtr(p);
                var objSectionHeader = (T)Marshal.PtrToStructure(ptr, typeof(T));
                return objSectionHeader;
            }
        }

        public static ushort ReadUInt16(this UnmanagedMemoryStream reader)
        {
            var buf = new byte[4];
            reader.Read(buf, 0, 4);

            return BitConverter.ToUInt16(buf, 0);
        }

        public static int ReadInt32(this UnmanagedMemoryStream reader)
        {
            var buf = new byte[4];
            reader.Read(buf, 0, 4);

            return BitConverter.ToInt32(buf, 0);
        }

        public static uint ReadUInt32(this UnmanagedMemoryStream reader)
        {
            var buf = new byte[4];
            reader.Read(buf, 0, 4);

            return BitConverter.ToUInt32(buf, 0);
        }

        public static byte ReadByte(this IntPtr addresss, ref int offset)
        {
            var result = Marshal.ReadByte(addresss, offset);
            offset += 1;
            return result;
        }

        public static unsafe byte ReadByte(this IntPtr ptr, int position)
        {
            return Marshal.ReadByte(ptr, position);
        }

        public static unsafe byte[] ReadBytes(this IntPtr ptr, int length)
        {
            var ums = new UnmanagedMemoryStream((byte*)ptr.ToPointer(), length);

            var buf = new byte[length];
            ums.Read(buf, 0, length);

            return buf;
        }

        public static unsafe ushort ReadUInt16ByIndex(this IntPtr ptr, int index)
        {
            var ums = new UnmanagedMemoryStream((byte*)ptr.ToPointer(), (index + 1) * sizeof(ushort))
            {
                Position = index * sizeof(ushort),
            };

            return ums.ReadUInt16();
        }

        public static unsafe uint ReadUInt32ByIndex(this IntPtr ptr, int index)
        {
            var ums = new UnmanagedMemoryStream((byte*)ptr.ToPointer(), (index + 1) * sizeof(uint))
            {
                Position = index * sizeof(uint),
            };

            return ums.ReadUInt32();
        }

        public static unsafe IntPtr ReadPtr(this IntPtr addresss)
        {
            return addresss.ReadPtr(0);
        }

        public static IntPtr ReadPtr(this IntPtr ptr, int offset)
        {
            return Marshal.ReadIntPtr(ptr, offset);
        }

        public static unsafe IntPtr ReadPtr(this IntPtr addresss, ref int offset)
        {
            var target = addresss + offset;
            offset += IntPtr.Size;

            return Marshal.ReadIntPtr(target, 0);
        }

        public static uint ReadUInt32(this IntPtr ptr, int offset)
        {
            return (uint)Marshal.ReadInt32(ptr, offset);
        }

        public static ulong ReadUInt64(this IntPtr ptr)
        {
            return (ulong)Marshal.ReadInt64(ptr, 0);
        }

        public static ulong ReadUInt64(this IntPtr ptr, int offset)
        {
            return (ulong)Marshal.ReadInt64(ptr, offset);
        }

        public static long ReadInt64(this IntPtr ptr)
        {
            return Marshal.ReadInt64(ptr, 0);
        }

        public static int ReadInt32(this IntPtr ptr)
        {
            return Marshal.ReadInt32(ptr, 0);
        }

        public static uint ReadUInt32(this IntPtr addresss, ref int offset)
        {
            var result = (uint)Marshal.ReadInt32(addresss, offset);
            offset += 4;
            return result;
        }

        public static short ReadInt16(this IntPtr addresss, ref int offset)
        {
            var result = Marshal.ReadInt16(addresss, offset);
            offset += 2;
            return result;
        }

        public static ushort ReadUInt16(this IntPtr addresss, ref int offset)
        {
            var result = (ushort)Marshal.ReadInt16(addresss, offset);
            offset += 2;
            return result;
        }
    }
}
