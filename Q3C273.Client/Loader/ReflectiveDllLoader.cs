using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using Ton618.Utilities;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Loader
{
    /// <summary>
    /// https://github.com/hsheric0210/SimpleSyringe/blob/main/SimpleSyringe.Syringe/BasicDLLInjection.cpp
    /// https://github.com/PowerShellMafia/PowerSploit/blob/master/CodeExecution/Invoke-DllInjection.ps1
    /// https://github.com/PowerShellMafia/PowerSploit/blob/master/CodeExecution/Invoke-ReflectivePEInjection.ps1
    /// </summary>
    public static class ReflectiveDllLoader
    {
        public static int SetPrivilege()
        {
            var myThread = OpenThread(ThreadAccessRights.THREAD_ALL_ACCESS, false, (int)GetCurrentThreadId());
            MessageBox.Show($"My thread handle is {myThread:X}");
            var result = OpenThreadToken(
                myThread,
                ThreadTokenAccessRights.TOKEN_QUERY | ThreadTokenAccessRights.TOKEN_ADJUST_PRIVILEGES,
                false,
                out var tokenHandle);

            if (!result)
            {
                var errCode = Marshal.GetLastWin32Error();
                if (errCode == 1008) // ERROR_NO_TOKEN
                {
                    MessageBox.Show("Failed with NO_SUCH_TOKEN. Retrying after impersonation.");
                    result = ImpersonateSelf(SecurityImpersonationLevel.SecurityImpersonation);
                    if (!result)
                        return 1;

                    result = OpenThreadToken(
                        myThread,
                        ThreadTokenAccessRights.TOKEN_QUERY | ThreadTokenAccessRights.TOKEN_ADJUST_PRIVILEGES,
                        false,
                        out tokenHandle);
                    if (!result)
                        return 2;
                }
                else
                    return 3;
            }

            var pLUID = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LUID)));
            if (!LookupPrivilegeValue(null, "SeDebugPrivilege", pLUID))
                return 4;
            MessageBox.Show("Got LUID, " + pLUID);

            var tokenPrivSize = Marshal.SizeOf<TOKEN_PRIVILEGES>();
            var tokenPrivMem = Marshal.AllocHGlobal(tokenPrivSize);
            var tokenPriv = Marshal.PtrToStructure<TOKEN_PRIVILEGES>(tokenPrivMem);
            tokenPriv.PrivilegeCount = 1;
            tokenPriv.Privileges.Luid = (LUID)Marshal.PtrToStructure(pLUID, typeof(LUID));
            tokenPriv.Privileges.Attributes = (uint)PrivilegeAttributes.SE_PRIVILEGE_ENABLED;
            Marshal.StructureToPtr(tokenPriv, tokenPrivMem, true);

            result = AdjustTokenPrivileges(tokenHandle, false, tokenPrivMem, tokenPrivSize, IntPtr.Zero, IntPtr.Zero);
            if (!result || Marshal.GetLastWin32Error() != 0)
            {
                MessageBox.Show("Cannot adjust token priv err " + Marshal.GetLastWin32Error());
            }

            Marshal.FreeHGlobal(pLUID);
            Marshal.FreeHGlobal(tokenPrivMem);
            CloseHandle(tokenHandle);
            CloseHandle(myThread);
            return 0;
        }
    }
}
