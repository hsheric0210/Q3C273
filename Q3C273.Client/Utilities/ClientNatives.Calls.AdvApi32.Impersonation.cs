using System.Runtime.InteropServices;

namespace Ton618.Utilities
{
    internal static partial class ClientNatives
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool ImpersonateSelfProc([In] SecurityImpersonationLevel ImpersonationLevel);
        internal static bool ImpersonateSelf([In] SecurityImpersonationLevel ImpersonationLevel) => Lookup<ImpersonateSelfProc>("advapi32.dll", "ImpersonateSelf")(ImpersonationLevel);
    }
}
