using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ton618.Utilities
{
    internal partial class ClientNatives
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct _MODLOAD_DATA
        {
            public uint ssize;
            public uint ssig;
            public IntPtr data;
            public uint size;
            public uint flags;
        }

    }
}
