﻿using ProtoBuf;

namespace Q3C273.Shared.Models
{
    [ProtoContract]
    public class Process
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public int Id { get; set; }

        [ProtoMember(3)]
        public string MainWindowTitle { get; set; }
    }
}