using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

#pragma warning disable IDE1006 // Naming Styles

#if _KSOBUILD
namespace KernelStructOffset
#else
namespace Quasar.Client.Win32PE.Structs
#endif
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StructFieldInfo
    {
        public readonly string Name;
        public readonly int Offset;
        public readonly string Type;

        public StructFieldInfo(int offset, string name, string type)
        {
            Name = name;
            Offset = offset;
            Type = type;
        }

        public static bool operator ==(StructFieldInfo t1, StructFieldInfo t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(StructFieldInfo t1, StructFieldInfo t2)
        {
            return !t1.Equals(t2);
        }

        public override bool Equals(object obj)
        {
            var target = (StructFieldInfo)obj;

            if (target.Name == Name)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

    }

    public class DllOrderLink
    {
        public IntPtr LoadOrderLink;
        public IntPtr MemoryOrderLink;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CodeViewRSDS
    {
        public uint CvSignature;
        public Guid Signature;
        public uint Age;
        public string PdbFileName;

        public static bool operator ==(CodeViewRSDS t1, CodeViewRSDS t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(CodeViewRSDS t1, CodeViewRSDS t2)
        {
            return !t1.Equals(t2);
        }

        public override bool Equals(object obj)
        {
            var target = (CodeViewRSDS)obj;

            if (target.Signature == Signature && target.Age == Age)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return Signature.GetHashCode() + Age.GetHashCode();
        }

        public string PdbLocalPath
        {
            get
            {
                var fileName = Path.GetFileName(PdbFileName);

                var uid = Signature.ToString("N") + Age;
                return Path.Combine(fileName, uid, fileName);
            }
        }

        public string PdbUriPath
        {
            get
            {
                var fileName = Path.GetFileName(PdbFileName);

                var uid = Signature.ToString("N") + Age;
                return $"{fileName}/{uid}/{fileName}";
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _PROCESS_HANDLE_TABLE_ENTRY_INFO
    {
        public IntPtr HandleValue;
        public UIntPtr HandleCount;
        public UIntPtr PointerCount;
        public uint GrantedAccess;
        public uint ObjectTypeIndex;
        public uint HandleAttributes;
        public uint Reserved;

        public string GetName(int ownerPid, out string handleTypeName)
        {
            return _SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX.GetName(HandleValue, ownerPid, out handleTypeName);
        }
    }

    public struct _PROCESS_HANDLE_SNAPSHOT_INFORMATION
    {
        public UIntPtr HandleCount;
        public UIntPtr Reserved;
        public _PROCESS_HANDLE_TABLE_ENTRY_INFO Handles; /* Handles[0] */

        public int NumberOfHandles
        {
            get { return (int)HandleCount.ToUInt32(); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _SYSTEM_HANDLE_TABLE_ENTRY_INFO
    {
        public short UniqueProcessId;
        public short CreatorBackTraceIndex;
        public byte ObjectType;
        public byte HandleFlags;
        public short HandleValue;
        public IntPtr ObjectPointer;
        public int AccessMask;

        public static bool operator ==(_SYSTEM_HANDLE_TABLE_ENTRY_INFO t1, _SYSTEM_HANDLE_TABLE_ENTRY_INFO t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(_SYSTEM_HANDLE_TABLE_ENTRY_INFO t1, _SYSTEM_HANDLE_TABLE_ENTRY_INFO t2)
        {
            return !t1.Equals(t2);
        }

        public override int GetHashCode()
        {
            return ObjectPointer.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var target = (_SYSTEM_HANDLE_TABLE_ENTRY_INFO)obj;

            if (target.ObjectPointer == ObjectPointer)
                return true;

            return false;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
    {
        public IntPtr ObjectPointer;
        public IntPtr UniqueProcessId;
        public IntPtr HandleValue;
        public uint GrantedAccess;
        public ushort CreatorBackTraceIndex;
        public ushort ObjectTypeIndex;
        public uint HandleAttributes;
        public uint Reserved;

        public int OwnerPid
        {
            get { return UniqueProcessId.ToInt32(); }
        }

        private static Dictionary<string, string> _deviceMap;
        private const int MAX_PATH = 260;
        private const string networkDevicePrefix = "\\Device\\LanmanRedirector\\";

        public override string ToString()
        {
            return $"0x{HandleValue.ToString("x")}(0x{ObjectPointer.ToString("x")})";
        }

        public string GetName(out string handleTypeName)
        {
            return GetName(HandleValue, UniqueProcessId.ToInt32(), out handleTypeName);
        }

        public static string GetName(IntPtr handleValue, int ownerPid, out string handleTypeName)
        {
            var handle = handleValue;
            var dupHandle = IntPtr.Zero;
            handleTypeName = "";

            try
            {
                var addAccessRights = 0;
                dupHandle = DuplicateHandle(ownerPid, handle, addAccessRights);

                if (dupHandle == IntPtr.Zero)
                    return "";

                handleTypeName = GetHandleType(dupHandle);

                switch (handleTypeName)
                {
                    case "EtwRegistration":
                        return "";

                    case "Process":
                        addAccessRights = (int)(ProcessAccessRights.PROCESS_VM_READ | ProcessAccessRights.PROCESS_QUERY_INFORMATION);
                        NativeMethods.CloseHandle(dupHandle);
                        dupHandle = DuplicateHandle(ownerPid, handle, addAccessRights);
                        break;

                    default:
                        break;
                }

                var devicePath = "";

                switch (handleTypeName)
                {
                    case "Process":
                    {
                        var processName = GetProcessName(dupHandle);
                        var processId = NativeMethods.GetProcessId(dupHandle);

                        return $"{processName}({processId})";
                    }

                    case "Thread":
                    {
                        var processName = GetProcessName(ownerPid);
                        var threadId = NativeMethods.GetThreadId(dupHandle);

                        return $"{processName}({ownerPid}): {threadId}";
                    }

                    case "Directory":
                    case "ALPC Port":
                    case "Desktop":
                    case "Event":
                    case "Key":
                    case "Mutant":
                    case "Section":
                    case "Semaphore":
                    case "Token":
                    case "WindowStation":
                    case "File":
                        devicePath = GetObjectNameFromHandle(dupHandle);

                        if (string.IsNullOrEmpty(devicePath) == true)
                            return "";

                        string dosPath;
                        if (ConvertDevicePathToDosPath(devicePath, out dosPath))
                            return dosPath;

                        return devicePath;
                }
            }
            finally
            {
                if (dupHandle != IntPtr.Zero)
                    NativeMethods.CloseHandle(dupHandle);
            }

            return "";
        }

        private static string GetProcessName(int ownerPid)
        {
            var processHandle = IntPtr.Zero;

            try
            {
                processHandle = NativeMethods.OpenProcess(
                    ProcessAccessRights.PROCESS_QUERY_INFORMATION | ProcessAccessRights.PROCESS_VM_READ, false, ownerPid);

                if (processHandle == IntPtr.Zero)
                    return "";

                return GetProcessName(processHandle);
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                    NativeMethods.CloseHandle(processHandle);
            }
        }

        private static string GetProcessName(IntPtr processHandle)
        {
            if (processHandle == IntPtr.Zero)
                return "";

            var sb = new StringBuilder(4096);
            var getResult = NativeMethods.GetModuleFileNameEx(processHandle, IntPtr.Zero, sb, sb.Capacity);
            if (getResult == 0)
                return "";

            try
            {
                return Path.GetFileName(sb.ToString());
            }
            catch (ArgumentException)
            {
                return "";
            }
        }

        private static bool ConvertDevicePathToDosPath(string devicePath, out string dosPath)
        {
            EnsureDeviceMap();
            var i = devicePath.Length;

            while (i > 0 && (i = devicePath.LastIndexOf('\\', i - 1)) != -1)
            {
                if (_deviceMap.TryGetValue(devicePath.Substring(0, i), out var drive))
                {
                    dosPath = string.Concat(drive, devicePath.Substring(i));
                    return dosPath.Length != 0;
                }
            }

            dosPath = string.Empty;
            return false;
        }

        private static void EnsureDeviceMap()
        {
            if (_deviceMap == null)
            {
                var localDeviceMap = BuildDeviceMap();
                Interlocked.CompareExchange(ref _deviceMap, localDeviceMap, null);
            }
        }

        private static Dictionary<string, string> BuildDeviceMap()
        {
            var logicalDrives = Environment.GetLogicalDrives();

            var localDeviceMap = new Dictionary<string, string>(logicalDrives.Length);
            var lpTargetPath = new StringBuilder(MAX_PATH);

            foreach (var drive in logicalDrives)
            {
                var lpDeviceName = drive.Substring(0, 2);
                NativeMethods.QueryDosDevice(lpDeviceName, lpTargetPath, MAX_PATH);
                localDeviceMap.Add(NormalizeDeviceName(lpTargetPath.ToString()), lpDeviceName);
            }

            localDeviceMap.Add(networkDevicePrefix.Substring(0, networkDevicePrefix.Length - 1), "\\");
            return localDeviceMap;
        }

        private static string NormalizeDeviceName(string deviceName)
        {
            if (string.Compare(deviceName, 0, networkDevicePrefix, 0, networkDevicePrefix.Length, StringComparison.InvariantCulture) == 0)
            {
                var shareName = deviceName.Substring(deviceName.IndexOf('\\', networkDevicePrefix.Length) + 1);
                return string.Concat(networkDevicePrefix, shareName);
            }
            return deviceName;
        }

        private static string GetObjectNameFromHandle(IntPtr handle)
        {
            using (var f = new FileNameFromHandleState(handle))
            {
                ThreadPool.QueueUserWorkItem(GetObjectNameFromHandleFunc, f);
                f.WaitOne(16);
                return f.FileName;
            }
        }

        private static void GetObjectNameFromHandleFunc(object obj)
        {
            var state = obj as FileNameFromHandleState;

            var guessSize = 1024;
            NT_STATUS ret;

            var ptr = Marshal.AllocHGlobal(guessSize);

            try
            {
                while (true)
                {
                    ret = NativeMethods.NtQueryObject(state.Handle,
                        OBJECT_INFORMATION_CLASS.ObjectNameInformation, ptr, guessSize, out var requiredSize);

                    if (ret == NT_STATUS.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        Marshal.FreeHGlobal(ptr);
                        guessSize = requiredSize;
                        ptr = Marshal.AllocHGlobal(guessSize);
                        continue;
                    }

                    if (ret == NT_STATUS.STATUS_SUCCESS)
                    {
                        var oti = (OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(ptr, typeof(OBJECT_NAME_INFORMATION));
                        state.FileName = oti.Name.GetText();
                        return;
                    }

                    break;
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);

                state.Set();
            }
        }

        private static string GetHandleType(IntPtr handle)
        {
            var guessSize = 1024;
            NT_STATUS ret;

            var ptr = Marshal.AllocHGlobal(guessSize);

            try
            {
                while (true)
                {
                    ret = NativeMethods.NtQueryObject(handle,
                        OBJECT_INFORMATION_CLASS.ObjectTypeInformation, ptr, guessSize, out var requiredSize);

                    if (ret == NT_STATUS.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        Marshal.FreeHGlobal(ptr);
                        guessSize = requiredSize;
                        ptr = Marshal.AllocHGlobal(guessSize);
                        continue;
                    }

                    if (ret == NT_STATUS.STATUS_SUCCESS)
                    {
                        var oti = (OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(ptr, typeof(OBJECT_TYPE_INFORMATION));
                        return oti.Name.GetText();
                    }

                    break;
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }

            return "(unknown)";
        }

        private static IntPtr DuplicateHandle(int ownerPid, IntPtr targetHandle, int addAccessRights)
        {
            var currentProcess = NativeMethods.GetCurrentProcess();

            var targetProcessHandle = IntPtr.Zero;
            var duplicatedHandle = IntPtr.Zero;

            try
            {
                targetProcessHandle = NativeMethods.OpenProcess(ProcessAccessRights.PROCESS_DUP_HANDLE, false, ownerPid);
                if (targetProcessHandle == IntPtr.Zero)
                    return IntPtr.Zero;

                var dupResult = NativeMethods.DuplicateHandle(targetProcessHandle, targetHandle, currentProcess,
                    out duplicatedHandle, addAccessRights, false,
                     addAccessRights == 0 ? DuplicateHandleOptions.DUPLICATE_SAME_ACCESS : 0);
                if (dupResult == true)
                    return duplicatedHandle;

                return IntPtr.Zero;
            }
            finally
            {
                if (targetProcessHandle != IntPtr.Zero)
                    NativeMethods.CloseHandle(targetProcessHandle);
            }
        }

        class FileNameFromHandleState : IDisposable
        {
            private ManualResetEvent _mr;
            private readonly IntPtr _handle;
            private string _fileName;
            private bool _retValue;

            public IntPtr Handle
            {
                get
                {
                    return _handle;
                }
            }

            public string FileName
            {
                get
                {
                    return _fileName;
                }
                set
                {
                    _fileName = value;
                }

            }

            public bool RetValue
            {
                get
                {
                    return _retValue;
                }
                set
                {
                    _retValue = value;
                }
            }

            public FileNameFromHandleState(IntPtr handle)
            {
                _mr = new ManualResetEvent(false);
                _handle = handle;
            }

            public bool WaitOne(int wait)
            {
                return _mr.WaitOne(wait, false);
            }

            public void Set()
            {
                if (_mr == null)
                    return;

                _mr.Set();
            }

            public void Dispose()
            {
                if (_mr != null)
                {
                    _mr.Close();
                    _mr = null;
                }
            }
        }

        public static bool operator ==(_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX t1, _SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX t2)
        {
            return t1.Equals(t2);
        }

        public static bool operator !=(_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX t1, _SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX t2)
        {
            return !t1.Equals(t2);
        }

        public override int GetHashCode()
        {
            return ObjectPointer.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var target = (_SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)obj;

            if (target.ObjectPointer == ObjectPointer)
                return true;

            return false;
        }
    }
}
