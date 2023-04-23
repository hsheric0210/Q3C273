using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Ton618.Utilities.PE
{
    public partial class PEImage
    {
        public ExportFunctionInfo GetExportFunction(string functionName)
        {
            var functions = GetExportFunctions();

            for (var i = 0; i < functions?.Length; i++)
            {
                if (functions[i].Name == functionName)
                    return functions[i];
            }

            return default;
        }

        public IEnumerable<ExportFunctionInfo> EnumerateExportFunctions() => GetExportFunctions();

        public unsafe ExportFunctionInfo[] GetExportFunctions()
        {
            if (ExportDirectory.VirtualAddress == 0)
                return null;

            var section = GetSection(ExportDirectory.VirtualAddress);

            GetSafeBuffer(0, section.VirtualAddress + section.SizeOfRawData, out var buffer);
            var list = new List<ExportFunctionInfo>();

            try
            {
                var exportDirPos = GetSafeBuffer(buffer, ExportDirectory.VirtualAddress);
                var dir = (IMAGE_EXPORT_DIRECTORY)Marshal.PtrToStructure(exportDirPos, typeof(IMAGE_EXPORT_DIRECTORY));

                var nameListPtr = GetSafeBuffer(buffer, dir.AddressOfNames);
                var ums = new UnmanagedMemoryStream((byte*)nameListPtr.ToPointer(), dir.NumberOfNames * sizeof(int));
                var br = new BinaryReader(ums);

                for (var i = 0; i < dir.NumberOfNames; i++)
                {
                    var namePos = br.ReadUInt32();
                    var namePtr = GetSafeBuffer(buffer, namePos);
                    var nameOrdinal = GetSafeBuffer(buffer, dir.AddressOfNameOrdinals).ReadUInt16ByIndex(i);
                    list.Add(new ExportFunctionInfo
                    {
                        Name = Marshal.PtrToStringAnsi(namePtr),
                        NameOrdinal = nameOrdinal,
                        RvaAddress = GetSafeBuffer(buffer, dir.AddressOfFunctions).ReadUInt32ByIndex(nameOrdinal),
                        Ordinal = dir.Base + nameOrdinal
                    });
                }
            }
            finally
            {
                buffer.Clear();
            }

            return list.ToArray();
        }
    }

    public struct ExportFunctionInfo
    {
        public string Name;
        public ushort NameOrdinal;
        public uint RvaAddress;

        /// <summary>
        /// Biased of NameOrdinal
        /// </summary>
        public uint Ordinal;

        public override string ToString() => $"{Name} at 0x{RvaAddress:x}";
    }
}
