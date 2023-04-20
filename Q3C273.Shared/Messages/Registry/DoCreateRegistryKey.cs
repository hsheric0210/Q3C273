using ProtoBuf;

namespace Q3C273.Shared.Messages.Registry
{
    [ProtoContract]
    public class DoCreateRegistryKey : IMessage
    {
        [ProtoMember(1)]
        public string ParentPath { get; set; }
    }
}
