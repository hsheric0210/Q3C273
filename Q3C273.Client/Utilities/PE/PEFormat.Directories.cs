using System;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006, CA1815 // Naming Styles

namespace Ton618.Utilities.PE
{

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DATA_DIRECTORY
    {
        public uint VirtualAddress;
        public uint Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_EXPORT_DIRECTORY
    {
        public uint Characteristics;
        public uint TimeDateStamp;
        public ushort MajorVersion;
        public ushort MinorVersion;
        public uint Name;
        public uint Base;
        public uint NumberOfFunctions;
        public uint NumberOfNames;
        public uint AddressOfFunctions;
        public uint AddressOfNames;
        public uint AddressOfNameOrdinals;
    }

    // Below here is related to debugging

    public enum DebugDirectoryType : uint
    {
        IMAGE_DEBUG_TYPE_UNKNOWN = 0,
        IMAGE_DEBUG_TYPE_COFF = 1,
        IMAGE_DEBUG_TYPE_CODEVIEW = 2,
        IMAGE_DEBUG_TYPE_FPO = 3,
        IMAGE_DEBUG_TYPE_MISC = 4,
        IMAGE_DEBUG_TYPE_EXCEPTION = 5,
        IMAGE_DEBUG_TYPE_FIXUP = 6,
        IMAGE_DEBUG_TYPE_OMAP_TO_SRC = 7,
        IMAGE_DEBUG_TYPE_OMAP_FROM_SRC = 8,
        IMAGE_DEBUG_TYPE_BORLAND = 9,
        IMAGE_DEBUG_TYPE_RESERVED10 = 10,
        IMAGE_DEBUG_TYPE_CLSID = 11,
        IMAGE_DEBUG_TYPE_VC_FEATURE = 12,
        IMAGE_DEBUG_TYPE_POGO = 13,
        IMAGE_DEBUG_TYPE_ILTCG = 14,
        IMAGE_DEBUG_TYPE_MPX = 15,
        IMAGE_DEBUG_TYPE_REPRO = 16,
    }

    public enum CodeViewSignature : uint
    {
        RSDS = 0x53445352, // SDSR
    }

    [StructLayout(LayoutKind.Sequential)]
    struct _CodeViewRSDS
    {
        public uint CvSignature;
        public Guid Signature;
        public uint Age;
        // fixed byte PdbFileName[1];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DEBUG_DIRECTORY
    {
        public uint Characteristics;
        public uint TimeDateStamp;
        public ushort MajorVersion;
        public ushort MinorVersion;
        public uint Type;
        public uint SizeOfData;
        public uint AddressOfRawData;
        public uint PointerToRawData;

        internal unsafe CodeViewRSDS GetCodeViewHeader(IntPtr codeViewPtr)
        {
            var dir = (CodeView_Header)Marshal.PtrToStructure(codeViewPtr, typeof(CodeView_Header));

            switch (dir.Signature)
            {
                case (uint)CodeViewSignature.RSDS:
                    var item = (_CodeViewRSDS)Marshal.PtrToStructure(codeViewPtr, typeof(_CodeViewRSDS));
                    CodeViewRSDS rsds;
                    rsds.CvSignature = item.CvSignature;
                    rsds.Signature = item.Signature;
                    rsds.Age = item.Age;

                    var rsdsSize = Marshal.SizeOf(item);
                    var fileNamePtr = codeViewPtr + rsdsSize;
                    rsds.PdbFileName = Marshal.PtrToStringAnsi(fileNamePtr);
                    return rsds;

                default:
                    throw new ApplicationException("Push the author to impl this: " + dir.Signature);
            }
        }
    }
}
