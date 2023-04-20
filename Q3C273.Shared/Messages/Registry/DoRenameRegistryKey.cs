using ProtoBuf;

namespace Q3C273.Shared.Messages.Registry
{
    [ProtoContract]
    public class DoRenameRegistryKey : IMessage
    {
        [ProtoMember(1)]
        public string ParentPath { get; set; }

        [ProtoMember(2)]
        public string OldKeyName { get; set; }

        [ProtoMember(3)]
        public string NewKeyName { get; set; }
    }
}
