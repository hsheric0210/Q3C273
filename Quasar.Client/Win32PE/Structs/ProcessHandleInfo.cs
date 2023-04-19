﻿using Quasar.Client.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Everything.Win32PE.Structs
{
    /// <summary>
    /// Supported since Windows 8/2012
    /// </summary>
    public sealed class ProcessHandleInfo : IDisposable
    {
        IntPtr _ptr = IntPtr.Zero;
        int _handleCount = 0;
        int _handleOffset = 0;

        public int HandleCount
        {
            get { return _handleCount; }
        }

        public ProcessHandleInfo(int pid)
        {
            Initialize(pid);
        }

        public void Dispose()
        {
            if (_ptr == IntPtr.Zero)
                return;

            Marshal.FreeHGlobal(_ptr);
            _ptr = IntPtr.Zero;
        }

        public _PROCESS_HANDLE_TABLE_ENTRY_INFO this[int index]
        {
            get
            {
                if (_ptr == IntPtr.Zero)
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
                        var handleTable = new IntPtr(_ptr.ToInt64() + _handleOffset);
                        entryPtr = new IntPtr(handleTable.ToInt64() + sizeof(_PROCESS_HANDLE_TABLE_ENTRY_INFO) * index);
                    }
                    else
                    {
                        var handleTable = new IntPtr(_ptr.ToInt32() + _handleOffset);
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

            var ptr = Marshal.AllocHGlobal(guessSize);
            var processHandle = ClientNatives.OpenProcess(
                ProcessAccessRights.PROCESS_QUERY_INFORMATION | ProcessAccessRights.PROCESS_DUP_HANDLE, false, pid);
            if (processHandle == IntPtr.Zero)
                return;

            while (true)
            {
                ret = ClientNatives.NtQueryInformationProcess(processHandle, PROCESS_INFORMATION_CLASS.ProcessHandleInformation, ptr, guessSize, out var requiredSize);

                if (ret == NT_STATUS.STATUS_INFO_LENGTH_MISMATCH)
                {
                    Marshal.FreeHGlobal(ptr);
                    guessSize = requiredSize;
                    ptr = Marshal.AllocHGlobal(guessSize);
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

                    _handleCount = Marshal.ReadIntPtr(ptr).ToInt32();

#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    var dummy = new _PROCESS_HANDLE_SNAPSHOT_INFORMATION();
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                    _handleOffset = Marshal.OffsetOf(typeof(_PROCESS_HANDLE_SNAPSHOT_INFORMATION), nameof(dummy.Handles)).ToInt32();
                    _ptr = ptr;
                    break;
                }

                Marshal.FreeHGlobal(ptr);
                break;
            }
        }
    }

}
