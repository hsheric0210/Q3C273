using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Q3C273.Shared.Messages.SystemInformation
{
    [ProtoContract]
    public class GetSystemInfoResponse : IMessage
    {
        [ProtoMember(1)]
        public List<Tuple<string, string>> SystemInfos { get; set; }
    }
}
