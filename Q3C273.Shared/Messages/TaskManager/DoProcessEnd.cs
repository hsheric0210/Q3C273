using ProtoBuf;

namespace Q3C273.Shared.Messages.TaskManager
{
    [ProtoContract]
    public class DoProcessEnd : IMessage
    {
        [ProtoMember(1)]
        public int Pid { get; set; }
    }
}
