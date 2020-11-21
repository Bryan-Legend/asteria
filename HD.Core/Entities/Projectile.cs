using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace HD
{
    public class Projectile : Entity
    {
        public const float GravityFactor = 1f / 8f;

        public Living Owner;
        public float Angle;
        public bool IsPlayerOwned;
        public Entity Target;
        public float DistanceSquaredToTarget;

        bool isFirstThink = true;

        public ProjectileType Type;
        public int Tier;

        bool hasHit;

        public override Color Light { get { return GetParticleColor(); } }

        public static Projectile ShootAtAngle(ProjectileId type, Living source, int tier, float angle)
        {
            return ShootAtAngle(ProjectileBase.Get(type), source, tier, angle);
        }

        public static Projectile ShootAtAngle(ProjectileType type, Living source, int tier, float angle)
        {
            var result = new Projectile() { Type = type, Owner = source, Tier = tier };
            result.Angle = angle;
            result.Position = source.Position;
            result.Velocity = (result.Owner.Velocity * 0.5f) + Vector2.Transform(new Vector2(result.Type.Speed, 0), Matrix.CreateRotationZ(result.Angle));
            if (type.FireSound != Sound.None)
                source.PlaySound(type.FireSound);
            result.Owner.Map.AddEntity(result);
            return result;
        }

        public static Projectile Shoot(ProjectileId type, Living source, int tier, float angleOffset = 0)
        {
            var player = source as Player;
            if (player != null) {
                float armAngle;
                var shoulderPosition = player.GetShoulderPosition(out armAngle);
                var shootingOrigin = player.GetShootingOrigin(shoulderPosition, armAngle);

                //if (!player.HasLineOfSight(shootingOrigin))
                //    return null;

                var angle = player.GetShootingAngle(shootingOrigin, angleOffset);
                //Utility.LogMessage("arm angle: {0} shooting angle: {1}", armAngle, angle);
                if (Math.Abs(Utility.NormalizeAngle(armAngle) - Utility.NormalizeAngle(angle)) > Math.PI / 8)
                    angle = armAngle;

                var result = ShootAtAngle(type, source, tier, angle);
                result.Position = shootingOrigin;
                result.IsPlayerOwned = true;

                var distanceToCheck = player.GetElbowPosition(shoulderPosition, armAngle) - shootingOrigin;
                //Utility.LogMessage(distanceToCheck);
                result.TerrainCollisionCheck(distanceToCheck);
                result.Position = shootingOrigin;

                return result;
            } else {
                var enemy = source as Enemy;
                if (enemy == null)
                    return null;
                return ShootAtAngle(type, source, tier, enemy.GetShootingAngle() + angleOffset);
            }
        }

        public override void Think(GameTime gameTime)
        {
            if (isFirstThink) {
                LivingCollisionCheck(Position);
                isFirstThink = false;
            }

            if (Type == null || Age > Type.MaxAge) {
                Remove();
                return;
            }

            var startPosition = Position;

            if (Type.IsHoming && Target != null) {
                var speed = Velocity.Length();
                var targetAngle = GetAngleTo(Target.Position);
                Angle = TurnAngleTowards(Angle, targetAngle, Type.TurningRateInRadiansPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds);
                Velocity = Vector2.Transform(new Vector2(speed, 0), Matrix.CreateRotationZ(Angle));
            }

            var acceleration = 0f;
            if (Type.HasGravity) {
                acceleration = Map.Gravity * GravityFactor * (float)gameTime.ElapsedGameTime.TotalSeconds;
                Velocity.Y += acceleration;

                Angle = Velocity.GetAngle();
            }

            if (!Type.Penetrate) {
                var distanceToMove = new Vector2(Velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds, ((Velocity.Y - acceleration) * (float)gameTime.ElapsedGameTime.TotalSeconds) + ((acceleration * ((float)gameTime.ElapsedGameTime.TotalSeconds * (float)gameTime.ElapsedGameTime.TotalSeconds)) / 2));
                if (TerrainCollisionCheck(distanceToMove))
                    return;
            } else
                Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Think(gameTime);

            LivingCollisionCheck(startPosition);
        }

        bool TerrainCollisionCheck(Vector2 distanceToMove)
        {
            var steps = (int)(distanceToMove.Length() / 4f);
            var step = distanceToMove / steps;
            for (var i = 0; i < steps; i++) {
                Position += step;

                var material = Map.GetMaterialAtPixel(Position);
                if ((MaterialInfo.IsLooseOrSolid(material) || material == Material.Slime) && material != Material.Platform) {
                    HitTerrain(material);
                    return true;
                }

                foreach (var solid in Map.SolidEntities) {
                    if (solid.OffsetBoundingBox.Contains(Position.ToPoint())) {
                        HitTerrain(material);
                        return true;
                    }
                }

                if (Type.OnProjectileMove != null)
                    Type.OnProjectileMove(this, material);
            }

            return false;
        }

        Vector2 LivingCollisionCheck(Vector2 startPosition)
        {
            LivingCollision((living) => {
                var enemy = living as Enemy;
                if (!hasHit && (enemy != null) == IsPlayerOwned && !living.IsDead) {
                    hasHit = true;
                    if (Type.Knockback > 0 && (enemy == null || (!enemy.Type.IsBoss && !enemy.Type.IsImmuneToKnockback))) {

                        // Do a raytrace to determine exact point of impact.
                        var moved = Position - startPosition;
                        var movedDistance = moved.Length();
                        var movedSegment = moved / movedDistance;
                        for (int raytrace = 0; raytrace < movedDistance; raytrace++) {
                            var checkPosition = startPosition + (raytrace * movedSegment);
                            if (living.OffsetBoundingBox.Contains(checkPosition.ToPoint())) {
                                Position = checkPosition;
                                break;
                            }
                        }

                        KnockBack(living, Type.Knockback);
                    }

                    var amount = living.Damage(Utility.GetTierAmount(Tier, Type.BaseDamage));
                    Remove();

                    if (Type.BaseDamage > 0) {
                        if (amount > 0) {
                            if (Type.HitSound != Sound.None)
                                living.PlaySound(Type.HitSound);
                        } else
                            living.PlaySound(Sound.WeaponMissed);
                    }

                    if (Type.OnHitParticleBurst > 0)
                        Map.AddEntity(new ParticleEmitter() { Position = Position, Color = GetParticleColor(), Type = ParticleEffect.Burst, Value = Type.OnHitParticleBurst });

                    if (Type.OnProjectileHit != null)
                        Type.OnProjectileHit(this, living, amount);
                    return true;
                }
                return false;
            });
            return startPosition;
        }

        void HitTerrain(Material material)
        {
            Velocity = Vector2.Zero;
            Remove();

            if (Type.OnHitParticleBurst > 0)
                Map.AddEntity(new ParticleEmitter() { Position = Position, Color = GetParticleColor(), Type = ParticleEffect.Burst, Value = Type.OnHitParticleBurst });

            if (Type.OnProjectileHitTerrain != null)
                Type.OnProjectileHitTerrain(this, material);

            if (Type.DigBrush != Brush.None) {
                var amount = Map.Dig(Position, Type.DigStrength == 0 ? Tier : Type.DigStrength, Type.DigBrush, Owner is Player);
                
                if (Owner is Player)
                    Counters.Increment(Counter.BlocksMined, amount);

                Owner.PlaySound(Sound.DigSmall);
                Map.AddEntity(new ParticleEmitter() { Position = Position, Color = GetParticleColor(), Type = ParticleEffect.Burst, Value = 30 });
            }
        }

        public float TurnAngleTowards(float source, float target, float amount)
        {
            var originalSource = source;

            if (target - source > Math.PI)
                target -= (float)Math.PI * 2;
            else if (target - source < -Math.PI)
                target += (float)Math.PI * 2;

            if (source > target) {
                source -= amount;
                if (source < target)
                    source = target;
            } else {
                if (source < target) {
                    source += amount;
                    if (source > target)
                        source = target;
                }
            }

            source %= (float)Math.PI * 2;

            //Utility.LogMessage("source {0} target {1} amount {2} result {3}", originalSource, target, amount, source);

            return source;
        }

        public override void LongThink(GameTime gameTime)
        {
            if (Type != null) {
                if (Type.IsHoming) {
                    if (!IsPlayerOwned)
                        Target = Map.FindClosestPlayer(Position, out DistanceSquaredToTarget);
                    else
                        Target = Map.FindClosestEnemy(Position, out DistanceSquaredToTarget);

                    if (DistanceSquaredToTarget > 500 * 500)
                        Target = null;
                }

                //if (Type.IsSticky)
                //{
                //    if (!IsPlayerOwned)
                //        Target = Map.FindClosestPlayer(Position, out DistanceSquaredToTarget);
                //    else
                //        Target = Map.FindClosestEnemy(Position, out DistanceSquaredToTarget);

                //    if (DistanceSquaredToTarget > 500 * 500)
                //        Target = null;
                //}

                base.LongThink(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Point screenOffset)
        {
            var scale = Math.Min(0.25f + ((float)(Age / (Type.IsSlowBloom ? 300 : 150)) * 0.75f), 1f);
            var texture = Type.UseTierTextures ? Type.TierTextures[Tier - 1] : Type.Texture;
            if (texture == null)
                texture = Type.Texture;
            if (texture == null)
                texture = Utility.MissingTexture;

            spriteBatch.Draw(texture, new Vector2((int)Position.X - screenOffset.X, (int)Position.Y - screenOffset.Y), null, Color.White, Angle, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 0);

            if (Age < 1 && Type.InitialParticleBurst > 0)
                Particles.AddSpread(Position, GetParticleColor(), Type.InitialParticleBurst, (Velocity / 20000) + new Vector2(((float)Utility.Next(-5, 5)) / 5000, ((float)Utility.Next(-5, 5)) / 5000));

            if (Type.ParticleStream)
                Particles.Add(new Particle() { Position = Position, Color = GetParticleColor(), Velocity = Utility.RandomVector() * 0.1f, MaxAge = 0.5, Scale = 0.5f });
        }

        Color GetParticleColor()
        {
            if (Type.UseTierParticleColor)
                return Utility.GetTierColor(Tier);
            return Type.ParticleColor;
        }

        public override void DrawDebug(SpriteBatch spriteBatch, Point screenOffset)
        {
            var box = OffsetBoundingBox;
            box.Offset((int)-screenOffset.X, (int)-screenOffset.Y);
            box.Inflate(1, 1);
            spriteBatch.DrawRectangle(box, Color.Cyan);
        }

#if WINDOWS
        public override EntityUpdate PrepareForWire(EntityUpdate previous, Player targetPlayer)
        {
            var result = new ProjectileUpdate();
            result.Id = Id;
            result.X = (int)Position.X;
            result.Y = (int)Position.Y;
            result.Angle = Angle;
            result.Tier = Tier;
            if (Type != null)
                result.ProjectileTypeId = Type.Id;
            return result;
        }

        public override void ProcessUpdate(EntityUpdate entityUpdate)
        {
            var update = (ProjectileUpdate)entityUpdate;
            Position = new Vector2(update.X, update.Y);
            Angle = update.Angle;
            Tier = update.Tier;
            if (Type == null)
                Type = ProjectileBase.Get(update.ProjectileTypeId);
        }
#endif
    }

#if WINDOWS
    [DataContract]
    public class ProjectileUpdate : EntityUpdate
    {
        [DataMember(Order = 10)]
        public float Angle { get; set; }

        [DataMember(Order = 11)]
        public ProjectileId ProjectileTypeId { get; set; }

        [DataMember(Order = 13)]
        public int Tier { get; set; }

        public override Type TargetType { get { return typeof(Projectile); } }
    }
#endif
}
