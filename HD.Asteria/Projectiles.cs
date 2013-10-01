using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HD.Asteria
{
    public static class Projectiles
    {
        public static void RegisterProjectiles()
        {
            ProjectileBase.AddType(new ProjectileType() { Id = ProjectileId.DigSmall, UseTierParticleColor = true, IsSlowBloom = true, InitialParticleBurst = 10, DigBrush = Brush.Size4, MaxAge = 600, Speed = 500, FireSound = Sound.MiningToolFire, TechName = "Mining Tool", UseTierTextures = true });
            ProjectileBase.AddType(new ProjectileType() { Id = ProjectileId.DigMedium, UseTierParticleColor = true, IsSlowBloom = true, InitialParticleBurst = 10, DigBrush = Brush.Size6, MaxAge = 600, Speed = 520, FireSound = Sound.MiningToolFire, TechName = "Mining Tool", UseTierTextures = true });
            ProjectileBase.AddType(new ProjectileType() { Id = ProjectileId.DigLarge, UseTierParticleColor = true, IsSlowBloom = true, InitialParticleBurst = 10, DigBrush = Brush.Size7, MaxAge = 600, Speed = 540, FireSound = Sound.MiningToolFire, TechName = "Mining Tool", UseTierTextures = true, });
            ProjectileBase.AddType(new ProjectileType() { Id = ProjectileId.DigExtraLarge, UseTierParticleColor = true, IsSlowBloom = true, InitialParticleBurst = 10, DigBrush = Brush.Size9, MaxAge = 600, Speed = 560, FireSound = Sound.MiningToolFire, TechName = "Mining Tool", UseTierTextures = true, });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.Blaster,
                BaseDamage = 5,
                MaxAge = 600,
                Speed = 750,
                InitialParticleBurst = 10,
                UseTierParticleColor = true,
                UseTierTextures = true,
                ParticleStream = true,
                FireSound = Sound.BlasterFire,
                HitSound = Sound.BlasterHit,
                Knockback = 1000,
            });

            var blasterCharged = ProjectileBase.Get(ProjectileId.Blaster).Clone();
            blasterCharged.Name = null;
            blasterCharged.Id = ProjectileId.BlasterCharged;
            blasterCharged.BaseDamage = 20;
            blasterCharged.Speed = 1000;
            blasterCharged.Knockback = 10;
            blasterCharged.UseTierTextures = true;
            blasterCharged.FireSound = Sound.BlasterChargedFire;
            ProjectileBase.AddType(blasterCharged);

            var slowBlaster = ProjectileBase.Get(ProjectileId.Blaster).Clone();
            slowBlaster.TechName = slowBlaster.Name;
            slowBlaster.Name = null;
            slowBlaster.Id = ProjectileId.SlowBlaster;
            slowBlaster.UseTierTextures = false;
            slowBlaster.BaseDamage = 8;
            slowBlaster.Speed = 300;
            slowBlaster.MaxAge = 6000;
            ProjectileBase.AddType(slowBlaster);

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.LongRangeBlaster,
                BaseDamage = 4,
                MaxAge = 4000,
                Speed = 650,
                Penetrate = true,
                InitialParticleBurst = 10,
                UseTierParticleColor = true,
                //UseTierTextures = true,
                FireSound = Sound.BlasterFire,
                HitSound = Sound.BlasterHit,
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.BallLightning,
                BaseDamage = 20,
                MaxAge = 6000,
                Speed = 500,
                Penetrate = true,
                InitialParticleBurst = 10,
                ParticleColor = Color.LightBlue,
                HitSound = Sound.ElectricityHit,
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.Disruptor,
                BaseDamage = 6,
                MaxAge = 200,
                Speed = 750,
                InitialParticleBurst = 5,
                UseTierTextures = true,
                UseTierParticleColor = true,
                FireSound = Sound.DisruptorFire,
                HitSound = Sound.DisruptorHit,
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.LaserRifle,
                BaseDamage = 13,
                MaxAge = 1000,
                Speed = 1000,
                //CoolDown = 500,
                InitialParticleBurst = 15,
                UseTierTextures = true,
                UseTierParticleColor = true,
                FireSound = Sound.LaserRifleFire,
                HitSound = Sound.LaserRifleHit,
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.PoisonDart,
                BaseDamage = 5,
                MaxAge = 2000,
                Speed = 700,
                InitialParticleBurst = 10,
                ParticleColor = Color.Green,
                FireSound = Sound.Spike,
                HitSound = Sound.BlasterHit,
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.HomingMissile,
                BaseDamage = 11,
                MaxAge = 3000,
                Speed = 500,
                IsHoming = true,
                UseTierTextures = true,
                InitialParticleBurst = 30,
                ParticleColor = Color.LightGray,
                ParticleStream = true,
                FireSound = Sound.HomingMissileFire,
                HitSound = Sound.HomingMissileHit,
                OnHitParticleBurst = 75,
            });

            var acidSpit = ProjectileBase.Get(ProjectileId.HomingMissile).Clone();
            acidSpit.Id = ProjectileId.AcidSpit;
            acidSpit.Name = "AcidSpit";
            acidSpit.UseTierTextures = false;
            acidSpit.ParticleColor = Color.FromNonPremultiplied(194, 245, 65, 200);
            acidSpit.FireSound = Sound.FireBreath;
            acidSpit.HitSound = Sound.AcidSplat;
            ProjectileBase.AddType(acidSpit);

            //var explosiveAcidSpit = ProjectileBase.Get(ProjectileId.HomingMissile).Clone();
            //explosiveAcidSpit.Id = ProjectileId.ExplosiveAcidSpit;
            //explosiveAcidSpit.Name = "Explosive AcidSpit";
            //explosiveAcidSpit.Penetrate = true;
            //explosiveAcidSpit.IsHoming = false;
            //explosiveAcidSpit.UseTierTextures = false;
            //explosiveAcidSpit.MaxAge = 6000;
            //explosiveAcidSpit.Speed = 400;
            //explosiveAcidSpit.ParticleColor = Color.FromNonPremultiplied(194, 245, 65, 200);
            //explosiveAcidSpit.FireSound = Sound.FireBreath;
            //explosiveAcidSpit.HitSound = Sound.AcidSplat;
            //explosiveAcidSpit.OnProjectileHitTerrain = (projectile, material) => {
            //        projectile.Map.RenderBrush(projectile.Position, Brush.Size4, Material.Acid, 0);
            //    };
            //explosiveAcidSpit.OnProjectileHit = (projectile, target, amount) => {
            //    projectile.Map.RenderBrush(projectile.Position, Brush.Size4, Material.Acid, 0);
            //};
            //ProjectileBase.AddType(explosiveAcidSpit);

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.FlameThrower,
                Speed = 1000,
                ParticleColor = Color.Red,
                OnProjectileMove = (projectile, material) => {
                    if (material != Material.Fire && MaterialInfo.IsLiquid(material))
                        projectile.Remove();
                    else {
                        if (projectile.Age > 150)
                            projectile.Map.RenderBrush(projectile.Position, Brush.Size4, Material.Fire, 0);
                    }
                },
                OnProjectileHitTerrain = (projectile, material) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size4, Material.Fire, 0);
                },
                OnProjectileHit = (projectile, target, amount) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size4, Material.Fire, 0);
                }
            });


            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.Fireball,
                BaseDamage = 25,
                Speed = 1000,
                MaxAge = 2000,
                DigBrush = Brush.Size10,
                DigStrength = 7,
                FireSound = Sound.FireBreath,
                ParticleColor = Color.FromNonPremultiplied(255, 113, 95, 200),
                OnProjectileHitTerrain = (projectile, material) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size5, Material.Fire, 0);
                },
                OnProjectileHit = (projectile, target, amount) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size5, Material.Fire, 0);
                }
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.Fireball_2,
                BaseDamage = 8,
                Speed = 800,
                MaxAge = 3000,
                Penetrate = true,
                FireSound = Sound.FireBreath,
                ParticleColor = Color.FromNonPremultiplied(255, 113, 95, 200),
                OnProjectileHit = (projectile, target, amount) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size10, Material.Fire, 0);
                }
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.Comet,
                BaseDamage = 10,
                Speed = 500,
                ParticleColor = Color.FromNonPremultiplied(255, 43, 61, 175),
                MaxAge = 2000,
                DigBrush = Brush.Size10,
                DigStrength = 7,
                HasGravity = true,
                FireSound = Sound.FireBreath,
                OnProjectileHitTerrain = (projectile, material) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size4, Material.Fire, 0);
                },
                OnProjectileHit = (projectile, target, amount) => {

                }
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.Spike,
                BaseDamage = 10,
                MaxAge = 2000,
                Speed = 750,
                InitialParticleBurst = 15,
                ParticleColor = Color.FromNonPremultiplied(114, 118, 20, 255),
                FireSound = Sound.Spike,
            });

            var project = ProjectileBase.Get(ProjectileId.Spike).Clone();
            project.TechName = project.Name;
            project.Name = null;
            project.Id = ProjectileId.SproutSpike;
            project.BaseDamage = 7;
            ProjectileBase.AddType(project);

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.BoseEinsteinCondenser,
                BaseDamage = 5,
                MaxAge = 1000,
                Speed = 500,
                FireSound = Sound.BoseEinsteinCondenserFire,
                InitialParticleBurst = 5,
                ParticleColor = Color.LightBlue,
                OnProjectileMove = (projectile, material) => {
                    if (MaterialInfo.IsLiquid(material)) {
                        projectile.Map.RenderBrush(projectile.Position, Brush.Size9, Material.Ice, 0);
                        projectile.Remove();
                    }
                },
                OnProjectileHitTerrain = (projectile, material) => {
                    //if (projectile.Age > 150)
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size9, Material.Ice, 0);
                },
                OnProjectileHit = (projectile, target, amount) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size9, Material.Ice, 0);
                }
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.DiggingAndDamaging,
                BaseDamage = 10,
                DigBrush = Brush.Size7,
                DigStrength = 7,
                ParticleColor = Color.FromNonPremultiplied(248, 113, 241, 100),
                MaxAge = 3000,
                Speed = 500,
                FireSound = Sound.SpaceShipBlaster,
                OnProjectileHitTerrain = (projectile, material) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size8, Material.Fire, 1);
                },
            });

            //Final Boss Grenades
            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.Grenade,
                BaseDamage = 5,
                ParticleColor = Color.White,
                Speed = 500,
                MaxAge = 3000,
                FireSound = Sound.HomingMissileFire,
                HitSound = Sound.HomingMissileHit,
                OnProjectileHitTerrain = (projectile, material) => {
                    projectile.Map.Explode(projectile.Position, 250, 9);
                },
                OnProjectileHit = (projectile, target, amount) => {
                    projectile.Map.Explode(projectile.Position, 250, 9);
                }
            });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.Grenade_2,
                BaseDamage = 1,
                Speed = 1000,
                MaxAge = 6000,
                HasGravity = true,
                FireSound = Sound.GrenadeFire,
                ParticleColor = Color.White,
                //OnProjectileMove = (projectile, material) =>
                //{
                //    if (projectile.Age > 2800)
                //        projectile.Map.Explode(projectile.Position, 5, 1);
                //},
                OnProjectileHitTerrain = (projectile, material) => {
                    projectile.Map.Explode(projectile.Position, 5, 1);
                },
                OnProjectileHit = (projectile, target, amount) => {
                    projectile.Map.Explode(projectile.Position, 5, 1);
                }
            });

            ProjectileBase.AddType(new ProjectileType() { Id = ProjectileId.MiniFlame, BaseDamage = 5, DigBrush = Brush.Size7, DigStrength = 8, ParticleColor = Color.White, MaxAge = 5000, Speed = 300, Penetrate = true });

            ProjectileBase.AddType(new ProjectileType() {
                Id = ProjectileId.MiniFlame_2,
                BaseDamage = 5,
                ParticleColor = Color.FromNonPremultiplied(255, 80, 98, 255),
                MaxAge = 5000,
                Speed = 650,
                FireSound = Sound.MiniFlame,
                OnProjectileHitTerrain = (projectile, material) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size3, Material.Fire, 0);
                },
                OnProjectileHit = (projectile, target, amount) => {
                    projectile.Map.RenderBrush(projectile.Position, Brush.Size3, Material.Fire, 0);
                }
            });
        }
    }
}