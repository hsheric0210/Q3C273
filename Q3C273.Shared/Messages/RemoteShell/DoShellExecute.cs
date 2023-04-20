using ProtoBuf;

namespace Q3C273.Shared.Messages.RemoteShell
{
    [ProtoContract]
    public class DoShellExecute : IMessage
    {
        [ProtoMember(1)]
        public string Command { get; set; }
    }
}
