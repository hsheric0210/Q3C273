using ProtoBuf;

namespace Q3C273.Shared.Messages
{
    [ProtoContract]
    public class DoKeyboardEvent : IMessage
    {
        [ProtoMember(1)]
        public byte Key { get; set; }

        [ProtoMember(2)]
        public bool KeyDown { get; set; }
    }
}
