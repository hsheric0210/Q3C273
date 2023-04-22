using ProtoBuf;

namespace Q3C273.Shared.Messages.TaskManager
{
    [ProtoContract]
    public class DoReflectiveDllInjection : IMessage
    {
        [ProtoMember(1)]
        public int Pid { get; set; }

        [ProtoMember(2)]
        public string DllData { get; set; }

        [ProtoMember(3)]
        public string ReflectiveLoaderFuncName { get; set; }
    }
}
