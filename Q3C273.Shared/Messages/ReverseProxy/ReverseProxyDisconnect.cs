using ProtoBuf;
using Q3C273.Shared.Messages;

namespace Q3C273.Shared.Messages.ReverseProxy
{
    [ProtoContract]
    public class ReverseProxyDisconnect : IMessage
    {
        [ProtoMember(1)]
        public int ConnectionId { get; set; }
    }
}
