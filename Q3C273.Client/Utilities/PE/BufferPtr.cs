using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities.PE
{
    public class BufferPtr : IDisposable
    {
        readonly byte[] _buffer;
        GCHandle _pinHandle;

        public byte[] Buffer
        {
            get { return _buffer; }
        }

        public int Length
        {
            get { return _buffer.Length; }
        }

        public BufferPtr(int size)
        {
            _buffer = new byte[size];
            _pinHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        }

        public IntPtr GetPtr(int offset)
        {
            var ptr = _pinHandle.AddrOfPinnedObject();
            return ptr + offset;
        }

        public void Dispose()
        {
            _pinHandle.Free();
        }
    }

    public static class BufferPtrExtension
    {
        public static void Clear(this BufferPtr buffer)
        {
            if (buffer == null)
                return;

            buffer.Dispose();
        }
    }
}
