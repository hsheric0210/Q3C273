using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ton618.Utilities.PE
{
    public partial class PEImage
    {
        public void UpdateBaseRelocations(IntPtr srcNative, IntPtr destNative, ulong originalBase)
        {
            if (originalBase == destNative.ToUInt64() || BaseRelocationDirectory.Size == 0)
                return; // Nothing to do

            var imageBaseRelocSize = Marshal.SizeOf<IMAGE_BASE_RELOCATION>();

            var subDifference = originalBase > destNative.ToUInt64();
            ulong baseDifference;
            if (subDifference)
                baseDifference = originalBase - destNative.ToUInt64();
            else
                baseDifference = destNative.ToUInt64() - originalBase;

            var sectionVA = srcNative.uplusptr(BaseRelocationDirectory.VirtualAddress);
            var sectionSize = BaseRelocationDirectory.Size;
            while (sectionSize > 0)
            {
                var section = Marshal.PtrToStructure<IMAGE_BASE_RELOCATION>(sectionVA);
                if (section.SizeOfBlock == 0) // Null-termination entry
                    break;

                var tableVA = srcNative.uplusptr(section.VirtualAddress);
                var tableEntryCount = (section.SizeOfBlock - imageBaseRelocSize) / 2;
                for (var i = 0; i < tableEntryCount; i++)
                {
                    var table = Marshal.PtrToStructure<ushort>(sectionVA + imageBaseRelocSize + i * 2);
                    var offset = table & 0x0FFFu;
                    var type = (ImageRelocationType)(((table & 0xF000u) >> 12) & 0xFu);
                    if (type == ImageRelocationType.IMAGE_REL_BASED_HIGHLOW | type == ImageRelocationType.IMAGE_REL_BASED_DIR64) // TODO: Support IMAGE_REL_BASED_HIGH IMAGE_REL_BASED_LOW
                    {
                        var addressRef = tableVA.uplusptr(offset);
                        var addressDeref = Marshal.PtrToStructure<IntPtr>(addressRef);
                        if (subDifference)
                            addressDeref = addressDeref.uminusptr(baseDifference);
                        else
                            addressDeref = addressDeref.uplusptr(baseDifference);
                        Marshal.StructureToPtr(addressDeref, addressRef, false);
                    }
                    else if (type != ImageRelocationType.IMAGE_REL_BASED_ABSOLUTE)
                        throw new Exception($"Unknown relocation: {type:X} at relocationInfo: {table:X}");
                }

                // Jump to next entry
                sectionSize -= section.SizeOfBlock; // Extra overflow protection layer
                sectionVA = sectionVA.uplusptr(section.SizeOfBlock);
            }
        }
    }
}
