using ProtoBuf;

namespace Q3C273.Shared.Messages.TaskManager
{
    [ProtoContract]
    public class DoReflectiveDllInjectionResponse : IMessage
    {
        [ProtoMember(1)]
        public int ProcessId { get; set; }

        [ProtoMember(2)]
        public ulong RemoteAddress { get; set; }
    }
}
