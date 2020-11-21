using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HD
{
    public static class EnemyBase
    {
        public static List<EnemyType> Types;
        public static Dictionary<int, EnemyType> TypesById;
        public static Dictionary<string, EnemyType> TypesByName;
        public static List<Item> Items;

        static EnemyBase()
        {
            Types = new List<EnemyType>();
            TypesByName = new Dictionary<string, EnemyType>();
            TypesById = new Dictionary<int, EnemyType>();
            Items = new List<Item>();
        }

        public static void AddItem(EnemyType type)
        {
            if (type.Id == 0)
                throw new Exception("Enemy must have a unique id set.");

            if (type.BoundingBox == default(Rectangle))
                type.BoundingBox = new Rectangle(-type.SpriteWidth / 2, -type.SpriteHeight / 2, type.SpriteWidth, type.SpriteHeight);

            Types.Add(type);
            TypesById.Add(type.Id, type);
            if (!TypesByName.ContainsKey(type.Name))
                TypesByName.Add(type.Name, type);
            Items.Add(new Item() { Type = new ItemType() { Name = type.Name, ActualDescription = type.Description, IsTextureMissing = true }, SlotNumber = type.Id });
        }

        public static EnemyType Get(string name)
        {
            EnemyType result = null;
            TypesByName.TryGetValue(name, out result);
            return result;
        }

        public static EnemyType Get(int id)
        {
            EnemyType result = null;
            TypesById.TryGetValue(id, out result);
            return result;
        }
    }
}