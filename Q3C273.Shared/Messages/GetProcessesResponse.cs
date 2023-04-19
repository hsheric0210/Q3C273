using ProtoBuf;
using Q3C273.Shared.Models;

namespace Q3C273.Shared.Messages
{
    [ProtoContract]
    public class GetProcessesResponse : IMessage
    {
        [ProtoMember(1)]
        public Process[] Processes { get; set; }
    }
}
