using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ton618.Utilities.PE
{
    public enum DebugNotifySession
    {
        Active = 0x0,
        Inactive = 0x01,
        Accessible = 0x02,
        InAccessible = 0x03,
    }

    public enum SYM_TYPE
    {
        SymNone = 0,
        SymCoff,
        SymCv,
        SymPdb,
        SymExport,
        SymDeferred,
        SymSym,       // .sym file
        SymDia,
        SymVirtual,
        NumSymTypes
    }

    [Flags]
    public enum SymOpt : uint
    {
        UNDNAME = 0x2,
        DEFERRED_LOADS = 0x4,
        LOAD_LINES = 0x10,
        IGNORE_NT_SYMPATH = 0x1000,
        DEBUG = 0x80000000,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal unsafe struct _SYMBOL_INFO
    {
        public uint SizeOfStruct;
        public uint TypeIndex;        // Type Index of symbol

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ulong[] Reserved;
        public uint Index;
        public uint Size;
        public ulong ModBase;          // Base Address of module comtaining this symbol
        public uint Flags;
        public ulong Value;            // Value of symbol, ValuePresent should be 1
        public ulong Address;          // Address of symbol including base address of module
        public uint Register;         // register holding value or pointer to value
        public uint Scope;            // scope of the symbol
        public uint Tag;              // pdb classification
        public uint NameLen;          // Actual length of name
        public uint MaxNameLen;
        public byte Name;          // Name of symbol
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SYMBOL_INFO
    {
        public uint SizeOfStruct;
        public uint TypeIndex;        // Type Index of symbol

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ulong Reserved0;
        public ulong Reserved1;
        public uint Index;
        public uint Size;
        public ulong ModBase;          // Base Address of module comtaining this symbol
        public uint Flags;
        public ulong Value;            // Value of symbol, ValuePresent should be 1
        public ulong Address;          // Address of symbol including base address of module
        public uint Register;         // register holding value or pointer to value
        public uint Scope;            // scope of the symbol
        public uint Tag;              // pdb classification
        public uint NameLen;          // Actual length of name
        public uint MaxNameLen;
        public string Name;          // Name of symbol

        public static SYMBOL_INFO Create(IntPtr baseAddress)
        {
            var info = (_SYMBOL_INFO)Marshal.PtrToStructure(baseAddress, typeof(_SYMBOL_INFO));

            SYMBOL_INFO si;
            si.SizeOfStruct = info.SizeOfStruct;
            si.TypeIndex = info.TypeIndex;
            si.Reserved0 = info.Reserved[0];
            si.Reserved1 = info.Reserved[1];
            si.Index = info.Index;
            si.Size = info.Size;
            si.ModBase = info.ModBase;
            si.Flags = info.Flags;
            si.Value = info.Value;
            si.Address = info.Address;
            si.Register = info.Register;
            si.Scope = info.Scope;
            si.Tag = info.Tag;
            si.NameLen = info.NameLen;
            si.MaxNameLen = info.MaxNameLen;

            var offset = Marshal.OffsetOf(typeof(_SYMBOL_INFO), nameof(info.Name)).ToInt32();
            si.Name = Marshal.PtrToStringAuto(baseAddress + offset, (int)info.NameLen).Trim('\0');

            return si;
        }
    }
}
