using ProtoBuf;
using Q3C273.Shared.Models;

namespace Q3C273.Shared.Messages
{
    [ProtoContract]
    public class GetConnectionsResponse : IMessage
    {
        [ProtoMember(1)]
        public TcpConnection[] Connections { get; set; }
    }
}
