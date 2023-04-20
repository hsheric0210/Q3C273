using ProtoBuf;
using Q3C273.Shared.Models;

namespace Q3C273.Shared.Messages.FileManager
{
    [ProtoContract]
    public class GetDirectoryResponse : IMessage
    {
        [ProtoMember(1)]
        public string RemotePath { get; set; }

        [ProtoMember(2)]
        public FileSystemEntry[] Items { get; set; }
    }
}
