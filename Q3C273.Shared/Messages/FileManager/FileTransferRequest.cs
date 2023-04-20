﻿using ProtoBuf;

namespace Q3C273.Shared.Messages.FileManager
{
    [ProtoContract]
    public class FileTransferRequest : IMessage
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string RemotePath { get; set; }
    }
}
