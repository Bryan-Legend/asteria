using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HD;
using Microsoft.Xna.Framework;

// Global class must not be in a namespace.
public static class Global
{
    public static void Register()
    {
        EnemyBase.AddItem(new EnemyType {
            Id = 3000,
            Name = "Proximity Mine",
            SpriteWidth = 20,
            SpriteHeight = 20,
            SpriteFramesPerRow = 2,
            MaxHealth = 1000,
            Defense = Double.MaxValue,
            IsAutoSpawn = true,
            MinSpawnTier = 4,
            BloodColor = Color.Black,
            OnSpawn = (enemy, tier) => {
                enemy.SetAnimation(Animation.Idle1);
            },
            OnLongThink = (enemy) => {
                if (enemy.Target != null && enemy.IsTargetCloserThan(300)) {
                    enemy.PlayingSound = Sound.TurretActivate;
                    enemy.SetAnimation(Animation.Attack1);
                    enemy.Delay(1000, () => {
                        enemy.Map.Explode(enemy.Position, 100);
                        enemy.Remove();
                    });
                }
            },
            GetFrame = (animation, age) => {
                switch (animation) {
                    case Animation.Attack1:
                        return age / 50 % 2;
                    default:
                        return age / 1000 % 2;
                }
            },
        });


        ProjectileBase.AddType(new ProjectileType() {
            Id = (ProjectileId)500,
            Name = "Wave",
            Speed = 500,
            MaxAge = 1300,
            ParticleColor = Color.Red,
            BaseDamage = 1,
            OnProjectileMove = (projectile, material) => {
                if (((int)projectile.Age / 100) % 2 == 0) {
                    projectile.Position += new Vector2(0, -10);
                } else {
                    projectile.Position += new Vector2(0, 10);
                }
            },
        });

        ItemBase.AddItem(new ItemType() {
            Id = (ItemId)500,
            Name = "Wave Gun",
            Tier = 3,
            Category = ItemCategory.Tool,
            Description = "This shoots a wave beam type of attack.",
            PlaceAndActivateRange = 10000,
            CoolDown = 0,
            IsManualFire = true,
            Light = Color.White,
            OnUse = (item) => {
                Projectile.Shoot((ProjectileId)500, item.Player, item.GetTier());
            },
//            Recipe = new Recipe() { Location = ItemId.MillingMachine, Components = new Component[] { new Component(ItemId.Copper, 15), new Component(ItemId.Topaz, 15), new Component(ItemId.Thermistor, 3), } },
        });
    }
}
