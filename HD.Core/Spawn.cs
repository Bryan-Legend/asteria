using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using ProtoBuf;

namespace HD
{
    [ProtoContract]
    public class Spawn
    {
        //[TypeConverter(typeof(Vector2Converter))]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector2 Position { get; set; }
        public DateTime SpawnRemoved;
        public Enemy Spawned;
        public bool IsDead;

        EnemyType type;
#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public EnemyType Type
        {
            get { return type; }
            set { type = value; }
        }

        [ProtoMember(1)]
        public int TypeId
        {
            get { return Type.Id; }
            set { Type = EnemyBase.Get(value); }
        }

        [ProtoMember(2)]
#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public int PositionX
        {
            get { return (int)Position.X; }
            set { Position = new Vector2(value, Position.Y); }
        }

        [ProtoMember(3)]
#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public int PositionY
        {
            get { return (int)Position.Y; }
            set { Position = new Vector2(Position.X, value); }
        }

        public Rectangle OffsetBoundingBox
        {
            get
            {
                var box = Type.BoundingBox;
                box.Offset((int)Position.X, (int)Position.Y);
                return box;
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch, Point screenOffset)
        {
            var box = OffsetBoundingBox;
            box.Offset(-screenOffset.X, -screenOffset.Y);
            spriteBatch.DrawRectangle(box, Color.FromNonPremultiplied(0, 255, 0, 64));
            spriteBatch.DrawString(Utility.SmallFont, Type.Name, new Vector2(box.X, box.Y), Color.White);
        }

        const int spawnCheckDistanceSquared = 3000 * 3000;

        public Enemy SpawnCheck(Map map, Vector2 checkPosition = default(Vector2), bool checkTimeout = true)
        {
            if (Spawned != null || Type == null)
                return null;

            if (checkTimeout && SpawnRemoved.AddSeconds(15) > map.Now)
                return null;

            if (Type.IsBoss && IsDead && SpawnRemoved.AddMinutes(3) > map.Now)
                return null;

            float distanceSquared;
            var closestPlayer = map.FindClosestPlayer(Position, out distanceSquared);

            if (checkPosition != default(Vector2))
            {
                if ((Position - checkPosition).LengthSquared() > spawnCheckDistanceSquared)
                    return null;
            }
            else
            {
                if (distanceSquared > spawnCheckDistanceSquared || closestPlayer == null)
                    return null;
            }

            if (closestPlayer != null && closestPlayer.IsOnScreen(OffsetBoundingBox))
                return null;

            Spawned = map.AddEnemy(Type, Position);
            Spawned.Spawn = this;
            return Spawned;
        }
    }
}