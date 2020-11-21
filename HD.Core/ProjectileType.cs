using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace HD
{
    public class ProjectileType
    {
        public ProjectileId Id { get; set; }
        public string Name { get; set; }
        public string TechName { get; set; }
        public Texture2D Texture { get; set; }
        public Texture2D[] TierTextures { get; set; }
        public bool IgnoreGravity = true;
        public bool Penetrate { get; set; }
        public double BaseDamage { get; set; }
        public int DigStrength { get; set; }
        public Brush DigBrush = Brush.None;
        public int MaxAge = 500;
        public bool IsHoming { get; set; }
        public bool IsSticky { get; set; }
        public float TurningRateInRadiansPerSecond = 3f;
        public float Speed = 1000;
        public Sound FireSound { get; set; }
        public Sound HitSound { get; set; }

        public Action<Projectile, Material> OnProjectileMove { get; set; }
        public Action<Projectile, Material> OnProjectileHitTerrain { get; set; }
        public Action<Projectile, Living, int> OnProjectileHit { get; set; }
        /// <summary>
        /// Enitity's change in velocity
        /// </summary>
        public int Knockback { get; set; }
        public bool UseTierParticleColor { get; set; }
        public bool IsSlowBloom { get; set; }
        public bool UseTierTextures { get; set; }

        /// <summary>
        /// The number of particles that will be shown in a burst when the projectile is fired.
        /// </summary>
        public int InitialParticleBurst { get; set; }

        public Color ParticleColor { get; set; }

        /// <summary>
        /// When set the particles will trail behind the projectile.
        /// </summary>
        public bool ParticleStream { get; set; }

        public int OnHitParticleBurst { get; set; }

        public bool HasGravity { get; set; }

        public ProjectileType Clone()
        {
            return (ProjectileType)MemberwiseClone();
        }
    }
}
