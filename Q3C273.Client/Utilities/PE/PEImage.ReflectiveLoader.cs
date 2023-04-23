using Q3C273.Shared;
using System;
using System.Runtime.InteropServices;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Utilities.PE
{
    public partial class PEImage
    {
        public void CopySectionsTo(IntPtr nativeMemory)
        {
            foreach (var section in EnumerateSections())
            {
                var dest = nativeMemory.uplusptr(section.VirtualAddress);
                var data = section.PointerToRawData;
                var rawSize = section.SizeOfRawData;
                if (data == 0)
                    rawSize = 0;

                var vsize = section.PhysicalAddressOrVirtualSize;
                if (rawSize > vsize)
                    rawSize = vsize;

                CopyToNative(nativeMemory.uplusptr(section.VirtualAddress), (int)data, (int)rawSize);

                // Fill extra space with zero
                if (rawSize < vsize)
                    SharedNatives.memset(dest.uplusptr(rawSize), 0, vsize - rawSize);
            }
        }

        public void ImportDllImports(IntPtr localMem, IntPtr processHandle)
        {
            if (ImportDescriptorTable.Size == 0)
                return;

            var importDescriptorPtr = localMem.uplusptr(ImportDescriptorTable.VirtualAddress);
            while (true)
            {
                var importDescriptor = Marshal.PtrToStructure<IMAGE_IMPORT_DESCRIPTOR>(importDescriptorPtr);
                if (importDescriptor.Characteristics == 0 && importDescriptor.ForwarderChain == 0 && importDescriptor.Name == 0 && importDescriptor.TimeDateStamp == 0)
                    break; // Null-terminating IDT

                var importDllPathPtr = localMem.uplusptr(importDescriptor.Name);

                var importDllHandle = ImportDllInRemoteProcess(processHandle, importDllPathPtr);
                if (importDllHandle == null || importDllHandle == IntPtr.Zero)
                    throw new AggregateException("Import dll failed: " + Marshal.PtrToStringAnsi(importDllPathPtr) + " Win32Error=" + GetLastError());

                var thunkRef = localMem.uplusptr(importDescriptor.FirstThunk);
                var originalThunkRef = localMem.uplusptr(importDescriptor.Characteristics); //Characteristics is overloaded with OriginalFirstThunk
                var originalThunkRefVal = Marshal.PtrToStructure<IntPtr>(originalThunkRef);

                while (originalThunkRefVal != IntPtr.Zero)
                {
                    var byOrdinal = IntPtr.Size == 4 && originalThunkRefVal.ToInt32() < 0 || IntPtr.Size == 8 && originalThunkRefVal.ToInt64() < 0;
                    var procNamePtr = IntPtr.Zero;
                    string procName = "";
                    if (!byOrdinal)
                    {
                        var stringAddr = new IntPtr(localMem.ToInt64() + originalThunkRefVal.ToInt64()) + Marshal.SizeOf<ushort>();
                        procName = Marshal.PtrToStringAnsi(stringAddr);
                        procNamePtr = Marshal.StringToHGlobalAnsi(procName);
                    }

                    var newThunkRef = processHandle.GetProcAddressRemote(importDllHandle, procNamePtr, byOrdinal, Is64Bitness);
                    if (newThunkRef == default || newThunkRef == IntPtr.Zero)
                    {
                        var dllName = Marshal.PtrToStringAnsi(importDllPathPtr);
                        if (byOrdinal)
                            throw new AggregateException($"Imported function not found: {dllName}!Ordinal{procNamePtr}");
                        else
                            throw new AggregateException($"Imported function not found: {dllName}!{procName}");
                    }

                    Marshal.StructureToPtr(newThunkRef, thunkRef, false);
                    thunkRef += Marshal.SizeOf<IntPtr>();
                    originalThunkRef += Marshal.SizeOf<IntPtr>();
                    originalThunkRefVal = Marshal.PtrToStructure<IntPtr>(originalThunkRef);

                    if (!byOrdinal && procNamePtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(procNamePtr);
                    }
                }

                importDescriptorPtr += Marshal.SizeOf<IMAGE_IMPORT_DESCRIPTOR>();
            }
        }

        public void UpdateSectionProtections(IntPtr nativeMemory)
        {
            foreach (var section in EnumerateSections())
            {
                var sectionPtr = nativeMemory.uplusptr(section.VirtualAddress);
                var protectionFlag = GetVirtualProtectionValue(section.Characteristics);
                var sectionSize = section.PhysicalAddressOrVirtualSize;
                var state = VirtualProtect(sectionPtr, (UIntPtr)sectionSize, protectionFlag, out var oldProtection);
                if (!state)
                    throw new AggregateException("Cannot update memory protection rule for section " + sectionPtr);
            }
        }

        private PageAccessRights GetVirtualProtectionValue(ImageSectionCharacteristics chr)
        {
            PageAccessRights accessFlags;
            if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_EXECUTE))
            {
                if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_READ))
                {
                    if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_WRITE))
                        accessFlags = PageAccessRights.PAGE_EXECUTE_READWRITE;
                    else
                        accessFlags = PageAccessRights.PAGE_EXECUTE_READ;
                }
                else
                {
                    if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_READ))
                        accessFlags = PageAccessRights.PAGE_WRITECOPY;
                    else
                        accessFlags = PageAccessRights.PAGE_EXECUTE;
                }
            }
            else
            {
                if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_READ))
                {
                    if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_WRITE))
                        accessFlags = PageAccessRights.PAGE_READWRITE;
                    else
                        accessFlags = PageAccessRights.PAGE_READONLY;
                }
                else
                {
                    if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_READ))
                        accessFlags = PageAccessRights.PAGE_WRITECOPY;
                    else
                        accessFlags = PageAccessRights.PAGE_NOACCESS;
                }
            }

            if (chr.HasFlag(ImageSectionCharacteristics.IMAGE_SCN_MEM_NOT_CACHED))
                accessFlags |= PageAccessRights.PAGE_NOCACHE;

            return accessFlags;
        }
    }
}
