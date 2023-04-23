using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    public class NativeMemoryException : ApplicationException
    {
        [Obsolete]
        public NativeMemoryException() : base()
        {
        }

        /// <summary>
        /// Allocation failure
        /// </summary>
        public NativeMemoryException(string memUseType) : base($"{memUseType} allocation failure: Win32 Error {Marshal.GetLastWin32Error()}")
        {
        }

        /// <summary>
        /// Read failure
        /// </summary>
        public NativeMemoryException(string memUseType, IntPtr address) : base($"{memUseType} read failure: 0x{address.ToString($"X{IntPtr.Size * 2}")}, Win32 Error {Marshal.GetLastWin32Error()}")
        {
        }

        /// <summary>
        /// Write failure
        /// </summary>
        public NativeMemoryException(string memUseType, IntPtr address, UIntPtr expectedSize, UIntPtr actualSize) : base($"{memUseType} write failure: 0x{address.ToString($"X{IntPtr.Size * 2}")}, {actualSize}/{expectedSize} bytes written, Win32 Error {Marshal.GetLastWin32Error()}")
        {
        }

        [Obsolete]
        public NativeMemoryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
