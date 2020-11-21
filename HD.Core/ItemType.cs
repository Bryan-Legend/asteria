using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HD
{
    public class ItemType
    {
        public const int IconSize = 46;

        public ItemId Id { get; set; }
        public string NameSource { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ActualDescription { get; set; }
        public ItemCategory Category { get; set; }
        public ItemCategory AltCategory { get; set; }
        public string CategoryString { get; set; }
        public bool IsConsumed { get; set; }
        public int PlaceAndActivateRange = 125;
        public int SpriteWidth { get; set; }
        public int SpriteHeight { get; set; }
        public Color Light = Color.Transparent;
        public Vector2 LightOrigin { get; set; }
        public int ListPriority { get; set; }

        public Material Material { get; set; }
        public double CoolDown = 500;

        public Rarity Rarity = Rarity.Common;

        public List<string> ComponentFor { get; set; }

        public Action<Item> OnUse { get; set; }
        public Action<Item> OnStartUsing { get; set; }
        public Action<Item> OnStopUsing { get; set; }

        public Action<Player, Item> OnEquip { get; set; }

        public Action<Player, Placeable> OnPlace { get; set; }
        public Action<Player, Placeable> OnActivate { get; set; }
        public Action<Placeable, Placeable, Player> OnSwitch { get; set; }

        public Action<Placeable> OnLongThink { get; set; }
        public Action<Placeable, SpriteBatch, Vector2> OnDraw { get; set; }
        public Action<Placeable, SpriteBatch, Vector2> OnDrawTop { get; set; }
        //public Action<Placeable, SpriteBatch> OnItemTip;
        public Action<Item, SpriteBatch> OnUsingDraw { get; set; }

        public Func<Player, bool, bool> CanUse { get; set; }
        public Func<Player, Placeable, bool> CanEdit { get; set; }

        public bool IsEditable { get; set; }
        public Texture2D Texture { get; set; }
        public Texture2D AlternateTexture { get; set; }
        public Texture2D IconTexture { get; set; }
        public bool IsTextureMissing { get; set; }
        public SpriteAnimation SpriteAnimation { get; set; }
        public int SpritesPerRow = 2;
        public bool IsDematerializable = true;
        public bool IsFloating { get; set; }
        public bool ShowToolTip = true;
        public bool DrawEnable = true;
        /// Delay of the animation per frame, in milliseconds.               
        public int SpriteAnimationDelay = 100;
        public int Tier { get; set; }
        public bool IsUsedImmediately { get; set; }
        public Recipe Recipe { get; set; }
        public bool IsTemporary { get; set; }
        public string AlternateTextureName { get; set; }        

        public Rectangle BoundingBox { get { return new Rectangle(-(SpriteWidth / 2), -(SpriteHeight / 2), SpriteWidth, SpriteHeight); } }
        public string TierType { get { return Utility.GetTierName(Tier); } }

        public void Draw(SpriteBatch spriteBatch, Vector2 location, int frameReference = 0)
        {
            if (DrawEnable)
            {
                switch (SpriteAnimation)
                {
                    case SpriteAnimation.None:
                        spriteBatch.Draw(Texture, location, Color.White);
                        break;
                    case SpriteAnimation.Cycle4:
                        spriteBatch.Draw(Texture, location, GetFrame((frameReference / SpriteAnimationDelay) % 4), Color.White);
                        break;
                    case SpriteAnimation.Cycle6:
                        spriteBatch.Draw(Texture, location, GetFrame((frameReference / SpriteAnimationDelay) % 6), Color.White);
                        break;
                    case SpriteAnimation.Cycle9:
                        spriteBatch.Draw(Texture, location, GetFrame((frameReference / SpriteAnimationDelay) % 9), Color.White);
                        break;
                    //case SpriteAnimation.ItermittentCycle10:
                    //    spriteBatch.Draw(Texture, location, GetFrame((frameReference / SpriteAnimationDelay) % 10), Color.White);
                    //    //var frame10 = (frameReference / SpriteAnimationDelay) % 20;
                    //    //if (frame10 > 9)
                    //    //    frame10 = 1;
                    //    //spriteBatch.Draw(Texture, location, GetWideFrame(frame10), Color.White);
                    //    break;
                    case SpriteAnimation.Cycle10:
                        spriteBatch.Draw(Texture, location, GetFrame((frameReference / SpriteAnimationDelay) % 10), Color.White);
                        break;
                    case SpriteAnimation.Cycle18:
                        spriteBatch.Draw(Texture, location, GetFrame((frameReference / SpriteAnimationDelay) % 18), Color.White);
                        break;
                    case SpriteAnimation.Oscillate4:
                        // shows frames 0 1 2 3 2 1
                        var frame = (frameReference / SpriteAnimationDelay) % 6; // 0-5
                        // oscillate backwards for 2 frames
                        if (frame > 3)
                            frame = 3 + 3 - frame;
                        spriteBatch.Draw(Texture, location, GetFrame(frame), Color.White);
                        break;
                    case SpriteAnimation.Oscillate6:
                        // shows frames 0 1 2 3 4 5 4 3 2 1
                        var frame6 = (frameReference / SpriteAnimationDelay) % 10; // 0-9
                        // oscillate backwards for 4 frames
                        if (frame6 > 5)
                            frame6 = 5 + 5 - frame6;
                        spriteBatch.Draw(Texture, location, GetFrame(frame6), Color.White);
                        break;
                    case SpriteAnimation.Oscillate9:
                        spriteBatch.Draw(Texture, location, GetFrame(Utility.Oscillate(frameReference / SpriteAnimationDelay, 9)), Color.White);
                        break;
                }
            }
        }

        public Rectangle GetFrame(int frame)
        {
            return new Rectangle((frame % SpritesPerRow) * SpriteWidth, (frame / SpritesPerRow) * SpriteHeight, SpriteWidth, SpriteHeight);
        }

        public void DrawIcon(SpriteBatch spriteBatch, Vector2 position, Color color, bool drawAtHalfSize = false)
        {
            if (IconTexture != null)
            {
                var sourceRectangle = (SpriteAnimation == HD.SpriteAnimation.None || IconTexture != Texture) ? new Rectangle(0, 0, IconTexture.Width, IconTexture.Height) : GetFrame(0);
                var scale = 1f;
                if (sourceRectangle.Width != IconSize || sourceRectangle.Height != IconSize)
                {
                    spriteBatch.Draw(Utility.FilledSlotTexture, position, color);

                    if (sourceRectangle.Width > sourceRectangle.Height)
                    {
                        scale = (float)IconSize / (float)sourceRectangle.Width;
                        position.Y += (IconSize - (sourceRectangle.Height * scale)) / 2;
                    }
                    else
                    {
                        scale = (float)IconSize / (float)sourceRectangle.Height;
                        position.X += (IconSize - (sourceRectangle.Width * scale)) / 2;
                    }
                }

                spriteBatch.Draw(IconTexture, position, sourceRectangle, color, 0, new Vector2(), scale * (drawAtHalfSize ? 0.5f : 1), SpriteEffects.None, 0);
            }
        }

        public ItemType Clone()
        {
            var result = (ItemType)MemberwiseClone();
            result.Name = null;
            return result;
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsManualFire { get; set; }
    }
}