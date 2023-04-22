using Ton618.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static Ton618.Utilities.ClientNatives;

#pragma warning disable IDE1006, CA1815 // Naming Styles

namespace Ton618.Utilities.PE
{
    [Flags]
    public enum CorVtableDefines : ushort
    {
        // V-table constants
        COR_VTABLE_32BIT = 0x01,          // V-table slots are 32-bits in size.
        COR_VTABLE_64BIT = 0x02,          // V-table slots are 64-bits in size.
        COR_VTABLE_FROM_UNMANAGED = 0x04,          // If set, transition from unmanaged.
        COR_VTABLE_FROM_UNMANAGED_RETAIN_APPDOMAIN = 0x08,  // If set, transition from unmanaged with keeping the current appdomain.
        COR_VTABLE_CALL_MOST_DERIVED = 0x10,          // Call most derived method described by
    }

    public enum CorTokenType
    {
        mdtModule = 0x00000000,       //
        mdtTypeRef = 0x01000000,       //
        mdtTypeDef = 0x02000000,       //
        mdtFieldDef = 0x04000000,       //
        mdtMethodDef = 0x06000000,       //
        mdtParamDef = 0x08000000,       //
        mdtInterfaceImpl = 0x09000000,       //
        mdtMemberRef = 0x0a000000,       //
        mdtCustomAttribute = 0x0c000000,       //
        mdtPermission = 0x0e000000,       //
        mdtSignature = 0x11000000,       //
        mdtEvent = 0x14000000,       //
        mdtProperty = 0x17000000,       //
        mdtMethodImpl = 0x19000000,       //
        mdtModuleRef = 0x1a000000,       //
        mdtTypeSpec = 0x1b000000,       //
        mdtAssembly = 0x20000000,       //
        mdtAssemblyRef = 0x23000000,       //
        mdtFile = 0x26000000,       //
        mdtExportedType = 0x27000000,       //
        mdtManifestResource = 0x28000000,       //
        mdtGenericParam = 0x2a000000,       //
        mdtMethodSpec = 0x2b000000,       //
        mdtGenericParamConstraint = 0x2c000000,

        mdtString = 0x70000000,       //
        mdtName = 0x71000000,       //
        mdtBaseType = 0x72000000,       // Leave this on the high end value. This does not correspond to metadata table
    }

    public enum WM_MESSAGE
    {
        WM_COPYDATA = 0x004A,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;    // Any value the sender chooses.  Perhaps its main window handle?
        public int cbData;       // The count of bytes in the message.
        public IntPtr lpData;    // The address of the message.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _CLIENT_ID
    {
        public IntPtr UniqueProcess;
        public IntPtr UniqueThread;

        public int Pid
        {
            get { return UniqueProcess.ToInt32(); }
        }

        public int Tid
        {
            get { return UniqueThread.ToInt32(); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _SYSTEM_HANDLE_INFORMATION
    {
        public int HandleCount;
        public _SYSTEM_HANDLE_TABLE_ENTRY_INFO Handles; /* Handles[0] */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _SYSTEM_HANDLE_INFORMATION_EX
    {
        public IntPtr HandleCount;
        public IntPtr Reserved;
        public _SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX Handles; /* Handles[0] */

        public int NumberOfHandles
        {
            get { return HandleCount.ToInt32(); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GENERIC_MAPPING
    {
        public uint GenericRead;
        public uint GenericWrite;
        public uint GenericExecute;
        public uint GenericAll;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct OBJECT_NAME_INFORMATION
    {
        public _UNICODE_STRING Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct OBJECT_TYPE_INFORMATION
    {
        public _UNICODE_STRING Name;
        public uint TotalNumberOfObjects;
        public uint TotalNumberOfHandles;
        public uint TotalPagedPoolUsage;
        public uint TotalNonPagedPoolUsage;
        public uint TotalNamePoolUsage;
        public uint TotalHandleTableUsage;
        public uint HighWaterNumberOfObjects;
        public uint HighWaterNumberOfHandles;
        public uint HighWaterPagedPoolUsage;
        public uint HighWaterNonPagedPoolUsage;
        public uint HighWaterNamePoolUsage;
        public uint HighWaterHandleTableUsage;
        public uint InvalidAttributes;
        public GENERIC_MAPPING GenericMapping;
        public uint ValidAccess;
        public byte SecurityRequired;
        public byte MaintainHandleCount;
        public ushort MaintainTypeList;

        /*
enum _POOL_TYPE
{
    NonPagedPool,
    PagedPool,
    NonPagedPoolMustSucceed,
    DontUseThisType,
    NonPagedPoolCacheAligned,
    PagedPoolCacheAligned,
    NonPagedPoolCacheAlignedMustS
}
        */

        public int PoolType;
        public uint PagedPoolUsage;
        public uint NonPagedPoolUsage;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CodeView_Header
    {
        public uint Signature;
        public int Offset;
    }

    // https://github.com/shuffle2/IDA-ClrNative/blob/master/ClrNativeLoader.py
    [StructLayout(LayoutKind.Sequential)]
    public struct VTableFixups
    {
        public uint rva;
        public ushort Count;
        public CorVtableDefines Type;

        public bool Is64bit
        {
            get
            {
                return (Type & CorVtableDefines.COR_VTABLE_64BIT) == CorVtableDefines.COR_VTABLE_64BIT;
            }
        }

        public int GetItemSize()
        {
            return Is64bit ? sizeof(long) : sizeof(int);
        }

        public override string ToString()
        {
            return $"RVA: 0x{rva:x}, # of entries: {Count}, Type: 0x{Type:x}";
        }
    }
}
