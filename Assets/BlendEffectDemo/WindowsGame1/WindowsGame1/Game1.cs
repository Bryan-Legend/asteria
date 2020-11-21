using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TextureBlendUsingAlpha
{
    /// <summary>
    /// Blends two textures, but only uses alpha of first.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D primaryTexture;
        Texture2D secondaryTexture;
        Effect blendEffect;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // something reasonable
            graphics.PreferredBackBufferWidth = 1440;
            graphics.PreferredBackBufferHeight = 768;

            IsFixedTimeStep = false;


        }

        protected override void Initialize()
        {


            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load our textures
            //primaryTexture = Content.Load<Texture2D>("MountainBack");
            //secondaryTexture = Content.Load<Texture2D>("MountainBackNight");

            //secondaryTexture = Content.Load<Texture2D>("MountainBack");
            //primaryTexture = Content.Load<Texture2D>("MountainBackNight");

            primaryTexture = Content.Load<Texture2D>("Cloud1Night");
            secondaryTexture = Content.Load<Texture2D>("Cloud1");
            
            // load that shader, yo.
            blendEffect = Content.Load<Effect>("BlendEffect");

        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();



            base.Update(gameTime);
        }

        private void BeginFade(SpriteBatch batch)
        {
            // going to assume that the blendeffect is in scope, if not then it can be passed in

            // going to keep everything pre multiplied
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, blendEffect);
        }

        private void EndFade(SpriteBatch batch)
        {
            // nothing fancy here yet, but maybe you'll want to restore the previous spritebatch if you're going
            // to reuse this batch to draw something else.
            batch.End();
        }

        private void DrawFade(SpriteBatch batch, Texture2D primary, Texture2D secondary, float amount, Vector2 position)
        {
            // since the first register gets filled with whatever you send to the draw method, we really only need to set
            // the second one as a parameter. 
            blendEffect.Parameters["xTexture2"].SetValue(secondary);

            // Optionally we can set our sampling rules, but I've just left them default for now and hard
            // coded them into the shader. If at some point you decide you want the "minecraftey pixelated look" on something,
            // you can change the magnification filters to "POINT" Your texels will have to be larger than output pixels for the effect to show, though.

            // set the value in the shader
            blendEffect.Parameters["xBlendAmount"].SetValue(amount);

            batch.Draw(primaryTexture, position, Color.White);

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // lets make the sprites follow the mouse, just so we know the positioning works.
            Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            // Here's where we can start drawing
            BeginFade(spriteBatch);

            // can have as many of these as you like, just like with the regular spritebatch
            DrawFade(spriteBatch, primaryTexture, secondaryTexture, 0.0f, mousePosition + (Vector2.UnitX * primaryTexture.Width));
            DrawFade(spriteBatch, primaryTexture, secondaryTexture, 1.0f, mousePosition + (Vector2.UnitX * -primaryTexture.Width));
            DrawFade(spriteBatch, primaryTexture, secondaryTexture, GetPulse(gameTime, 4.0f), mousePosition);

            // call this at the end, just like with a regular sprite batch.
            EndFade(spriteBatch);

            spriteBatch.Begin();

            spriteBatch.Draw(primaryTexture, Vector2.Zero, Color.White);
            spriteBatch.Draw(secondaryTexture, new Vector2(0, primaryTexture.Height), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private float GetPulse(GameTime gameTime, float freq)
        {
            float remappedSin = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * freq);
            // normalize
            remappedSin = (remappedSin + 1) / 2;

            return remappedSin;
        }
    }
}
