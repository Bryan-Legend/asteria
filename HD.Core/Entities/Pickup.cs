using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization;
using ProtoBuf;

namespace HD
{
    public class Pickup : Entity
    {
        [DefaultValue(null)]
        public Item[] Inventory { get; set; }

        ItemType type;
#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public ItemType Type
        {
            get { return type; }
            set
            {
                type = value;
                if (type != null)
                    BoundingBox = new Rectangle(-ItemType.IconSize / 4, -ItemType.IconSize / 4, ItemType.IconSize / 2, ItemType.IconSize / 2);
                //BoundingBox = new Rectangle(type.BoundingBox.X / 2, type.BoundingBox.Y / 2, type.BoundingBox.Width / 2, type.BoundingBox.Height / 2);
                else
                    BoundingBox = Rectangle.Empty;
            }
        }

        public int TypeId
        {
            get { return (int)Type.Id; }
            set { Type = ItemBase.Get((ItemId)value); }
        }

        public override Color Light { get { return Type.Light; } }
        public int Amount { get; set; }
        public string Owner { get; set; }

        const int initialKnockback = 300;

        public Pickup()
        {
            Velocity = new Vector2(Utility.Next(-initialKnockback, initialKnockback), Utility.Next(-initialKnockback, initialKnockback));
        }

        public override void Think(GameTime gameTime)
        {
            if (Age > 60000) {
                Remove();
                return;
            }

            if (Age > 5000)
                Owner = null;

            //Velocity = Velocity * 0.5f; // dampen speed

            ApplyGravity(gameTime);
            MoveVertical(gameTime);
            MoveHorizontal(gameTime);

            if (IsOnGround)
                Velocity.X = 0;

            base.Think(gameTime);

            var originalBoundingBox = OffsetBoundingBox;
            OffsetBoundingBox.Inflate(50, 50);

            LivingCollision((living) => {
                var player = living as Player;
                if (player != null && player.Name != Owner) {
                    if (WithinRange(player, 50)) {
                        var item = player.GiveItem(Type.Id, Amount);
                        Remove();
                        if (Type.Category == ItemCategory.Useable)
                            player.PlaySound(Sound.PickupUseable);
                        else if (Type.AltCategory == ItemCategory.KeyComponent)
                            player.PlaySound(Sound.PickupKeyComponet);
                        else
                            player.PlaySound(Sound.Pickup);
                        return true;
                    } else {
                        var direction = (Position - player.Position);
                        direction.Normalize();

                        Velocity -= direction * 500;
                    }
                }
                return false;
            });

            OffsetBoundingBox = originalBoundingBox;
        }

        public override void Draw(SpriteBatch spriteBatch, Point screenOffset)
        {
            var position = new Vector2((Position.X - screenOffset.X) + BoundingBox.Left, (Position.Y - screenOffset.Y) + BoundingBox.Top);
            spriteBatch.Draw(Utility.EmptySlotTexture, position, null, Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);

            Type.DrawIcon(spriteBatch, position + new Vector2(1.5f, 1.5f), Color.White, true);

            //Type.Draw(spriteBatch, new Vector2((Position.X - screenOffset.X) + BoundingBox.Left, (Position.Y - screenOffset.Y) + BoundingBox.Top));
        }

        public override string ToString()
        {
            return Type.Name;
        }

#if WINDOWS
        public override EntityUpdate PrepareForWire(EntityUpdate previous, Player targetPlayer)
        {
            var result = previous as PickupUpdate;
            if (result == null)
                result = new PickupUpdate();
            var dirty = false;

            result.Id = Id;

            if (result.X != (int)Position.X || result.Y != (int)Position.Y) {
                result.X = (int)Position.X;
                result.Y = (int)Position.Y;
                dirty = true;
            }

            result.TypeId = Type.Id;

            return dirty ? result : null;
        }


        public override void ProcessUpdate(EntityUpdate entityUpdate)
        {
            var update = (PickupUpdate)entityUpdate;
            Position = new Vector2(update.X, update.Y);
            TypeId = (int)update.TypeId;
        }
#endif
    }

#if WINDOWS
    [DataContract]
    public class PickupUpdate : EntityUpdate
    {
        public override Type TargetType { get { return typeof(Pickup); } }

        [DataMember(Order = 1)]
        public ItemId TypeId { get; set; }
    }
#endif
}