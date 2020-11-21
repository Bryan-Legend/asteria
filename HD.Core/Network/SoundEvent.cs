using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using ProtoBuf;

namespace HD
{
    [ProtoContract]
    public class SoundEvent
    {
        [ProtoMember(1)]
        public Sound Sound { get; set; }

        [ProtoMember(2)]
        public float Volume { get; set; }

        [ProtoMember(3)]
        public int EnemyTypeId { get; set; }

        [ProtoMember(4)]
        public int X { get; set; }

        [ProtoMember(5)]
        public int Y { get; set; }
    }
}