using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Ton618.Utilities.PE
{
    public partial class PEImage
    {
        private IMAGE_FILE_HEADER fileHeader;
        private IMAGE_OPTIONAL_HEADER32 optionalHeader32;
        private IMAGE_OPTIONAL_HEADER64 optionalHeader64;
        private IMAGE_SECTION_HEADER[] sections;
        private byte[] bufferCached;
        private bool readFromFile;

        public IMAGE_DOS_HEADER DosHeader { get; private set; }

        public bool Is64Bitness { get; private set; }

        public IntPtr BaseAddress { get; private set; }

        public string ModulePath { get; private set; }

        public int MemorySize { get; private set; }

        public ulong OriginalImageBase => Is64Bitness ? optionalHeader64.ImageBase : optionalHeader32.ImageBase;

        public uint SizeOfImage => Is64Bitness ? optionalHeader64.SizeOfImage : optionalHeader32.SizeOfImage;

        public uint SizeOfHeader => Is64Bitness ? optionalHeader64.SizeOfHeaders : optionalHeader32.SizeOfHeaders;

        public DllCharacteristicsType DllCharacteristics => Is64Bitness ? optionalHeader64.DllCharacteristics : optionalHeader32.DllCharacteristics;

        public uint EntryPoint => Is64Bitness ? optionalHeader64.AddressOfEntryPoint : optionalHeader32.AddressOfEntryPoint;

        public IMAGE_DATA_DIRECTORY ExportDirectory => Is64Bitness ? optionalHeader64.ExportTable : optionalHeader32.ExportTable;

        public IMAGE_DATA_DIRECTORY Debug => Is64Bitness ? optionalHeader64.Debug : optionalHeader32.Debug;

        public IMAGE_DATA_DIRECTORY BaseRelocationDirectory => Is64Bitness ? optionalHeader64.BaseRelocationTable : optionalHeader32.BaseRelocationTable;

        public IMAGE_DATA_DIRECTORY ImportDescriptorTable => Is64Bitness ? optionalHeader64.ImportTable : optionalHeader32.ImportTable;

        private static bool IsValidNTHeaders(int signature)
        {
            // PE 헤더임을 확인 (IMAGE_NT_SIGNATURE == 0x00004550)
            // if (signature[0] == 0x50 && signature[1] == 0x45 && signature[2] == 0 && signature[3] == 0)
            if (signature == 0x00004550) // 'PE'
                return true;

            return false;
        }

        public IEnumerable<IMAGE_SECTION_HEADER> EnumerateSections() => sections;
        private IMAGE_SECTION_HEADER GetSection(uint virtualAddress)
        {
            for (var i = 0; i < sections.Length; i++)
            {
                var section = sections[i];

                var startAddr = section.VirtualAddress;
                var endAddr = section.VirtualAddress + section.PhysicalAddressOrVirtualSize;

                if (startAddr <= virtualAddress && virtualAddress <= endAddr)
                    return section;
            }

            return default;
        }


        private uint Rva2Raw(uint virtualAddress)
        {
            var section = GetSection(virtualAddress);
            return virtualAddress - section.VirtualAddress + section.PointerToRawData;
        }

        public unsafe byte[] ReadBytes(uint rvaAddress, int nBytes)
        {
            var byteBuffer = new byte[nBytes];

            var section = GetSection(rvaAddress);
            GetSafeBuffer(0, section.EndAddress, out var buffer);

            try
            {
                var bytePos = GetSafeBuffer(buffer, rvaAddress);
                var maxRead = Math.Min((int)(rvaAddress + nBytes), section.EndAddress);

                for (var i = (int)rvaAddress; i < maxRead; i++)
                {
                    var pos = (int)(i - rvaAddress);
                    byteBuffer[pos] = bytePos.ReadByte(pos);
                }
            }
            finally
            {
                buffer.Clear();
            }

            return byteBuffer;
        }

        public unsafe T Read<T>(uint rvaAddress) where T : struct
        {
            var section = GetSection(rvaAddress);
            GetSafeBuffer(0, section.VirtualAddress + section.SizeOfRawData, out var buffer);

            try
            {
                var bytePos = GetSafeBuffer(buffer, rvaAddress);
                return (T)Marshal.PtrToStructure(bytePos, typeof(T));
            }
            finally
            {
                buffer.Clear();
            }
        }

        public unsafe T[] Reads<T>(uint rvaAddress, uint totalSize) where T : struct
        {
            var section = GetSection(rvaAddress);
            GetSafeBuffer(0, section.VirtualAddress + section.SizeOfRawData, out var buffer);

            var list = new List<T>();

            var entrySize = Marshal.SizeOf(default(T));
            var count = totalSize / entrySize;

            try
            {
                for (uint i = 0; i < count; i++)
                {
                    var bytePos = GetSafeBuffer(buffer, rvaAddress + i * (uint)entrySize);
                    list.Add((T)Marshal.PtrToStructure(bytePos, typeof(T)));
                }
            }
            finally
            {
                buffer.Clear();
            }

            return list.ToArray();
        }

        public unsafe void CopyToNative(IntPtr nativeMemory, int offset, int size)
        {
            if (readFromFile)
            {
                var buffer = new byte[size];
                using (var fs = new FileStream(ModulePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var br = new BinaryReader(fs);
                    br.Read(buffer, 0, size);
                }

                Marshal.Copy(buffer, offset, nativeMemory, size);

            }
            else if (bufferCached != null)
            {
                Marshal.Copy(bufferCached, offset, nativeMemory, size);
            }
        }

        IntPtr GetSafeBuffer(uint rva, uint size, out BufferPtr buffer)
        {
            buffer = null;

            if (readFromFile)
            {
                var startAddress = Rva2Raw(rva);
                var endAddress = startAddress + size;

                buffer = new BufferPtr((int)endAddress);

                using (var fs = new FileStream(ModulePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var br = new BinaryReader(fs);
                    br.Read(buffer.Buffer, 0, buffer.Length);
                }
            }
            else if (bufferCached != null)
            {
                buffer = new BufferPtr(bufferCached.Length);
                Array.Copy(bufferCached, 0, buffer.Buffer, 0, bufferCached.Length);
            }

            return GetSafeBuffer(buffer, rva);
        }

        IntPtr GetSafeBuffer(BufferPtr buffer, uint rva)
        {
            IntPtr ptr;

            if (bufferCached != null)
                ptr = buffer.GetPtr((int)rva);
            else if (buffer != null)
            {
                var startAddress = Rva2Raw(rva);
                ptr = buffer.GetPtr((int)startAddress);
            }
            else
            {
                ptr = BaseAddress.uplusptr(rva);
            }

            return ptr;
        }

        private unsafe static PEImage ReadPEHeader(BinaryReader br)
        {
            var image = new PEImage();

            // IMAGE_DOS_HEADER 를 읽어들이고,
            var dosHeader = br.Read<IMAGE_DOS_HEADER>();
            {
                if (!dosHeader.IsValid)
                    return null;

                image.DosHeader = dosHeader;
            }

            // IMAGE_NT_HEADERS - signature 를 읽어들이고,
            {
                br.BaseStream.Position = dosHeader.e_lfanew; // Jump to NT header (IMAGE_NT_HEADERS)
                if (!IsValidNTHeaders(br.ReadInt32()))
                    return null;
            }

            // IMAGE_NT_HEADERS - IMAGE_FILE_HEADER를 읽어들임
            var ntFileHeader = br.Read<IMAGE_FILE_HEADER>();
            image.fileHeader = ntFileHeader;

            /*
            AnyCPU로 빌드된 .NET Image인 경우,
            OptionalHeader의 첫 번째 필드 Magic 값이 원래는 0x010B로 PE32였지만, 실행 후 메모리에 매핑되면서 0x020B로 변경

            따라서 64비트 이미지로 매핑되었음을 판단하려면 Magic 필드로 판단해야 함.
            */

            var magic = br.PeekUInt16();
            if (magic == (ushort)MagicType.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                image.Is64Bitness = true;

            // ushort optionalHeaderSize = ntFileHeader.SizeOfOptionalHeader;
            // optionalHeaderSize
            // 32bit PE == 0xe0(224)bytes
            // 64bit PE == 0xF0(240)bytes

            if (!image.Is64Bitness)
                image.optionalHeader32 = br.Read<IMAGE_OPTIONAL_HEADER32>();
            else
            {
                image.optionalHeader64 = br.Read<IMAGE_OPTIONAL_HEADER64>();
            }

            {
                var sections = new List<IMAGE_SECTION_HEADER>();

                for (var i = 0; i < image.fileHeader.NumberOfSections; i++)
                {
                    var sectionHeader = br.Read<IMAGE_SECTION_HEADER>();
                    sections.Add(sectionHeader);
                }

                image.sections = sections.ToArray();
            }

            return image;
        }

        public unsafe static PEImage FromLoadedModule(string moduleName)
        {
            foreach (ProcessModule pm in Process.GetCurrentProcess().Modules)
            {
                if (pm.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    var image = ReadFromMemory(pm.BaseAddress, pm.ModuleMemorySize);
                    image.ModulePath = pm.FileName;
                    image.readFromFile = false;
                    image.BaseAddress = pm.BaseAddress;
                    image.MemorySize = pm.ModuleMemorySize;
                    return image;
                }
            }

            return null;
        }

        public unsafe static PEImage ReadFromMemory(IntPtr baseAddress, int memorySize)
        {
            var ums = new UnmanagedMemoryStream((byte*)baseAddress.ToPointer(), memorySize);
            return ReadFromMemory(ums, baseAddress, memorySize);
        }

        public unsafe static PEImage ReadFromMemory(byte[] buffer, IntPtr baseAddress, int memorySize)
        {
            var ms = new MemoryStream(buffer);
            var image = ReadFromMemory(ms, baseAddress, memorySize);
            image.bufferCached = buffer;

            return image;
        }

        public unsafe static PEImage ReadFromMemory(Stream stream, IntPtr baseAddress, int memorySize)
        {
            var br = new BinaryReader(stream);

            var image = ReadPEHeader(br);
            if (image == null)
                return null;

            image.readFromFile = false;
            image.BaseAddress = baseAddress;
            image.MemorySize = memorySize;

            return image;
        }

        public unsafe static PEImage ReadFromFile(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var br = new BinaryReader(fs);
                var image = ReadPEHeader(br);
                if (image == null)
                    return null;

                image.readFromFile = true;
                image.ModulePath = filePath;
                image.BaseAddress = new IntPtr(image.Is64Bitness ? (long)image.optionalHeader64.ImageBase
                                        : image.optionalHeader32.ImageBase);

                return image;
            }
        }
    }
}
