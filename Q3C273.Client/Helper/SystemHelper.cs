using Q3C273.Shared.Helpers;
using System;
using System.Management;

namespace Ton618.Helper
{
    public static class SystemHelper
    {
        public static string GetUptime()
        {
            try
            {
                var uptime = string.Empty;

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem WHERE Primary='true'"))
                {
                    foreach (ManagementObject mObject in searcher.Get())
                    {
                        var lastBootUpTime = ManagementDateTimeConverter.ToDateTime(mObject["LastBootUpTime"].ToString());
                        var uptimeSpan = TimeSpan.FromTicks((DateTime.Now - lastBootUpTime).Ticks);

                        uptime = string.Format("{0}d : {1}h : {2}m : {3}s", uptimeSpan.Days, uptimeSpan.Hours, uptimeSpan.Minutes, uptimeSpan.Seconds);
                        break;
                    }
                }

                if (string.IsNullOrEmpty(uptime))
                    throw new Exception("Getting uptime failed");

                return uptime;
            }
            catch (Exception)
            {
                return string.Format("{0}d : {1}h : {2}m : {3}s", 0, 0, 0, 0);
            }
        }

        public static string GetPcName()
        {
            return Environment.MachineName;
        }

        public static string GetAntivirus()
        {
            try
            {
                var antivirusName = string.Empty;
                // starting with Windows Vista we must use the root\SecurityCenter2 namespace
                var scope = PlatformHelper.VistaOrHigher ? "root\\SecurityCenter2" : "root\\SecurityCenter";
                var query = "SELECT * FROM AntivirusProduct";

                using (var searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (ManagementObject mObject in searcher.Get())
                    {
                        antivirusName += mObject["displayName"].ToString() + "; ";
                    }
                }
                antivirusName = StringHelper.RemoveLastChars(antivirusName);

                return !string.IsNullOrEmpty(antivirusName) ? antivirusName : "N/A";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string GetFirewall()
        {
            try
            {
                var firewallName = string.Empty;
                // starting with Windows Vista we must use the root\SecurityCenter2 namespace
                var scope = PlatformHelper.VistaOrHigher ? "root\\SecurityCenter2" : "root\\SecurityCenter";
                var query = "SELECT * FROM FirewallProduct";

                using (var searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (ManagementObject mObject in searcher.Get())
                    {
                        firewallName += mObject["displayName"].ToString() + "; ";
                    }
                }
                firewallName = StringHelper.RemoveLastChars(firewallName);

                return !string.IsNullOrEmpty(firewallName) ? firewallName : "N/A";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
