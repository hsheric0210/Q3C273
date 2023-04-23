using System;
using System.Runtime.InteropServices;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Utilities.PE
{
    /// <summary>
    /// Supported since Windows 8/2012
    /// </summary>
    public sealed class ProcessHandleInfo : IDisposable
    {
        private IntPtr ptr = IntPtr.Zero;
        private int handleCount = 0;
        private int handleOffset = 0;

        public int HandleCount => handleCount;

        public ProcessHandleInfo(int pid) => Initialize(pid);

        public void Dispose()
        {
            if (ptr == IntPtr.Zero)
                return;

            Marshal.FreeHGlobal(ptr);
            ptr = IntPtr.Zero;
        }

        public _PROCESS_HANDLE_TABLE_ENTRY_INFO this[int index]
        {
            get
            {
                if (ptr == IntPtr.Zero)
                    return default;

                unsafe
                {
                    /*
                    Span<_PROCESS_HANDLE_TABLE_ENTRY_INFO> handles = new Span<_PROCESS_HANDLE_TABLE_ENTRY_INFO>((_ptr + _handleOffset).ToPointer(), _handleCount);
                    return handles[index];
                    */

                    IntPtr entryPtr;

                    if (IntPtr.Size == 8)
                    {
                        var handleTable = new IntPtr(ptr.ToInt64() + handleOffset);
                        entryPtr = new IntPtr(handleTable.ToInt64() + sizeof(_PROCESS_HANDLE_TABLE_ENTRY_INFO) * index);
                    }
                    else
                    {
                        var handleTable = new IntPtr(ptr.ToInt32() + handleOffset);
                        entryPtr = new IntPtr(handleTable.ToInt32() + sizeof(_PROCESS_HANDLE_TABLE_ENTRY_INFO) * index);
                    }

                    var entry =
                        (_PROCESS_HANDLE_TABLE_ENTRY_INFO)Marshal.PtrToStructure(entryPtr, typeof(_PROCESS_HANDLE_TABLE_ENTRY_INFO));
                    return entry;
                }
            }
        }

        private void Initialize(int pid)
        {
            var guessSize = 4096;
            NT_STATUS ret;

            var _ptr = Marshal.AllocHGlobal(guessSize);
            var processHandle = OpenProcess(
                ProcessAccessRights.PROCESS_QUERY_INFORMATION | ProcessAccessRights.PROCESS_DUP_HANDLE, false, pid);
            if (processHandle == IntPtr.Zero)
                return;

            while (true)
            {
                ret = NtQueryInformationProcess(processHandle, PROCESS_INFORMATION_CLASS.ProcessHandleInformation, _ptr, guessSize, out var requiredSize);

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
                        typedef struct _PROCESS_HANDLE_SNAPSHOT_INFORMATION {
                            ULONG_PTR NumberOfHandles;
                            ULONG_PTR Reserved;
                            PROCESS_HANDLE_TABLE_ENTRY_INFO Handles[1];
                        } PROCESS_HANDLE_SNAPSHOT_INFORMATION, * PPROCESS_HANDLE_SNAPSHOT_INFORMATION;
                    */

                    handleCount = Marshal.ReadIntPtr(_ptr).ToInt32();

#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    var dummy = new _PROCESS_HANDLE_SNAPSHOT_INFORMATION();
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                    handleOffset = Marshal.OffsetOf(typeof(_PROCESS_HANDLE_SNAPSHOT_INFORMATION), nameof(dummy.Handles)).ToInt32();
                    ptr = _ptr;
                    break;
                }

                Marshal.FreeHGlobal(_ptr);
                break;
            }
        }
    }

}
