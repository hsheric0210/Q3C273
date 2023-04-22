using System;
using System.Runtime.InteropServices;
using Ton618.Utilities.PE;

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
    }
}
