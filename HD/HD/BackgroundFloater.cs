using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;

namespace HD
{
    public class BackgroundFloater
    {
        public Vector2 Position; // X of this is position on the map. Y is altitude above sea level.
        public float Depth;
        public Texture2D DayTexture;
        public Texture2D NightTexture;
        //public bool IsMoving;

        public void Draw(SpriteBatch batch, Vector2 screenOffset, int cameraAltitude, float dayAmount)
        {
            //if (IsMoving)
            //    Position += new Vector2(Depth * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 100, 0);

            var position = new Vector2(Position.X - (screenOffset.X * Depth), (cameraAltitude - Position.Y) * Depth);
            Utility.DrawBlend(batch, DayTexture, NightTexture, dayAmount, position);

            //var height = 500;
            //var depth = 0.35f;
            //var visiblePosition = new Vector2(screenOffset.X, (cameraAltitude - height) * depth);
            //var renderPosition = new Vector2(0, visiblePosition.Y);
            //spriteBatch.Draw(Background.Cloud1, renderPosition, Color.White);

        }

        const int totalCloudHeight = 4000;
        const int cloudSectionHeight = totalCloudHeight / 3;

        public static List<BackgroundFloater> GenerateFarClouds(Map map)
        {
            var result = new List<BackgroundFloater>();

            if (map.SeaLevel <= 0)
                return result;

            const float baseDepth = 0.11f;

            for (int x = -100; x < map.PixelWidth * baseDepth + (500 / baseDepth); x += 32)
            {
                if (Utility.Roll8())
                {
                    var newFloater = new BackgroundFloater() { Position = new Vector2(x, Utility.Next(cloudSectionHeight) - 300), Depth = baseDepth + (float)(Utility.NextDouble() * 0.05) };
                    result.Add(newFloater);
                    switch (Utility.Next(4) + 1)
                    {
                        //11-14 (far & low group) -
                        //should be behind all mountains and cruising around the horizon level, in the area generally above the mountain tops
                        case 1:
                            newFloater.DayTexture = Background.Cloud11;
                            newFloater.NightTexture = Background.Cloud11Night;
                            break;
                        case 2:
                            newFloater.DayTexture = Background.Cloud12;
                            newFloater.NightTexture = Background.Cloud12Night;
                            break;
                        case 3:
                            newFloater.DayTexture = Background.Cloud13;
                            newFloater.NightTexture = Background.Cloud13Night;
                            break;
                        case 4:
                            newFloater.DayTexture = Background.Cloud14;
                            newFloater.NightTexture = Background.Cloud14Night;
                            break;
                    }
                }
            }

            result = result.OrderBy(item => item.Depth).ToList();
            return result;
        }

        public static List<BackgroundFloater> GenerateMidClouds(Map map)
        {
            var result = new List<BackgroundFloater>();

            if (map.SeaLevel <= 0)
                return result;

            const float baseDepth = 0.26f;

            for (int x = -100; x < map.PixelWidth * baseDepth + (500 / baseDepth); x += 32)
            {
                if (Utility.Roll8())
                {
                    var newFloater = new BackgroundFloater() { Position = new Vector2(x, Utility.Next(cloudSectionHeight) + cloudSectionHeight + 100), Depth = baseDepth + (float)(Utility.NextDouble() * 0.10) };
                    result.Add(newFloater);
                    switch (Utility.Next(5) + 1)
                    {
                        //6-10 (mid & mid group) -
                        //mid altitude, and can be interspersed anywhere behind the fore mountains, so they could be higher up, or could be drifting in front of the mid or far mountains
                        case 1:
                            newFloater.DayTexture = Background.Cloud6;
                            newFloater.NightTexture = Background.Cloud6Night;
                            break;
                        case 2:
                            newFloater.DayTexture = Background.Cloud7;
                            newFloater.NightTexture = Background.Cloud7Night;
                            break;
                        case 3:
                            newFloater.DayTexture = Background.Cloud8;
                            newFloater.NightTexture = Background.Cloud8Night;
                            break;
                        case 4:
                            newFloater.DayTexture = Background.Cloud9;
                            newFloater.NightTexture = Background.Cloud9Night;
                            break;
                        case 5:
                            newFloater.DayTexture = Background.Cloud10;
                            newFloater.NightTexture = Background.Cloud10Night;
                            break;
                    }
                }
            }

            result = result.OrderBy(item => item.Depth).ToList();
            return result;
        }

        public static List<BackgroundFloater> GenerateForeClouds(Map map)
        {
            var result = new List<BackgroundFloater>();

            if (map.SeaLevel <= 0)
                return result;

            const float baseDepth = 0.36f;

            for (int x = -100; x < map.PixelWidth * baseDepth + (500 / baseDepth); x += 32)
            {
                if (Utility.Roll8())
                {
                    var newFloater = new BackgroundFloater() { Position = new Vector2(x, Utility.Next(cloudSectionHeight) + cloudSectionHeight + cloudSectionHeight + 200), Depth = baseDepth + (float)(Utility.NextDouble() * 0.15) };
                    result.Add(newFloater);
                    switch (Utility.Next(5) + 1)
                    {
                        //1-5 (near & high group) -
                        //normally I'd say upper altitude, but they could be brought down lower to drift in front of the fore mountains, especially if scaled up a bit and made more transparent (like a foggier misty cloud).
                        case 1:
                            newFloater.DayTexture = Background.Cloud1;
                            newFloater.NightTexture = Background.Cloud1Night;
                            break;
                        case 2:
                            newFloater.DayTexture = Background.Cloud2;
                            newFloater.NightTexture = Background.Cloud2Night;
                            break;
                        case 3:
                            newFloater.DayTexture = Background.Cloud3;
                            newFloater.NightTexture = Background.Cloud3Night;
                            break;
                        case 4:
                            newFloater.DayTexture = Background.Cloud4;
                            newFloater.NightTexture = Background.Cloud4Night;
                            break;
                        case 5:
                            newFloater.DayTexture = Background.Cloud5;
                            newFloater.NightTexture = Background.Cloud5Night;
                            break;
                    }
                }
            }

            result = result.OrderBy(item => item.Depth).ToList();
            return result;
        }

        public static List<BackgroundFloater> GenerateFarIslands(Map map)
        {
            var result = new List<BackgroundFloater>();
            if (map.SeaLevel <= 0)
                return result;

            const int minHeight = 0;
            const int randomHeight = 650;
            const float baseDepth = 0.15f;

            for (int x = 0; x < map.PixelWidth * baseDepth + (500 / baseDepth); x += 32)
            {
                if (Utility.Roll8())
                {
                    var newFloater = new BackgroundFloater() { Position = new Vector2(x, minHeight + Utility.Next(randomHeight)), Depth = baseDepth + (float)(Utility.NextDouble() * 0.1) };
                    result.Add(newFloater);
                    switch (Utility.Next(9) + 1)
                    {
                        case 1:
                            newFloater.DayTexture = Background.FloaterBack1;
                            newFloater.NightTexture = Background.FloaterBack1n;
                            break;
                        case 2:
                            newFloater.DayTexture = Background.FloaterBack2;
                            newFloater.NightTexture = Background.FloaterBack2n;
                            break;
                        case 3:
                            newFloater.DayTexture = Background.FloaterBack3;
                            newFloater.NightTexture = Background.FloaterBack3n;
                            break;
                        case 4:
                            newFloater.DayTexture = Background.FloaterBack4;
                            newFloater.NightTexture = Background.FloaterBack4n;
                            break;
                        case 5:
                            newFloater.DayTexture = Background.FloaterBack5;
                            newFloater.NightTexture = Background.FloaterBack5n;
                            break;
                        case 6:
                            newFloater.DayTexture = Background.FloaterBack6;
                            newFloater.NightTexture = Background.FloaterBack6n;
                            break;
                        case 7:
                            newFloater.DayTexture = Background.FloaterBack7;
                            newFloater.NightTexture = Background.FloaterBack7n;
                            break;
                        case 8:
                            newFloater.DayTexture = Background.FloaterBack7;
                            newFloater.NightTexture = Background.FloaterBack7n;
                            break;
                        case 9:
                            newFloater.DayTexture = Background.FloaterBack7;
                            newFloater.NightTexture = Background.FloaterBack7n;
                            break;
                    }
                }
            }

            result = result.OrderBy(item => item.Depth).ToList();
            return result;
        }

        public static List<BackgroundFloater> GenerateMidIslands(Map map, List<BackgroundFloater> result)
        {
            if (map.SeaLevel <= 0)
                return result;

            const int minHeight = 100;
            const int randomHeight = 2000;
            const float baseDepth = 0.25f;

            for (int x = 0; x < map.PixelWidth * baseDepth + (500 / baseDepth); x += 32)
            {
                if (Utility.Roll8())
                {
                    var newFloater = new BackgroundFloater() { Position = new Vector2(x, minHeight + Utility.Next(randomHeight)), Depth = baseDepth + (float)(Utility.NextDouble() * 0.1) };
                    result.Add(newFloater);
                    switch (Utility.Next(7) + 1)
                    {
                        case 1:
                            newFloater.DayTexture = Background.FloaterMid1;
                            newFloater.NightTexture = Background.FloaterMid1n;
                            break;
                        case 2:
                            newFloater.DayTexture = Background.FloaterMid2;
                            newFloater.NightTexture = Background.FloaterMid2n;
                            break;
                        case 3:
                            newFloater.DayTexture = Background.FloaterMid3;
                            newFloater.NightTexture = Background.FloaterMid3n;
                            break;
                        case 4:
                            newFloater.DayTexture = Background.FloaterMid4;
                            newFloater.NightTexture = Background.FloaterMid4n;
                            break;
                        case 5:
                            newFloater.DayTexture = Background.FloaterMid5;
                            newFloater.NightTexture = Background.FloaterMid5n;
                            break;
                        case 6:
                            newFloater.DayTexture = Background.FloaterMid6;
                            newFloater.NightTexture = Background.FloaterMid6n;
                            break;
                        case 7:
                            newFloater.DayTexture = Background.FloaterMid7;
                            newFloater.NightTexture = Background.FloaterMid7n;
                            break;
                    }
                }
            }

            result = result.OrderBy(item => item.Depth).ToList();
            return result;
        }


        public static List<BackgroundFloater> GenerateForeIslands(Map map, List<BackgroundFloater> result)
        {
            if (map.SeaLevel <= 0)
                return result;

            const int minHeight = -300;
            const int randomHeight = 2500;
            const float baseDepth = 0.40f;

            for (int x = 0; x < map.PixelWidth * baseDepth + (500 / baseDepth); x += 32)
            {
                if (Utility.Roll8())
                {
                    var newFloater = new BackgroundFloater() { Position = new Vector2(x, minHeight + Utility.Next(randomHeight)), Depth = baseDepth + (float)(Utility.NextDouble() * 0.30) };
                    result.Add(newFloater);
                    switch (Utility.Next(7) + 1)
                    {
                        case 1:
                            newFloater.DayTexture = Background.FloaterFore1;
                            newFloater.NightTexture = Background.FloaterFore1n;
                            break;
                        case 2:
                            newFloater.DayTexture = Background.FloaterFore2;
                            newFloater.NightTexture = Background.FloaterFore2n;
                            break;
                        case 3:
                            newFloater.DayTexture = Background.FloaterFore3;
                            newFloater.NightTexture = Background.FloaterFore3n;
                            break;
                        case 4:
                            newFloater.DayTexture = Background.FloaterFore4;
                            newFloater.NightTexture = Background.FloaterFore4n;
                            break;
                        case 5:
                            newFloater.DayTexture = Background.FloaterFore5;
                            newFloater.NightTexture = Background.FloaterFore5n;
                            break;
                        case 6:
                            newFloater.DayTexture = Background.FloaterFore6;
                            newFloater.NightTexture = Background.FloaterFore6n;
                            break;
                        case 7:
                            newFloater.DayTexture = Background.FloaterFore7;
                            newFloater.NightTexture = Background.FloaterFore7n;
                            break;
                    }
                }
            }

            result = result.OrderBy(item => item.Depth).ToList();
            return result;
        }
    }
}