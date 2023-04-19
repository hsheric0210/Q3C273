using ProtoBuf;
using Q3C273.Shared.Models;

namespace Q3C273.Shared.Messages
{
    [ProtoContract]
    public class GetCreateRegistryKeyResponse : IMessage
    {
        [ProtoMember(1)]
        public string ParentPath { get; set; }

        [ProtoMember(2)]
        public RegSeekerMatch Match { get; set; }

        [ProtoMember(3)]
        public bool IsError { get; set; }

        [ProtoMember(4)]
        public string ErrorMsg { get; set; }
    }
}
