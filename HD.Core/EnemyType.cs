using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace HD
{
    public class EnemyType
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int SpriteWidth { get; set; }
        public int SpriteHeight { get; set; }
        public int MaxHealth { get; set; }
        public int Speed { get; set; }

        /// <summary>
        /// percentage of players health
        /// </summary>
        public int CollisionDamage { get; set; }
        public int CollisionKnockback { get; set; }

        public Color Light { get; set; }
        public bool IsHoming { get; set; }
       
        public bool IsFlying { get; set; }
        public bool IsFlyingHeightFixed { get; set; }
        public bool IsFlyingDistanceFixed { get; set; }
        public bool IsBoss { get; set; }
        public int MinSpawnTier { get; set; }
        public int MaxSpawnTier = 9;

        public bool CanSeeThruWalls { get; set; }

        public Rectangle BoundingBox;

        public Texture2D Texture; // don't serialize
        public Texture2D AlternateTexture; // don't serialize

        public Action<Enemy, int> OnSpawn { get; set; }
        public Action<Enemy> OnThink { get; set; }
        public Action<Enemy> OnLongThink { get; set; }
        public Action<Enemy> OnDie { get; set; }
        public Action<Enemy, int> OnHit { get; set; }
        public Action<Enemy, Player> OnCollision { get; set; }
        public Func<Enemy, SpriteBatch, Vector2, int, Color, bool> OnDraw { get; set; }
        public Action<Enemy, SpriteBatch, Vector2, int> OnDrawTop { get; set; }

        /// <summary>Is triggered after the enemy is removed and after the death animation plays.</summary>
        public Action<Enemy> OnRemove { get; set; }
        
        public bool IsAutoSpawn { get; set; }

        public SoundEffect SpawnSound { get; set; }
        public float SpawnSoundVolume;
        public SoundEffect AttackSound { get; set; }
        public float AttackSoundVolume;
        public SoundEffect DamageSound { get; set; }
        public float DamageSoundVolume;
        public SoundEffect DieSound { get; set; }
        public float DieSoundVolume;

        public double SpawnRateLimitInSeconds { get; set; }
        public double DeathAnimationLengthInSeconds { get; set; }
        public bool CanMoveWhileDead { get; set; }
        public bool RotateRender { get; set; }
        public float UpperRotationBounds;
        public float LowerRotationBounds;
        public bool IsSwimming { get; set; }
        public string SoundName { get; set; }
        public double CoolDown { get; set; }

        public bool AvoidsCliffs { get; set; }
        public bool PenetrateWalls { get; set; }
        public double Defense { get; set; }
        public bool SpawnAtCeiling { get; set; }

        public int SpriteFramesPerRow = 1;
        public Func<Animation, int, int> GetFrame { get; set; }
        public Func<int, int> GetFrameHeightOverride { get; set; }
        public Vector2 ShootingOrigin { get; set; }
        public Color BloodColor = Color.DarkRed;
        public Point RenderOffset { get; set; }
        public string AlternateTextureName { get; set; }

        /// <summary>
        /// Arm length from shooting origin that the bullets will spawn at. This will not be used when lobbing gravity effected projectiles.
        /// </summary>
        public int ShootingOffset { get; set; }
        public bool IsImmuneToKnockback { get; set; }
        public DateTime LastSpawn { get; set; }

        public Counter KillCounter;

        internal Rectangle GetFrameSourceRectangle(int frame)
        {
            var height = SpriteHeight;
            if (GetFrameHeightOverride != null)
            {
                height = GetFrameHeightOverride(frame);
                if (height <= 0)
                    height = SpriteHeight;
            }
            return new Rectangle((frame % SpriteFramesPerRow) * SpriteWidth, (frame / SpriteFramesPerRow) * SpriteHeight, SpriteWidth, SpriteHeight);
        }
       
        public EnemyType Clone()
        {
            return (EnemyType)MemberwiseClone();
        }

        public string SecondAlternateTextureName { get; set; }

        public Texture2D SecondAlternateTexture { get; set; }

        public string ThirdAlternateTextureName { get; set; }

        public Texture2D ThirdAlternateTexture { get; set; }
    }
}