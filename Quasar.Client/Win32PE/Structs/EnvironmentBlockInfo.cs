using Quasar.Client.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Everything.Win32PE.Structs
{
    public class EnvironmentBlockInfo
    {
        private readonly static byte[] _x64TebBytes =
        {
            0x40, 0x57, // push rdi
        
            0x65, 0x48, 0x8B, 0x04, 0x25, 0x30, 0x00, 0x00, 0x00, // mov rax, qword ptr gs:[30h]

            0x5F, // pop rdi
            0xC3, // ret
        };

        static IntPtr _codePointer;
        static GetTebDelegate _getTebDelg;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long GetTebDelegate();

        static EnvironmentBlockInfo()
        {
            if (IntPtr.Size != 8)
                return;

            var codeBytes = _x64TebBytes;

            _codePointer = ClientNatives.VirtualAlloc(IntPtr.Zero, new UIntPtr((uint)codeBytes.Length),
                AllocationType.COMMIT | AllocationType.RESERVE,
                MemoryProtection.EXECUTE_READWRITE
            );

            Marshal.Copy(codeBytes, 0, _codePointer, codeBytes.Length);

            _getTebDelg = (GetTebDelegate)Marshal.GetDelegateForFunctionPointer(
                _codePointer, typeof(GetTebDelegate));
        }

        public static IntPtr GetTebAddress()
        {
            if (IntPtr.Size == 8)
            {
                if (_getTebDelg == null)
                    throw new ObjectDisposedException("GetTebAddress");

                return new IntPtr(_getTebDelg());
            }
            else
            {
                return ClientNatives.NtCurrentTeb();
            }
        }

        public static IntPtr GetPebAddress(out IntPtr tebAddress)
        {
            tebAddress = GetTebAddress();
            var teb = _TEB.Create(tebAddress);
            return teb.ProcessEnvironmentBlock;
        }

        public static _PEB GetPeb()
        {
            var pebAddress = GetPebAddress(out _);
            return GetPeb(pebAddress);
        }

        public static _PEB GetPeb(IntPtr pebAddress)
        {
            return _PEB.Create(pebAddress);
        }
    }
}
