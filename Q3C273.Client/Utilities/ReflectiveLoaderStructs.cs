using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ton618.Utilities
{
    [StructLayout(LayoutKind.Auto)]
    internal struct IMAGE_DATA_DIRECTORY
    {
        public uint VA;
        public uint Size;
    }

    [StructLayout(LayoutKind.Auto)]
    internal struct IMAGE_FILE_HEADER
    {
        public ushort Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumbeROfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }
}
