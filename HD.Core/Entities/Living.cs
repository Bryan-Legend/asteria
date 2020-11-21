using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ProtoBuf;

namespace HD
{
    public abstract class Living : Entity
    {
        static HashSet<Material> touchMaterials = new HashSet<Material>();
        static HashSet<Material> immersionMaterials = new HashSet<Material>();

        public bool IsFacingLeft;
        public bool IsImmersed;
        public int Health = 100;
        public int MaxHealth = 100;
        public int Defense;
        public bool IsDead { get { return Health <= 0; } }
        public DateTime Died;
        public double FireResistance = 0;
        public bool IsImmuneToEnvironment;

        public MaterialInfo CurrentMaterialInfo;

        Material currentMaterial;
        public virtual Material CurrentMaterial
        {
            get
            {
                return currentMaterial;
            }
            set
            {
                currentMaterial = value;
            }
        }

        public void Heal(int amount, bool showCombatText = true)
        {
            if (!IsDead) {
                if (amount > MaxHealth - Health)
                    amount = MaxHealth - Health;
                Health += amount;

                if (amount > 0 && showCombatText)
                    AddCombatText(amount.ToString(), CombatTextType.Heal);
            }
        }

        public virtual int Damage(int amount)
        {
            amount -= Defense;

            if (!IsDead && Age > 500 && amount > 0) {
                if (amount > Health)
                    amount = Health;

                Health -= amount;

                if (Health <= 0)
                    Die();
            } else
                amount = 0;

            return amount;
        }

        public virtual void Die()
        {
            Health = 0;
            Died = Map.Now;
        }

        public override void Think(GameTime gameTime)
        {
            if (AnimationLength > 0 && AnimationAge > AnimationLength)
                SetAnimation(Animation.None);

            base.Think(gameTime);

            CurrentMaterial = Map.GetMaterialAtPixel(Position);
        }

        public override void LongThink(GameTime gameTime)
        {
            base.LongThink(gameTime);

            touchMaterials.Clear();
            for (int x = 0; x <= BoundingBox.Width; x += Map.BlockWidth) {
                touchMaterials.Add(Map.GetMaterialAtPixel(OffsetBoundingBox.X + x, OffsetBoundingBox.Bottom));
            }
            foreach (var material in touchMaterials) {
                var materialInfo = MaterialInfo.Get(material);
                if (materialInfo.TouchDamage > 0 && !IsImmuneToEnvironment) {
                    Damage(materialInfo.TouchDamage);
                }
            }

            immersionMaterials.Clear();
            for (int x = 0; x <= BoundingBox.Width; x += Map.BlockWidth) {
                for (int y = 0; y <= BoundingBox.Height; y += Map.BlockHeight) {
                    immersionMaterials.Add(Map.GetMaterialAtPixel(OffsetBoundingBox.X + x, OffsetBoundingBox.Y + y));
                }
            }
            var isImmersed = true;

            foreach (var material in immersionMaterials) {
                if (material == Material.Air)
                    isImmersed = false;
                var materialInfo = MaterialInfo.Get(material);
                if (materialInfo.ImmersionDamage > 0 && !IsImmuneToEnvironment) {
                    var immersionDamage = materialInfo.ImmersionDamage / 100.0;
                    if (material == Material.Fire) {
                        immersionDamage *= (1 - FireResistance);
                        Damage((int)Math.Ceiling((double)MaxHealth * immersionDamage));
                    } else
                        Damage(materialInfo.ImmersionDamage);
                }
            }
            IsImmersed = isImmersed;
        }

        public void AddCombatText(string text, CombatTextType type, int typeId = 0, int amount = 0, Rarity pickupRarity = Rarity.Poor)
        {
            if (Map != null)
                Map.AddEntity(new CombatText() { Position = new Vector2(Position.X, Position.Y + BoundingBox.Top), Text = text, Type = type, SourceId = Id, TypeId = typeId, Amount = amount, PickupRarity = pickupRarity });
        }
    }
}
