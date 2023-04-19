using ProtoBuf;

namespace Q3C273.Shared.Models
{
    [ProtoContract]
    public class Drive
    {
        [ProtoMember(1)]
        public string DisplayName { get; set; }

        [ProtoMember(2)]
        public string RootDirectory { get; set; }
    }
}
