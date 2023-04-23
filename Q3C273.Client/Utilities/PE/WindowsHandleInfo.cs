using System;
using System.Runtime.InteropServices;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Utilities.PE
{
    public class WindowsHandleInfo : IDisposable
    {
        private IntPtr ptr = IntPtr.Zero;
        private int handleCount = 0;
        private int handleOffset = 0;

        public int HandleCount => handleCount;

        public WindowsHandleInfo() => Initialize();

        public void Dispose()
        {
            if (ptr == IntPtr.Zero)
                return;

            Marshal.FreeHGlobal(ptr);
            ptr = IntPtr.Zero;
        }

        public _SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX this[int index]
        {
            get
            {
                if (ptr == IntPtr.Zero)
                    return default;

                unsafe
                {
                    /*

                    Span<_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX> handles = new Span<_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>((_ptr + _handleOffset).ToPointer(), _handleCount);
                    return handles[index];
                    */

                    IntPtr entryPtr;

                    if (IntPtr.Size == 8)
                    {
                        var handleTable = new IntPtr(ptr.ToInt64() + handleOffset);
                        entryPtr = new IntPtr(handleTable.ToInt64() + sizeof(_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX) * index);
                    }
                    else
                    {
                        var handleTable = new IntPtr(ptr.ToInt32() + handleOffset);
                        entryPtr = new IntPtr(handleTable.ToInt32() + sizeof(_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX) * index);
                    }

                    var entry =
                        (_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)Marshal.PtrToStructure(entryPtr, typeof(_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX));
                    return entry;
                }
            }
        }

        private void Initialize()
        {
            var guessSize = 4096;
            NT_STATUS ret;

            var _ptr = Marshal.AllocHGlobal(guessSize);

            while (true)
            {
                ret = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation, _ptr, guessSize, out var requiredSize);

                if (ret == NT_STATUS.STATUS_INFO_LENGTH_MISMATCH)
                {
                    Marshal.FreeHGlobal(_ptr);
                    guessSize = requiredSize;
                    _ptr = Marshal.AllocHGlobal(guessSize);
                    continue;
                }

                if (ret == NT_STATUS.STATUS_SUCCESS)
                {
                    /*
                    typedef struct _SYSTEM_HANDLE_INFORMATION
                    {
                        ULONG HandleCount;
                        _SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX Handles[1];
                    } SYSTEM_HANDLE_INFORMATION, *PSYSTEM_HANDLE_INFORMATION;
                    */

                    handleCount = Marshal.ReadIntPtr(_ptr).ToInt32();

                    var dummy = new _SYSTEM_HANDLE_INFORMATION_EX();
                    handleOffset = Marshal.OffsetOf(typeof(_SYSTEM_HANDLE_INFORMATION_EX), nameof(dummy.Handles)).ToInt32();
                    ptr = _ptr;
                    break;
                }

                Marshal.FreeHGlobal(_ptr);
                break;
            }
        }
    }

}
