using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Windows.Forms;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Utilities
{
    /// <summary>
    /// https://github.com/PowerShellMafia/PowerSploit/blob/master/CodeExecution/Invoke-ReflectivePEInjection.ps1
    /// </summary>
    public static class Privileges
    {
        public static int EnableDebugPrivilege()
        {
            var myThread = OpenThread(ThreadAccessRights.THREAD_ALL_ACCESS, false, GetCurrentThreadId());
            var state = OpenThreadToken(
                myThread,
                ThreadTokenAccessRights.TOKEN_QUERY | ThreadTokenAccessRights.TOKEN_ADJUST_PRIVILEGES,
                false,
                out var tokenHandle);

            if (!state)
            {
                if (Marshal.GetLastWin32Error() == 1008) // ERROR_NO_TOKEN -> Retry after impersonation
                {
                    state = ImpersonateSelf(SecurityImpersonationLevel.SecurityImpersonation);
                    if (!state)
                        throw new AggregateException("Failed to impersonate self.");

                    state = OpenThreadToken(
                        myThread,
                        ThreadTokenAccessRights.TOKEN_QUERY | ThreadTokenAccessRights.TOKEN_ADJUST_PRIVILEGES,
                        false,
                        out tokenHandle);
                    if (!state)
                        throw new AggregateException("Failed to acquire the thread token of the current thread after impersonation. Win32 Error: " + Marshal.GetLastWin32Error());
                }
                else
                {
                    throw new AggregateException("Failed to acquire the thread token of the current thread. Win32 Error: " + Marshal.GetLastWin32Error());
                }
            }

            var pLUID = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LUID)));
            if (!LookupPrivilegeValue(null, "SeDebugPrivilege", pLUID))
                throw new AggregateException("Failed to lookup privilege value. Win32 Error: " + Marshal.GetLastWin32Error());

            var tokenPrivSize = Marshal.SizeOf<TOKEN_PRIVILEGES>();
            var tokenPrivMem = Marshal.AllocHGlobal(tokenPrivSize);
            var tokenPriv = Marshal.PtrToStructure<TOKEN_PRIVILEGES>(tokenPrivMem);
            tokenPriv.PrivilegeCount = 1;
            tokenPriv.Privileges = new LUID_AND_ATTRIBUTES[1];
            tokenPriv.Privileges[0].Luid = Marshal.PtrToStructure<LUID>(pLUID);
            tokenPriv.Privileges[0].Attributes = (uint)PrivilegeAttributes.SE_PRIVILEGE_ENABLED;
            Marshal.StructureToPtr(tokenPriv, tokenPrivMem, false);

            state = AdjustTokenPrivileges(tokenHandle, false, tokenPrivMem, tokenPrivSize, IntPtr.Zero, IntPtr.Zero);
            if (!state)
                throw new AggregateException("Failed to adjust token privilege. Win32 Error: " + Marshal.GetLastWin32Error());

            Marshal.FreeHGlobal(pLUID);
            Marshal.FreeHGlobal(tokenPrivMem);
            CloseHandle(tokenHandle);
            CloseHandle(myThread);
            return 0;
        }
    }
}
