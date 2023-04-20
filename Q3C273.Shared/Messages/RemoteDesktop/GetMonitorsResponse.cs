using ProtoBuf;

namespace Q3C273.Shared.Messages.RemoteDesktop
{
    [ProtoContract]
    public class GetMonitorsResponse : IMessage
    {
        [ProtoMember(1)]
        public int Number { get; set; }
    }
}
