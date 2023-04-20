using ProtoBuf;
using Q3C273.Shared.Models;
using System.Collections.Generic;

namespace Q3C273.Shared.Messages
{
    [ProtoContract]
    public class GetStartupItemsResponse : IMessage
    {
        [ProtoMember(1)]
        public List<StartupItem> StartupItems { get; set; }
    }
}
