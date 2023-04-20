using ProtoBuf;
using Q3C273.Shared.Enums;

namespace Q3C273.Shared.Messages.TaskManager
{
    [ProtoContract]
    public class DoProcessResponse : IMessage
    {
        [ProtoMember(1)]
        public ProcessAction Action { get; set; }

        [ProtoMember(2)]
        public bool Result { get; set; }
    }
}
