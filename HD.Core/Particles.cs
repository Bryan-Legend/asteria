using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HD
{
    public struct Particle
    {
        public bool IsActive;
        public DateTime Born;
        public double Age;
        public double MaxAge;

        public Vector2 Position;
        public Vector2 Velocity;
        public bool Gravity;
        public float Angle;
        public float Rotation;

        public Color Color;
        public float Scale;

        public int Frame;
        public bool IsScalable;
        public bool IsNotFading;

        public void Update(DateTime now, GameTime gameTime)
        {
            if (IsActive) {
                if (Born == default(DateTime)) {
                    Born = now.AddSeconds(-MaxAge * Age);
                }

                Age = (now - Born).TotalSeconds / MaxAge;
                if (Age > 1) {
                    IsActive = false;
                } else {
                    if (Gravity) {
                        const float g = 0.01f / 30;
                        Velocity.Y += g * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                    Position += Velocity * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    Angle += Rotation * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (Scale <= 1.0f && IsScalable)
                        Scale += Math.Min(0.25f + ((float)(Age / 1.0f) * 0.75f), 1f);
                }
            }
        }
    }

    public static class Particles
    {
        const int explosionFrame = 23;

        static Particle[] particles = new Particle[2048];
        static Texture2D[] textures;

        public static void LoadContent(GraphicsDevice device)
        {
            textures = new Texture2D[100];
            textures[0] = Utility.LoadTexture(device, "Particles/Basic.png");
            for (int count = 0; count < 9; count++)
                textures[count + 1] = Utility.LoadTexture(device, String.Format("Particles/Blood {0}.png", count + 1));
            for (int count = 0; count < 8; count++)
                textures[count + 10] = Utility.LoadTexture(device, String.Format("Particles/Krawnix Gut {0}.png", count + 1));
            for (int count = 0; count < 5; count++)
                textures[count + 18] = Utility.LoadTexture(device, String.Format("Particles/Sky Boss Giblet {0}.png", count + 1));
            textures[explosionFrame] = Utility.LoadTexture(device, "Particles/Explosion.png");
            for (int count = 0; count < 7; count++)
                textures[count + 24] = Utility.LoadTexture(device, String.Format("Particles/Harpy Giblet {0}.png", count + 1));
            for (int count = 0; count < 6; count++)
                textures[count + 31] = Utility.LoadTexture(device, String.Format("Particles/Acid Beast Giblet {0}.png", count + 1));
            for (int count = 0; count < 6; count++)
                textures[count + 37] = Utility.LoadTexture(device, String.Format("Particles/Basilisk Giblet {0}.png", count + 1));
            for (int count = 0; count < 8; count++)
                textures[count + 43] = Utility.LoadTexture(device, String.Format("Particles/Burrower Giblet {0}.png", count + 1));
            for (int count = 0; count < 5; count++)
                textures[count + 51] = Utility.LoadTexture(device, String.Format("Particles/Dragon Giblet {0}.png", count + 1));
            for (int count = 0; count < 7; count++)
                textures[count + 56] = Utility.LoadTexture(device, String.Format("Particles/Final Boss 3 Giblet {0}.png", count + 1));
        }

        static void AddGib(Vector2 position, int frame)
        {
            Add(new Particle() { Position = position, Color = Color.White, Velocity = Utility.RandomVector() * 0.5f, MaxAge = 5, Scale = 1, Gravity = true, Frame = frame, Rotation = (float)Utility.NextDoubleBalanced() / 120 });
        }

        public static void AddKrawnixGib(Vector2 position, int frame)
        {
            AddGib(position, frame + 10);
        }

        public static void AddSkybossGib(Vector2 position, int frame)
        {
            AddGib(position, frame + 18);
        }

        public static void AddHarpyGib(Vector2 position, int frame)
        {
            AddGib(position, frame + 24);
        }

        public static void AddAcidBeastGib(Vector2 position, int frame)
        {
            AddGib(position, frame + 31);
        }
        
        public static void AddBasiliskGib(Vector2 position, int frame)
        {
            AddGib(position, frame + 37);
        }

        public static void AddBurrowerGib(Vector2 position, int frame)
        {
            AddGib(position, frame + 43);
        }
        
        public static void AddDragonGib(Vector2 position, int frame)
        {
            AddGib(position, frame + 51);
        }

        public static void AddFinalBossGib(Vector2 position, int frame)
        {
            AddGib(position, frame + 56);
        }
        
        public static void AddExplosion(Vector2 position)
        {
            Add(new Particle() {
                Position = position,
                Color = Color.White,
                MaxAge = 1,
                Age = Math.Min(Utility.NextDouble(), 0.75),
                Scale = (float)(Utility.NextDouble() * 0.5) + 0.5f,
                Frame = explosionFrame,
                Angle = Utility.RandomAngle(),
                Velocity = Utility.RandomVector() * 0.15f,
                IsNotFading = true
            });
        }

        public static void AddSmallExplosion(Vector2 position)
        {
            Add(new Particle() {
                Position = position,
                Color = Color.White,
                MaxAge = 1,
                Age = Math.Min(Utility.NextDouble(), 0.75),
                Scale = (float)(Utility.NextDouble() * 0.3) + 0.3f,
                Frame = explosionFrame,
                Angle = Utility.RandomAngle(),
                Velocity = Utility.RandomVector() * 0.15f,
                IsNotFading = true
            });
        }

        public static void AddBlood(Vector2 position, Color color = default(Color), float maxSize = 1f)
        {
            const int amount = 10;
            if (color == default(Color))
                color = Color.Red;
            for (int count = 0; count < amount; count++) {
                Add(new Particle() { Position = position, Color = color, Velocity = Utility.RandomVector() * 0.25f * maxSize, MaxAge = Utility.NextDouble() + 0.5, Scale = (float)Utility.NextDouble() * maxSize, Rotation = (float)Utility.NextDoubleBalanced() / 50, Gravity = true, Frame = Utility.Next(9) + 1 });
            }
        }

        public static void AddExplode(Vector2 position, Color color = default(Color), int amount = 10)
        {
            if (color == default(Color))
                color = Color.Red;
            for (int count = 0; count < amount; count++) {
                Add(new Particle() { Position = position, Color = color, Velocity = Utility.RandomVector() * 0.35f, MaxAge = 1, Scale = 0.5f, Gravity = true });
            }
        }

        public static void AddSpread(Vector2 position, Color color = default(Color), int amount = 4, Vector2 velocity = default(Vector2))
        {
            if (color == default(Color))
                color = Color.Red;
            for (int count = 0; count < amount; count++) {
                Add(new Particle() { Position = position, Color = color, Velocity = velocity * (8 * (float)Utility.NextDouble()), MaxAge = 0.3, Scale = (float)Utility.NextDouble() });
            }
        }

        public static void AddSplash(Vector2 position, Color color = default(Color), int amount = 20)
        {
            if (color == default(Color))
                color = Color.White;
            for (int count = 0; count < amount; count++) {
                var splashVector = new Vector2((float)Utility.Next(-5, 5), (float)Utility.Next(-5, 0));
                Add(new Particle() { Position = position + new Vector2(0, -10), Color = color, Velocity = (splashVector / 30) * (float)Utility.NextDouble(), MaxAge = 0.5, Scale = (float)Utility.NextDouble(), Gravity = true });
            }
        }

        public static void Add(Particle value)
        {
            for (int count = 0; count < particles.Length; count++) {
                if (!particles[count].IsActive) {
                    value.IsActive = true;
                    if (value.Scale == 0)
                        value.Scale = 1;
                    particles[count] = value;
                    return;
                }
            }
        }

        public static void Update(DateTime now, GameTime gameTime)
        {
            for (int count = 0; count < particles.Length; count++) {
                particles[count].Update(now, gameTime);
            }
        }

        public static void Draw(SpriteBatch batch, Point screenOffset)
        {
            batch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp, null, null);

            foreach (var particle in particles) {
                if (particle.IsActive) {
                    var texture = textures[particle.Frame];
                    if (texture == null) {
                        continue;
                    }

                    var textureWidth = texture.Width;
                    var textureHeight = texture.Height;
                    Rectangle? sourceRectangle = null;

                    if (particle.Frame == explosionFrame) {
                        var frame = Math.Min((int)(particle.Age / (1.0 / 48.0)), 47);
                        //Utility.LogMessage("{0} = {1}", particle.Age, frame);
                        sourceRectangle = Utility.GetFrameSourceRectangle(128, 128, 8, frame);
                        textureWidth /= 8;
                        textureHeight /= 6;
                    }

                    batch.Draw
                    (
                        texture,
                        new Vector2((int)particle.Position.X - screenOffset.X, (int)particle.Position.Y - screenOffset.Y),
                        sourceRectangle,
                        particle.IsNotFading ? particle.Color : particle.Color.SetAlpha(1 - particle.Age),
                        particle.Angle,
                        new Vector2(textureWidth / 2, textureHeight / 2), // origin
                        particle.Scale,
                        SpriteEffects.None,
                        0
                    );

                    //krawnix blood
                    if (particle.Frame >= 10 && particle.Frame <= 17) {
                        Add(new Particle() { Position = particle.Position, Color = Color.FromNonPremultiplied(174, 236, 13, 255), MaxAge = 1, Gravity = true, Velocity = Utility.RandomVector() * 0.2f });
                    }

                    //skyboss blood
                    if (particle.Frame >= 18 && particle.Frame <= 22) {
                        if (Utility.Flip())
                            AddExplosion(particle.Position + Utility.RandomVector() * 20);
                    }

                    // harpy blood
                    if (particle.Frame >= 24 && particle.Frame <= 30) {
                        Add(new Particle() { Position = particle.Position, Color = Color.DarkRed, MaxAge = 1, Gravity = true, Velocity = Utility.RandomVector() * 0.2f });
                    }

                    if (particle.Frame >= 31) {
                        Add(new Particle() { Position = particle.Position, Color = Color.FromNonPremultiplied(174, 236, 13, 255), MaxAge = 1, Gravity = true, Velocity = Utility.RandomVector() * 0.2f });
                    }
                }
            }

            batch.End();
        }
    }
}
