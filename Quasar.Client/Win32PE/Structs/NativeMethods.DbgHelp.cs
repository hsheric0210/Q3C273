using Microsoft.Win32.SafeHandles;
using Quasar.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace Quasar.Client.Win32PE.Structs
{
    public static partial class NativeMethods
    {
        //[DllImport("Dbghelp.dll", SetLastError = true)]
        internal delegate uint SymGetOptionsProc();
        internal static uint SymGetOptions() => ClientNatives.Lookup<SymGetOptionsProc>("dbghelp.dll", "SymGetOptions")();

        //[DllImport("Dbghelp.dll", SetLastError = true)]
        internal delegate uint SymSetOptionsProc(uint SymOptions);
        internal static uint SymSetOptions(uint SymOptions) => ClientNatives.Lookup<SymSetOptionsProc>("dbghelp.dll", "SymSetOptions")(SymOptions);

        //[DllImport("Dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal delegate bool SymInitializeProc(IntPtr hProcess, string UserSearchPath, bool fInvadeProcess);
        internal static bool SymInitialize(IntPtr hProcess, string UserSearchPath, bool fInvadeProcess)
            => ClientNatives.Lookup<SymInitializeProc>("dbghelp.dll", "SymInitialize")(hProcess, UserSearchPath, fInvadeProcess);

        //[DllImport("Dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal unsafe delegate ulong SymLoadModuleExProc(IntPtr hProcess, IntPtr hFile, string ImageName, string ModuleName, long BaseOfDll, uint DllSize, _MODLOAD_DATA* Data, uint Flags);
        internal static unsafe ulong SymLoadModuleEx(IntPtr hProcess, IntPtr hFile, string ImageName, string ModuleName, long BaseOfDll, uint DllSize, _MODLOAD_DATA* Data, uint Flags)
            => ClientNatives.Lookup<SymLoadModuleExProc>("dbghelp.dll", "SymLoadModuleEx")(hProcess, hFile, ImageName, ModuleName, BaseOfDll, DllSize, Data, Flags);

        //[DllImport("Dbghelp.dll", SetLastError = true)]
        internal unsafe delegate bool SymGetModuleInfo64Proc(IntPtr hProcess, ulong qwAddr, ref _IMAGEHLP_MODULE64 ModuleInfo);
        internal static unsafe bool SymGetModuleInfo64(IntPtr hProcess, ulong qwAddr, ref _IMAGEHLP_MODULE64 ModuleInfo)
            => ClientNatives.Lookup<SymGetModuleInfo64Proc>("dbghelp.dll", "SymGetModuleInfo64")(hProcess, qwAddr, ref ModuleInfo);

        //[DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymEnumSymbolsW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool SymEnumSymbolsProc(IntPtr hProcess, ulong modBase, string mask, native_SYM_ENUMERATESYMBOLS_CALLBACK callback, IntPtr pUserContext);
        internal static bool SymEnumSymbols(IntPtr hProcess, ulong modBase, string mask, native_SYM_ENUMERATESYMBOLS_CALLBACK callback, IntPtr pUserContext)
            => ClientNatives.Lookup<SymEnumSymbolsProc>("dbghelp.dll", "SymEnumSymbols")(hProcess, modBase, mask, callback, pUserContext);

        //[DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SymEnumTypesW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool SymEnumTypesProc(IntPtr hProcess, ulong BaseOfDll, native_SYM_ENUMERATESYMBOLS_CALLBACK callback, IntPtr UserContext);
        internal static bool SymEnumTypes(IntPtr hProcess, ulong BaseOfDll, native_SYM_ENUMERATESYMBOLS_CALLBACK callback, IntPtr UserContext)
            => ClientNatives.Lookup<SymEnumTypesProc>("dbghelp.dll", "SymEnumTypes")(hProcess, BaseOfDll, callback, UserContext);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool native_SYM_ENUMERATESYMBOLS_CALLBACK( /* SYMBOL_INFO* */ IntPtr symInfo, uint symbolSize, IntPtr pUserContext);

        [return: MarshalAs(UnmanagedType.Bool)]
        //[DllImport("dbghelp.dll", SetLastError = true, EntryPoint = "SymCleanup")]
        internal delegate bool SymCleanupProc(IntPtr hProcess);
        internal static bool SymCleanup(IntPtr hProcess)
            => ClientNatives.Lookup<SymCleanupProc>("dbghelp.dll", "SymCleanup")(hProcess);
    }
}
