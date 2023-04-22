using System;
using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        internal delegate NT_STATUS NtQuerySystemInformationProc(
            [In] SYSTEM_INFORMATION_CLASS SystemInformationClass,
            [In] IntPtr SystemInformation,
            [In] int SystemInformationLength,
            [Out] out int ReturnLength);
        internal static NT_STATUS NtQuerySystemInformation(
            [In] SYSTEM_INFORMATION_CLASS SystemInformationClass,
            [In] IntPtr SystemInformation,
            [In] int SystemInformationLength,
            [Out] out int ReturnLength) => Lookup<NtQuerySystemInformationProc>("ntdll.dll", "NtQuerySystemInformation")(SystemInformationClass, SystemInformation, SystemInformationLength, out ReturnLength);

        internal delegate NT_STATUS NtQueryInformationProcessProc(
            [In] IntPtr ProcessHandle,
            [In] PROCESS_INFORMATION_CLASS ProcessInformationClass,
            [In] IntPtr ProcessInformation,
            [In] int ProcessInformationLength,
            [Out] out int ReturnLength);
        internal static NT_STATUS NtQueryInformationProcess(
            [In] IntPtr ProcessHandle,
            [In] PROCESS_INFORMATION_CLASS ProcessInformationClass,
            [In] IntPtr ProcessInformation,
            [In] int ProcessInformationLength,
            [Out] out int ReturnLength) => Lookup<NtQueryInformationProcessProc>("ntdll.dll", "NtQueryInformationProcess")(ProcessHandle, ProcessInformationClass, ProcessInformation, ProcessInformationLength, out ReturnLength);

        internal delegate NT_STATUS NtQueryObjectProc(
            [In] IntPtr Handle,
            [In] OBJECT_INFORMATION_CLASS ObjectInformationClass,
            [In] IntPtr ObjectInformation,
            [In] int ObjectInformationLength,
            [Out] out int ReturnLength);
        internal static NT_STATUS NtQueryObject(
            [In] IntPtr Handle,
            [In] OBJECT_INFORMATION_CLASS ObjectInformationClass,
            [In] IntPtr ObjectInformation,
            [In] int ObjectInformationLength,
            [Out] out int ReturnLength) => Lookup<NtQueryObjectProc>("ntdll.dll", "NtQueryObject")(Handle, ObjectInformationClass, ObjectInformation, ObjectInformationLength, out ReturnLength);

        // Only available on Windows Vista and Windows 7
        internal delegate uint NtCreateThreadExProc(
            ref IntPtr lpThreadHandle,
            uint flags,
            IntPtr reserved1,
            IntPtr hProcess,
            IntPtr lpStartAddress,
            IntPtr lpParameterAddress,
            bool reserved2,
            uint reserved3,
            uint reserved4,
            uint reserved5,
            IntPtr reserved6);
        internal static uint NtCreateThreadEx(
            ref IntPtr lpThreadHandle,
            uint flags,
            IntPtr reserved1,
            IntPtr hProcess,
            IntPtr lpStartAddress,
            IntPtr lpParameterAddress,
            bool reserved2,
            uint reserved3,
            uint reserved4,
            uint reserved5,
            IntPtr reserved6) => Lookup<NtCreateThreadExProc>("ntdll.dll", "NtCreateThreadEx")(ref lpThreadHandle, flags, reserved1, hProcess, lpStartAddress, lpParameterAddress, reserved2, reserved3, reserved4, reserved5, reserved6);
    }
}
