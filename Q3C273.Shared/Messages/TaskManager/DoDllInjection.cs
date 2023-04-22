using ProtoBuf;

namespace Q3C273.Shared.Messages.TaskManager
{
    [ProtoContract]
    public class DoDllInjection : IMessage
    {
        [ProtoMember(1)]
        public string DllDownloadUrl { get; set; }

        [ProtoMember(2)]
        public string DllFilePath { get; set; }

        [ProtoMember(3)]
        public bool IsUpdate { get; set; }
    }
}
