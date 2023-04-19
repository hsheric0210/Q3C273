using ProtoBuf;
using Q3C273.Shared.Enums;

namespace Q3C273.Shared.Models
{
    [ProtoContract]
    public class StartupItem
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public string Path { get; set; }

        [ProtoMember(3)]
        public StartupType Type { get; set; }
    }
}
