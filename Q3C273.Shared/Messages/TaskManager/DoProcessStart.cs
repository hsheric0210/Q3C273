using ProtoBuf;

namespace Q3C273.Shared.Messages.TaskManager
{
    [ProtoContract]
    public class DoProcessStart : IMessage
    {
        [ProtoMember(1)]
        public string DownloadUrl { get; set; }

        [ProtoMember(2)]
        public string FilePath { get; set; }

        [ProtoMember(3)]
        public bool IsUpdate { get; set; }
    }
}
