using ProtoBuf;
using Q3C273.Shared.Models;

namespace Q3C273.Shared.Messages
{
    [ProtoContract]
    public class DoStartupItemRemove : IMessage
    {
        [ProtoMember(1)]
        public StartupItem StartupItem { get; set; }
    }
}
