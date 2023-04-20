using ProtoBuf;

namespace Q3C273.Shared.Messages.WebsiteVisitor
{
    [ProtoContract]
    public class DoVisitWebsite : IMessage
    {
        [ProtoMember(1)]
        public string Url { get; set; }

        [ProtoMember(2)]
        public bool Hidden { get; set; }
    }
}
