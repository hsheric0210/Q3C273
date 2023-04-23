using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static Ton618.Utilities.ClientNatives;

namespace Ton618.Utilities
{
    internal class ShellCode
    {
        #region Predefined shellcodes
        public static readonly ShellCode LoadLibraryA_x64 = new ShellCode(new byte[][]
                {
                    new byte[] { 0x53, 0x48, 0x89, 0xe3, 0x48, 0x83, 0xec, 0x20, 0x66, 0x83, 0xe4, 0xc0, 0x48, 0xb9 },
                    new byte[] { 0x48, 0xba },
                    new byte[] { 0xff, 0xd2, 0x48, 0xba },
                    new byte[] { 0x48, 0x89, 0x02, 0x48, 0x89, 0xdc, 0x5b, 0xc3 }
                }, new Type[] { typeof(IntPtr), typeof(IntPtr), typeof(IntPtr) });
        public static readonly ShellCode DllMain_x64 = new ShellCode(new byte[][]
                {
                    new byte[] {0x53, 0x48, 0x89, 0xe3, 0x66, 0x83, 0xe4, 0x00, 0x48, 0xb9 },
                    new byte[] {0xba, 0x01, 0x00, 0x00, 0x00, 0x41, 0xb8, 0x00, 0x00, 0x00, 0x00, 0x48, 0xb8 },
                    new byte[] {0xff, 0xd0, 0x48, 0x89, 0xdc, 0x5b, 0xc3 }
                }, new Type[] { typeof(IntPtr), typeof(IntPtr) });
        public static readonly ShellCode DllMain_x86 = new ShellCode(new byte[][]
                {
                     new byte[] {0x53, 0x89, 0xe3, 0x83, 0xe4, 0xf0, 0xb9 },
                     new byte[] {0xba, 0x01, 0x00, 0x00, 0x00, 0xb8, 0x00, 0x00, 0x00, 0x00, 0x50, 0x52, 0x51, 0xb8 },
                     new byte[] {0xff, 0xd0, 0x89, 0xdc, 0x5b, 0xc3 }
                }, new Type[] { typeof(IntPtr), typeof(IntPtr) });
        public static readonly ShellCode GetProcAddress_x64 = new ShellCode(new byte[][]
                {
                    new byte[] { 0x53, 0x48, 0x89, 0xe3, 0x48, 0x83, 0xec, 0x20, 0x66, 0x83, 0xe4, 0xc0, 0x48, 0xb9 },
                    new byte[] { 0x48, 0xba },
                    new byte[] { 0x48, 0xb8 },
                    new byte[] { 0xff, 0xd0, 0x48, 0xb9 },
                    new byte[] { 0x48, 0x89, 0x01, 0x48, 0x89, 0xdc, 0x5b, 0xc3 }
                }, new Type[] { typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr) });
        public static readonly ShellCode GetProcAddress_x86 = new ShellCode(new byte[][]
                {
                    new byte[] { 0x53, 0x89, 0xe3, 0x83, 0xe4, 0xc0, 0xb8 },
                    new byte[] { 0xb9 },
                    new byte[] { 0x51, 0x50, 0xb8 },
                    new byte[] { 0xff, 0xd0, 0xb9 },
                    new byte[] { 0x89, 0x01, 0x89, 0xdc, 0x5b, 0xc3 }
                }, new Type[] { typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr) });
        #endregion

        private readonly byte[][] codeParts;
        private readonly Type[] expectedParameterTypes;

        public int SizeOfCode { get; }

        public ShellCode(byte[][] codeParts, Type[] expectedParameterTypes)
        {
            this.codeParts = codeParts;
            this.expectedParameterTypes = expectedParameterTypes;
            SizeOfCode = codeParts.Sum(x => x.Length) + expectedParameterTypes.Sum(x => Marshal.SizeOf(x));
        }

        public unsafe void WriteTo(object[] parameters, IntPtr nativeMemory)
        {
            // Validate parameters
            if (parameters.Length != codeParts.Length - 1)
                throw new ArgumentException("Parameter count must be " + (codeParts.Length - 1), nameof(parameters));
            for (int i = 0, j = parameters.Length; i < j; i++)
            {
                var parameter = parameters[i];
                if (parameter == null || parameter.GetType() != expectedParameterTypes[i])
                    throw new ArgumentException($"Expected type {expectedParameterTypes[i]} on parameter {i}, but got {parameters[i]?.GetType()}", nameof(parameters));
            }

            using (var nativeStream = new UnmanagedMemoryStream((byte*)nativeMemory.ToPointer(), SizeOfCode, SizeOfCode, FileAccess.ReadWrite))
            {
                for (int i = 0, j = codeParts.Length - 1; i < j; i++)
                {
                    nativeStream.WriteBytes(codeParts[i]);
                    nativeStream.WriteObject(parameters[i]);
                }

                nativeStream.WriteBytes(codeParts[codeParts.Length - 1]);
            }
        }

        public void ExecuteOn(IntPtr processHandle, params object[] parameters)
        {
            var localMemory = Marshal.AllocHGlobal(SizeOfCode);
            WriteTo(parameters, localMemory);
            var remoteMemory = processHandle.WriteToProcess(localMemory, (UIntPtr)SizeOfCode, PageAccessRights.PAGE_EXECUTE_READWRITE);
            //Marshal.FreeHGlobal(localMemory);

            var threadHandle = processHandle.CreateRemoteThreadAuto(remoteMemory, IntPtr.Zero);
            if (threadHandle == IntPtr.Zero)
                throw new AggregateException("Failed to create shellcode execution thread");
            if (WaitForSingleObject(threadHandle, 10000) != 0)
                throw new AggregateException("Shellcode execution thread exit timed out");
            VirtualFreeEx(processHandle, remoteMemory, UIntPtr.Zero, MemFreeType.MEM_RELEASE);
        }
    }
}
