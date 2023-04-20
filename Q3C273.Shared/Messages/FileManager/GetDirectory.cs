using ProtoBuf;

namespace Q3C273.Shared.Messages.FileManager
{
    [ProtoContract]
    public class GetDirectory : IMessage
    {
        [ProtoMember(1)]
        public string RemotePath { get; set; }
    }
}
