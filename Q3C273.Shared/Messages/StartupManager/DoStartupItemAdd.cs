using ProtoBuf;
using Q3C273.Shared.Models;

namespace Q3C273.Shared.Messages.StartupManager
{
    [ProtoContract]
    public class DoStartupItemAdd : IMessage
    {
        [ProtoMember(1)]
        public StartupItem StartupItem { get; set; }
    }
}
