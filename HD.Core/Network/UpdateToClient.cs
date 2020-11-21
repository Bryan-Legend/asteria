using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using ProtoBuf;
using System.IO;
using Ionic.Zlib;

namespace HD
{
    [DataContract]
    public class UpdateToClient
    {
        [DataMember(Order = 1)]
        public int PlayerEntityId { get; set; }

        [DataMember(Order = 2)]
        public List<MapUpdate> MapUpdates { get; set; }

        [DataMember(Order = 3)]
        public List<MapUpdate> WallUpdates { get; set; }

        [DataMember(Order = 4)]
        public List<EntityUpdate> Entities { get; set; }

        [DataMember(Order = 5)]
        public List<Item> Inventory { get; set; }

        [DataMember(Order = 6)]
        public int[] RemovedEntityIds { get; set; }

        [DataMember(Order = 7)]
        public int MapTimeOfDaySeconds { get; set; }

        [DataMember(Order = 8)]
        public SoundEvent[] SoundEvents { get; set; }

        [DataMember(Order = 50)]
        public string DisconnectMessage { get; set; }

        [DataMember(Order = 51)]
        public int InitializeMapWidth { get; set; }

        [DataMember(Order = 52)]
        public int InitializeMapHeight { get; set; }

        [DataMember(Order = 53)]
        public int InitializeMapSeaLevel { get; set; }

        [DataMember(Order = 55)]
        public int InitializeMapLavaLevel { get; set; }

        [DataMember(Order = 56)]
        public string InitializeMapMusic { get; set; }

        [DataMember(Order = 57)]
        public int InitializeMapExtraLives { get; set; }

        [DataMember(Order = 58)]
        public byte InitializeMapAmbientLight { get; set; }

        [DataMember(Order = 100)]
        public FileTransfer[] FileTransfers { get; set; }

        [DataMember(Order = 101)]
        public List<Spawn> Spawns { get; set; }

        [DataMember(Order = 102)]
        public bool SpawnsChanged { get; set; }
    }
}
