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
    public class ParticleEmitter : Entity
    {
        double maxAge = 100;
        double lastAge;

        public ParticleEffect Type { get; set; }
        public Color Color { get; set; }
        public float Value = 1f;

        public ParticleEmitter()
        {
        }

        public override void Think(GameTime gameTime)
        {
            if (Age > maxAge)
                Remove();
        }

        public override void Draw(SpriteBatch spriteBatch, Point screenOffset)
        {
            var age = Age;
            var elapsed = age - lastAge;

            switch (Type) {
                case ParticleEffect.Blood:
                    Particles.AddBlood(Position, Color, Value);
                    Remove();
                    break;
                case ParticleEffect.Burst:
                    Particles.AddExplode(Position, Color, (int)Value);
                    Remove();
                    break;
                case ParticleEffect.Splash:
                    Particles.AddSplash(Position);
                    Remove();
                    break;
                case ParticleEffect.Dust:
                    const int dustParticlesPerSecond = 100;
                    var count = (int)(dustParticlesPerSecond * (elapsed / 1000));
                    if (count > 0) {
                        Emit(Color.Gray, count);
                        lastAge = Age;
                    }
                    break;
                case ParticleEffect.KrawnixExplode:
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 0);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 4);

                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 5);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 6);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 7);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 5);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 6);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 7);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 5);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 6);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 7);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 5);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 6);
                    Particles.AddKrawnixGib(Position + Utility.RandomVector() * 200, 7);

                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    break;
                case ParticleEffect.SkybossExplode:
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 0);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 4);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 4);
                    Particles.AddSkybossGib(Position + Utility.RandomVector() * 200, 4);

                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Remove();
                    break;
                case ParticleEffect.HarpyExplode:
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 0);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 4);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 5);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 6);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 5);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 6);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddHarpyGib(Position + Utility.RandomVector() * 200, 3);

                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Remove();
                    break;
                case ParticleEffect.AcidBeastExplode:
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 0);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 4);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 5);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 0);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddAcidBeastGib(Position + Utility.RandomVector() * 200, 3);

                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Remove();
                    break;
                case ParticleEffect.BasiliskExplode:
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 0);
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 1);
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 2);
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 3);
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 4);
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 5);
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 0);
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 2);
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 3);
                    Particles.AddBasiliskGib(Position + Utility.RandomVector() * 400, 5);

                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Remove();
                    break;
                case ParticleEffect.BurrowerExplode:
                    Particles.AddBurrowerGib(Position + Utility.RandomVector() * 200, 0);
                    Particles.AddBurrowerGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddBurrowerGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddBurrowerGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddBurrowerGib(Position + Utility.RandomVector() * 200, 4);
                    Particles.AddBurrowerGib(Position + Utility.RandomVector() * 200, 5);
                    Particles.AddBurrowerGib(Position + Utility.RandomVector() * 200, 6);
                    Particles.AddBurrowerGib(Position + Utility.RandomVector() * 200, 7);

                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Remove();
                    break;
                case ParticleEffect.DragonExplode:
                    Particles.AddDragonGib(Position + Utility.RandomVector() * 200, 0);
                    Particles.AddDragonGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddDragonGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddDragonGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddDragonGib(Position + Utility.RandomVector() * 200, 4);

                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Remove();
                    break;
                case ParticleEffect.FinalBossExplode:
                    Particles.AddFinalBossGib(Position + Utility.RandomVector() * 200, 0);
                    Particles.AddFinalBossGib(Position + Utility.RandomVector() * 200, 1);
                    Particles.AddFinalBossGib(Position + Utility.RandomVector() * 200, 2);
                    Particles.AddFinalBossGib(Position + Utility.RandomVector() * 200, 3);
                    Particles.AddFinalBossGib(Position + Utility.RandomVector() * 200, 4);
                    Particles.AddFinalBossGib(Position + Utility.RandomVector() * 200, 5);
                    Particles.AddFinalBossGib(Position + Utility.RandomVector() * 200, 6);

                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Particles.AddBlood(Position + Utility.RandomVector() * 200, Color, Value);
                    Remove();
                    break;
                case ParticleEffect.Explosion:
                    if (Utility.Flip()) {
                        switch (Utility.Next(5)) {
                            case 0:
                                PlaySound(Sound.Explosion1, 0.2f);
                                break;
                            case 1:
                                PlaySound(Sound.Explosion2, 0.2f);
                                break;
                            case 2:
                                PlaySound(Sound.Explosion3, 0.2f);
                                break;
                            case 3:
                                PlaySound(Sound.Explosion4, 0.2f);
                                break;
                            case 4:
                                PlaySound(Sound.Explosion5, 0.2f);
                                break;
                        }
                    }

                    Particles.AddExplosion(Position + Utility.RandomVector() * 200);
                    Particles.AddExplosion(Position + Utility.RandomVector() * 200);
                    Particles.AddExplosion(Position + Utility.RandomVector() * 200);
                    Particles.AddExplosion(Position + Utility.RandomVector() * 200);
                    Remove();
                    break;
                case ParticleEffect.SmallExplosion:
                    Particles.AddSmallExplosion(Position + Utility.RandomVector() * 200);
                    Particles.AddSmallExplosion(Position + Utility.RandomVector() * 200);
                    Particles.AddSmallExplosion(Position + Utility.RandomVector() * 200);
                    Remove();
                    break;
            }
        }

        void Emit(Color color = default(Color), int amount = 4)
        {
            if (color == default(Color))
                color = Color.Red;
            for (int count = 0; count < amount; count++) {
                Particles.Add(new Particle() { Position = Position, Color = color, Velocity = Utility.RandomVector() * 0.05f * (float)Utility.NextDouble(), MaxAge = 1, Scale = (float)Utility.NextDouble() });
            }
        }

#if WINDOWS
        public override EntityUpdate PrepareForWire(EntityUpdate previous, Player targetPlayer)
        {
            var result = previous as ParticleEmitterUpdate;
            if (result == null)
                result = new ParticleEmitterUpdate();
            else
                return null;

            result.Id = Id;

            result.X = (int)Position.X;
            result.Y = (int)Position.Y;
            result.Type = Type;

            result.Color = Color.PackedValue;
            result.Value = Value;

            return result;
        }

        public override void ProcessUpdate(EntityUpdate entityUpdate)
        {
            var update = (ParticleEmitterUpdate)entityUpdate;
            Position = new Vector2(update.X, update.Y);
            Type = update.Type;
            Color = new Color() { PackedValue = update.Color };
            Value = update.Value;
        }
#endif

    }

#if WINDOWS
    [DataContract]
    public class ParticleEmitterUpdate : EntityUpdate
    {
        public override Type TargetType { get { return typeof(ParticleEmitter); } }

        [DataMember(Order = 1)]
        public ParticleEffect Type { get; set; }

        [DataMember(Order = 2)]
        public uint Color { get; set; }

        [DataMember(Order = 3)]
        public float Value { get; set; }
    }
#endif
}