using ProtoBuf;

namespace Q3C273.Shared.Messages.Status
{
    [ProtoContract]
    public class SetStatus : IMessage
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
