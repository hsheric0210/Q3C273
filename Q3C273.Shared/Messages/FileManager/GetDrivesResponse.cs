using ProtoBuf;
using Q3C273.Shared.Models;

namespace Q3C273.Shared.Messages
{
    [ProtoContract]
    public class GetDrivesResponse : IMessage
    {
        [ProtoMember(1)]
        public Drive[] Drives { get; set; }
    }
}
