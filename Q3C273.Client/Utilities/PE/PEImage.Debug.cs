using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Ton618.Utilities.PE
{
    public partial class PEImage
    {
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

        public IEnumerable<IMAGE_DEBUG_DIRECTORY> EnumerateDebugDir()
        {
            if (Debug.VirtualAddress == 0)
                yield break;

            var debugDirPtr = GetSafeBuffer(Debug.VirtualAddress, Debug.Size, out var buffer);

            try
            {
                var safeObj = new IMAGE_DEBUG_DIRECTORY();
                var sizeOfDir = Marshal.SizeOf(safeObj);

                var count = Debug.Size / sizeOfDir;

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

                if (!Directory.Exists(localFolder))
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
