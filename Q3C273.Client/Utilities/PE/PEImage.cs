using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Ton618.Utilities.PE
{
    public class PEImage
    {
        IMAGE_DOS_HEADER _dosHeader;
        public IMAGE_DOS_HEADER DosHeader
        {
            get { return _dosHeader; }
        }

        IMAGE_FILE_HEADER _fileHeader;
        IMAGE_OPTIONAL_HEADER32 _optionalHeader32;
        IMAGE_OPTIONAL_HEADER64 _optionalHeader64;
        IMAGE_COR20_HEADER? _corHeader;

        IMAGE_SECTION_HEADER[] _sections;

        bool _is64BitHeader;
        public bool Is64Bitness
        {
            get { return _is64BitHeader; }
        }

        byte[] _bufferCached;

        IntPtr _baseAddress;
        public IntPtr BaseAddress
        {
            get { return _baseAddress; }
        }

        bool _readFromFile = false;

        string _filePath;
        public string ModulePath
        {
            get { return _filePath; }
        }

        int _memorySize;
        public int MemorySize
        {
            get { return _memorySize; }
        }

        static bool IsValidNTHeaders(int signature)
        {
            // PE 헤더임을 확인 (IMAGE_NT_SIGNATURE == 0x00004550)
            // if (signature[0] == 0x50 && signature[1] == 0x45 && signature[2] == 0 && signature[3] == 0)
            if (signature == 0x00004550)
                return true;

            return false;
        }

        public bool IsManaged
        {
            get
            {
                return CLRRuntimeHeaderDirectory.VirtualAddress != 0;
            }
        }

        public IMAGE_DATA_DIRECTORY CLRRuntimeHeaderDirectory
        {
            get
            {
                if (_is64BitHeader)
                    return _optionalHeader64.CLRRuntimeHeader;
                else
                {
                    return _optionalHeader32.CLRRuntimeHeader;
                }
            }
        }

        public IMAGE_DATA_DIRECTORY ExportDirectory
        {
            get
            {
                if (_is64BitHeader)
                    return _optionalHeader64.ExportTable;
                else
                {
                    return _optionalHeader32.ExportTable;
                }
            }
        }

        public IMAGE_DATA_DIRECTORY Debug
        {
            get
            {
                if (_is64BitHeader)
                    return _optionalHeader64.Debug;
                else
                {
                    return _optionalHeader32.Debug;
                }
            }
        }

        public IEnumerable<IMAGE_SECTION_HEADER> EnumerateSections()
        {
            return _sections;
        }

        public IEnumerable<VTableFixups> EnumerateVTableFixups()
        {
            var corHeader = GetClrDirectoryHeader();
            var vtfs = Reads<VTableFixups>(corHeader.VTableFixups.VirtualAddress, corHeader.VTableFixups.Size);
            return vtfs;
        }

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

        public IEnumerable<ExportFunctionInfo> EnumerateExportFunctions()
        {
            return GetExportFunctions();
        }

        public unsafe byte[] ReadBytes(uint rvaAddress, int nBytes)
        {
            var byteBuffer = new byte[nBytes];

            var section = GetSection((int)rvaAddress);
            GetSafeBuffer(0, section.EndAddress, out var buffer);

            try
            {
                var bytePos = GetSafeBuffer(buffer, rvaAddress);
                var maxRead = Math.Min((int)(rvaAddress + nBytes), (int)section.EndAddress);

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
            var section = GetSection((int)rvaAddress);
            GetSafeBuffer(0, (uint)section.VirtualAddress + (uint)section.SizeOfRawData, out var buffer);

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
            var section = GetSection((int)rvaAddress);
            GetSafeBuffer(0, (uint)section.VirtualAddress + (uint)section.SizeOfRawData, out var buffer);

            var list = new List<T>();

            var entrySize = (uint)Marshal.SizeOf(default(T));
            var count = totalSize / entrySize;

            try
            {
                for (uint i = 0; i < count; i++)
                {
                    var bytePos = GetSafeBuffer(buffer, rvaAddress + i * entrySize);
                    list.Add((T)Marshal.PtrToStructure(bytePos, typeof(T)));
                }
            }
            finally
            {
                buffer.Clear();
            }

            return list.ToArray();
        }

        public IMAGE_COR20_HEADER GetClrDirectoryHeader()
        {
            if (CLRRuntimeHeaderDirectory.VirtualAddress == 0)
                return default;

            if (_corHeader == null)
                _corHeader = Read<IMAGE_COR20_HEADER>(CLRRuntimeHeaderDirectory.VirtualAddress);

            return _corHeader.Value;
        }

        public unsafe ExportFunctionInfo[] GetExportFunctions()
        {
            if (ExportDirectory.VirtualAddress == 0)
                return null;

            var section = GetSection((int)ExportDirectory.VirtualAddress);

            GetSafeBuffer(0, (uint)section.VirtualAddress + (uint)section.SizeOfRawData, out var buffer);
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

                    ExportFunctionInfo efi;
                    efi.Name = Marshal.PtrToStringAnsi(namePtr);

                    efi.NameOrdinal = GetSafeBuffer(buffer, dir.AddressOfNameOrdinals).ReadUInt16ByIndex(i);
                    efi.RvaAddress = GetSafeBuffer(buffer, dir.AddressOfFunctions).ReadUInt32ByIndex(efi.NameOrdinal);

                    efi.Ordinal = efi.NameOrdinal + dir.Base;

                    list.Add(efi);
                }
            }
            finally
            {
                buffer.Clear();
            }

            return list.ToArray();
        }

        public IEnumerable<CodeViewRSDS> EnumerateCodeViewDebugInfo()
        {
            foreach (var debugDir in EnumerateDebugDir())
            {
                if (debugDir.Type != (uint)DebugDirectoryType.IMAGE_DEBUG_TYPE_CODEVIEW)
                    continue;

                var debugDirPtr = GetSafeBuffer(debugDir.AddressOfRawData, debugDir.SizeOfData, out var buffer);

                try
                {
                    yield return debugDir.GetCodeViewHeader(debugDirPtr);
                }
                finally
                {
                    buffer.Clear();
                }
            }
        }

        IntPtr GetSafeBuffer(uint rva, uint size, out BufferPtr buffer)
        {
            buffer = null;

            if (_readFromFile)
            {
                var startAddress = Rva2Raw((int)rva);
                var endAddress = startAddress + (int)size;

                buffer = new BufferPtr(endAddress);

                using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var br = new BinaryReader(fs);
                    br.Read(buffer.Buffer, 0, buffer.Length);
                }
            }
            else if (_bufferCached != null)
            {
                buffer = new BufferPtr(_bufferCached.Length);
                Array.Copy(_bufferCached, 0, buffer.Buffer, 0, _bufferCached.Length);
            }

            return GetSafeBuffer(buffer, rva);
        }

        IntPtr GetSafeBuffer(BufferPtr buffer, uint rva)
        {
            IntPtr ptr;

            if (_bufferCached != null)
                ptr = buffer.GetPtr((int)rva);
            else if (buffer != null)
            {
                var startAddress = Rva2Raw((int)rva);
                ptr = buffer.GetPtr(startAddress);
            }
            else
            {
                ptr = _baseAddress + (int)rva;
            }

            return ptr;
        }

        public IEnumerable<IMAGE_DEBUG_DIRECTORY> EnumerateDebugDir()
        {
            if (Debug.VirtualAddress == 0)
                yield break;

            var debugDirPtr = GetSafeBuffer(Debug.VirtualAddress, Debug.Size, out var buffer);

            try
            {
                var safeObj = new IMAGE_DEBUG_DIRECTORY();
                var sizeOfDir = Marshal.SizeOf(safeObj);

                var count = (int)Debug.Size / sizeOfDir;

                for (var i = 0; i < count; i++)
                {
                    var dir = (IMAGE_DEBUG_DIRECTORY)Marshal.PtrToStructure(debugDirPtr, typeof(IMAGE_DEBUG_DIRECTORY));
                    yield return dir;

                    debugDirPtr += sizeOfDir;
                }
            }
            finally
            {
                buffer.Clear();
            }
        }

        private int Rva2Raw(int virtualAddress)
        {
            var section = GetSection(virtualAddress);
            return virtualAddress - section.VirtualAddress + section.PointerToRawData;
        }

        private IMAGE_SECTION_HEADER GetSection(int virtualAddress)
        {
            for (var i = 0; i < _sections.Length; i++)
            {
                var section = _sections[i];

                var startAddr = section.VirtualAddress;
                var endAddr = section.VirtualAddress + section.PhysicalAddressOrVirtualSize;

                if (startAddr <= virtualAddress && virtualAddress <= endAddr)
                    return section;
            }

            return default;
        }

        unsafe static PEImage ReadPEHeader(BinaryReader br)
        {
            var image = new PEImage();

            // IMAGE_DOS_HEADER 를 읽어들이고,
            var dosHeader = br.Read<IMAGE_DOS_HEADER>();
            {
                if (dosHeader.IsValid == false)
                    return null;

                image._dosHeader = dosHeader;
            }

            // IMAGE_NT_HEADERS - signature 를 읽어들이고,
            {
                br.BaseStream.Position = dosHeader.e_lfanew;
                if (IsValidNTHeaders(br.ReadInt32()) == false)
                    return null;
            }

            // IMAGE_NT_HEADERS - IMAGE_FILE_HEADER를 읽어들임
            var ntFileHeader = br.Read<IMAGE_FILE_HEADER>();
            image._fileHeader = ntFileHeader;

            /*
            AnyCPU로 빌드된 .NET Image인 경우,
            OptionalHeader의 첫 번째 필드 Magic 값이 원래는 0x010B로 PE32였지만, 실행 후 메모리에 매핑되면서 0x020B로 변경

            따라서 64비트 이미지로 매핑되었음을 판단하려면 Magic 필드로 판단해야 함.
            */

            var magic = br.PeekUInt16();
            if (magic == (ushort)MagicType.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                image._is64BitHeader = true;

            // ushort optionalHeaderSize = ntFileHeader.SizeOfOptionalHeader;
            // optionalHeaderSize
            // 32bit PE == 0xe0(224)bytes
            // 64bit PE == 0xF0(240)bytes

            if (image._is64BitHeader == false)
                image._optionalHeader32 = br.Read<IMAGE_OPTIONAL_HEADER32>();
            else
            {
                image._optionalHeader64 = br.Read<IMAGE_OPTIONAL_HEADER64>();
            }

            {
                var sections = new List<IMAGE_SECTION_HEADER>();

                for (var i = 0; i < image._fileHeader.NumberOfSections; i++)
                {
                    var sectionHeader = br.Read<IMAGE_SECTION_HEADER>();
                    sections.Add(sectionHeader);
                }

                image._sections = sections.ToArray();
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
                    image._filePath = pm.FileName;
                    image._readFromFile = false;
                    image._baseAddress = pm.BaseAddress;
                    image._memorySize = pm.ModuleMemorySize;
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
            image._bufferCached = buffer;

            return image;
        }

        public unsafe static PEImage ReadFromMemory(Stream stream, IntPtr baseAddress, int memorySize)
        {
            var br = new BinaryReader(stream);

            var image = ReadPEHeader(br);
            if (image == null)
                return null;

            image._readFromFile = false;
            image._baseAddress = baseAddress;
            image._memorySize = memorySize;

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

                image._readFromFile = true;
                image._filePath = filePath;
                image._baseAddress = new IntPtr(image._is64BitHeader ? (long)image._optionalHeader64.ImageBase
                                        : image._optionalHeader32.ImageBase);

                return image;
            }
        }

        public static string DownloadPdb(string modulePath, byte[] buffer, IntPtr baseOffset, int imageSize, string rootPathToSave)
        {
            var pe = ReadFromMemory(buffer, baseOffset, imageSize);

            if (pe == null)
            {
                Console.WriteLine("Failed to read images");
                return null;
            }

            return pe.DownloadPdb(modulePath, rootPathToSave);
        }

        public string DownloadPdb(string modulePath, string rootPathToSave)
        {
            var baseUri = new Uri("https://msdl.microsoft.com/download/symbols/");
            var pdbDownloadedPath = string.Empty;

            foreach (var codeView in EnumerateCodeViewDebugInfo())
            {
                if (string.IsNullOrEmpty(codeView.PdbFileName))
                    continue;

                var pdbFileName = codeView.PdbFileName;
                if (Path.IsPathRooted(codeView.PdbFileName))
                    pdbFileName = Path.GetFileName(codeView.PdbFileName);

                var localPath = Path.Combine(rootPathToSave, pdbFileName);
                var localFolder = Path.GetDirectoryName(localPath);

                if (Directory.Exists(localFolder) == false)
                {
                    try
                    {
                        Directory.CreateDirectory(localFolder);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Console.WriteLine("NOT Found on local: " + codeView.PdbLocalPath);
                        continue;
                    }
                }

                if (File.Exists(localPath))
                {
                    if (Path.GetExtension(localPath).Equals(".pdb", StringComparison.OrdinalIgnoreCase))
                        pdbDownloadedPath = localPath;

                    continue;
                }

                if (CopyPdbFromLocal(modulePath, codeView.PdbFileName, localPath))
                    continue;

                var target = new Uri(baseUri, codeView.PdbUriPath);
                var pdbLocation = GetPdbLocation(target);

                if (pdbLocation == null)
                {
                    var underscorePath = ProbeWithUnderscore(target.AbsoluteUri);
                    pdbLocation = GetPdbLocation(new Uri(underscorePath));
                }

                if (pdbLocation != null)
                {
                    DownloadPdbFile(pdbLocation, localPath);

                    if (Path.GetExtension(localPath).Equals(".pdb", StringComparison.OrdinalIgnoreCase))
                        pdbDownloadedPath = localPath;
                }
                else
                {
                    Console.WriteLine("Not Found on symbol server: " + codeView.PdbFileName);
                }
            }

            return pdbDownloadedPath;
        }

        private static string ProbeWithUnderscore(string path)
        {
            path = path.Remove(path.Length - 1);
            path = path.Insert(path.Length, "_");
            return path;
        }

        private static Uri GetPdbLocation(Uri target)
        {
            var req = System.Net.WebRequest.Create(target) as System.Net.HttpWebRequest;
            req.Method = "HEAD";

            try
            {
                using (var resp = req.GetResponse() as System.Net.HttpWebResponse)
                {
                    return resp.ResponseUri;
                }
            }
            catch (System.Net.WebException)
            {
                return null;
            }
        }

        private static bool CopyPdbFromLocal(string modulePath, string pdbFileName, string localTargetPath)
        {
            if (File.Exists(pdbFileName))
            {
                File.Copy(pdbFileName, localTargetPath);
                return File.Exists(localTargetPath);
            }

            var fileName = Path.GetFileName(pdbFileName);
            var pdbPath = Path.Combine(Environment.CurrentDirectory, fileName);

            if (File.Exists(pdbPath))
            {
                File.Copy(pdbPath, localTargetPath);
                return File.Exists(localTargetPath);
            }

            pdbPath = Path.ChangeExtension(modulePath, ".pdb");
            if (File.Exists(pdbPath))
            {
                File.Copy(pdbPath, localTargetPath);
                return File.Exists(localTargetPath);
            }

            return false;
        }

        private static void DownloadPdbFile(Uri target, string pathToSave)
        {
            var req = System.Net.WebRequest.Create(target) as System.Net.HttpWebRequest;

            using (var resp = req.GetResponse() as System.Net.HttpWebResponse)
            using (var fs = new FileStream(pathToSave, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var bw = new BinaryWriter(fs))
            {
                var reader = new BinaryReader(resp.GetResponseStream());
                var contentLength = resp.ContentLength;

                while (contentLength > 0)
                {
                    var buffer = new byte[4096];
                    var readBytes = reader.Read(buffer, 0, buffer.Length);
                    bw.Write(buffer, 0, readBytes);

                    contentLength -= readBytes;
                }
            }
        }
    }
}
