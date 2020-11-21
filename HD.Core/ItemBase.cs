using System;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace HD
{
    public static class ItemBase
    {
        public static List<ItemType> Types;
        public static Dictionary<ItemId, ItemType> TypesById;
        public static Dictionary<string, ItemType> TypesByName;
        public static Dictionary<Material, ItemType> TypesByMaterial;
        public static List<Item> Items;

        static ItemBase()
        {
            Types = new List<ItemType>();
            TypesById = new Dictionary<ItemId, ItemType>();
            TypesByName = new Dictionary<string, ItemType>();
            TypesByMaterial = new Dictionary<Material, ItemType>();
            Items = new List<Item>();

            foreach (var itemId in Utility.GetEnumValues<ItemId>())
            {
                var material = (Material)Enum.Parse(typeof(Material), itemId.ToString(), true);
                var materialInfo = MaterialInfo.Get(material);

                AddItem(new ItemType()
                {
                    Id = itemId,
                    Tier = materialInfo.Hardness > 9 ? 0 : materialInfo.Hardness,
                    Category = ItemCategory.Material,
                    Material = material,
                    Description = "Use to place material.\nShift-click to place as wall.",
                    ListPriority = 98,
                    CoolDown = 0,
                    PlaceAndActivateRange = 200,
                    SpriteWidth = 24,
                    SpriteHeight = 24,
                    OnUse = (item) => { item.Player.PlaceMaterial(item); },
                    Rarity = materialInfo.Rarity,
                });

                if (itemId == ItemId.Boundry)
                    break;
            }
        }

        public static void AddItem(ItemType itemType)
        {
            if (itemType.NameSource != null)
                itemType.Name = itemType.FormatWith(itemType.NameSource);

            if (String.IsNullOrEmpty(itemType.Name))
            {
                itemType.Name = itemType.Id.ToString().AddSpaces();
            }

            if (itemType.Rarity == Rarity.Common && itemType.Category != ItemCategory.Material && itemType.Category != ItemCategory.Useable)
            {
                if (itemType.Category == ItemCategory.Component)
                    itemType.Rarity = Rarity.Epic;
                else
                    itemType.Rarity = Rarity.Rare;
            }

            Types.Add(itemType);
            TypesById.Add(itemType.Id, itemType);
            TypesByName.Add(itemType.Name.ToUpperInvariant(), itemType);

            if (itemType.Material != Material.None)
            {
                TypesByMaterial[itemType.Material] = itemType;
                MaterialInfo.MaterialTypes[(byte)itemType.Material].Item = new Item() { Type = itemType, SlotNumber = (int)itemType.Id };
            }

            if (itemType.Category != ItemCategory.Material)
                Items.Add(new Item() { Type = itemType, SlotNumber = (int)itemType.Id });

            itemType.CategoryString = itemType.Category.ToString().AddSpaces();

            if (itemType.Description != null)
            {
                itemType.ActualDescription = itemType.FormatWith(itemType.Description, null,
                    (value) =>
                    {
                        double amount;
                        if (Double.TryParse(value, out amount))
                            return Utility.GetTierAmount(itemType.Tier, amount).ToString("N0");
                        return "";
                    });
            }

            if (itemType.Recipe != null)
            {
                itemType.Recipe.Creates = new Item() { Type = itemType, Amount = itemType.Recipe.CreateAmount };
                RecipeBase.AddRecipe(itemType.Recipe);
            }
        }

        public static ItemType Get(string name)
        {
            name = name.ToUpperInvariant();
            if (!TypesByName.ContainsKey(name))
                return null;
            return TypesByName[name];
        }

        public static ItemType Get(ItemId id)
        {
            if (!TypesById.ContainsKey(id))
                return null;
            return TypesById[id];
        }

        //static FileSystemWatcher watcher;
        public static void LoadContent(GraphicsDevice device)
        {
            Utility.LogMessage("Loading item textures.");

            Utility.MissingTexture = Utility.LoadTexture(device, "Items/Missing.png");
            foreach (var type in Types)
            {
                type.Texture = Utility.LoadTexture(device, String.Format("Items/{0}.png", type.Name.ToString()));
                if (!String.IsNullOrEmpty(type.AlternateTextureName))
                    type.AlternateTexture = Utility.LoadTexture(device, String.Format("Items/{0}.png", type.AlternateTextureName));
                if (type.Texture == null)
                {
                    type.Texture = Utility.MissingTexture;
                    type.IsTextureMissing = true;
                }
                if (type.SpriteWidth == 0)
                    type.SpriteWidth = type.Texture.Width;
                if (type.SpriteHeight == 0)
                    type.SpriteHeight = type.Texture.Height;
                switch (type.SpriteAnimation)
                {
                    case SpriteAnimation.Cycle4:
                    case SpriteAnimation.Oscillate4:
                        type.SpriteWidth /= 2;
                        type.SpriteHeight /= 2;
                        break;
                    case SpriteAnimation.Cycle6:
                    case SpriteAnimation.Oscillate6:
                        type.SpriteWidth /= 2;
                        type.SpriteHeight /= 3;
                        break;
                    //case SpriteAnimation.ItermittentCycle10:
                    case SpriteAnimation.Cycle10:
                        type.SpriteWidth /= 2;
                        type.SpriteHeight /= 5;
                        type.SpritesPerRow = 2;
                        break;
                    case SpriteAnimation.Cycle18:
                        type.SpriteWidth /= 3;
                        type.SpriteHeight /= 6;
                        type.SpritesPerRow = 3;
                        break;
                    case SpriteAnimation.Cycle9:
                    case SpriteAnimation.Oscillate9:
                        type.SpriteWidth /= 3;
                        type.SpriteHeight /= 3;
                        type.SpritesPerRow = 3;
                        break;
                }

                type.IconTexture = Utility.LoadTexture(device, String.Format("Items/{0} Icon.png", type.Name.ToString()));
                if (type.IconTexture == null)
                    type.IconTexture = type.Texture;
            }

            //if (watcher == null)
            //{
            //    watcher = new FileSystemWatcher(Path.Combine(Utility.ContentDirectory, "Items"));
            //    watcher.Created += (sender, e) => { LoadContent(device); };
            //    watcher.Changed += (sender, e) => { LoadContent(device); };
            //    watcher.EnableRaisingEvents = true;
            //}
        }

#if WINDOWS
        public static void LoadItemSizes()
        {
            var itemFolder = Path.GetFullPath(Path.Combine(Utility.ContentDirectory, "Items"));
            Utility.LogMessage("Loading item sizes from " + itemFolder);
            if (!Directory.Exists(itemFolder))
                throw new Exception("Unable to load item sizes from " + itemFolder);
            foreach (var type in Types)
            {
                var bitmap = Utility.OpenPng(String.Format("Items/{0}.png", type.Name.ToString()));
                if (bitmap == null)
                {
                    type.IsTextureMissing = true;
                    if (type.SpriteWidth == 0)
                        type.SpriteWidth = 16;
                    if (type.SpriteHeight == 0)
                        type.SpriteHeight = 16;
                }
                else
                {
                    if (type.SpriteWidth == 0)
                        type.SpriteWidth = bitmap.PixelWidth;
                    if (type.SpriteHeight == 0)
                        type.SpriteHeight = bitmap.PixelHeight;
                }

                switch (type.SpriteAnimation)
                {
                    case SpriteAnimation.Cycle4:
                    case SpriteAnimation.Oscillate4:
                        type.SpriteWidth /= 2;
                        type.SpriteHeight /= 2;
                        break;
                    case SpriteAnimation.Cycle6:
                    case SpriteAnimation.Oscillate6:
                        type.SpriteWidth /= 2;
                        type.SpriteHeight /= 3;
                        break;
                    //case SpriteAnimation.ItermittentCycle10:
                    case SpriteAnimation.Cycle10:
                        type.SpriteWidth /= 2;
                        type.SpriteHeight /= 5;
                        type.SpritesPerRow = 2;
                        break;
                    case SpriteAnimation.Cycle18:
                        type.SpriteWidth /= 3;
                        type.SpriteHeight /= 6;
                        type.SpritesPerRow = 3;
                        break;
                    case SpriteAnimation.Cycle9:
                    case SpriteAnimation.Oscillate9:
                        type.SpriteWidth /= 3;
                        type.SpriteHeight /= 3;
                        type.SpritesPerRow = 3;
                        break;
                }
            }
        }
#endif
    }
}
