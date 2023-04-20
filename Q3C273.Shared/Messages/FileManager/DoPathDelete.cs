using ProtoBuf;
using Q3C273.Shared.Enums;

namespace Q3C273.Shared.Messages.FileManager
{
    [ProtoContract]
    public class DoPathDelete : IMessage
    {
        [ProtoMember(1)]
        public string Path { get; set; }

        [ProtoMember(2)]
        public FileType PathType { get; set; }
    }
}
