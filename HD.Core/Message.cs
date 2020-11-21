using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using ProtoBuf;

namespace HD
{
    [ProtoContract]
    public class Message
    {
        [ProtoMember(1)]
        public MessageType Type { get; set; }

        [ProtoMember(2)]
        public string Text { get; set; }

        public DateTime Created;
    }
}