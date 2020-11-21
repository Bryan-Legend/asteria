using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace HD
{
    public class MaterialInfo
    {
        public static Texture2D MaterialsTexture;
        public static MaterialInfo[] MaterialTypes;
        static Dictionary<string, Material> materialsByName = new Dictionary<string,Material>();

        public Material Id { get; set; }
        public Color DrawColor { get; set; }
        Texture2D texture;
        public int Hardness = 0;
        Color light = Color.Transparent;
        public Color Light
        {
            get { return light; }
            set
            {
                light = value;
                LightPackedValue = light.PackedValue;
            }
        }
        public uint LightPackedValue;
        public Item Item;
        public int TouchDamage;
        public int ImmersionDamage;
        public bool IsTextureLoaded;
        public bool IsTransparent;
        public Point TextureOffset;
        public float MaxVelocity;
        public Sound EnterSound;
        public Sound ExitSound;
        public Rarity Rarity;
        public Material DigMaterial;

        static MaterialInfo()
        {
            var tempList = new List<MaterialInfo>();

            foreach (Material type in Utility.GetEnumValues<Material>()) {
                var materialInfo = new MaterialInfo() { Id = type, IsVisible = !type.ToString().StartsWith("Reserved") };
                if (IsGas(type))
                    materialInfo.Hardness = 0;
                else if (IsLiquid(type))
                    materialInfo.Hardness = 1;

                tempList.Add(materialInfo);
                materialsByName[type.ToString().ToUpperInvariant()] = type;
            }

            MaterialTypes = (from t in tempList orderby t.Id select t).ToArray();

            MaterialTypes[(byte)Material.Boundry].Hardness = Int32.MaxValue;

            //liquids
            var material = MaterialTypes[(byte)Material.Air];
            material.DrawColor = Color.Transparent;

            MaterialTypes[(byte)Material.Steam].DrawColor = Color.FromNonPremultiplied(200, 200, 200, 64);
            MaterialTypes[(byte)Material.Smoke].DrawColor = Utility.HexToColor("60000000");

            material = MaterialTypes[(byte)Material.NaturalGas];
            material.ImmersionDamage = 64;

            material = MaterialTypes[(byte)Material.PoisonGas];
            material.ImmersionDamage = 128;

            material = MaterialTypes[(byte)Material.Vacuum];
            material.ImmersionDamage = 256;

            material = Get(Material.Fire);
            material.DrawColor = Utility.HexToColor("90FF0000");
            material.Light = Color.FromNonPremultiplied(255, 228, 3, 250);
            material.ImmersionDamage = 4;

            material = Get(Material.BlueFire);
            material.Light = Color.FromNonPremultiplied(160, 58, 255, 250);
            material.ImmersionDamage = 12;

            material = MaterialTypes[(byte)Material.Water];
            material.DrawColor = Color.FromNonPremultiplied(0, 0, 255, 128);
            material.MaxVelocity = 200f;
            material.EnterSound = Sound.EnterWater;
            material.ExitSound = Sound.ExitWater;

            material = MaterialTypes[(byte)Material.Lava];
            material.DrawColor = Utility.HexToColor("C8AA0000");
            material.Light = Color.FromNonPremultiplied(226, 170, 4, 250);
            material.ImmersionDamage = 512;
            material.EnterSound = Sound.EnterLava;
            material.ExitSound = Sound.ExitLava;
            material.MaxVelocity = 10f;

            material = MaterialTypes[(byte)Material.Oil];
            material.DrawColor = Utility.HexToColor("000042");
            material.EnterSound = Sound.EnterWater;
            material.ExitSound = Sound.ExitWater;
            material.MaxVelocity = 20f;

            material = MaterialTypes[(byte)Material.Slime];
            material.DrawColor = Utility.HexToColor("009342");
            material.ImmersionDamage = 3;
            material.MaxVelocity = 15f;
            material.EnterSound = Sound.EnterSlime;
            material.ExitSound = Sound.ExitSlime;

            material = MaterialTypes[(byte)Material.Acid];
            material.DrawColor = Utility.HexToColor("009342");
            material.ImmersionDamage = 32;
            material.EnterSound = Sound.EnterAcid;
            material.ExitSound = Sound.ExitAcid;
            material.MaxVelocity = 20f;

            material = MaterialTypes[(byte)Material.Tar];
            material.DrawColor = Utility.HexToColor("976802");
            material.MaxVelocity = 5f;
            material.EnterSound = Sound.EnterTar;
            material.ExitSound = Sound.ExitTar;

            //loose materials
            material = MaterialTypes[(byte)Material.Ash];
            material.DrawColor = Utility.HexToColor("524C56");
            material.Hardness = 1;

            material = MaterialTypes[(byte)Material.Sand];
            material.DrawColor = Utility.HexToColor("D1CD8A");
            material.Hardness = 1;

            material = MaterialTypes[(byte)Material.Snow];
            material.DrawColor = Utility.HexToColor("D3FFFF");
            material.Hardness = 1;

            material = MaterialTypes[(byte)Material.Gravel];
            material.DrawColor = Utility.HexToColor("000000");
            material.Hardness = 1;

            //solid
            material = MaterialTypes[(byte)Material.Grass];
            material.DrawColor = Utility.HexToColor("19CA58");
            material.DigMaterial = Material.Dirt;
            material.Hardness = 1;

            material = MaterialTypes[(byte)Material.Dirt];
            material.DrawColor = Utility.HexToColor("976B4B");
            material.Hardness = 1;

            material = MaterialTypes[(byte)Material.Clay];
            material.DrawColor = Utility.HexToColor("A4584C");
            material.Hardness = 1;

            material = MaterialTypes[(byte)Material.Sandstone];
            material.DrawColor = Utility.HexToColor("787878");
            material.Hardness = 2;

            material = MaterialTypes[(byte)Material.Limestone];
            material.DrawColor = Utility.HexToColor("5A7C5A");
            material.Hardness = 3;

            material = MaterialTypes[(byte)Material.Quartzite];
            material.DrawColor = Utility.HexToColor("716161");
            material.Hardness = 4;

            material = MaterialTypes[(byte)Material.Granite];
            material.DrawColor = Utility.HexToColor("515151");
            material.Hardness = 5;

            material = MaterialTypes[(byte)Material.Marble];
            material.DrawColor = Utility.HexToColor("9B9FA3");
            material.Hardness = 6;

            material = MaterialTypes[(byte)Material.Rhyolite];
            material.DrawColor = Utility.HexToColor("694642");
            material.Hardness = 7;

            material = MaterialTypes[(byte)Material.Basalt];
            material.DrawColor = Utility.HexToColor("8C232B");
            material.Hardness = 8;
            material.TouchDamage = 32;

            // ores
            material = MaterialTypes[(byte)Material.Coal];
            material.DrawColor = Utility.HexToColor("000010");
            material.Hardness = 1;
            material.Rarity = Rarity.Uncommon;

            material = MaterialTypes[(byte)Material.Sulfur];
            material.DrawColor = Utility.HexToColor("FFF12D");
            material.Hardness = 1;
            material.Rarity = Rarity.Uncommon;

            material = MaterialTypes[(byte)Material.IronOre];
            material.DrawColor = Utility.HexToColor("84614D");
            material.Hardness = 1;
            material.Rarity = Rarity.Uncommon;

            material = MaterialTypes[(byte)Material.AluminumOre];
            material.Hardness = 2;
            material.Rarity = Rarity.Uncommon;

            material = MaterialTypes[(byte)Material.CopperOre];
            material.DrawColor = Utility.HexToColor("AB5216");
            material.Hardness = 2;
            material.Rarity = Rarity.Uncommon;

            material = MaterialTypes[(byte)Material.SilverOre];
            material.DrawColor = Utility.HexToColor("8A9699");
            material.Hardness = 3;
            material.Rarity = Rarity.Uncommon;

            material = MaterialTypes[(byte)Material.GoldOre];
            material.DrawColor = Utility.HexToColor("B9A417");
            material.Hardness = 4;
            material.Rarity = Rarity.Uncommon;

            material = MaterialTypes[(byte)Material.Dilithium];
            material.Hardness = 5;
            material.Light = Color.FromNonPremultiplied(71, 89, 185, 32);
            material.TouchDamage = 16;
            material.Rarity = Rarity.Epic;

            material = MaterialTypes[(byte)Material.RadiumOre];
            material.DrawColor = Utility.HexToColor("00E11C");
            material.Light = Color.FromNonPremultiplied(43, 205, 207, 45);
            material.Hardness = 6;
            material.TouchDamage = 32;
            material.Rarity = Rarity.Uncommon;

            material = MaterialTypes[(byte)Material.UraniumOre];
            material.DrawColor = Utility.HexToColor("8080A7");
            material.Light = Color.FromNonPremultiplied(10, 241, 7, 45);
            material.Hardness = 7;
            material.TouchDamage = 64;
            material.Rarity = Rarity.Uncommon;

            material = MaterialTypes[(byte)Material.Obsidian];
            material.DrawColor = Utility.HexToColor("000042");
            material.Hardness = 7;
            material.TouchDamage = 48;
            material.Rarity = Rarity.Epic;

            // jems
            material = MaterialTypes[(byte)Material.Topaz];
            material.Hardness = 2;
            material.Light = Color.FromNonPremultiplied(245, 220, 0, 32);
            material.Rarity = Rarity.Epic;
            material = MaterialTypes[(byte)Material.Amethyst];
            material.Hardness = 3;
            material.Light = Color.FromNonPremultiplied(240, 0, 245, 32);
            material.Rarity = Rarity.Epic;
            material = MaterialTypes[(byte)Material.Emerald];
            material.Hardness = 4;
            material.Light = Color.FromNonPremultiplied(0, 245, 29, 32);
            material.Rarity = Rarity.Epic;
            material = MaterialTypes[(byte)Material.Sapphire];
            material.Hardness = 5;
            material.Light = Color.FromNonPremultiplied(0, 78, 245, 32);
            material.Rarity = Rarity.Epic;
            material = MaterialTypes[(byte)Material.Ruby];
            material.Hardness = 6;
            material.Light = Color.FromNonPremultiplied(245, 0, 0, 32);
            material.Rarity = Rarity.Epic;
            material = MaterialTypes[(byte)Material.Diamond];
            material.Hardness = 7;
            material.Light = Color.FromNonPremultiplied(180, 251, 255, 32);
            material.Rarity = Rarity.Epic;

            // bricks
            material = MaterialTypes[(byte)Material.BlackBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.DarkGrayBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.LightGrayBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.WhiteBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.BlueBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.LightBlueBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.CyanBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.GreenBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.LightGreenBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.YellowBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.BrownBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.OrangeBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.TanBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.RedBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.PurpleBrick];
            material.Hardness = 8;
            material = MaterialTypes[(byte)Material.PinkBrick];
            material.Hardness = 8;

            // other
            material = MaterialTypes[(byte)Material.Platform];
            material.Hardness = 9;
            material.IsTransparent = true;
            material = MaterialTypes[(byte)Material.Sodium];
            material.Hardness = 1;
            material = MaterialTypes[(byte)Material.Sulfur];
            material.Hardness = 1;
            material = MaterialTypes[(byte)Material.Bone];
            material.Hardness = 9;
            material = MaterialTypes[(byte)Material.Glass];
            material.Hardness = 1;
            material.IsTransparent = true;
            material = MaterialTypes[(byte)Material.GrayMetal];
            material.Hardness = 5;
            material = MaterialTypes[(byte)Material.BlackMetal];
            material.Hardness = 5;
            material = MaterialTypes[(byte)Material.WhiteMetal];
            material.Hardness = 5;
            material = MaterialTypes[(byte)Material.DarkGrayMetal];
            material.Hardness = 5;
            material = MaterialTypes[(byte)Material.RedMetal];
            material.Hardness = 5;
            material.Light = Color.FromNonPremultiplied(245, 0, 0, 200);

            material = MaterialTypes[(byte)Material.Plastic];
            material.Hardness = 4;
            material = MaterialTypes[(byte)Material.Pipe];
            material.Hardness = 5;
            material = MaterialTypes[(byte)Material.Wire];
            material.Hardness = 5;

            material = MaterialTypes[(byte)Material.Ice];
            material.DrawColor = Color.FromNonPremultiplied(210, 240, 255, 128);
            material.Hardness = 1;

            material = Get(Material.Spike);
            material.TouchDamage = Int32.MaxValue;
            material.Hardness = 9;

            material = MaterialTypes[(byte)Material.Boundry];
            material.IsVisible = false;
            material.DrawColor = Color.Cyan;
            material.Hardness = Int32.MaxValue;

            material = MaterialTypes[(byte)Material.None];
            material.IsVisible = false;

            material = MaterialTypes[(byte)Material.Life];
            material.IsVisible = false;

            material = MaterialTypes[(byte)Material.Wood];
            material.DrawColor = Utility.HexToColor("815D43");
            material.Hardness = 1;
            material.IsVisible = false;
        }

        public MaterialInfo()
        {
            DrawColor = Color.Cyan;
        }

        public static MaterialInfo Get(Material material)
        {
            return MaterialTypes[(int)material];
        }

        public static bool IsGas(Material material)
        {
            return material < Material.Fire;
        }

        public static bool IsLiquid(Material material)
        {
            return material >= Material.Fire && material < Material.Snow;
        }

        public static bool IsLooseOrSolid(Material material)
        {
            return material >= Material.Snow;
        }

        public static bool IsSolid(Material material)
        {
            return material >= Material.Platform;
        }

        internal bool IsBreakable(int strength)
        {
            if (strength <= 0)
                return false;
            return strength >= Hardness;
        }

        public bool IsVisible { get; set; }

        //static FileSystemWatcher watcher;
        public static void LoadContent(GraphicsDevice graphicsDevice)
        {
            Utility.LogMessage("Loading material textures.");

            var materialsPath = Path.Combine(Utility.ContentDirectory, "Tiles");
#if WINDOWS
            foreach (var materialName in Directory.GetFiles(materialsPath, "*.png"))
#else
            foreach (var materialName in Directory.GetFiles(materialsPath, "*.xnb"))
#endif
            {
                //Utility.LogMessage("Loading material " + materialName);

                var name = Path.GetFileNameWithoutExtension(materialName);
                Material material;
#if WINDOWS
                if (Enum.TryParse(name, out material))
#else
                material = (Material)Enum.Parse(typeof(Material), name, true);
#endif
                {
                    var materialInfo = MaterialInfo.MaterialTypes[(byte)material];
                    materialInfo.texture = Utility.LoadTexture(graphicsDevice, "Tiles/" + name + ".png");

                    using (var renderTarget = new RenderTarget2D(graphicsDevice, ItemType.IconSize, ItemType.IconSize))
                    {
                        graphicsDevice.SetRenderTarget(renderTarget);
                        graphicsDevice.Clear(Color.Transparent);
                        using (var spriteBatch = new SpriteBatch(graphicsDevice))
                        {
                            spriteBatch.Begin();

                            spriteBatch.Draw(Utility.FilledSlotTexture, Vector2.Zero, Color.White);

                            const int offsetX = 7;
                            const int offsetY = 7;
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(0 + offsetX, 0 + offsetY, 8, 8), new Rectangle(0, 27, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(8 + offsetX, 0 + offsetY, 8, 8), new Rectangle(9, 0, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(16 + offsetX, 0 + offsetY, 8, 8), new Rectangle(9, 0, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(24 + offsetX, 0 + offsetY, 8, 8), new Rectangle(9, 27, 8, 8), Color.White);

                            spriteBatch.Draw(materialInfo.texture, new Rectangle(0 + offsetX, 8 + offsetY, 8, 8), new Rectangle(0, 0, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(8 + offsetX, 8 + offsetY, 8, 8), new Rectangle(9, 9, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(16 + offsetX, 8 + offsetY, 8, 8), new Rectangle(9, 9, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(24 + offsetX, 8 + offsetY, 8, 8), new Rectangle(36, 0, 8, 8), Color.White);

                            spriteBatch.Draw(materialInfo.texture, new Rectangle(0 + offsetX, 16 + offsetY, 8, 8), new Rectangle(0, 0, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(8 + offsetX, 16 + offsetY, 8, 8), new Rectangle(9, 9, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(16 + offsetX, 16 + offsetY, 8, 8), new Rectangle(9, 9, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(24 + offsetX, 16 + offsetY, 8, 8), new Rectangle(36, 0, 8, 8), Color.White);

                            spriteBatch.Draw(materialInfo.texture, new Rectangle(0 + offsetX, 24 + offsetY, 8, 8), new Rectangle(0, 36, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(8 + offsetX, 24 + offsetY, 8, 8), new Rectangle(9, 18, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(16 + offsetX, 24 + offsetY, 8, 8), new Rectangle(9, 18, 8, 8), Color.White);
                            spriteBatch.Draw(materialInfo.texture, new Rectangle(24 + offsetX, 24 + offsetY, 8, 8), new Rectangle(9, 36, 8, 8), Color.White);
                            spriteBatch.End();
                        }
                        graphicsDevice.SetRenderTarget(null);

                        // we have to clone the texture here or it will get stomped when toggle fullscreen
                        var data = new Color[ItemType.IconSize * ItemType.IconSize];
                        renderTarget.GetData<Color>(data);
                        materialInfo.Item.Type.Texture = new Texture2D(graphicsDevice, ItemType.IconSize, ItemType.IconSize);
                        materialInfo.Item.Type.Texture.SetData(data);
                        materialInfo.Item.Type.IconTexture = materialInfo.Item.Type.Texture;
                    }

                    materialInfo.IsTextureLoaded = true;
                    materialInfo.Item.Type.IsTextureMissing = false;
                    materialInfo.Item.Type.SpriteWidth = ItemType.IconSize;
                    materialInfo.Item.Type.SpriteHeight = ItemType.IconSize;
                }
            }

            const int textureWidth = 117;
            const int textureHeight = 45;

            // build large texture to hold all material textures
            using (var renderTarget = new RenderTarget2D(graphicsDevice, 1024, 512))
            {
                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Transparent);
                var position = Vector2.Zero;
                using (var spriteBatch = new SpriteBatch(graphicsDevice))
                {
                    spriteBatch.Begin();
                    foreach (var materialInfo in MaterialTypes)
                    {
                        if (materialInfo.texture != null)
                        {
                            materialInfo.TextureOffset = new Point((int)position.X, (int)position.Y);
                            spriteBatch.Draw(materialInfo.texture, position, Color.White);
                            position.X += textureWidth;
                            if (position.X + textureWidth > renderTarget.Width)
                            {
                                position.X = 0;
                                position.Y += textureHeight;
                            }
                        }
                    }
                    spriteBatch.End();
                }
                graphicsDevice.SetRenderTarget(null);

                var data = new Color[1024 * 512];
                renderTarget.GetData<Color>(data);
                MaterialsTexture = new Texture2D(graphicsDevice, 1024, 512);
                MaterialsTexture.SetData(data);

                //var data2 = new Int32[1024 * 512];
                //renderTarget.GetData<Int32>(data2);
                //var image = BitmapSource.Create(1024, 512, 96, 96, System.Windows.Media.PixelFormats.Pbgra32, null, data2, 1024 * 4);
                //using (var stream = new FileStream("test.png", FileMode.Create))
                //{
                //    var encoder = new PngBitmapEncoder();
                //    encoder.Interlace = PngInterlaceOption.Off;
                //    encoder.Frames.Add(BitmapFrame.Create(image));
                //    encoder.Save(stream);
                //}
            }

            foreach (var materialInfo in MaterialTypes)
            {
                if (materialInfo.texture != null)
                {
                    materialInfo.texture.Dispose();
                    materialInfo.texture = null;
                }
            }

            //if (watcher == null)
            //{
            //    watcher = new FileSystemWatcher(materialsPath);
            //    watcher.Created += (sender, e) => { LoadContent(graphicsDevice); };
            //    watcher.Changed += (sender, e) => { LoadContent(graphicsDevice); };
            //    watcher.EnableRaisingEvents = true;
            //}
        }

        public static Material GetMaterialByName(string name)
        {
            if (name == null)
                return Material.None;
            var key = name.ToString().ToUpperInvariant();
            if (materialsByName.ContainsKey(key))
                return materialsByName[key];
            return Material.None;
        }
    }
}
