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
    public sealed class Placeable : Entity
    {
        [DefaultValue(null)]
        public string Owner { get; set; }
        [DefaultValue(null)]
        public string Value { get; set; }
        [DefaultValue(null)]
        public Item[] Inventory { get; set; }
        [DefaultValue(false)]
        public bool Flag { get; set; }
        [DefaultValue(null)]
        public string Name { get; set; }

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
                    BoundingBox = type.BoundingBox;
                else
                    BoundingBox = Rectangle.Empty;
            }
        }

#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public ItemId TypeId
        {
            get { return Type.Id; }
            set { Type = ItemBase.Get(value); }
        }

        public int TypeIdInt
        {
            get { return Type == null ? 0 : (int)Type.Id; }
            set { Type = ItemBase.Get((ItemId)value); }
        }

        public override Color Light { get { return Type.Light; } }

        public override void Think(GameTime gameTime)
        {
            if (!Type.IsFloating) {
                ApplyGravity(gameTime);
                MoveVertical(gameTime);
            }
        }

        public override void LongThink(GameTime gameTime)
        {
            if (Type.OnLongThink != null)
                Type.OnLongThink(this);
        }

        public override void Draw(SpriteBatch spriteBatch, Point screenOffset)
        {
            var location = new Vector2((Position.X - screenOffset.X) + BoundingBox.Left, (Position.Y - screenOffset.Y) + BoundingBox.Top);
            if (Type.OnDraw != null)
                Type.OnDraw(this, spriteBatch, location);
            else
                Type.Draw(spriteBatch, location, (int)Age);
        }

        public override void DrawTop(SpriteBatch spriteBatch, Point screenOffset)
        {
            if (Type.OnDrawTop != null) {
                var location = new Vector2((Position.X - screenOffset.X) + BoundingBox.Left, (Position.Y - screenOffset.Y) + BoundingBox.Top);
                Type.OnDrawTop(this, spriteBatch, location);
            }
        }

        public override string ToString()
        {
            return Type.Name;
        }

        public void InitalizeInventory(int amount = 24)
        {
            Inventory = new Item[amount];
        }

        public bool HasInventoryItems(Placeable placement)
        {
            int slot = 0;
            if (placement.Inventory == null)
                return false;
            {
                while (Inventory[slot] == null) {
                    slot++;
                    if (slot >= Inventory.Length)
                        return false;
                }
            }
            return true;
        }

        public void AddItem(Item item, int slot = -1)
        {
            if (slot == -1) {
                // find a slot
                slot = 0;
                while (Inventory[slot] != null) {
                    slot++;
                    if (slot >= Inventory.Length)
                        return; // Inventory is full
                }
            }

            if (item != null) {
                item.SlotNumber = slot;
                item.SlotType = SlotType.Chest;
                Inventory[slot] = item;
            }
        }

        public void RemoveItem(Item item)
        {
            Inventory[item.SlotNumber] = null;
        }

        public void TriggerSwitch(Player sourcePlayer)
        {
            Map.TriggerSwitches(Name, sourcePlayer, this);
        }

        public bool CanEdit(Player player)
        {
            return Type.CanEdit == null || Type.CanEdit(player, this);
        }

#if WINDOWS
        public override EntityUpdate PrepareForWire(EntityUpdate previous, Player targetPlayer)
        {
            var result = previous as PlaceableUpdate;
            if (result == null)
                result = new PlaceableUpdate();
            var dirty = false;

            result.Id = Id;

            if (result.X != (int)Position.X || result.Y != (int)Position.Y) {
                result.X = (int)Position.X;
                result.Y = (int)Position.Y;
                dirty = true;
            }

            if (SoundEvents.Count > 0) {
                result.SoundEvents = SoundEvents.ToArray();
                //SoundEvents.Clear();
                dirty = true;
            } else
                result.SoundEvents = null;

            if (Inventory != null && IsInventoryDifferent(result.Inventory)) {
                result.InventorySize = Inventory.Length;
                result.Inventory = (Item[])Inventory.Clone();
                dirty = true;
            }

            if (result.Value != Value) {
                result.Value = Value;
                dirty = true;
            }

            if (result.Flag != Flag) {
                result.Flag = Flag;
                dirty = true;
            }

            if (result.IsSolid != IsSolid) {
                result.IsSolid = IsSolid;
                dirty = true;
            }

            if (result.Name != Name) {
                result.Name = Name;
                dirty = true;
            }

            if (result.Owner != Owner) {
                result.Owner = Owner;
                dirty = true;
            }

            if (result.PlayingSound != PlayingSound) {
                result.PlayingSound = PlayingSound;
                dirty = true;
            }

            if (result.Animation != Animation) {
                result.Animation = Animation;
                dirty = true;
            }

            result.TypeId = TypeId;

            return dirty ? result : null;
        }

        bool IsInventoryDifferent(Item[] target)
        {
            if (target == null)
                return true;
            for (int i = 0; i < target.Length; i++) {
                if (Inventory[i] != target[i])
                    return true;
            }
            return false;
        }

        public override void ProcessUpdate(EntityUpdate entityUpdate)
        {
            var update = (PlaceableUpdate)entityUpdate;
            Position = new Vector2(update.X, update.Y);
            TypeId = update.TypeId;
            Value = update.Value;
            Flag = update.Flag;
            Name = update.Name;
            Owner = update.Owner;
            IsSolid = update.IsSolid;
            PlayingSound = update.PlayingSound;

            if (update.InventorySize != 0)
                InitalizeInventory(update.InventorySize);
            if (update.Inventory != null) {
                foreach (var item in update.Inventory)
                    Inventory[item.SlotNumber] = item;
            }

            if (Animation != update.Animation) {
                Animation = update.Animation;
                AnimationStart = Map.Now;
            }
        }
#endif
    }

#if WINDOWS
    [DataContract]
    public class PlaceableUpdate : EntityUpdate
    {
        public override Type TargetType { get { return typeof(Placeable); } }

        [DataMember(Order = 1)]
        public ItemId TypeId { get; set; }

        [DataMember(Order = 2)]
        public int InventorySize { get; set; }

        [DataMember(Order = 3)]
        public Item[] Inventory { get; set; }

        [DataMember(Order = 4)]
        public string Value { get; set; }

        [DataMember(Order = 5)]
        public bool Flag { get; set; }

        [DataMember(Order = 6)]
        public bool IsSolid { get; set; }

        [DataMember(Order = 7)]
        public string Name { get; set; }

        [DataMember(Order = 8)]
        public Sound PlayingSound { get; set; }

        [DataMember(Order = 9)]
        public Animation Animation { get; set; }

        [DataMember(Order = 10)]
        public string Owner { get; set; }
    }
#endif
}