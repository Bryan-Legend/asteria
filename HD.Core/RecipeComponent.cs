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
    public class Component
    {
        public ItemType Type { get { return ItemBase.Get(TypeId); } }
        public ItemId TypeId;
        public int Amount { get; set; }

        public Component()
        {
        }

        public Component(ItemId typeId, int amount = 1)
        {
            TypeId = typeId;
            Amount = amount;
        }

        public override string ToString()
        {
            return Type.Name;
        }

        public static string ToLongString(string name, int amount)
        {
            if (amount > 1)
                return name + " x" + amount.ToString();
            else
                return name;
        }

        public string ToLongString()
        {
            return ToLongString(Type.Name, Amount);
        }
    }
}
