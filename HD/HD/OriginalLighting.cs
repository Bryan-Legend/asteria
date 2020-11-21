using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HD
{
    public static class OriginalLighting
    {
        const float diagonalBrightestFactor = 0.935f;
        const int margin = 10;
        const int blockSize = 8;

        public static Texture2D Texture { get; set; }
        static uint[] textureBuffer;
        static int width;
        static int height;
        static Material[,] materialBuffer;
        static float[,] shadingBuffer;
        static float[,] shadingBuffer2;
        static int bufferOffsetX;
        static int bufferOffsetY;

        public static Color GetSkyColor()
        {
            return Main.DayNightGradientData[(int)Main.Map.GameTime.TimeOfDay.TotalMinutes];

            //return Color.FromNonPremultiplied(0, 0, 0, 128);
        }

        public static void Update(GraphicsDevice device)
        {
            var currentOffsetX = (Main.ScreenOffset.X / blockSize);
            var currentOffsetY = (Main.ScreenOffset.Y / blockSize);

            if (Texture == null)
            {
                width = (Main.ResolutionWidth / blockSize) + margin * 2;
                height = (Main.ResolutionHeight / blockSize) + margin * 2;
                Texture = new Texture2D(device, width, height, false, SurfaceFormat.Color);
                textureBuffer = new uint[width * height];
                materialBuffer = new Material[width, height];
                shadingBuffer = new float[width, height];
                shadingBuffer2 = new float[width, height];
            }
            else
            {
                // move the buffer to match player movement
                if (currentOffsetX != bufferOffsetX || currentOffsetY != bufferOffsetY)
                {
                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            var sourceX = x + (currentOffsetX - bufferOffsetX);
                            var sourceY = y + (currentOffsetY - bufferOffsetY);
                            if (sourceX < 0 || sourceX >= width || sourceY < 0 || sourceY >= height)
                                shadingBuffer2[x, y] = 0;
                            else
                                shadingBuffer2[x, y] = shadingBuffer[sourceX, sourceY];
                        }
                    }

                    var tempBuffer = shadingBuffer;
                    shadingBuffer = shadingBuffer2;
                    shadingBuffer2 = tempBuffer;
                }
            }

            bufferOffsetX = currentOffsetX;
            bufferOffsetY = currentOffsetY;

            var skyColor = 1f - (GetSkyColor().A / 255f);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xOffset = (currentOffsetX + x) * blockSize;
                    var yOffset = (currentOffsetY + y) * blockSize;

                    xOffset -= margin * blockSize;
                    yOffset -= margin * blockSize;

                    var material = Main.Map.GetMaterialAtPixel(xOffset, yOffset);
                    materialBuffer[x, y] = material;

                    if (MaterialInfo.IsLooseOrSolid(material))
                        //shadingBuffer[x, y] = Math.Max(0, (shadingBuffer[x, y] - 0.05f) * shadingBuffer[x, y]);
                        shadingBuffer[x, y] = Math.Max(0, shadingBuffer[x, y] - 0.1f);
                    else
                    {
                        // set day light
                        if (yOffset < Main.Map.SeaLevel * Map.BlockHeight)
                        {
                            shadingBuffer[x, y] = Math.Max(shadingBuffer[x, y], skyColor);
                        }
                    }

                    var materialInfo = MaterialInfo.MaterialTypes[(byte)material];
                    if (materialInfo.Light.A > 0)
                    {
                        shadingBuffer[x, y] = Math.Max(shadingBuffer[x, y], 1f - (materialInfo.Light.A / 255f));
                    }
                }
            }

            // entity lighting
            foreach (var entity in Main.Map.Entities)
            {
                if (entity.Light.A > 0 && Main.Player.IsOnScreen(entity))
                {
                    var x = ((int)entity.Position.X - (currentOffsetX * blockSize) + 100 - (blockSize)) / blockSize;
                    var y = ((int)entity.Position.Y - (currentOffsetY * blockSize) + 100 - (blockSize)) / blockSize;
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        //shadingBuffer[x, y] = 1f - (placeable.Type.Light.A / 255f);
                        shadingBuffer[x, y] = 1f;
                    }
                }
            }

            for (var y = 5; y < height - 5; y++)
            {
                for (var x = 5; x < width - 5; x++)
                {
                    var value = shadingBuffer[x, y];
                    var material = materialBuffer[x, y];
                    if (MaterialInfo.IsLooseOrSolid(material))
                        shadingBuffer2[x, y] = BrightestNeighbor(x, y);
                    //shadingBuffer2[x, y] = Math.Max(0, BrightestNeighbor(x, y) - 0.05f);
                    else
                    {
                        if (MaterialInfo.IsGas(material))
                            shadingBuffer2[x, y] = Math.Max(0, BrightestNeighbor(x, y) - 0.01f);
                        else
                            // liquid
                            shadingBuffer2[x, y] = Math.Max(0, BrightestNeighbor(x, y) - 0.02f);
                    }
                    //shadingBuffer2[x, y] = BrightestNeighbor(x, y);
                    
                    textureBuffer[(y * width) + x] = Color.FromNonPremultiplied(0, 0, 0, 255 - (byte)(value * 255)).PackedValue;
                }
            }

            var temp = shadingBuffer;
            shadingBuffer = shadingBuffer2;
            shadingBuffer2 = temp;

            /*
             * fade grid 10%
             * 
             * set lights to 100%
             * 
             * foreach block
             *     if solid
             *         80% of brightest neighbor
             *     else
             *         95% of brightest neighbor
             */

            if (!Texture.IsDisposed)
                Texture.SetData<uint>(textureBuffer);
        }

        static float BrightestNeighbor(int x, int y)
        {
            var brightest = 0f;
            var current = shadingBuffer[x + 1, y];
            if (current > brightest)
                brightest = current;
            current = shadingBuffer[x - 1, y];
            if (current > brightest)
                brightest = current;
            current = shadingBuffer[x, y + 1];
            if (current > brightest)
                brightest = current;
            current = shadingBuffer[x, y - 1];
            if (current > brightest)
                brightest = current;

            current = shadingBuffer[x + 1, y + 1] * diagonalBrightestFactor;
            if (current > brightest)
                brightest = current;
            current = shadingBuffer[x - 1, y + 1] * diagonalBrightestFactor;
            if (current > brightest)
                brightest = current;
            current = shadingBuffer[x + 1, y - 1] * diagonalBrightestFactor;
            if (current > brightest)
                brightest = current;
            current = shadingBuffer[x - 1, y - 1] * diagonalBrightestFactor;
            if (current > brightest)
                brightest = current;

            return brightest;
        }

        public static void Draw(SpriteBatch spriteBatch, Rectangle viewPort)
        {
            if (Texture != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null);
                spriteBatch.Draw(Texture, new Rectangle((-margin * blockSize) - (Main.ScreenOffset.X % blockSize), (-margin * blockSize) - (Main.ScreenOffset.Y % blockSize), Main.ResolutionWidth + (margin * blockSize * 2), Main.ResolutionHeight + (margin * blockSize * 2)), Color.White);
                spriteBatch.End();
            }
        }

        public static void Update2(GraphicsDevice device)
        {
            var reductionMap = CreateReductionMap();
        }

        public static float[][] CreateReductionMap()
        {
            var currentOffsetX = (Main.ScreenOffset.X / blockSize);
            var currentOffsetY = (Main.ScreenOffset.Y / blockSize);
            var reductionMap = new float[height][];

            for (int count = 0; count < reductionMap.Length; count++)
            {
                reductionMap[count] = new float[width];
            }

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xOffset = (currentOffsetX + x) * blockSize;
                    var yOffset = (currentOffsetY + y) * blockSize;

                    xOffset -= margin * blockSize;
                    yOffset -= margin * blockSize;

                    var material = Main.Map.GetMaterialAtPixel(xOffset, yOffset);
                    //var materialInfo = MaterialInfo.MaterialTypes[(byte)material];

                    if (MaterialInfo.IsGas(material))
                        reductionMap[y][x] = 0;
                    else
                    {
                        if (MaterialInfo.IsLooseOrSolid(material))
                        {
                            reductionMap[y][x] = 0.8f;
                        }
                        else
                        {
                            reductionMap[y][x] = 0.9f;
                        }
                    }
                }
            }

            return reductionMap;
        }
    }
}