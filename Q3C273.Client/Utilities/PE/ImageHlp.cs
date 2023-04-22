using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ton618.Utilities.PE
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _IMAGEHLP_LINE64
    {
        public uint SizeOfStruct;           // set to sizeof(IMAGEHLP_LINE64)
        public IntPtr Key;                    // internal
        public uint LineNumber;             // line number in file
        public IntPtr FileName;               // full filename
        public ulong Address;                // first instruction of line
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct _IMAGEHLP_MODULE64
    {
        public uint SizeOfStruct;           // set to sizeof(IMAGEHLP_MODULE64)
        public ulong BaseOfImage;            // base load address of module
        public uint ImageSize;              // virtual size of the loaded module
        public uint TimeDateStamp;          // date/time stamp from pe header
        public uint CheckSum;               // checksum from the pe header
        public uint NumSyms;                // number of symbols in the symbol table
        public SYM_TYPE SymType;                // type of symbols loaded

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] ModuleName;         // module name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] ImageName;         // image name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] LoadedImageName;   // symbol file name
                                         // new elements: 07-Jun-2002
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] LoadedPdbName;     // pdb file name

        public uint CVSig;                  // Signature of the CV record in the debug directories

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ClientNatives.MAX_PATH * 3)]
        public byte[] CVData;   // Contents of the CV record
        public uint PdbSig;                 // Signature of PDB
        public Guid PdbSig70;               // Signature of PDB (VC 7 and up)
        public uint PdbAge;                 // DBI age of pdb
        public bool PdbUnmatched;           // loaded an unmatched pdb
        public bool DbgUnmatched;           // loaded an unmatched dbg
        public bool LineNumbers;            // we have line number information
        public bool GlobalSymbols;          // we have internal symbol information
        public bool TypeInfo;               // we have type information
                                            // new elements: 17-Dec-2003
        public bool SourceIndexed;          // pdb supports source server
        public bool Publics;                // contains public symbols
                                            // new element: 15-Jul-2009
        public uint MachineType;            // IMAGE_FILE_MACHINE_XXX from ntimage.h and winnt.h
        public uint Reserved;               // Padding - don't remove.

        public unsafe string GetLoadedPdbName()
        {
            fixed (byte* ptr = LoadedPdbName)
            {
                return Marshal.PtrToStringAnsi(new IntPtr(ptr));
            }
        }

        internal static _IMAGEHLP_MODULE64 Create()
        {
            var mod = new _IMAGEHLP_MODULE64();
            mod.SizeOfStruct = (uint)Marshal.SizeOf(mod);
            return mod;
        }

        /*
            _IMAGEHLP_MODULE64 module = _IMAGEHLP_MODULE64.Create();
            if (ClientNatives.SymGetModuleInfo64(processHandle, (ulong)base_addr.ToInt64(), ref module) == false)
            {
                Console.WriteLine("Unexpected failure from SymGetModuleInfo64().");
                return;
            }

            Console.WriteLine("GetLoadedPdbName: " + module.GetLoadedPdbName());
        */
    }

}
