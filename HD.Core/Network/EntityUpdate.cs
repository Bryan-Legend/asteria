using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using ProtoBuf;

namespace HD
{
    [ProtoInclude(10, typeof(PlayerUpdate))]
    [ProtoInclude(11, typeof(PlaceableUpdate))]
    [ProtoInclude(12, typeof(EnemyUpdate))]
    [ProtoInclude(13, typeof(ProjectileUpdate))]
    [ProtoInclude(14, typeof(CombatTextUpdate))]
    [ProtoInclude(15, typeof(ParticleEmitterUpdate))]
    [ProtoInclude(16, typeof(PickupUpdate))]
    [DataContract]
    public abstract class EntityUpdate
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public int X { get; set; }

        [DataMember(Order = 3)]
        public int Y { get; set; }

        [DataMember(Order = 4)]
        public SoundEvent[] SoundEvents { get; set; }

        public abstract Type TargetType { get; }
    }
}