using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ton618.Utilities.PE
{
    public partial class EnvBlock
    {
        // https://docs.microsoft.com/en-us/windows/win32/api/winternl/ns-winternl-teb
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct _TEB
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public IntPtr[] Reserved1;
            public IntPtr ProcessEnvironmentBlock;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 399)]
            public IntPtr[] Reserved2;
            public fixed byte Reserved3[1952];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public IntPtr[] TlsSlots;
            public fixed byte Reserved4[8];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public IntPtr[] Reserved5;
            public IntPtr ReservedForOle;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public IntPtr[] Reserved6;
            public IntPtr TlsExpansionSlots;

            public static _TEB Create(IntPtr tebAddress)
            {
                var teb = (_TEB)Marshal.PtrToStructure(tebAddress, typeof(_TEB));
                return teb;
            }
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/winternl/ns-winternl-peb
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct _PEB
        {
            public fixed byte Reserved1[2];
            public byte BeingDebugged;
            public fixed byte Reserved2[1];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public IntPtr[] Reserved3;
            public IntPtr Ldr;
            public IntPtr ProcessParameters;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public IntPtr[] Reserved4;
            public IntPtr AtlThunkSListPtr;
            public IntPtr Reserved5;
            public uint Reserved6;
            public IntPtr Reserved7;
            public uint Reserved8;
            public uint AtlThunkSListPtr32;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 45)]
            public IntPtr[] Reserved9;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
            public byte[] Reserved10;
            public IntPtr PostProcessInitRoutine;
            public fixed byte Reserved11[128];
            public IntPtr Reserved12;
            public uint SessionId;

            public static _PEB Create(IntPtr pebAddress)
            {
                var peb = (_PEB)Marshal.PtrToStructure(pebAddress, typeof(_PEB));
                return peb;
            }

            //public static IEnumerable<IntPtr> EnumerateHeaps(IntPtr pebAddress)
            //{
            //    var pebOffset = DbgOffset.Get("_PEB");
            //
            //    var processHeapsPtr = pebOffset.GetPointer(pebAddress, "ProcessHeaps").ReadPtr();
            //    if (processHeapsPtr == IntPtr.Zero)
            //        yield break;
            //
            //    if (pebOffset.TryRead<int>(pebAddress, "NumberOfHeaps", out var numberOfHeaps) == false)
            //        yield break;
            //
            //    for (var i = 0; i < numberOfHeaps; i++)
            //    {
            //        var entryPtr = processHeapsPtr + IntPtr.Size * i;
            //        yield return entryPtr.ReadPtr();
            //    }
            //}
        }

        public class DllOrderLink
        {
            public IntPtr LoadOrderLink;
            public IntPtr MemoryOrderLink;
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/winternl/ns-winternl-peb_ldr_data
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct _PEB_LDR_DATA
        {
            public fixed byte Reserved1[8];
            public IntPtr Reserved2;
            public _LIST_ENTRY InLoadOrderModuleList;
            public _LIST_ENTRY InMemoryOrderModuleList;

            public static _PEB_LDR_DATA Create(IntPtr ldrAddress)
            {
                var ldrData = (_PEB_LDR_DATA)Marshal.PtrToStructure(ldrAddress, typeof(_PEB_LDR_DATA));
                return ldrData;
            }

            public IEnumerable<_LDR_DATA_TABLE_ENTRY> EnumerateLoadOrderModules()
            {
                var startLink = InLoadOrderModuleList.Flink;
                var item = _LDR_DATA_TABLE_ENTRY.CreateFromLoadOrder(startLink);

                while (true)
                {
                    if (item.DllBase != IntPtr.Zero)
                        yield return item;

                    if (item.InLoadOrderLinks.Flink == startLink)
                        break;

                    item = _LDR_DATA_TABLE_ENTRY.CreateFromLoadOrder(item.InLoadOrderLinks.Flink);
                }
            }

            public IEnumerable<_LDR_DATA_TABLE_ENTRY> EnumerateMemoryOrderModules()
            {
                var startLink = InMemoryOrderModuleList.Flink;
                var item = _LDR_DATA_TABLE_ENTRY.CreateFromMemoryOrder(startLink);

                while (true)
                {
                    if (item.DllBase != IntPtr.Zero)
                        yield return item;

                    if (item.InMemoryOrderLinks.Flink == startLink)
                        break;

                    item = _LDR_DATA_TABLE_ENTRY.CreateFromMemoryOrder(item.InMemoryOrderLinks.Flink);
                }
            }

            public _LDR_DATA_TABLE_ENTRY Find(string dllFileName)
            {
                return Find(dllFileName, true);
            }

            public _LDR_DATA_TABLE_ENTRY Find(string dllFileName, bool memoryOrder)
            {
                foreach (var entry in
                    memoryOrder ? EnumerateMemoryOrderModules() : EnumerateLoadOrderModules())
                {
                    if (entry.FullDllName.GetText().EndsWith(dllFileName, StringComparison.OrdinalIgnoreCase))
                        return entry;
                }

                return default;
            }

            public unsafe void UnhideDLL(DllOrderLink hiddenModuleLink)
            {
                var dllLink = EnumerateMemoryOrderModules().First();

                dllLink.InMemoryOrderLinks.LinkTo(hiddenModuleLink.MemoryOrderLink);
                dllLink.InLoadOrderLinks.LinkTo(hiddenModuleLink.LoadOrderLink);
            }

            public unsafe DllOrderLink HideDLL(string fileName)
            {
                var dllLink = Find(fileName);

                if (dllLink.DllBase == IntPtr.Zero)
                    return null;

                var orderLink = new DllOrderLink()
                {
                    MemoryOrderLink = dllLink.InMemoryOrderLinks.Unlink(),
                    LoadOrderLink = dllLink.InLoadOrderLinks.Unlink(),
                };

                return orderLink;
            }
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/winternl/ns-winternl-peb_ldr_data
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct _LDR_DATA_TABLE_ENTRY
        {
            public _LIST_ENTRY InLoadOrderLinks;
            public _LIST_ENTRY InMemoryOrderLinks;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public IntPtr[] Reserved2;
            public IntPtr DllBase;
            public IntPtr EntryPoint;
            public IntPtr SizeOfImage;
            public _UNICODE_STRING FullDllName;
            public fixed byte Reserved4[8];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public IntPtr[] Reserved5;

            /*
            union {
                ULONG CheckSum;
                IntPtr Reserved6;
            };
            */
            public IntPtr Reserved6;

            public uint TimeDateStamp;

            public static _LDR_DATA_TABLE_ENTRY CreateFromMemoryOrder(IntPtr memoryOrderLink)
            {
                var head = memoryOrderLink - Marshal.SizeOf(typeof(_LIST_ENTRY));

                var entry = (_LDR_DATA_TABLE_ENTRY)Marshal.PtrToStructure(
                    head, typeof(_LDR_DATA_TABLE_ENTRY));

                return entry;
            }

            public static _LDR_DATA_TABLE_ENTRY CreateFromLoadOrder(IntPtr loadOrderLink)
            {
                var head = loadOrderLink;

                var entry = (_LDR_DATA_TABLE_ENTRY)Marshal.PtrToStructure(
                    head, typeof(_LDR_DATA_TABLE_ENTRY));

                return entry;
            }
        }
    }
}
