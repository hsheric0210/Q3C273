using Everything.Config;
using Everything.IO;
using Q3C273.Shared.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Everything.Setup
{
    public class ClientUpdater : ClientSetupBase
    {
        public void Update(string newFilePath)
        {
            FileHelper.DeleteZoneIdentifier(newFilePath);

            var bytes = File.ReadAllBytes(newFilePath);
            if (!FileHelper.HasExecutableIdentifier(bytes))
                throw new Exception("No executable file.");

            var batchFile = BatchFile.CreateUpdateBatch(Application.ExecutablePath, newFilePath);

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                FileName = batchFile
            };
            Process.Start(startInfo);

            if (Settings.STARTUP)
            {
                var clientStartup = new ClientStartup();
                clientStartup.RemoveFromStartup(Settings.STARTUPKEY);
            }
        }
    }
}
