using ProtoBuf;

namespace Q3C273.Shared.Messages.RemoteDesktop
{
    [ProtoContract]
    public class GetDesktop : IMessage
    {
        [ProtoMember(1)]
        public bool CreateNew { get; set; }

        [ProtoMember(2)]
        public int Quality { get; set; }

        [ProtoMember(3)]
        public int DisplayIndex { get; set; }
    }
}
