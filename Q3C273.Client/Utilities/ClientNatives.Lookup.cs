using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Ton618.Win32PE.PE;
using Ton618.Win32PE.Structs;

namespace Ton618.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    internal static partial class ClientNatives
    {
        private static Dictionary<int, IntPtr> addresses = new Dictionary<int, IntPtr>();

        internal static void LoadNatives()
        {
            LoadLibrary("user32.dll");
            LoadLibrary("gdi32.dll");
            LoadLibrary("iphlpapi.dll");
            LoadLibrary("dbghelp.dll");
            LoadLibrary("psapi.dll");
        }

        // Self-implemented GetProcAddress (bypass Antivirus GetProcAddress hooking)
        // 고마워요, 정성태님!
        //https://www.sysnet.pe.kr/2/0/12101
        // For calling convention: https://stackoverflow.com/questions/5155180/changing-a-c-sharp-delegates-calling-convention-to-cdecl
        internal static T Lookup<T>(string module, string proc) // NOTE: The strings will be automatically encrypted using ConfuserEx
        {
            var key = (module + '!' + proc).GetHashCode(); // Address caching
            if (!addresses.TryGetValue(key, out var addr))
            {
                var peb = EnvironmentBlockInfo.GetPeb();
                var ldrData = _PEB_LDR_DATA.Create(peb.Ldr);
                var mod = ldrData.Find(module);
                if (Equals(mod, default(_LDR_DATA_TABLE_ENTRY)))
                    Environment.FailFast("Inexistent module: " + module);
                var img = PEImage.ReadFromMemory(mod.DllBase, (int)mod.SizeOfImage);
                var procStr = img.GetExportFunction(proc);
                if (Equals(procStr, default(ExportFunctionInfo)))
                    Environment.FailFast("Inexistent proc: " + proc);
                addr = (IntPtr)((ulong)mod.DllBase + procStr.RvaAddress);
                addresses.Add(key, addr);
            }
            return Marshal.GetDelegateForFunctionPointer<T>(addr);
        }
    }
}
