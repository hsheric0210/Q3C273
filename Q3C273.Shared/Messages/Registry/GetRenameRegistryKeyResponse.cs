﻿using ProtoBuf;

namespace Q3C273.Shared.Messages.Registry
{
    [ProtoContract]
    public class GetRenameRegistryKeyResponse : IMessage
    {
        [ProtoMember(1)]
        public string ParentPath { get; set; }

        [ProtoMember(2)]
        public string OldKeyName { get; set; }

        [ProtoMember(3)]
        public string NewKeyName { get; set; }

        [ProtoMember(4)]
        public bool IsError { get; set; }

        [ProtoMember(5)]
        public string ErrorMsg { get; set; }
    }
}