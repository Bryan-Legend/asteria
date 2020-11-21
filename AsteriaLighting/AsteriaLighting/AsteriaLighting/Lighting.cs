using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace AsteriaLighting
{
    public static class Lighting
    {
        private struct CellInfo
        {
            public bool isBorder;
            public int depth;
        }

        const float diagonalBrightestFactor = 0.935f;
        const int margin = 10;
        const int blockSize = 8;

        //public static Texture2D Texture { get; set; }
        static RenderTarget2D shadowTarget;
        static RenderTarget2D lightmapTarget;
        static RenderTarget2D backBuffer;
        public static Effect lightmapEffect;
        static uint[] textureBuffer;
        static int width;
        static int height;
        static Material[,] materialBuffer;
        static float[,] shadingBuffer;
        static float[,] shadingBuffer2;
        static int bufferOffsetX;
        static int bufferOffsetY;
        static Texture2D pixel;
        static CellInfo[,] cells;
        static Dictionary<string, TimeSpan> timers;
        static DynamicVertexBuffer shadowGeometryBuffer;
        static VertexPositionColor[] shadowGeometry;
        static int index;
        static BasicEffect shadowEffect;
        static Matrix orthoMatrix;
        public static Effect blurEffect;
        public static Texture2D spotLight;

        static Lighting()
        {
            cells = new CellInfo[1024 / blockSize, 768 / blockSize];
            timers = new Dictionary<string, TimeSpan>();
            orthoMatrix = Matrix.CreateOrthographicOffCenter(0, 1024, 768, 0, -1, 1);
        }

        public static Color GetSkyColor()
        {
//            return Main.DayNightGradientData[(int)Main.Map.GameTime.TimeOfDay.TotalMinutes];

            //return Color.FromNonPremultiplied(0, 0, 0, 128);
            return Color.CornflowerBlue;
        }

        public static void Update(GraphicsDevice device)
        {
            var currentOffsetX = (Main.ScreenOffset.X / blockSize);
            var currentOffsetY = (Main.ScreenOffset.Y / blockSize);

            if (pixel == null)
            {
                pixel = new Texture2D(device, blockSize, blockSize);
                Color[] data = new Color[blockSize * blockSize];
                for (int i = 0; i < blockSize * blockSize; i++)
                {
                    data[i] = Color.White;
                }
                pixel.SetData(data);
            }

            if (shadowTarget == null)
            {
                shadowTarget = new RenderTarget2D(device, 512, 384);
            }

            if (lightmapTarget == null)
            {
                lightmapTarget = new RenderTarget2D(device, 512, 384);
            }

            if (backBuffer == null)
            {
                backBuffer = new RenderTarget2D(device, 1024, 768);
            }

            //Stopwatch watchTextureBuffer = new Stopwatch();
            //watchTextureBuffer.Start();
            //if (Texture == null)
            //{
            //    width = (Main.ResolutionWidth / blockSize) + margin * 2;
            //    height = (Main.ResolutionHeight / blockSize) + margin * 2;
            //    Texture = new Texture2D(device, width, height, false, SurfaceFormat.Color);
            //    textureBuffer = new uint[width * height];
            //    materialBuffer = new Material[width, height];
            //    shadingBuffer = new float[width, height];
            //    shadingBuffer2 = new float[width, height];
            //}
            //else
            //{
            //    // move the buffer to match player movement
            //    if (currentOffsetX != bufferOffsetX || currentOffsetY != bufferOffsetY)
            //    {
            //        for (var y = 0; y < height; y++)
            //        {
            //            for (var x = 0; x < width; x++)
            //            {
            //                var sourceX = x + (currentOffsetX - bufferOffsetX);
            //                var sourceY = y + (currentOffsetY - bufferOffsetY);
            //                if (sourceX < 0 || sourceX >= width || sourceY < 0 || sourceY >= height)
            //                    shadingBuffer2[x, y] = 0;
            //                else
            //                    shadingBuffer2[x, y] = shadingBuffer[sourceX, sourceY];
            //            }
            //        }

            //        var tempBuffer = shadingBuffer;
            //        shadingBuffer = shadingBuffer2;
            //        shadingBuffer2 = tempBuffer;
            //    }
            //}

            //bufferOffsetX = currentOffsetX;
            //bufferOffsetY = currentOffsetY;

            //var skyColor = 1f - (GetSkyColor().A / 255f);

            //for (var y = 0; y < height; y++)
            //{
            //    for (var x = 0; x < width; x++)
            //    {
            //        var xOffset = (currentOffsetX + x) * blockSize;
            //        var yOffset = (currentOffsetY + y) * blockSize;

            //        xOffset -= margin * blockSize;
            //        yOffset -= margin * blockSize;

            //        var material = Main.Map.GetMaterialAtPixel(xOffset, yOffset);
            //        materialBuffer[x, y] = material;

            //        if (MaterialInfo.IsLooseOrSolid(material))
            //            //shadingBuffer[x, y] = Math.Max(0, (shadingBuffer[x, y] - 0.05f) * shadingBuffer[x, y]);
            //            shadingBuffer[x, y] = Math.Max(0, shadingBuffer[x, y] - 0.1f);
            //        else
            //        {
            //            // set day light
            //            if (yOffset < Main.Map.SeaLevel * Map.BlockHeight)
            //            {
            //                shadingBuffer[x, y] = Math.Max(shadingBuffer[x, y], skyColor);
            //            }
            //        }

            //        var materialInfo = MaterialInfo.MaterialTypes[material];
            //        if (materialInfo.Light.A > 0)
            //        {
            //            shadingBuffer[x, y] = Math.Max(shadingBuffer[x, y], 1f - (materialInfo.Light.A / 255f));
            //        }
            //    }
            //}

            //// entity lighting
            //foreach (var entity in Main.Map.Entities)
            //{
            //    //if (entity.Light.A > 0 && Main.Player.IsOnScreen(entity))
            //    if (entity.Light.A > 0)
            //    {
            //        var x = ((int)entity.Position.X - (currentOffsetX * blockSize) + 100 - (blockSize)) / blockSize;
            //        var y = ((int)entity.Position.Y - (currentOffsetY * blockSize) + 100 - (blockSize)) / blockSize;
            //        if (x >= 0 && x < width && y >= 0 && y < height)
            //        {
            //            //shadingBuffer[x, y] = 1f - (placeable.Type.Light.A / 255f);
            //            shadingBuffer[x, y] = 1f;
            //        }
            //    }
            //}

            //for (var y = 5; y < height - 5; y++)
            //{
            //    for (var x = 5; x < width - 5; x++)
            //    {
            //        var value = shadingBuffer[x, y];
            //        var material = materialBuffer[x, y];
            //        if (MaterialInfo.IsLooseOrSolid(material))
            //            shadingBuffer2[x, y] = BrightestNeighbor(x, y);
            //        //shadingBuffer2[x, y] = Math.Max(0, BrightestNeighbor(x, y) - 0.05f);
            //        else
            //        {
            //            if (MaterialInfo.IsGas(material))
            //                shadingBuffer2[x, y] = Math.Max(0, BrightestNeighbor(x, y) - 0.01f);
            //            else
            //                // liquid
            //                shadingBuffer2[x, y] = Math.Max(0, BrightestNeighbor(x, y) - 0.02f);
            //        }
            //        //shadingBuffer2[x, y] = BrightestNeighbor(x, y);

            //        textureBuffer[(y * width) + x] = Color.FromNonPremultiplied(0, 0, 0, 255 - (byte)(value * 255)).PackedValue;
            //    }
            //}
            //watchTextureBuffer.Stop();
            //timers["ShadingBuffer"] = watchTextureBuffer.Elapsed;

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

            device.Textures[0] = null;

            //if (Texture != null && !Texture.IsDisposed)
            //    Texture.SetData<uint>(textureBuffer);
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

        static BlendState specialBlend;

        public static void Draw(SpriteBatch spriteBatch, Rectangle viewPort)
        {
            if (specialBlend == null)
            {
                specialBlend = new BlendState();
                specialBlend.ColorSourceBlend = Blend.DestinationColor;
                specialBlend.AlphaSourceBlend = Blend.DestinationAlpha;
                specialBlend.ColorDestinationBlend = Blend.Zero;
                specialBlend.AlphaDestinationBlend = Blend.Zero;
            }

            Entity light = Main.Map.Entities[0];

            int w = 1024 / blockSize;
            int h = 768 / blockSize;
            
            shadowEffect.GraphicsDevice.SetRenderTarget(shadowTarget);
            shadowEffect.GraphicsDevice.Clear(Color.Transparent);

            shadowEffect.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            shadowEffect.GraphicsDevice.BlendState = BlendState.Opaque;
            shadowEffect.CurrentTechnique.Passes[0].Apply();
            shadowEffect.GraphicsDevice.SetVertexBuffer(shadowGeometryBuffer);
            shadowEffect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, index / 3);
            shadowEffect.GraphicsDevice.SetVertexBuffer(null);

            shadowEffect.GraphicsDevice.SetRenderTarget(null);

            spriteBatch.GraphicsDevice.SetRenderTarget(lightmapTarget);
            spriteBatch.GraphicsDevice.Clear(Color.Black);
            int texsize = spotLight.Width;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Draw(spotLight, new Rectangle(((int)light.Position.X - texsize) / 2, ((int)light.Position.Y - texsize) / 2, texsize, texsize), Color.White);
            spriteBatch.End();

            spriteBatch.GraphicsDevice.SetRenderTarget(backBuffer);

            blurEffect.GraphicsDevice.Clear(Color.CornflowerBlue);

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            //spriteBatch.Draw(lightmapTarget, new Rectangle(0, 0, 1024, 768), Color.White);
            //spriteBatch.End();

            blurEffect.Parameters["hPixel"].SetValue(1.0f / shadowTarget.Width);
            blurEffect.Parameters["vPixel"].SetValue(1.0f / shadowTarget.Height);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, blurEffect);
            spriteBatch.Draw(shadowTarget, new Rectangle(0, 0, 1024, 768), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (MaterialInfo.IsLooseOrSolid(Main.Map.cells[i, j]))
                    {
                        spriteBatch.Draw(pixel, new Vector2(i * blockSize, j * blockSize), (cells[i, j].isBorder ? Color.Green : Color.White) * Main.Map.cells[i, j].LightBlock);
                    }
                }
            }
            spriteBatch.Draw(pixel, new Vector2(Main.Map.Entities[0].Position.X - 4, Main.Map.Entities[0].Position.Y - 4), Color.Yellow);
            spriteBatch.End();

            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.GraphicsDevice.Clear(Color.Black);
            lightmapEffect.Parameters["Texture2"].SetValue(lightmapTarget);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, lightmapEffect);
            spriteBatch.Draw(backBuffer, new Rectangle(0, 0, 1024, 768), Color.White);
            spriteBatch.End();

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            //spriteBatch.Draw(spotLight, new Rectangle(((int)light.Position.X - texsize), ((int)light.Position.Y - texsize), texsize * 2, texsize * 2), Color.White);
            //spriteBatch.End();


            //if (Texture != null)
            //{
            //    spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null);
            //    spriteBatch.Draw(Texture, new Rectangle((-margin * blockSize) - (Main.ScreenOffset.X % blockSize), (-margin * blockSize) - (Main.ScreenOffset.Y % blockSize), Main.ResolutionWidth + (margin * blockSize * 2), Main.ResolutionHeight + (margin * blockSize * 2)), Color.White);
            //    spriteBatch.End();
            //}

            Debug.WriteLine("");
            foreach (KeyValuePair<string, TimeSpan> timer in timers)
            {
                Debug.WriteLine(timer.Key + ": " + timer.Value.ToString());
            }
        }

        public static void Update2(GraphicsDevice device)
        {
            if (shadowGeometryBuffer == null)
            {
                shadowGeometryBuffer = new DynamicVertexBuffer(device, typeof(VertexPositionColor), 128 * 96 * 4, BufferUsage.WriteOnly);
            }

            if (shadowGeometry == null)
            {
                shadowGeometry = new VertexPositionColor[128 * 96 * 4];
                for (int i = 0; i < shadowGeometry.Length; i++)
                {
                    shadowGeometry[i].Color = Color.Black;
                }
            }

            if (shadowEffect == null)
            {
                shadowEffect = new BasicEffect(device);
                shadowEffect.World = orthoMatrix;
                shadowEffect.VertexColorEnabled = true;
                shadowEffect.TextureEnabled = false;
            }

            int w = 1024 / blockSize;
            int h = 768 / blockSize;

            Entity light = Main.Map.Entities[0];

            index = 0;

            // TODO: flood fill from light's starting point.    
            Stopwatch watchUpdateBorders = new Stopwatch();
            watchUpdateBorders.Start();

            Vector3 pos1, pos2, dir1, dir2;
            pos1 = pos2 = dir1 = dir2 = Vector3.Zero;
            for (int i = 1; i < w - 1; i++)
            {
                for (int j = 1; j < h - 1; j++)
                {
                    if (MaterialInfo.IsLooseOrSolid(Main.Map.cells[i, j]))
                    {
                        int cellDepth = 0;
                        if (MaterialInfo.IsGas(Main.Map.cells[i - 1, j - 1])) cellDepth++;
                        if (MaterialInfo.IsGas(Main.Map.cells[i    , j - 1])) cellDepth++;
                        if (MaterialInfo.IsGas(Main.Map.cells[i + 1, j - 1])) cellDepth++;
                        if (MaterialInfo.IsGas(Main.Map.cells[i - 1, j    ])) cellDepth++;
                        if (MaterialInfo.IsGas(Main.Map.cells[i + 1, j    ])) cellDepth++;
                        if (MaterialInfo.IsGas(Main.Map.cells[i - 1, j + 1])) cellDepth++;
                        if (MaterialInfo.IsGas(Main.Map.cells[i    , j + 1])) cellDepth++;
                        if (MaterialInfo.IsGas(Main.Map.cells[i + 1, j + 1])) cellDepth++;

                        if (cellDepth > 0)
                        {
                            cells[i, j].isBorder = true;

                            if (light.Position.Y > j * blockSize)
                            {
                                if (light.Position.X > i * blockSize)
                                {
                                    pos1.X = i * blockSize;
                                    pos1.Y = (j + 1) * blockSize;
                                    pos2.X = (i + 1) * blockSize;
                                    pos2.Y = j * blockSize;
                                }
                                else
                                {
                                    pos1.X = i * blockSize;
                                    pos1.Y = j * blockSize;
                                    pos2.X = (i + 1) * blockSize;
                                    pos2.Y = (j + 1) * blockSize;
                                }
                            }
                            else
                            {
                                if (light.Position.X > i * blockSize)
                                {
                                    pos1.X = i * blockSize;
                                    pos1.Y = j * blockSize;
                                    pos2.X = (i + 1) * blockSize;
                                    pos2.Y = (j + 1) * blockSize;
                                }
                                else
                                {
                                    pos1.X = i * blockSize;
                                    pos1.Y = (j + 1) * blockSize;
                                    pos2.X = (i + 1) * blockSize;
                                    pos2.Y = j * blockSize;
                                }
                            }

                            dir1 = Vector3.Normalize(pos1 - new Vector3(light.Position, 0)) * 1024;
                            dir2 = Vector3.Normalize(pos2 - new Vector3(light.Position, 0)) * 1024;

                            //Color c = Color.Black;

                            shadowGeometry[index].Position = pos1;
                            //shadowGeometry[index].Color = c;
                            index++;
                            shadowGeometry[index].Position = pos1 + dir1;
                            //shadowGeometry[index].Color = c;
                            index++;
                            shadowGeometry[index].Position = pos2 + dir2;
                            //shadowGeometry[index].Color = c;
                            index++;

                            shadowGeometry[index].Position = pos2 + dir2;
                            //shadowGeometry[index].Color = c;
                            index++;
                            shadowGeometry[index].Position = pos2;
                            //shadowGeometry[index].Color = c;
                            index++;
                            shadowGeometry[index].Position = pos1;
                            //shadowGeometry[index].Color = c;
                            index++;

                            continue;
                        }

                        cells[i, j].depth = cellDepth;
                    }

                    cells[i, j].isBorder = false;
                }
            }
            shadowGeometryBuffer.SetData(shadowGeometry, 0, index);

            watchUpdateBorders.Stop();
            timers["UpdateBorders"] = watchUpdateBorders.Elapsed;

            //Stopwatch watchFloodFill = new Stopwatch();
            //watchFloodFill.Start();

            //Queue<Point> pointQueue = new Queue<Point>();
            //pointQueue.Enqueue(new Point((int)(light.Position.X / blockSize), (int)(light.Position.Y / blockSize)));

            //int y1;
            //bool spanLeft, spanRight;
            //PointComparer comp = new PointComparer();

            //while (pointQueue.Count > 0)
            //{
            //    Point cell = pointQueue.Dequeue();

            //    y1 = cell.Y;
            //    while (y1 >= 0 /*&& screenBuffer[x][y1] == oldColor*/)
            //    {
            //        y1--;
            //    }
            //    y1++;
            //    spanLeft = spanRight = false;
            //    while (y1 < (768 / blockSize) /*&& screenBuffer[x][y1] == oldColor*/)
            //    {
            //    //    screenBuffer[x][y1] = newColor;
            //        if (!spanLeft && cell.X > 0 /*&& screenBuffer[x - 1][y1] == oldColor*/)
            //        {
            //            Point newPoint = new Point(cell.X - 1, y1);
            //            if (!pointQueue.Contains(newPoint, comp))
            //            {
            //                pointQueue.Enqueue(newPoint);
            //            }
            //            spanLeft = true;
            //        }
            //        else if (spanLeft && cell.X > 0 /*&& screenBuffer[x - 1][y1] != oldColor*/)
            //        {
            //            spanLeft = false;
            //        }
            //        if (!spanRight && cell.X < (1024 / blockSize) - 1 /*&& screenBuffer[x + 1][y1] == oldColor*/)
            //        {
            //            Point newPoint = new Point(cell.X + 1, y1);
            //            if (!pointQueue.Contains(newPoint, comp))
            //            {
            //                pointQueue.Enqueue(newPoint);
            //            }
            //            spanRight = true;
            //        }
            //        else if (spanRight && cell.X < (1024 / blockSize) - 1 /*&& screenBuffer[x + 1][y1] != oldColor*/)
            //        {
            //            spanRight = false;
            //        }
            //        y1++;
            //    }
            //}

            //timers["FloodFill"] = watchFloodFill.Elapsed;


            //Stopwatch watchReductionMap = new Stopwatch();
            //watchReductionMap.Start();
            //var reductionMap = CreateReductionMap();
            //watchReductionMap.Stop();
            //timers["ReductionMap"] = watchReductionMap.Elapsed;

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Main.Map.Entities[0].Position.X += 1;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Main.Map.Entities[0].Position.X -= 1;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Main.Map.Entities[0].Position.Y += 1;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Main.Map.Entities[0].Position.Y -= 1;
            }
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

        private static void FloodFill(ref List<Point> pending, ref List<Point> processed)
        {
            PointComparer comp = new PointComparer();

            if (pending.Count > 0)
            {
                Point cell = pending[0];

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        Point newPoint = new Point(cell.X + i, cell.Y + j);

                        if (!processed.Contains(newPoint, comp) && !pending.Contains(newPoint, comp) && newPoint.X >= 0 && newPoint.X < (1024 / blockSize) && newPoint.Y >= 0 && newPoint.Y < (768 / blockSize) && newPoint != cell)
                        {
                            pending.Add(newPoint);
                        }
                    }
                }

                processed.Add(cell);
                pending.Remove(cell);
            }
        }
    }

    public class PointComparer : IEqualityComparer<Point>
    {
        public bool Equals(Point x, Point y)
        {
            if (x.X == y.X && x.Y == y.Y)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(Point obj)
        {
            return obj.GetHashCode();
        }
    }
}