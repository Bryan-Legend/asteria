using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using System.Threading.Tasks;
using ProtoBuf;

namespace HD
{
    public class Enemy : Living
    {
        static Color EnemyHealthMeterColor = Color.FromNonPremultiplied(255, 0, 0, 128);
        const int MaxPlayerDistanceBeforeDerez = 3000;

#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public Player Target { get; set; }

#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public Placeable TargetPlaceable { get; set; }

        public Enemy TargetEnemy { get; set; }

        [DefaultValue(null)]
        public Item Weapon { get; set; } // This should be removed. Can't remove it because xaml serializer is stupid.

        [DefaultValue(false)]
        public bool IsFlying { get; set; }
        [DefaultValue(false)]
        public bool IsFlyingHeightFixed { get; set; }
        [DefaultValue(0)]
        public int Speed { get; set; }
        public float TargetDistanceSquared;
        public float ClosestPlayerDistanceSquared;
        public float Direction;
        public float TurningRateInRadiansPerSecond = 3f;
        public Spawn Spawn;
        public int Tier { get; set; }
        public DateTime CoolingDownSince { get; set; }

        // This should be removed when IsImmobilized is no longer present in save games
#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public bool IsImmobilized { get; set; }

        DateTime lastHit;
        bool isHit;
        public bool IsHit
        {
            get { return isHit; }
            set
            {
                isHit = value;
                if (Map != null && isHit)
                    lastHit = Map.Now;
            }
        }

        const float up = (float)(3 * (Math.PI / 2));
        const float down = (float)(Math.PI / 2);

        bool isOnEdge;
        bool hasTouchedGround;
        Queue<DelayedAction> DelayedActions = new Queue<DelayedAction>();

        Dictionary<string, Object> bag;
        /// <summary>
        /// An expando object that can be used for adding variables to an entity.
        /// All variables in it should be initalized on OnSpawn.
        /// Does not get saved.
        /// Does not get sent to client so don't use it in OnDraw.
        /// </summary>
        public Dictionary<string, Object> Bag
        {
            get
            {
                if (bag == null)
                    bag = new Dictionary<string, Object>();
                return bag;
            }
        }

        List<Enemy> spawned;
        public List<Enemy> Spawned
        {
            get
            {
                if (spawned == null)
                    spawned = new List<Enemy>();
                return spawned;
            }
        }

        public int SpawnedCount { get { return (from e in Spawned where !e.IsDead select e).Count(); } }
        public Color CastLight;
        public override Color Light { get { return CastLight; } }

        public Vector2 FacePosition
        {
            get
            {
                return Position + Vector2.Transform(new Vector2(Type.SpriteWidth / 2, 0), Matrix.CreateRotationZ(Direction));
            }
        }

        public Vector2 BackPosition
        {
            get
            {
                return Position - Vector2.Transform(new Vector2(Type.SpriteWidth / 2, 0), Matrix.CreateRotationZ(Direction));
            }
        }

        EnemyType type;
        bool wasOnEdge;

#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public EnemyType Type
        {
            get { return type; }
            set
            {
                type = value;
                if (type != null)
                {
                    BoundingBox = type.BoundingBox;
                    CastLight = type.Light;
                }
                else
                    BoundingBox = Rectangle.Empty;
            }
        }

        public int TypeId
        {
            get { return Type.Id; }
            set { Type = EnemyBase.Get(value); }
        }

#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public String Name
        {
            get { return Type.Name; }
            set { /*Type = EnemyBase.Get(value);*/ } // this has to be here temporarily to be backwords compatable with existing save files. Might be able to be removed in the future.
        }

        public static void LoadContent(GraphicsDevice device)
        {
            Utility.LogMessage("Loading enemy textures.");
            foreach (var enemy in EnemyBase.Types)
            {
                enemy.Texture = Utility.LoadTexture(device, String.Format("Enemies/{0}.png", enemy.Name));
                if (!String.IsNullOrEmpty(enemy.AlternateTextureName))
                    enemy.AlternateTexture = Utility.LoadTexture(device, String.Format("Enemies/{0}.png", enemy.AlternateTextureName));
                if (!String.IsNullOrEmpty(enemy.SecondAlternateTextureName))
                    enemy.SecondAlternateTexture = Utility.LoadTexture(device, String.Format("Enemies/{0}.png", enemy.SecondAlternateTextureName));
                if (!String.IsNullOrEmpty(enemy.ThirdAlternateTextureName))
                    enemy.ThirdAlternateTexture = Utility.LoadTexture(device, String.Format("Enemies/{0}.png", enemy.ThirdAlternateTextureName));
            }
        }

        internal void Initalize()
        {
            KnockbackResistance = 1;
            MaxHealth = Utility.GetTierAmount(Tier, Type.MaxHealth);

            // bump up health to increase multiplayer difficulty.
            if (Map.Players.Count > 1)
                MaxHealth = (int)(MaxHealth * 1.75);

            Health = MaxHealth;
            Defense = Utility.GetTierAmount(Tier, Type.Defense);

            //if (Type.WeaponType != ItemId.None)
            //    Weapon = new Item() { TypeId = Type.WeaponType, Tier = Math.Max(1, Tier - 1) };

            IsFlying = Type.IsFlying;
            Speed = Type.Speed;
            SetAnimation(Animation.None);

            if (Tier >= 7 || Type.IsBoss)
                IsImmuneToEnvironment = true;

            if (type.OnSpawn != null)
                type.OnSpawn(this, Tier);
        }

        public float TurnAngleTowards(float source, float target, float amount)
        {
            var originalSource = source;

            if (target - source > Math.PI)
                target -= (float)Math.PI * 2;
            else if (target - source < -Math.PI)
                target += (float)Math.PI * 2;

            if (source > target)
            {
                source -= amount;
                if (source < target)
                    source = target;
            }
            else
            {
                if (source < target)
                {
                    source += amount;
                    if (source > target)
                        source = target;
                }
            }

            source %= (float)Math.PI * 2;

            //Utility.LogMessage("source {0} target {1} amount {2} result {3}", originalSource, target, amount, source);

            return source;
        }

        public override void Think(GameTime gameTime)
        {
            var startPosition = Position;
            var startMaterial = CurrentMaterial;

            if (Type.IsHoming && (Target != null || TargetEnemy != null))
            {
                var speed = Velocity.Length();
                float targetAngle = 0.0f;
                if (Target != null)
                    targetAngle = GetAngleTo(Target.Position);
                else
                    targetAngle = GetAngleTo(TargetEnemy.Position);
                Direction = TurnAngleTowards(Direction, targetAngle, TurningRateInRadiansPerSecond * (float)gameTime.ElapsedGameTime.TotalSeconds);
                Velocity = Vector2.Transform(new Vector2(speed, 0), Matrix.CreateRotationZ(Direction));
            }

            if (type.IsSwimming && !MaterialInfo.IsLiquid(CurrentMaterial))
            {
                // fish out of water just falls
                ApplyGravity(gameTime);
                MoveVertical(gameTime);
            }
            else
            {
                if (!IsFlying && !type.IsSwimming)
                {
                    DoGroundMovement(gameTime);
                }
                else
                {
                    Velocity = Vector2.Transform(new Vector2(Speed, 0), Matrix.CreateRotationZ(Direction));

                    if (Type.IsFlyingHeightFixed)
                        Velocity.Y = 0;
                    if (Type.IsFlyingDistanceFixed)
                        Velocity.X = 0;
                    if (Type.PenetrateWalls)
                    {
                        Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        MoveVertical(gameTime);
                        MoveHorizontal(gameTime);
                    }

                    if (type.IsSwimming && !MaterialInfo.IsLiquid(Map.GetMaterialAtPixel(Position))) //|| (type.IsFlying && !MaterialInfo.IsGas(Map.GetMaterialAtPixel(Position))))
                        Position = startPosition;
                }
            }

            base.Think(gameTime);

            if (DelayedActions.Count > 0)
            {
                var next = DelayedActions.Peek();
                if (next.TriggerAge < Age)
                {
                    DelayedActions.Dequeue();
                    next.Action();
                }
            }

            if (Type.OnThink != null)
                Type.OnThink(this);
        }

        private void DoGroundMovement(GameTime gameTime)
        {
            ApplyGravity(gameTime);

            MoveVertical(gameTime);

            if (IsOnGround)
                hasTouchedGround = true;

            if (!IsDead && hasTouchedGround)
            {
                if (Type.AvoidsCliffs)
                {
                    isOnEdge = CliffCheck();
                    if (isOnEdge)
                    {
                        if (!wasOnEdge)
                            IsFacingLeft = !IsFacingLeft;

                        wasOnEdge = true;
                    }
                    else
                        wasOnEdge = false;
                }

                if (IsFacingLeft)
                    Velocity.X = -Speed;
                else
                    Velocity.X = Speed;
            }

            MoveHorizontal(gameTime);
        }

        public override void LongThink(GameTime gameTime)
        {
            if (IsDead)
            {
                if ((Map.Now - Died).TotalSeconds > Type.DeathAnimationLengthInSeconds)
                    Remove();
                if (IsOnGround && !Type.CanMoveWhileDead)
                {
                    Velocity = Vector2.Zero;
                    return;
                }
            }

            TargetClosestPlayer();

            if (Map.DespawnOutOfRangeEnemies && ClosestPlayerDistanceSquared > MaxPlayerDistanceBeforeDerez * MaxPlayerDistanceBeforeDerez)
            {
                //Utility.LogMessage(Type.Name + " derezzed.");
                if (Spawn != null)
                    Spawn.SpawnRemoved = DateTime.MinValue;
                Remove();
            }
            else
            {
                base.LongThink(gameTime);

                if (Type.OnLongThink != null)
                    Type.OnLongThink(this);

                if (Type.CollisionDamage > 0 || Type.OnCollision != null)
                {
                    LivingCollision((living) =>
                    {
                        var player = living as Player;
                        if (player != null)
                        {
                            if (Type.CollisionDamage > 0)
                            {
                                var amount = living.Damage(Utility.GetTierAmount(Tier, Type.CollisionDamage));
                                if (amount > 0)
                                {
                                    PlaySound(Sound.EnemyAttack, 0, Type.Id);
                                    KnockBack(living, Type.CollisionKnockback);
                                }
                            }

                            if (Type.OnCollision != null)
                                Type.OnCollision(this, player);

                            return true;
                        }
                        return false;

                    });
                }
            }
        }

        public bool IsTargetCloserThan(int distance)
        {
            return TargetDistanceSquared < distance * distance;
        }

        public bool IsTargetPlaceableCloserThan(int distance)
        {
            var distanceFromTargetPlaceable = (Position - TargetPlaceable.Position).LengthSquared();
            return distanceFromTargetPlaceable < distance * distance;
        }

        public override int Damage(int amount)
        {
            amount = base.Damage(amount);
            if (amount > 0)
            {
                if (type.OnHit != null)
                    type.OnHit(this, amount);

                AddCombatText(amount.ToString(), CombatTextType.EnemyDamage, 0, amount);
                PlaySound(Sound.EnemyDamage, 0, Type.Id);
                IsHit = true;
            }

            return amount;
        }

        public override void Die()
        {
            base.Die();

            Counters.Increment(Counter.Kills);
            Counters.Increment(Type.KillCounter);

            PlaySound(Sound.EnemyDie, 0, Type.Id);

            if (Spawn != null)
                Spawn.IsDead = true;

            if (!Type.CanMoveWhileDead)
                IsFlying = false;

            SetAnimation(Animation.Dead);
            Map.AddEntity(new ParticleEmitter() { Position = Position, Type = ParticleEffect.Blood, Color = Type.BloodColor });

            if (Type.OnDie != null)
                Type.OnDie(this);
        }

        public override void Remove()
        {
            base.Remove();

            if (Spawn != null)
            {
                Spawn.SpawnRemoved = Map.Now;
                Spawn.Spawned = null;
            }

            if (Type.OnRemove != null)
                Type.OnRemove(this);
        }

        public void Drop(ItemId itemId, int amount = 1)
        {
            Map.AddPickup(Position, itemId, amount);
        }

        public void TargetClosestPlayer()
        {
            if (Type.CanSeeThruWalls)
            {
                Target = null;
                foreach (var player in Map.Players)
                {
                    if (!player.IsDead)
                    {
                        Target = Map.FindClosestPlayer(Position, out TargetDistanceSquared);
                        ClosestPlayerDistanceSquared = TargetDistanceSquared;
                    }
                }
            }
            else
            {
                var closestPlayerDistanceWithLOS = Single.MaxValue;
                ClosestPlayerDistanceSquared = Single.MaxValue;
                Player closest = null;
                foreach (var player in Map.Players)
                {
                    var playerDistance = (player.Position - Position).LengthSquared();
                    if (playerDistance < ClosestPlayerDistanceSquared)
                        ClosestPlayerDistanceSquared = playerDistance;

                    if (!player.IsDead && playerDistance < closestPlayerDistanceWithLOS && HasLineOfSight(player))
                    {
                        closestPlayerDistanceWithLOS = playerDistance;
                        closest = player;
                    }
                }

                Target = closest;
                TargetDistanceSquared = closestPlayerDistanceWithLOS;
            }
        }

        public float GetShootingAngle()
        {
            if (Target == null)
                return 0;

            var shootingOrigin = Position + (IsFacingLeft ? new Vector2(-Type.ShootingOrigin.X, Type.ShootingOrigin.Y) : Type.ShootingOrigin);
            return (float)Math.Atan2(Target.Position.Y - shootingOrigin.Y, Target.Position.X - shootingOrigin.X);
        }

        public void CheckFacingDirection()
        {
            if (Math.Abs(Direction) < down || Math.Abs(Direction) >= up)
                IsFacingLeft = false;
            else
                IsFacingLeft = true;
        }

        public void SetRandomAngle()
        {
            var newDirection = Utility.RandomAngle();
            Direction = newDirection;
        }

        void SetDirection(float newDirection)
        {
            if (type.UpperRotationBounds != 0 || type.LowerRotationBounds != 0)
            {
                if (Math.Abs(newDirection) > type.UpperRotationBounds && Math.Abs(newDirection) < type.LowerRotationBounds)
                {
                    if (Math.Abs(newDirection) > type.UpperRotationBounds)
                    {
                        if (newDirection < 0)
                            newDirection = -type.UpperRotationBounds;
                        else
                            newDirection = type.UpperRotationBounds;
                    }
                    if (Math.Abs(newDirection) < type.LowerRotationBounds)
                    {
                        if (newDirection < 0)
                            newDirection = -type.LowerRotationBounds;
                        else
                            newDirection = type.LowerRotationBounds;
                    }
                }
            }

            Direction = newDirection;
        }

        public void SetAngleToTarget()
        {
            if (Target != null)
            {
                SetDirection(MathHelper.WrapAngle((float)Math.Atan2(Target.Position.Y - Position.Y, Target.Position.X - Position.X)));
            }
        }

        public void SetAngleAwayFromTarget()
        {
            if (Target != null)
            {
                SetDirection((float)Math.Atan2(Target.Position.Y - Position.Y, Target.Position.X - Position.X) + (float)Math.PI);
            }
        }

        public void SetAngleToTargetEnemy()
        {
            if (TargetEnemy != null)
            {
                SetDirection((float)Math.Atan2(TargetEnemy.Position.Y - Position.Y, TargetEnemy.Position.X - Position.X));
            }
        }

        public void SetAngleToTargetPlaceable()
        {
            if (TargetPlaceable != null)
            {
                SetDirection((float)Math.Atan2(TargetPlaceable.Position.Y - Position.Y, TargetPlaceable.Position.X - Position.X));
            }
        }

        public void SetAngleAwayFromTargetPlaceable()
        {
            if (TargetPlaceable != null)
            {
                SetDirection((float)Math.Atan2(TargetPlaceable.Position.Y - Position.Y, TargetPlaceable.Position.X - Position.X) + (float)Math.PI);
            }
        }

        //public bool ShootAtTarget()
        //{
        //    if (Weapon != null)
        //    {
        //        return Weapon.Use(this);
        //    }
        //    return false;
        //}

        public bool IsCooledDown()
        {
            return IsCooledDown(this.Type.CoolDown);
        }

        public bool IsCooledDown(double cooldown)
        {
            if ((Map.Now - CoolingDownSince).TotalMilliseconds < cooldown)
                return false;
            return true;
        }

        public bool CooldownCheck(double cooldown)
        {
            if (cooldown <= 0)
                return true;

            if (IsCooledDown(cooldown))
            {
                CoolingDownSince = Map.Now;
                return true;
            }
            return false;
        }

        Vector2 GetActualShootingOrigin(float shootingAngle)
        {
            var shootingOrigin = IsFacingLeft ? new Vector2(-Type.ShootingOrigin.X, Type.ShootingOrigin.Y) : Type.ShootingOrigin;

            if (Type.ShootingOffset > 0)
                return shootingOrigin + Vector2.Transform(new Vector2(Type.ShootingOffset, 0), Matrix.CreateRotationZ(shootingAngle));
            return shootingOrigin;
        }

        public bool ShootAtTarget(ProjectileId type, float angleOffset = 0)
        {
            var projectileType = ProjectileBase.Get(type);
            if (!CooldownCheck(this.Type.CoolDown))
                return false;
            var projectile = Projectile.Shoot(type, this, Tier, angleOffset);
            projectile.Position += GetActualShootingOrigin(GetShootingAngle());
            projectile.Target = Target;
            return true;
        }

        public Projectile ShootAtAngle(ProjectileId type, float angle, bool cooldownCheck = true, Vector2 offset = default(Vector2))
        {
            var projectileType = ProjectileBase.Get(type);
            if (cooldownCheck && !CooldownCheck(this.Type.CoolDown))
                return null;
            var projectile = Projectile.ShootAtAngle(projectileType, this, Tier, angle);
            projectile.Position += GetActualShootingOrigin(angle) + offset;
            return projectile;
        }

        public bool LobObjectAtTarget(ProjectileId type, float angleOffset = 0)
        {
            var projectileType = ProjectileBase.Get(type);
            if (!CooldownCheck(this.Type.CoolDown) || Target == null)
                return false;

            Vector2 distaceFromTarget = Position + Type.ShootingOrigin - Target.Position;
            //var dis = Math.Ceiling(distaceFromTarget.X);
            //var gravity = Math.Ceiling(Map.Gravity/4);
            //var velocity = Math.Pow(projectileType.Speed, 2);
            var dis = distaceFromTarget.X;
            var gravity = Map.Gravity * Projectile.GravityFactor;
            var velocity = Math.Pow(projectileType.Speed, 2);
            var angle = 0.5 * Math.Asin((dis * gravity) / velocity);

            if (!Single.IsNaN((float)angle))
            {
                var projectile = Projectile.Shoot(type, this, Tier, (float)angle);
                projectile.Position += Type.ShootingOrigin;
            }
            return true;
        }

        public void WalkTowardTarget()
        {
            if (Target != null)
            {
                //if (enemy.IsTargetCloserThan(420) && enemy.HasLineOfSight(enemy.Target))
                //    enemy.ShootAtTarget();
                if (Target.Position.X < Position.X)
                {
                    IsFacingLeft = true;
                }
                else
                {
                    IsFacingLeft = false;
                }
            }
        }

        public void WalkAwayFromTarget()
        {
            if (Target != null)
            {
                if (Target.Position.X > Position.X)
                {
                    IsFacingLeft = true;
                }
                else
                {
                    IsFacingLeft = false;
                }
            }
        }

        public Color GetRenderColor()
        {
            if (IsHit)
            {
                var hitLength = (int)(Map.Now - lastHit).TotalMilliseconds;
                if (hitLength > 180)
                    IsHit = false;
                return ((hitLength / 60) % 2 == 0) ? Color.HotPink : Color.White;
            }
            return Color.White;
        }

        public override void Draw(SpriteBatch spriteBatch, Point screenOffset)
        {
            if (Type.Texture != null)
            {
                var currentFrame = GetCurrentFrame();
                if (currentFrame >= 0)
                {
                    screenOffset.X -= Type.RenderOffset.X;
                    screenOffset.Y -= Type.RenderOffset.Y;

                    if (IsFacingLeft)
                        screenOffset.X += Type.SpriteWidth - Type.BoundingBox.Width;

                    var renderColor = GetRenderColor();
                    var sourceRectangle = currentFrame >= 0 ? Type.GetFrameSourceRectangle(currentFrame) : Type.GetFrameSourceRectangle(0);
                    var mainDraw = true;
                    if (Type.OnDraw != null)
                        mainDraw = Type.OnDraw(this, spriteBatch, Position - screenOffset.ToVector2(), currentFrame, renderColor);

                    if (mainDraw)
                    {
                        spriteBatch.Draw
                        (
                            currentFrame >= 0 ? Type.Texture : Type.AlternateTexture,
                            new Rectangle((int)Position.X - screenOffset.X, (int)Position.Y - screenOffset.Y, Type.SpriteWidth, Type.SpriteHeight),
                            sourceRectangle,
                            renderColor,
                            (Type.RotateRender && IsFacingLeft) ? Direction - (float)Math.PI : (Type.RotateRender ? Direction : 0),
                            new Vector2(Type.BoundingBox.Width / 2, Type.BoundingBox.Height / 2),
                            IsFacingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                            0
                        );
                    }

                    if (Type.OnDrawTop != null)
                        Type.OnDrawTop(this, spriteBatch, Position - screenOffset.ToVector2(), currentFrame);
                }
            }
            else
            {
                var color = IsBlocked ? Color.Cyan : isOnEdge ? Color.Red : Color.Orange;
                if (IsFlying)
                    color = Color.Pink;
                spriteBatch.DrawRectangle(new Rectangle((int)Position.X - screenOffset.X - Type.SpriteWidth / 2, (int)Position.Y - screenOffset.Y - Type.SpriteHeight / 2, Type.SpriteWidth, Type.SpriteHeight), color);
                spriteBatch.DrawString(Utility.SmallFont, Type.Name, new Vector2(Position.X - screenOffset.X, Position.Y - screenOffset.Y), GetRenderColor());
                spriteBatch.DrawRectangle(new Rectangle((int)Position.X - screenOffset.X, OffsetBoundingBox.Bottom - screenOffset.Y, 1, 33), Color.Red);
            }

        }

        int GetCurrentFrame()
        {
            if (Type.GetFrame != null)
            {
                var result = Type.GetFrame(Animation, AnimationAge);
                //Utility.LogMessage("{0} {1} = {2}", Animation, AnimationAge, result);
                return result;
            }
            return 0;
        }

        public override void DrawTop(SpriteBatch spriteBatch, Point screenOffset)
        {
            base.DrawTop(spriteBatch, screenOffset);

            if (Health != MaxHealth && !type.IsBoss)
                spriteBatch.DrawRectangle(new Rectangle((int)Position.X - screenOffset.X - Type.SpriteWidth / 2, (int)Position.Y - 10 - screenOffset.Y - Type.SpriteHeight / 2, (int)(Type.SpriteWidth * ((float)Health / (float)MaxHealth)), 5), EnemyHealthMeterColor);
        }

        public void Delay(int millisecondsDelay, Action action)
        {
            DelayedActions.Enqueue(new DelayedAction() { TriggerAge = Age + millisecondsDelay, Action = action });
        }

        public override string ToString()
        {
            return Type.Name;
        }

#if WINDOWS
        public override EntityUpdate PrepareForWire(EntityUpdate previous, Player targetPlayer)
        {
            var result = previous as EnemyUpdate;
            if (result == null)
                result = new EnemyUpdate();
            var dirty = false;

            result.Id = Id;
            result.TypeId = Type.Id;

            if (SoundEvents.Count > 0)
            {
                result.SoundEvents = SoundEvents.ToArray();
                //SoundEvents.Clear();
                dirty = true;
            }
            else
                result.SoundEvents = null;

            if (result.X != (int)Position.X || result.Y != (int)Position.Y)
            {
                result.X = (int)Position.X;
                result.Y = (int)Position.Y;
                dirty = true;
            }

            if (result.Health != Health)
            {
                result.Health = Health;
                dirty = true;
            }

            if (result.MaxHealth != MaxHealth)
            {
                result.MaxHealth = MaxHealth;
                dirty = true;
            }

            if (result.IsFacingLeft != IsFacingLeft)
            {
                result.IsFacingLeft = IsFacingLeft;
                dirty = true;
            }

            if (result.Tier != Tier)
            {
                result.Tier = Tier;
                dirty = true;
            }

            if (result.PlayingSound != PlayingSound)
            {
                result.PlayingSound = PlayingSound;
                dirty = true;
            }

            if (result.Direction != Direction)
            {
                result.Direction = Direction;
                dirty = true;
            }

            if (result.Animation != Animation)
            {
                result.Animation = Animation;
                dirty = true;
            }

            if (result.CastLight != CastLight.PackedValue)
            {
                result.CastLight = CastLight.PackedValue;
                dirty = true;
            }

            if (result.IsHit != IsHit)
            {
                result.IsHit = IsHit;
                IsHit = false;
                dirty = true;
            }

            return dirty ? result : null;
        }

        public override void ProcessUpdate(EntityUpdate entityUpdate)
        {
            var update = (EnemyUpdate)entityUpdate;
            Position = new Vector2(update.X, update.Y);
            Health = update.Health;
            MaxHealth = update.MaxHealth;
            IsFacingLeft = update.IsFacingLeft;
            Type = EnemyBase.Get(update.TypeId);
            Tier = update.Tier;
            PlayingSound = update.PlayingSound;
            Direction = update.Direction;
            CastLight = new Color() { PackedValue = update.CastLight };
            if (update.IsHit && !IsHit)
            {
                IsHit = true;
                lastHit = Map.Now;
            }
            if (Animation != update.Animation)
            {
                Animation = update.Animation;
                //Console.WriteLine("Received animation " + CurrentAnimation);
                AnimationStart = Map.Now;
            }
        }
#endif
    }

#if WINDOWS
    [DataContract]
    public class EnemyUpdate : EntityUpdate
    {
        [DataMember(Order = 1)]
        public int Health { get; set; }

        [DataMember(Order = 2)]
        public int MaxHealth { get; set; }

        [DataMember(Order = 3)]
        public bool IsFacingLeft { get; set; }

        [DataMember(Order = 4)]
        public int TypeId { get; set; }

        [DataMember(Order = 5)]
        public int Tier { get; set; }

        [DataMember(Order = 6)]
        public Sound PlayingSound { get; set; }

        [DataMember(Order = 7)]
        public float Direction { get; set; }

        [DataMember(Order = 8)]
        public Animation Animation { get; set; }

        [DataMember(Order = 9)]
        public uint CastLight { get; set; }

        [DataMember(Order = 10)]
        public bool IsHit { get; set; }

        public override Type TargetType { get { return typeof(Enemy); } }
    }
#endif
}