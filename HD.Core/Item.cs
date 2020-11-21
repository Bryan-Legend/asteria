using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using Microsoft.Xna.Framework.Audio;
using System.Runtime.Serialization;
using ProtoBuf;

namespace HD
{
    [ProtoContract]
    public class Item
    {
        public ItemType Type;

#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public ItemId TypeId
        {
            get { if (Type == null) return ItemId.None; return Type.Id; }
            set { Type = ItemBase.Get(value); }
        }

        [ProtoMember(1)]
        public int TypeIdInt
        {
            get { return (int)Type.Id; }
            set { Type = ItemBase.Get((ItemId)value); }
        }

        [DefaultValue(1)]
        [ProtoMember(2)]
        public int Amount { get; set; }

        [DefaultValue(SlotType.Inventory)]
        [ProtoMember(3)]
        public SlotType SlotType { get; set; }

        [DefaultValue(-1)]
        [ProtoMember(4)]
        public int SlotNumber { get; set; }

        [DefaultValue(0)]
        [ProtoMember(5)]
        public int Tier { get; set; }

        public DateTime LastUse;
        public DateTime StartUse;
        public Player Player;

        public double UseLength
        {
            get
            {
                if (Player == null)
                    return 0;
                return (Player.Map.Now - StartUse).TotalMilliseconds;
            }
        }

        public Item()
        {
            Amount = 1;
            SlotNumber = -1;
            SlotType = SlotType.Inventory;
        }

        public override string ToString()
        {
            return Type.Name;
        }

        public static string ToLongString(string name, int amount)
        {
            if (amount > 1)
                return name + " x" + amount.ToString("N0");
            else
                return name;
        }

        public string ToLongString()
        {
            return ToLongString(Type.Name, Amount);
        }

        public Item Clone()
        {
            return (Item)MemberwiseClone();
        }

        public bool Use(Player player, bool alreadyUsing = false)
        {
            if (Type.IsManualFire && alreadyUsing)
                return false;

            if (Type.CoolDown > 0 && Type.Category != ItemCategory.Placable)
            {
                if ((player.Map.Now - LastUse).TotalMilliseconds < Type.CoolDown)
                    return false;
            }

            if (Type.CanUse != null && !Type.CanUse(player, alreadyUsing))
                return false;

            if (Type.Category == ItemCategory.Placable)
            {
                if (alreadyUsing)
                    return false;

                if (!Place(player, player.GetToolTargetPosition()))
                    return false;
            }
            else
            {
                if (Type.IsConsumed)
                    player.RemoveItem(this, 1);
            }

            Player = player;
            LastUse = player.Map.Now;
            if (Type.OnUse != null)
                Type.OnUse(this);

            return true;
        }

        public bool CanPlace(Player source, Vector2 location)
        {
            if (!source.WithinRange(location, 125))
                return false;

            var box = Type.BoundingBox;
            box.Offset((int)location.X, (int)location.Y);

            if (source.Map.IsLooseOrSolid(box))
                return false;

            //if (source.Map.FindEntity(box) != null)
            //    return false;

            return true;
        }

        public bool Place(Player source, Vector2 location)
        {
            var canPlace = CanPlace(source, location);
            if (canPlace)
            {
                source.Map.AddPlaceable(source, Type, location);
                source.PlaySound(Sound.PlaceItem);

                source.RemoveItem(this, 1);
            }
            return canPlace;
        }

        public int GetTierAmount(double factor)
        {
            return Utility.GetTierAmount(GetTier(), factor);
        }

        public int GetTier()
        {
            if (Tier == 0)
                return Type.Tier;
            return Tier;
        }

        public void StartUsing(Player source)
        {
            Player = source;
            StartUse = Player.Map.Now;

            if (Type.OnStartUsing != null)
                Type.OnStartUsing(this);
        }

        public void StopUsing()
        {
            if (Type.OnStopUsing != null && Player != null)
                Type.OnStopUsing(this);
        }
    }
}
