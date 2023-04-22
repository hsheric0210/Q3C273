using Org.BouncyCastle.Utilities;
using System;
using System.Runtime.InteropServices;
using Ton618.Utilities;
using Ton618.Utilities.PE;
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
        public static void LoadDLL(int processId, byte[] dll)
        {
            // Check privilege

        }

        private static void RelocateImage(IntPtr localPE, IntPtr remotePE)
        {

        }

        private static int SetPrivilege()
        {
            var myThread = ClientNatives.GetCurrentThread();
            var result = ClientNatives.OpenThreadToken(
                myThread,
                ThreadTokenAccessRights.TOKEN_QUERY | ThreadTokenAccessRights.TOKEN_ADJUST_PRIVILEGES,
                false,
                out var tokenHandle);

            if (!result)
            {
                var errCode = Marshal.GetLastWin32Error();
                if (errCode == 1008) // ERROR_NO_TOKEN
                {
                    result = ClientNatives.ImpersonateSelf(ClientNatives.SecurityImpersonationLevel.SecurityImpersonation);
                    if (!result)
                        return 1;

                    result = ClientNatives.OpenThreadToken(
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

            var pLUID = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ClientNatives.LUID)));
            if (!ClientNatives.LookupPrivilegeValue(null, "SeDebugPrivilege", pLUID))
                return 4;

            var tokenPrivSize = Marshal.SizeOf(typeof(ClientNatives.TOKEN_PRIVILEGES));
            var tokenPrivMem = Marshal.AllocHGlobal(tokenPrivSize);
            var tokenPriv = (ClientNatives.TOKEN_PRIVILEGES)Marshal.PtrToStructure(tokenPrivMem, typeof(ClientNatives.TOKEN_PRIVILEGES));
            tokenPriv.PrivilegeCount = 1;
            tokenPriv.Privileges.Luid = (ClientNatives.LUID)Marshal.PtrToStructure(pLUID, typeof(ClientNatives.LUID));
            tokenPriv.Privileges.Attributes = (uint)ClientNatives.PrivilegeAttributes.SE_PRIVILEGE_ENABLED;
            Marshal.StructureToPtr(tokenPriv, tokenPrivMem, true);

            result = ClientNatives.AdjustTokenPrivileges(tokenHandle, false, tokenPrivMem, tokenPrivSize, IntPtr.Zero, IntPtr.Zero);
            if (!result || Marshal.GetLastWin32Error() != 0)
                return 5;

            Marshal.FreeHGlobal(pLUID);
            Marshal.FreeHGlobal(tokenPrivMem);
            ClientNatives.CloseHandle(tokenHandle);
            ClientNatives.CloseHandle(myThread);
            return 0;
        }
    }
}
