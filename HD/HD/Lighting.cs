using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using ParallelTasks;

namespace HD
{
    public static class Lighting
    {
        //const float diagonalBrightestFactor = 0.99f;
        ////const float diagonalBrightestFactor = 0.935f;
        const int blockSize = 8;

        //public static Texture2D Texture { get; set; }
        static int width;
        static int height;

        static uint[] lightSources;
        static Texture2D lightSourcesTexture;
        static float[] occulsionMap;
        static Texture2D occulsionMapTexture;

        //static uint[] shadingBuffer;
        //static uint[] shadingBuffer2;

        static RenderTarget2D shadingTexture;
        static RenderTarget2D shadingTexture2;
        
        static int bufferOffsetX;
        static int bufferOffsetY;
        static BlendState specialBlend;
        public static Effect LightSpreadEffect;
        private static SpriteBatch spriteBatch;

        static int offsetFromPreviousX;
        static int offsetFromPreviousY;

        public static Color GetSkyColor()
        {
            return Main.DayNightGradientData[(int)Main.Map.GameTime.TimeOfDay.TotalMinutes];
        }

        public static void Initialize(GraphicsDevice device)
        {
            LightSpreadEffect.CurrentTechnique = LightSpreadEffect.Techniques["LightSpreadEffect"];
            spriteBatch = new SpriteBatch(device);
        }

        public static void Update(GraphicsDevice device)
        {
            var currentOffsetX = (Main.ScreenOffset.X / blockSize);
            var currentOffsetY = (Main.ScreenOffset.Y / blockSize);

            if (lightSourcesTexture == null)
            {
                width = (Main.ResolutionWidth / blockSize) + Utility.LightingMargin * 2;
                height = (Main.ResolutionHeight / blockSize) + Utility.LightingMargin * 2;
                //Texture = new Texture2D(device, width, height, false, SurfaceFormat.Color);

                lightSources = new uint[height * width];
                occulsionMap = new float[height * width];
                //shadingBuffer = new uint[width * height];
                //shadingBuffer2 = new uint[width * height];

                lightSourcesTexture = new Texture2D(device, width, height);
                occulsionMapTexture = new Texture2D(device, width, height, false, SurfaceFormat.Single);
                shadingTexture = new RenderTarget2D(device, width, height);
                shadingTexture2 = new RenderTarget2D(device, width, height);

                bufferOffsetX = currentOffsetX;
                bufferOffsetY = currentOffsetY;
            }

            offsetFromPreviousX = currentOffsetX - bufferOffsetX;
            offsetFromPreviousY = currentOffsetY - bufferOffsetY;

            bufferOffsetX = currentOffsetX;
            bufferOffsetY = currentOffsetY;

            UpdateOcculsionMapAndMaterialLighting();

            // entity lighting
            var playerScreenOffset = Main.Player.GetScreenOffset();
            var screenRectangle = new Rectangle((int)playerScreenOffset.X, (int)playerScreenOffset.Y, Main.ResolutionWidth, Main.ResolutionHeight);
            screenRectangle.Inflate(Utility.LightingMargin * blockSize * 2, Utility.LightingMargin * blockSize * 2);

            lock (Main.Map.Entities)
            {
                foreach (var entity in Main.Map.Entities)
                {
                    if (entity.Light.A > 0 && screenRectangle.Intersects(entity.OffsetBoundingBox))
                    {
                        var x = ((int)entity.Position.X / blockSize) - currentOffsetX + Utility.LightingMargin;
                        var y = ((int)entity.Position.Y / blockSize) - currentOffsetY + Utility.LightingMargin;
                        if (x >= 0 && x < width && y >= 0 && y < height)
                        {
                            lightSources[(y * width) + x] = entity.Light.SetAlpha(1).PackedValue;
                        }
                    }
                }
            }

            device.Textures[0] = null;
            device.Textures[1] = null;
            device.Textures[2] = null;

            lightSourcesTexture.SetData<uint>(lightSources);
            occulsionMapTexture.SetData<float>(occulsionMap);

            //DoLightingSpread(shadingBuffer, shadingBuffer2, offsetFromPreviousX, offsetFromPreviousY);
            //DoLightingSpread(shadingBuffer2, shadingBuffer, 0, 0);

            //if (!Texture.IsDisposed)
            //    Texture.SetData<uint>(shadingBuffer);
                //Texture.SetData<uint>(lightSources);

            DoShaderLightingSpread(shadingTexture, shadingTexture2);
            DoShaderLightingSpread(shadingTexture2, shadingTexture);
            DoShaderLightingSpread(shadingTexture, shadingTexture2);
            DoShaderLightingSpread(shadingTexture2, shadingTexture);
            DoShaderLightingSpread(shadingTexture, shadingTexture2);
            DoShaderLightingSpread(shadingTexture2, shadingTexture);
            DoShaderLightingSpread(shadingTexture, shadingTexture2);
            DoShaderLightingSpread(shadingTexture2, shadingTexture);
            DoShaderLightingSpread(shadingTexture, shadingTexture2);
            DoShaderLightingSpread(shadingTexture2, shadingTexture);

            DoShaderLightingSpread(shadingTexture, shadingTexture2);
            DoShaderLightingSpread(shadingTexture2, shadingTexture);
            DoShaderLightingSpread(shadingTexture, shadingTexture2);
            DoShaderLightingSpread(shadingTexture2, shadingTexture);
        }

        static void DoShaderLightingSpread(RenderTarget2D previousLightMap, RenderTarget2D newLightMap)
        {
            LightSpreadEffect.GraphicsDevice.Textures[1] = previousLightMap;
            LightSpreadEffect.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

            LightSpreadEffect.GraphicsDevice.Textures[2] = occulsionMapTexture;
            LightSpreadEffect.GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;

            var horizontalPixel = 1.0f / width;
            var verticalPixel = 1.0f / height;
            LightSpreadEffect.Parameters["horizontalPixel"].SetValue(horizontalPixel);
            LightSpreadEffect.Parameters["verticalPixel"].SetValue(verticalPixel);
            LightSpreadEffect.Parameters["offsetFromPrevious"].SetValue(new Vector2(offsetFromPreviousX * horizontalPixel, offsetFromPreviousY * verticalPixel));

            offsetFromPreviousX = 0;
            offsetFromPreviousY = 0;

            spriteBatch.GraphicsDevice.SetRenderTarget(newLightMap);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, LightSpreadEffect);
            spriteBatch.Draw(lightSourcesTexture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();

            spriteBatch.GraphicsDevice.SetRenderTarget(null);
        }

        //static void DoLightingSpread(uint[] previousLightMap, uint[] newLightMap, int offsetFromPreviousX, int offsetFromPreviousY)
        //{
        //    Parallel.For(0, height, (y) =>
        //    {
        //        for (var x = 0; x < width; x++)
        //        {
        //            var position = (y * width) + x;
        //            newLightMap[position] = new Color(BrightestNeighbor(Get(lightSources, x, y), previousLightMap, x + offsetFromPreviousX, y + offsetFromPreviousY) * occulsionMap[position]).PackedValue;
        //        }
        //    });
        //}

        //static Vector3 BrightestNeighbor(uint lightValue, uint[] source, int x, int y)
        //{
        //    // BUG: This needs to handle wrapping of edges! Off screen should always be 0.
        //    var result = lightValue.ToVector3();

        //    result = Max(Get(source, x + 1, y).ToVector3(), result);
        //    result = Max(Get(source, x - 1, y).ToVector3(), result);
        //    result = Max(Get(source, x, y + 1).ToVector3(), result);
        //    result = Max(Get(source, x, y - 1).ToVector3(), result);
        //    result = Max(Get(source, x + 1, y + 1).ToVector3() * diagonalBrightestFactor, result);
        //    result = Max(Get(source, x - 1, y + 1).ToVector3() * diagonalBrightestFactor, result);
        //    result = Max(Get(source, x + 1, y - 1).ToVector3() * diagonalBrightestFactor, result);
        //    result = Max(Get(source, x - 1, y - 1).ToVector3() * diagonalBrightestFactor, result);

        //    return result;
        //}

        //static uint Get(uint[] source, int x, int y)
        //{
        //    if (x < 0 || y < 0 || x >= width || y >= height)
        //        return Color.Black.PackedValue;
        //    return source[(y * width) + x];
        //}

        //static Vector3 Max(Vector3 value1, Vector3 value2)
        //{
        //    return new Vector3(Math.Max(value1.X, value2.X), Math.Max(value1.Y, value2.Y), Math.Max(value1.Z, value2.Z));
        //}

        //static Vector3 ToVector3(this uint color)
        //{
        //    // ARGB?
        //    return new Vector3(((byte)color) / 255f, ((byte)(color >> 8)) / 255f, ((byte)(color >> 16)) / 255f);
        //}

        public static void Draw(GraphicsDevice device, SpriteBatch spriteBatch, Rectangle viewPort)
        {
            if (lightSourcesTexture != null)
            {
                if (specialBlend == null)
                {
                    specialBlend = new BlendState();
                    specialBlend.ColorSourceBlend = Blend.DestinationColor;
                    specialBlend.AlphaSourceBlend = Blend.DestinationAlpha;
                    specialBlend.ColorDestinationBlend = Blend.Zero;
                    specialBlend.AlphaDestinationBlend = Blend.Zero;
                }

                //if (Main.CurrentKeyboard.IsShift())
                //    spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null);
                //else
                    spriteBatch.Begin(SpriteSortMode.Deferred, specialBlend, SamplerState.LinearWrap, null, null);
                    spriteBatch.Draw(shadingTexture, new Rectangle((-Utility.LightingMargin * blockSize) - (Main.ScreenOffset.X % blockSize), (-Utility.LightingMargin * blockSize) - (Main.ScreenOffset.Y % blockSize), Main.ResolutionWidth + (Utility.LightingMargin * blockSize * 2), Main.ResolutionHeight + (Utility.LightingMargin * blockSize * 2)), Color.White);
                spriteBatch.End();
            }
        }

        static void UpdateOcculsionMapAndMaterialLighting()
        {
            var skyColor = GetSkyColor();
            var mapSeaLevel = Main.Map.SeaLevelInPixels;

            Parallel.For(0, height, (y) =>
            {
                for (var x = 0; x < width; x++)
                {
                    var xOffset = (bufferOffsetX + x) * blockSize;
                    var yOffset = (bufferOffsetY + y) * blockSize;

                    xOffset -= Utility.LightingMargin * blockSize;
                    yOffset -= Utility.LightingMargin * blockSize;

                    var offset = (y * width) + x;

                    var material = Main.Map.GetMaterialAtPixel(xOffset, yOffset);

                    if (MaterialInfo.IsGas(material))
                        occulsionMap[offset] = 1;
                    else
                    {                        
                        if (MaterialInfo.IsLooseOrSolid(material)) {
                            occulsionMap[offset] = 0.8f; // solid material light co-efficient
                        } else {
                            occulsionMap[offset] = 0.99f; // liquid material light co-efficient
                        }
                    }

                    // ambient lighting
                    var ambientLight = Color.Black;
                    var materialInfo = MaterialInfo.MaterialTypes[(byte)material];
                    if (materialInfo.Light.A > 0)
                        ambientLight = materialInfo.Light;

                    if (yOffset < mapSeaLevel || Main.Map.AmbientLight > 0) {
                        var wallMaterial = Main.Map.GetWallMaterialAtPixel(xOffset, yOffset);
                        if (!MaterialInfo.IsLooseOrSolid(material) && !MaterialInfo.IsLooseOrSolid(wallMaterial)) {                                                        
                            if (yOffset < mapSeaLevel)
                                ambientLight = new Color(Math.Max(ambientLight.R, skyColor.R), Math.Max(ambientLight.G, skyColor.G), Math.Max(ambientLight.B, skyColor.B));
                            
                            if (Main.Map.AmbientLight > 0)
                                ambientLight = new Color(Math.Max(ambientLight.R, Main.Map.AmbientLight), Math.Max(ambientLight.G, Main.Map.AmbientLight), Math.Max(ambientLight.B, Main.Map.AmbientLight));
                        }
                    }

                    lightSources[offset] = ambientLight.PackedValue;
                }
            });
        }

        internal static void Reset()
        {
            if (shadingTexture != null)
            {
                shadingTexture.Dispose();
                shadingTexture = null;
            }

            if (shadingTexture2 != null)
            {
                shadingTexture2.Dispose();
                shadingTexture2 = null;
            }

            if (occulsionMapTexture != null)
            {
                occulsionMapTexture.Dispose();
                occulsionMapTexture = null;
            }

            if (lightSourcesTexture != null)
            {
                lightSourcesTexture.Dispose();
                lightSourcesTexture = null;
            }
        }
    }
}