using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;
using ProtoBuf;

namespace HD
{
    public abstract class Entity
    {
        public Map Map;

        public int Id { get; set; }

        Vector2 position;
        //[TypeConverter(typeof(Vector2Converter))]
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                UpdateOffsetBoundingBox();
            }
        }

        public DateTime AnimationStart;
        public Animation Animation;
        public int AnimationLength;

        public int AnimationAge
        {
            get
            {
                var result = (int)(Map.Now - AnimationStart).TotalMilliseconds;
                if (result < 0)
                    result = 0;
                return result;
            }
        }

        bool isSolid;
        [DefaultValue(false)]
        public bool IsSolid
        {
            get { return isSolid; }
            set
            {
                if (isSolid != value) {
                    if (Map != null && isSolid)
                        Map.SolidEntities.Remove(this);
                    isSolid = value;
                    if (Map != null && isSolid)
                        Map.SolidEntities.Add(this);
                }
            }
        }

        public bool IsActivated;

        public Vector2 Velocity;

        Rectangle boundingBox;
#if WINDOWS
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        [ProtoIgnore]
        public Rectangle BoundingBox
        {
            get { return boundingBox; }
            set
            {
                boundingBox = value;
                UpdateOffsetBoundingBox();
            }
        }
        public Rectangle OffsetBoundingBox;

        public bool IsOnGround;
        public bool IsDroppingThruPlatform;
        public bool IsBlocked;
        public bool IsAgainstCeiling;
        public float KnockbackResistance;

        public DateTime Created;

        public List<SoundEvent> SoundEvents = new List<SoundEvent>();
        public Sound PlayingSound;
        public SoundEffectInstance PlayingSoundEffect;
        public Sound PlayingSoundEffectCurrentId;

        /// <summary> Age of this entity in milliseconds. </summary>
        public double Age { get { return (Map.Now - Created).TotalMilliseconds; } }

        public virtual Color Light { get { return Color.Transparent; } }

        public virtual void Think(GameTime gameTime)
        {
            //UpdateOffsetBoundingBox();
        }

        public virtual void LongThink(GameTime gameTime)
        {
        }

        void UpdateOffsetBoundingBox()
        {
            OffsetBoundingBox = BoundingBox;
            OffsetBoundingBox.Offset((int)Position.X, (int)Position.Y);
        }

        public abstract void Draw(SpriteBatch spriteBatch, Point screenOffset);
        public virtual void DrawTop(SpriteBatch spriteBatch, Point screenOffset) { }

#if WINDOWS
        public abstract EntityUpdate PrepareForWire(EntityUpdate previous, Player targetPlayer);
        public abstract void ProcessUpdate(EntityUpdate entityUpdate);
#endif

        public virtual void DrawDebug(SpriteBatch spriteBatch, Point screenOffset)
        {
            var box = OffsetBoundingBox;
            box.Offset(-screenOffset.X, -screenOffset.Y);
            spriteBatch.DrawRectangle(box, Color.FromNonPremultiplied(255, 0, 0, 64));
        }

        const int climbHeight = 16;

        public float ApplyGravity(GameTime gameTime)
        {
            var amount = Map.Gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Velocity.Y += amount;
            return amount;
        }

        protected void MoveHorizontal(GameTime gameTime)
        {
            var distanceToMove = Math.Abs(Velocity.X * gameTime.ElapsedGameTime.TotalSeconds);
            if (distanceToMove > 0) {
                var isMovingLeft = Velocity.X < 0;
                var checkHorizontalPixel = (int)Position.X + (isMovingLeft ? BoundingBox.Left - 1 : BoundingBox.Right);
                var checkTopPixel = (int)Position.Y + BoundingBox.Top;
                var checkBottomPixel = (int)Position.Y + BoundingBox.Bottom;

                // for each pixel moved
                while (distanceToMove > 0) {
                    var wallHeight = WallCollisionDetect(checkHorizontalPixel, checkTopPixel);
                    if (wallHeight > climbHeight ||
                        (wallHeight > 0 && IsThereCeilingForWallClimb(wallHeight, checkTopPixel))) {
                        //stop at wall
                        Velocity.X = 0;
                        IsBlocked = true;
                        return;
                    } else {
                        if (wallHeight > 0) {
                            // step up the hill
                            Position += new Vector2(0, -wallHeight);
                            checkTopPixel -= wallHeight;
                            checkBottomPixel -= wallHeight;
                        }

                        // check solids....
                        foreach (var entity in Map.SolidEntities) {
                            if (entity.OffsetBoundingBox.Left <= checkHorizontalPixel && entity.OffsetBoundingBox.Right > checkHorizontalPixel && !(entity.OffsetBoundingBox.Top > checkBottomPixel || entity.OffsetBoundingBox.Bottom < checkTopPixel)) {
                                //stop at solid
                                Velocity.X = 0;
                                IsBlocked = true;
                                return;
                            }
                        }

                        Position += new Vector2(isMovingLeft ? -1 : 1, 0);
                        checkHorizontalPixel += isMovingLeft ? -1 : 1;
                    }

                    distanceToMove--;
                }

                IsBlocked = false;
            }
        }

        bool IsThereCeilingForWallClimb(int wallHeight, int top)
        {
            //Utility.LogMessage("IsThereCeilingForWallClimb: {0}, {1}", wallHeight, top);

            if (wallHeight <= Map.BlockHeight) {
                //Utility.LogMessage("Check");
                if (GroundCollisionDetect((int)Position.X + BoundingBox.Left, top - wallHeight, true)) {
                    //Utility.LogMessage("Ceiling Detected");
                    return true;
                } else
                    return false;
            }

            for (int i = 1; i <= wallHeight; i += Map.BlockHeight) {
                //Utility.LogMessage("Check");
                if (GroundCollisionDetect((int)Position.X + BoundingBox.Left, top - i, true)) {
                    //Utility.LogMessage("Ceiling Detected");
                    return true;
                }
            }

            return false;
        }

        protected bool CliffCheck()
        {
            const int margin = 6;
            var y = OffsetBoundingBox.Bottom + 4;
            for (int x = OffsetBoundingBox.Left + margin; x < OffsetBoundingBox.Right - margin; x += Map.BlockWidth) {
                if (MaterialInfo.IsLooseOrSolid(Map.GetMaterialAtPixel(x, y)))
                    return false;
                if (MaterialInfo.IsLooseOrSolid(Map.GetMaterialAtPixel(x, y + Map.BlockHeight)))
                    return false;
                if (MaterialInfo.IsLooseOrSolid(Map.GetMaterialAtPixel(x, y + Map.BlockHeight + Map.BlockHeight)))
                    return false;
            }

            // check the right most edge, since the loop above isn't guarenteed to hit it
            if (MaterialInfo.IsLooseOrSolid(Map.GetMaterialAtPixel(OffsetBoundingBox.Right - margin, y)))
                return false;
            if (MaterialInfo.IsLooseOrSolid(Map.GetMaterialAtPixel(OffsetBoundingBox.Right - margin, y + Map.BlockHeight)))
                return false;
            if (MaterialInfo.IsLooseOrSolid(Map.GetMaterialAtPixel(OffsetBoundingBox.Right - margin, y + Map.BlockHeight + Map.BlockHeight)))
                return false;

            return true;
        }

        // returns height of wall in pixels
        protected int WallCollisionDetect(int x, int top)
        {
            for (int y = 0; y <= BoundingBox.Height; y += Map.BlockHeight) {
                var material = Map.GetMaterialAtPixel(x, y + top);
                if (MaterialInfo.IsLooseOrSolid(material) && material != Material.Platform)
                    return BoundingBox.Height - y + (top % Map.BlockHeight); // add modulus of top here to cover when entity is positioned between blocks
            }
            return 0;
        }

        protected void MoveVertical(GameTime gameTime, Vector2 acceleration = default(Vector2))
        {
            Debug.Assert(BoundingBox.Width > 0, "Unable to do collision detection on an entity with no bounding box");

            //// http://en.wikipedia.org/wiki/Acceleration#Uniform_acceleration
            var displacement = ((Velocity.Y - acceleration.Y) * gameTime.ElapsedGameTime.TotalSeconds) + ((acceleration.Y * (gameTime.ElapsedGameTime.TotalSeconds * gameTime.ElapsedGameTime.TotalSeconds)) / 2);
            var distanceToMove = (float)Math.Abs(displacement);
            if (distanceToMove > 0) {
                var isMovingUp = displacement < 0;

                // for each pixel moved
                while (distanceToMove > 0) {
                    if (GroundCollisionDetect((int)Position.X + BoundingBox.Left, (int)Position.Y + (isMovingUp ? BoundingBox.Top : BoundingBox.Bottom), isMovingUp)) {
                        if (isMovingUp)
                            IsAgainstCeiling = true;
                        else
                            IsOnGround = true;
                        Velocity.Y = 0;
                        return;
                    } else {
                        // TODO: check solids....

                        var amount = isMovingUp ? -1f : 1f;
                        if (distanceToMove < 1)
                            amount *= distanceToMove;

                        Position += new Vector2(0, amount);
                    }

                    distanceToMove--;
                }

                IsAgainstCeiling = false;
                IsOnGround = false;
            }
        }

        bool GroundCollisionDetect(int left, int y, bool isMovingUp)
        {
            // add modulus of left here to cover when entity is positioned between blocks
            for (int x = left - (left % Map.BlockWidth); x < left + BoundingBox.Width; x += Map.BlockWidth) {
                var material = Map.GetMaterialAtPixel(x, y);
                if (MaterialInfo.IsLooseOrSolid(material)) {
                    if (material != Material.Platform) {
                        return true;
                    } else {
                        if (!isMovingUp && !IsDroppingThruPlatform)
                            return true;
                    }
                }
            }

            return false;
        }

        public bool ContainsPoint(Point position)
        {
            return OffsetBoundingBox.Contains(position);
        }

        public bool WithinRange(Entity target, int range = 125)
        {
            return WithinRange(target.Position, range);
        }

        public bool WithinRange(Vector2 target, int range = 125)
        {
            var rangeSquared = range * range;
            var distance = (Position - target).LengthSquared();
            return rangeSquared > distance;
        }

        public float GetAngleTo(Vector2 target)
        {
            return (float)Math.Atan2(target.Y - Position.Y, target.X - Position.X);
        }

        public bool HasLineOfSight(Entity target)
        {
            return HasLineOfSight(target.Position);
        }

        public bool HasLineOfSight(Vector2 target)
        {
            var distanceToMove = target - Position;
            var steps = (int)(distanceToMove.Length() / 4f);
            target = Position;

            for (int i = 0; i < steps; i++) {
                target += distanceToMove / steps;

                var material = Map.GetMaterialAtPixel(target);
                if (MaterialInfo.IsLooseOrSolid(material) && material != Material.Platform)
                    return false;
            }

            return true;
        }

        public void LivingCollision(Func<Living, bool> onCollision)
        {
            foreach (var entity in Map.Entities) {
                var living = entity as Living;
                if (living != null && entity != this && OffsetBoundingBox.Intersects(entity.OffsetBoundingBox)) {
                    if (onCollision(living))
                        return;
                }
            }
        }

        public virtual void Remove()
        {
            Map.RemoveEntity(this);
        }

        public void PlaySound(Sound sound, float volume = 0, int enemyTypeId = 0)
        {
            lock (SoundEvents) {
                SoundEvents.Add(new SoundEvent() { Sound = sound, Volume = volume, EnemyTypeId = enemyTypeId });
            }
        }

        protected void KnockBack(Living target, int speed)
        {
            if (KnockbackResistance < 1) {
                var vector = (target.Position - Position);
                vector.Normalize();

                //Utility.LogMessage(vector.ToString());

                target.Velocity += vector * (speed * KnockbackResistance);
            }
        }

        public void SetAnimation(Animation animation, bool forceAnimationStart = false, int animationLength = 0)
        {
            if (Animation != animation || forceAnimationStart) {
                Animation = animation;
                AnimationLength = animationLength;
                if (Map != null)
                    AnimationStart = Map.Now;
                else
                    AnimationStart = DateTime.UtcNow;
            }
        }
    }
}