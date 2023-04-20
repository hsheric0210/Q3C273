using ProtoBuf;
using Q3C273.Shared.Enums;

namespace Q3C273.Shared.Messages.Status
{
    [ProtoContract]
    public class SetUserStatus : IMessage
    {
        [ProtoMember(1)]
        public UserStatus Message { get; set; }
    }
}
