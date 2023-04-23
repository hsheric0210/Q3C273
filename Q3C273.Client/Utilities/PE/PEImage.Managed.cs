using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Ton618.Utilities.PE
{
    public partial class PEImage
    {
        IMAGE_COR20_HEADER? corHeader;

        public bool IsManaged => CLRRuntimeHeaderDirectory.VirtualAddress != 0;

        public IMAGE_DATA_DIRECTORY CLRRuntimeHeaderDirectory => Is64Bitness ? optionalHeader64.CLRRuntimeHeader : optionalHeader32.CLRRuntimeHeader;

        public IMAGE_COR20_HEADER GetClrDirectoryHeader()
        {
            if (CLRRuntimeHeaderDirectory.VirtualAddress == 0)
                return default;

            if (corHeader == null)
                corHeader = Read<IMAGE_COR20_HEADER>(CLRRuntimeHeaderDirectory.VirtualAddress);

            return corHeader.Value;
        }

        public IEnumerable<VTableFixups> EnumerateVTableFixups()
        {
            var corHeader = GetClrDirectoryHeader();
            return Reads<VTableFixups>(corHeader.VTableFixups.VirtualAddress, corHeader.VTableFixups.Size);
        }
    }
}
