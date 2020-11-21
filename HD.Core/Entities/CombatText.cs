using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace HD
{
    public enum CombatTextType
    {
        PlayerDamage,
        EnemyDamage,
        Heal,
        Pickup
    }

    public class CombatText : Entity
    {
        const double maxAge = 1400;
        const double fadeStartAge = maxAge - 200;

        public string Text { get; set; }
        public CombatTextType Type { get; set; }
        public Rarity PickupRarity { get; set; }
        public int SourceId { get; set; }
        public int TypeId { get; set; }
        public int Amount { get; set; }

        double lastAge;

        public override void Think(GameTime gameTime)
        {
            base.Think(gameTime);

            if (Age > maxAge)
                Remove();
        }

        public override void Draw(SpriteBatch spriteBatch, Point screenOffset) { }

        public override void DrawTop(SpriteBatch spriteBatch, Point screenOffset)
        {
            var age = Age;
            if (Velocity == Vector2.Zero) {
                if (CheckForMerge())
                    return;
                Velocity = new Vector2((float)Utility.NextDouble() * 0.2f - 0.1f, -0.5f);
                var textMeasure = Utility.MediumFont.MeasureString(Text);
                BoundingBox = new Rectangle(0, 0, (int)textMeasure.X, (int)textMeasure.Y);
                Position = new Vector2(Position.X - textMeasure.X / 2f, Position.Y - textMeasure.Y);

                CheckForOverlap();

                lastAge = age;
            }

            var elapsed = age - lastAge;
            Velocity.Y += (float)elapsed * 0.0005f;
            Position += Velocity * (float)elapsed;

            CheckForOverlap();

            Color color;
            switch (Type) {
                case CombatTextType.Heal:
                    color = Color.LimeGreen;
                    break;
                case CombatTextType.PlayerDamage:
                    color = Color.SkyBlue;
                    break;
                case CombatTextType.EnemyDamage:
                    color = Color.OrangeRed;
                    break;
                case CombatTextType.Pickup:
                    color = PickupRarity.GetColor();
                    break;
                default:
                    color = Color.White;
                    break;
            }

            if (age > fadeStartAge) {
                color = color.SetAlpha(1 - ((age - fadeStartAge) / (maxAge - fadeStartAge)));
            }

            spriteBatch.DrawString(Utility.MediumFont, Text, new Vector2(Position.X - screenOffset.X, Position.Y - screenOffset.Y), color);
            lastAge = Age;
        }

        void CheckForOverlap()
        {
            foreach (var combatText in Map.Entities.OfType<CombatText>()) {
                if (combatText != this && combatText.OffsetBoundingBox.Intersects(OffsetBoundingBox)) {
                    if (Position.Y >= combatText.Position.Y) {
                        combatText.Position = new Vector2(combatText.Position.X, Position.Y - (BoundingBox.Height + 1));
                        //else
                        //    Position += new Vector2(0, -(BoundingBox.Height + 10));
                        //CheckForOverlap();
                        //return;
                    }
                }
            }
        }

        static char[] digits = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ',' };

        bool CheckForMerge()
        {
            if (Type == CombatTextType.Pickup) { // || Type == CombatTextType.EnemyDamage) {
                var other = (from e in Map.Entities where e is CombatText && e.Id != Id && ((CombatText)e).SourceId == SourceId && ((CombatText)e).TypeId == TypeId select e).FirstOrDefault() as CombatText;
                if (other != null) {
                    other.Amount += Amount;
                    other.Text = other.Text.TrimEnd(digits) + other.Amount.ToString("N0");
                    //other.Velocity = new Vector2(other.Velocity.X, Math.Min(other.Velocity.Y, -0.1f));
                    //other.Created = Map.Now;
                    //other.lastAge = 0;
                    Remove();
                    return true;
                }
            }

            return false;
        }

#if WINDOWS

        public override EntityUpdate PrepareForWire(EntityUpdate previous, Player targetPlayer)
        {
            var result = previous as CombatTextUpdate;
            if (result == null)
                result = new CombatTextUpdate();
            else
                return null;

            result.Id = Id;

            result.X = (int)Position.X;
            result.Y = (int)Position.Y;
            result.Type = Type;
            result.Text = Text;
            result.SourceId = SourceId;
            result.TypeId = TypeId;
            result.Amount = Amount;

            return result;
        }

        public override void ProcessUpdate(EntityUpdate entityUpdate)
        {
            var update = (CombatTextUpdate)entityUpdate;
            Position = new Vector2(update.X, update.Y);
            Type = update.Type;
            Text = update.Text;
            SourceId = update.SourceId;
            TypeId = update.TypeId;
            Amount = update.Amount;
        }
#endif

    }

#if WINDOWS
    [DataContract]
    public class CombatTextUpdate : EntityUpdate
    {
        public override Type TargetType { get { return typeof(CombatText); } }

        [DataMember(Order = 1)]
        public CombatTextType Type { get; set; }

        [DataMember(Order = 2)]
        public string Text { get; set; }

        [DataMember(Order = 3)]
        public int SourceId { get; set; }
        [DataMember(Order = 4)]
        public int TypeId { get; set; }
        [DataMember(Order = 5)]
        public int Amount { get; set; }
    }
#endif
}