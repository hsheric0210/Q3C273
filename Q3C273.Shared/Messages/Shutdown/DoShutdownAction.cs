using ProtoBuf;
using Q3C273.Shared.Enums;

namespace Q3C273.Shared.Messages.Shutdown
{
    [ProtoContract]
    public class DoShutdownAction : IMessage
    {
        [ProtoMember(1)]
        public ShutdownAction Action { get; set; }
    }
}
