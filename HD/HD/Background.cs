using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ParallelTasks;

namespace HD
{
    public static class Background
    {
        static Texture2D MountainBack { get; set; }
        static Texture2D MountainMid { get; set; }
        static Texture2D MountainFore { get; set; }
        static Texture2D Grass { get; set; }
        static Texture2D MountainBackNight { get; set; }
        static Texture2D MountainMidNight { get; set; }
        static Texture2D MountainForeNight { get; set; }
        static Texture2D GrassNight { get; set; }
        static Texture2D CaveBack { get; set; }
        static Texture2D CaveMid { get; set; }
        static Texture2D CaveFore { get; set; }
        static Texture2D DaySky { get; set; }
        static Texture2D NightSky { get; set; }
        static Texture2D Sun { get; set; }
        static Texture2D GasGiant { get; set; }
        static Texture2D LavaTop { get; set; }
        static Texture2D LavaBottom { get; set; }
        static Texture2D LavaBack { get; set; }
        static Texture2D LavaMid { get; set; }
        //static Texture2D LavaFore { get; set; }
        static Texture2D LavaFore1 { get; set; }
        //static Texture2D LavaFore2 { get; set; }
        //static Texture2D LavaFore3 { get; set; }
        //static Texture2D LavaFore4 { get; set; }

        internal static Texture2D Cloud1 { get; set; }
        internal static Texture2D Cloud2 { get; set; }
        internal static Texture2D Cloud3 { get; set; }
        internal static Texture2D Cloud4 { get; set; }
        internal static Texture2D Cloud5 { get; set; }
        internal static Texture2D Cloud6 { get; set; }
        internal static Texture2D Cloud7 { get; set; }
        internal static Texture2D Cloud8 { get; set; }
        internal static Texture2D Cloud9 { get; set; }
        internal static Texture2D Cloud10 { get; set; }
        internal static Texture2D Cloud11 { get; set; }
        internal static Texture2D Cloud12 { get; set; }
        internal static Texture2D Cloud13 { get; set; }
        internal static Texture2D Cloud14 { get; set; }
        internal static Texture2D Cloud1Night { get; set; }
        internal static Texture2D Cloud2Night { get; set; }
        internal static Texture2D Cloud3Night { get; set; }
        internal static Texture2D Cloud4Night { get; set; }
        internal static Texture2D Cloud5Night { get; set; }
        internal static Texture2D Cloud6Night { get; set; }
        internal static Texture2D Cloud7Night { get; set; }
        internal static Texture2D Cloud8Night { get; set; }
        internal static Texture2D Cloud9Night { get; set; }
        internal static Texture2D Cloud10Night { get; set; }
        internal static Texture2D Cloud11Night { get; set; }
        internal static Texture2D Cloud12Night { get; set; }
        internal static Texture2D Cloud13Night { get; set; }
        internal static Texture2D Cloud14Night { get; set; }

        internal static Texture2D FloaterBack1 { get; set; }
        internal static Texture2D FloaterBack1n { get; set; }
        internal static Texture2D FloaterBack2 { get; set; }
        internal static Texture2D FloaterBack2n { get; set; }
        internal static Texture2D FloaterBack3 { get; set; }
        internal static Texture2D FloaterBack3n { get; set; }
        internal static Texture2D FloaterBack4 { get; set; }
        internal static Texture2D FloaterBack4n { get; set; }
        internal static Texture2D FloaterBack5 { get; set; }
        internal static Texture2D FloaterBack5n { get; set; }
        internal static Texture2D FloaterBack6 { get; set; }
        internal static Texture2D FloaterBack6n { get; set; }
        internal static Texture2D FloaterBack7 { get; set; }
        internal static Texture2D FloaterBack7n { get; set; }
        internal static Texture2D FloaterBack8 { get; set; }
        internal static Texture2D FloaterBack8n { get; set; }
        internal static Texture2D FloaterBack9 { get; set; }
        internal static Texture2D FloaterBack9n { get; set; }

        internal static Texture2D FloaterMid1 { get; set; }
        internal static Texture2D FloaterMid1n { get; set; }
        internal static Texture2D FloaterMid2 { get; set; }
        internal static Texture2D FloaterMid2n { get; set; }
        internal static Texture2D FloaterMid3 { get; set; }
        internal static Texture2D FloaterMid3n { get; set; }
        internal static Texture2D FloaterMid4 { get; set; }
        internal static Texture2D FloaterMid4n { get; set; }
        internal static Texture2D FloaterMid5 { get; set; }
        internal static Texture2D FloaterMid5n { get; set; }
        internal static Texture2D FloaterMid6 { get; set; }
        internal static Texture2D FloaterMid6n { get; set; }
        internal static Texture2D FloaterMid7 { get; set; }
        internal static Texture2D FloaterMid7n { get; set; }

        internal static Texture2D FloaterFore1 { get; set; }
        internal static Texture2D FloaterFore1n { get; set; }
        internal static Texture2D FloaterFore2 { get; set; }
        internal static Texture2D FloaterFore2n { get; set; }
        internal static Texture2D FloaterFore3 { get; set; }
        internal static Texture2D FloaterFore3n { get; set; }
        internal static Texture2D FloaterFore4 { get; set; }
        internal static Texture2D FloaterFore4n { get; set; }
        internal static Texture2D FloaterFore5 { get; set; }
        internal static Texture2D FloaterFore5n { get; set; }
        internal static Texture2D FloaterFore6 { get; set; }
        internal static Texture2D FloaterFore6n { get; set; }
        internal static Texture2D FloaterFore7 { get; set; }
        internal static Texture2D FloaterFore7n { get; set; }

        static List<BackgroundFloater> farFloaters;
        static List<BackgroundFloater> midFloaters;
        static List<BackgroundFloater> nearFloaters;
        static List<BackgroundFloater> backFloaters;

        internal static void LoadContent(GraphicsDevice device)
        {
            Utility.LogMessage("Loading background textures.");
            var start = DateTime.UtcNow;

            var properties = typeof(Background).GetProperties(BindingFlags.NonPublic | BindingFlags.Static);
            Parallel.ForEach(properties, property =>
            {
                if (property.PropertyType == typeof(Texture2D))
                {
                    var texture = Utility.LoadTexture(device, "Background/" + property.Name + ".png");
                    if (texture == null)
                        texture = Utility.LoadTexture(device, "Background/" + property.Name + ".jpg");
                    property.SetValue(null, texture, null);
                }
            });

            Utility.LogMessage("Background textures loaded in " + (DateTime.UtcNow - start).ToString());
        }

        public static void Draw(SpriteBatch spriteBatch, Map map, int x, int y, GameTime gameTime)
        {
            var sunPosition = 0f;
            var gasGiantPosition = 0.0f;
            var nightAmount = 0.0;
            var timeOfDay = map.GameTime.Hour * 60.0;
            timeOfDay += map.GameTime.Minute;
            timeOfDay += map.GameTime.Second / 60.0;
            timeOfDay /= 60.0;
            if (timeOfDay < 6)
            {
                nightAmount = 1; // night
            }
            else if (timeOfDay < 7)
            {
                nightAmount = 1 - ((timeOfDay - 6.0) / 1.0); // dawn
            }
            else if (timeOfDay < 19)
            {
                nightAmount = 0; // day
            }
            else if (timeOfDay < 20)
            {
                nightAmount = (timeOfDay - 19.0) / 1.0; // dusk
            }
            else
            {
                nightAmount = 1; // night
            }

            sunPosition = 1 - (((float)timeOfDay - 6f) / 14f);
            if (sunPosition < 0)
                sunPosition = 0;

            var timeOfNight = (timeOfDay + 36f) % 24f;
            gasGiantPosition = 1 - (((float)timeOfNight - 7f) / 11f);
            //Utility.LogMessage(gasGiantPosition.ToString());
            if (gasGiantPosition < 0)
                gasGiantPosition = 0;

            var dayAmount = 1 - (float)nightAmount;
            var dayColor = Color.White.SetAlpha(dayAmount);

            spriteBatch.Begin();
            var maxSize = Math.Max(Main.ResolutionWidth, Main.ResolutionHeight);
            spriteBatch.Draw(Main.Starfield, new Rectangle(0, 0, maxSize, maxSize), Color.FromNonPremultiplied(255, 255, 255, 192));

            if (DaySky != null && map.SeaLevel > 0)
            {
                var verticalScrollRate = (float)DaySky.Height / (float)(map.SeaLevelInPixels);
                //Utility.LogMessage(verticalScrollRate.ToString());
                var skyVerticalOffset = Math.Max((map.SeaLevelInPixels - DaySky.Height - y - 5000) * 0.15f, Main.ResolutionHeight - 4096);

                spriteBatch.Draw(DaySky, new Rectangle(0, (int)skyVerticalOffset, Main.ResolutionWidth, 4096), null, dayColor);

                if (nightAmount < 1)
                {
                    var quadraticX = (sunPosition - 0.5f) * 2;
                    var verticalPosition = quadraticX * quadraticX;
                    //Utility.LogMessage(verticalPosition.ToString());
                    spriteBatch.Draw(Sun, new Vector2(((Main.ResolutionWidth + Sun.Width) * sunPosition) - Sun.Width / 2, Main.ResolutionHeight * verticalPosition), null, Color.White, sunPosition * (float)Math.PI, new Vector2(Sun.Width / 2, Sun.Height / 2), 1f, SpriteEffects.None, 0);
                }

                //if (nightAmount > 0)
                {
                    var quadraticX = (gasGiantPosition - 0.5f) * 2;
                    var verticalPosition = quadraticX * quadraticX;
                    //Utility.LogMessage(verticalPosition.ToString());
                    spriteBatch.Draw(GasGiant, new Vector2(((Main.ResolutionWidth + GasGiant.Width) * gasGiantPosition) - GasGiant.Width / 2, Main.ResolutionHeight * verticalPosition), null, Color.White, gasGiantPosition * (float)Math.PI, new Vector2(GasGiant.Width / 2, GasGiant.Height / 2), 1f, SpriteEffects.None, 0);
                }
            }

            // draw a black background for underground bits
            spriteBatch.DrawRectangle(new Rectangle(0, map.SeaLevelInPixels - y, Main.ResolutionWidth, map.PixelHeight - map.SeaLevelInPixels), Color.Black);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, null, Utility.BlendEffect);

            if (farFloaters == null && map != null)
            {
                backFloaters = BackgroundFloater.GenerateFarClouds(map);

                midFloaters = BackgroundFloater.GenerateMidClouds(map);
                midFloaters = BackgroundFloater.GenerateMidIslands(map, midFloaters);

                nearFloaters = BackgroundFloater.GenerateForeClouds(map);
                nearFloaters = BackgroundFloater.GenerateForeIslands(map, nearFloaters);

                farFloaters = BackgroundFloater.GenerateFarIslands(map);
            }

            var screenOffset = new Vector2(x, y);
            var cameraAltitude = -(y + (Main.ResolutionHeight / 2) - map.SeaLevelInPixels);
            //Utility.LogMessage(altitude.ToString());

            if (backFloaters != null)
            {
                foreach (var floater in backFloaters)
                    floater.Draw(spriteBatch, screenOffset, cameraAltitude, dayAmount);
            }

            RenderParallaxTiledMountain(spriteBatch, MountainBack, MountainBackNight, x, 0, (int)(Main.ResolutionHeight - 200) + (int)(cameraAltitude * 0.15f), 0, 0.15f, 900, dayAmount);

            if (farFloaters != null)
            {
                foreach (var floater in farFloaters)
                    floater.Draw(spriteBatch, screenOffset, cameraAltitude, dayAmount);
            }

            RenderParallaxTiledMountain(spriteBatch, MountainMid, MountainMidNight, x, 0, (int)(Main.ResolutionHeight - 200) + (int)(cameraAltitude * 0.25f), 0, 0.25f, 900, dayAmount);

            if (midFloaters != null)
            {
                foreach (var floater in midFloaters)
                    floater.Draw(spriteBatch, screenOffset, cameraAltitude, dayAmount);
            }

            RenderParallaxTiledMountain(spriteBatch, MountainFore, MountainForeNight, x, 0, (int)(Main.ResolutionHeight - 175) + (int)(cameraAltitude * 0.35f), 0, 0.35f, 600, dayAmount);

            if (nearFloaters != null)
            {
                foreach (var floater in nearFloaters)
                    floater.Draw(spriteBatch, screenOffset, cameraAltitude, dayAmount);
            }

            var caveStart = map.SeaLevelInPixels + 2248;
            var caveEnd = (map.LavaLevel - map.SeaLevel) * Map.BlockHeight - 200;
            if (map.LavaLevel == 0)
                caveEnd = map.PixelHeight;
            RenderParallaxTiled(spriteBatch, CaveBack, null, x, y, map.SeaLevelInPixels + 2248, caveEnd, 0.1f, 0.9f, 0);
            RenderParallaxTiled(spriteBatch, CaveMid, null, x, y, map.SeaLevelInPixels + 2248, caveEnd, 0.4f, 0.7f, 0);
            RenderParallaxTiled(spriteBatch, CaveFore, null, x, y, map.SeaLevelInPixels + 2248, caveEnd, 0.85f, 0, 0);

            RenderParallaxTiled(spriteBatch, Grass, GrassNight, x, y, map.SeaLevelInPixels + 200, 0, 0.85f, 0, 0, dayAmount);

            if (map.LavaLevel != 0 && LavaBack != null)
            {
                var lavaStartY = map.LavaLevel * Map.BlockHeight + LavaBack.Height;
                RenderParallaxTiled(spriteBatch, LavaBack, null, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.1f, 0.9f, 0);
                RenderParallaxTiled(spriteBatch, LavaMid, null, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.4f, 0.6f, 0);

                RenderParallaxTiled(spriteBatch, LavaFore1, null, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.90f, 0, 0);
                //var frame = gameTime.TotalGameTime.TotalSeconds % 4;
                //if (frame < 1)
                //{
                //    RenderParallaxTiled(spriteBatch, LavaFore1, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.90f, 0, 0, Color.White.SetAlpha(1.0 - frame));
                //    RenderParallaxTiled(spriteBatch, LavaFore2, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.90f, 0, 0, Color.White.SetAlpha(frame));
                //}
                //else if (frame < 2)
                //{
                //    frame = frame % 1;
                //    RenderParallaxTiled(spriteBatch, LavaFore2, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.90f, 0, 0, Color.White.SetAlpha(1.0 - frame));
                //    RenderParallaxTiled(spriteBatch, LavaFore3, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.90f, 0, 0, Color.White.SetAlpha(frame));
                //}
                //else if (frame < 3)
                //{
                //    frame = frame % 1;
                //    RenderParallaxTiled(spriteBatch, LavaFore3, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.90f, 0, 0, Color.White.SetAlpha(1.0 - frame));
                //    RenderParallaxTiled(spriteBatch, LavaFore4, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.90f, 0, 0, Color.White.SetAlpha(frame));
                //}
                //else
                //{
                //    frame = frame % 1;
                //    RenderParallaxTiled(spriteBatch, LavaFore4, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.90f, 0, 0, Color.White.SetAlpha(1.0 - frame));
                //    RenderParallaxTiled(spriteBatch, LavaFore1, x, y, lavaStartY, map.PixelHeight - lavaStartY + LavaBack.Height, 0.90f, 0, 0, Color.White.SetAlpha(frame));
                //}

                RenderParallaxTiled(spriteBatch, LavaTop, null, x, y, lavaStartY - LavaBack.Height, LavaTop.Height, 0.90f, 0, 0);
                RenderParallaxTiled(spriteBatch, LavaBottom, null, x, y, map.PixelHeight, 0, 0.95f, 0, 0);
            }

            spriteBatch.End();
        }

        static void RenderParallaxTiledMountain(SpriteBatch batch, Texture2D texture, Texture2D blendTexture, int screenX, int screenY, int startY, int height, float horizontalRate, int horizontalOffset, float blendAmount = 1)
        {
            if (texture == null)
                return;

            startY -= texture.Height;
            var source = new Rectangle(Convert.ToInt32(screenX * horizontalRate) + horizontalOffset, 0, Main.ResolutionWidth, texture.Height);

            if (blendTexture != null)
                Utility.BlendEffect.Parameters["xTexture2"].SetValue(blendTexture);
            Utility.BlendEffect.Parameters["xBlendAmount"].SetValue(blendAmount);

            batch.Draw(texture, new Vector2(0, startY - screenY), source, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);

            //Utility.LogMessage((startY - screenY).ToString());
        }

        static void RenderParallaxTiled(SpriteBatch batch, Texture2D texture, Texture2D blendTexture, int screenX, int screenY, int startY, int height, float horizontalRate, float verticalRate, int horizontalOffset, float blendAmount = 1)
        {
            if (texture == null)
                return;

            // todo: on screen optimization check?

            if (height == 0)
                height = texture.Height;

            startY -= texture.Height;
            var source = new Rectangle(Convert.ToInt32(screenX * horizontalRate) + horizontalOffset, Convert.ToInt32(screenY * -verticalRate), Main.ResolutionWidth, height);

            if (blendTexture != null)
                Utility.BlendEffect.Parameters["xTexture2"].SetValue(blendTexture);
            Utility.BlendEffect.Parameters["xBlendAmount"].SetValue(blendAmount);

            batch.Draw(texture, new Vector2(0, startY - screenY), source, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
        }

        internal static void ClearMap()
        {
            backFloaters = null;
            midFloaters = null;
            nearFloaters = null;

            farFloaters = null; // this must be last since it's used to check existence of the rest.
        }
    }
}