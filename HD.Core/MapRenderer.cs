using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;

namespace HD
{
    public static class MapRenderer
    {
        static byte[,] frames;

        static MapRenderer()
        {
            frames = new byte[256, 256];
            for (var y = 0; y < 256; y++)
            {
                for (var x = 0; x < 256; x++)
                {
                    frames[x, y] = (byte)Utility.Next(3);
                }
            }
        }

        public static void Draw(Map map, SpriteBatch wallBatch, SpriteBatch foreBatch, int x, int y, int viewWidth, int viewHeight, Vector2 offset = default(Vector2))
        {
            var startX = x / Map.BlockWidth;
            var startY = y / Map.BlockHeight;

            if (offset != default(Vector2))
            {
                x -= (int)offset.X;
                y -= (int)offset.Y;
            }

            var animationFrame = (map.Now.Millisecond + map.Now.Second * 1000) / 250;

            for (var matY = startY; matY <= startY + (viewHeight / Map.BlockHeight); matY++)
            {
                for (var matX = startX; matX <= startX + (viewWidth / Map.BlockWidth); matX++)
                {
                    if (foreBatch != null)
                    {
                        var renderWallTile = RenderTile(map, foreBatch, x, y, Color.White, animationFrame, matY, matX, false);
                        if (renderWallTile && wallBatch != null)
                        {
                            RenderTile(map, wallBatch, x, y, Color.FromNonPremultiplied(160, 160, 160, 255), animationFrame, matY, matX, true);
                        }
                    }
                    else
                    {
                        if (wallBatch != null)
                            RenderTile(map, wallBatch, x, y, Color.FromNonPremultiplied(160, 160, 160, 255), animationFrame, matY, matX, true);
                    }
                }
            }
        }

        static bool RenderTile(Map map, SpriteBatch wallBatch, int x, int y, Color color, int animationFrame, int matY, int matX, bool isWall)
        {
            var material = isWall ? map.GetWallMaterial(matX, matY) : map.GetMaterial(matX, matY);

            if (material == Material.Air)
                return true;

            var matInfo = MaterialInfo.MaterialTypes[(byte)material];

            if (matInfo.IsTextureLoaded)
            {
                var pixelX = (matX * Map.BlockWidth) - x;
                var pixelY = (matY * Map.BlockHeight) - y;
                var frame = frames[matX % 256, matY % 256];

                if (!MaterialInfo.IsLooseOrSolid(material))
                    frame = (byte)((frame + animationFrame) % 3);

                var left = isWall ? map.GetWallMaterial(matX - 1, matY) : map.GetMaterial(matX - 1, matY);
                var right = isWall ? map.GetWallMaterial(matX + 1, matY) : map.GetMaterial(matX + 1, matY);
                var top = isWall ? map.GetWallMaterial(matX, matY - 1) : map.GetMaterial(matX, matY - 1);
                var bottom = isWall ? map.GetWallMaterial(matX, matY + 1) : map.GetMaterial(matX, matY + 1);

                var leftSame = left == material;
                var rightSame = right == material;
                var topSame = top == material;
                var bottomSame = bottom == material;

                if (left > material)
                    leftSame = true;
                if (right > material)
                    rightSame = true;
                if (top > material)
                    topSame = true;
                if (bottom > material)
                    bottomSame = true;

                if (material == Material.Grass)
                {
                    if (left == Material.Dirt)
                        leftSame = true;
                    if (right == Material.Dirt)
                        rightSame = true;
                    if (top == Material.Dirt)
                        topSame = true;
                    if (bottom == Material.Dirt)
                        bottomSame = true;
                }
                
                // render underlay gradients
                if (!matInfo.IsTransparent)
                {
                    if (right < material)
                    {
                        var altMatInfo = MaterialInfo.MaterialTypes[(byte)right];
                        if (altMatInfo.IsTextureLoaded)
                        {
                            RenderTextureTile(wallBatch, pixelX, pixelY, altMatInfo.TextureOffset, color, 10, 0 + frame); // Right side overlay gradient
                        }
                    }
                    if (left < material)
                    {
                        var altMatInfo = MaterialInfo.MaterialTypes[(byte)left];
                        if (altMatInfo.IsTextureLoaded)
                        {
                            RenderTextureTile(wallBatch, pixelX, pixelY, altMatInfo.TextureOffset, color, 11, 0 + frame); // left side overlay gradient
                        }
                    }
                    if (top < material)
                    {
                        var altMatInfo = MaterialInfo.MaterialTypes[(byte)top];
                        if (altMatInfo.IsTextureLoaded)
                        {
                            RenderTextureTile(wallBatch, pixelX, pixelY, altMatInfo.TextureOffset, color, 6 + frame, 2); // top side overlay gradient
                        }
                    }
                    if (bottom < material)
                    {
                        var altMatInfo = MaterialInfo.MaterialTypes[(byte)bottom];
                        if (altMatInfo.IsTextureLoaded)
                        {
                            RenderTextureTile(wallBatch, pixelX, pixelY, altMatInfo.TextureOffset, color, 6 + frame, 1); // bottom side overlay gradient
                        }
                    }
                }

                if (topSame) // bottom edges and center
                {
                    if (leftSame)
                    {
                        if (rightSame)
                        {
                            if (bottomSame)
                            {
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 1 + frame, 1); // center
                                return !MaterialInfo.IsLooseOrSolid(material) || material == Material.Glass || material == Material.Spike;
                            }
                            else
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 1 + frame, 2); // bottom surface
                        }
                        else
                        {
                            if (bottomSame)
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 4, 0 + frame); // right wall
                            else
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 1 + frame * 2, 4); // BR Corner
                        }
                    }
                    else
                    {
                        if (rightSame)
                        {
                            if (bottomSame)
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 0, 0 + frame); // left wall
                            else
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 0 + frame * 2, 4); // BL Corner
                        }
                        else
                        {
                            if (bottomSame)
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 5, 0 + frame); // vertical wall
                            else
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 6 + frame, 3); // bottom protruding stub
                        }
                    }
                }
                else
                {
                    if (bottomSame) // top edges
                    {
                        if (leftSame)
                        {
                            if (rightSame)
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 1 + frame, 0); // top surface
                            else
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 1 + frame * 2, 3); // TL Corner
                        }
                        else
                        {
                            if (rightSame)
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 0 + frame * 2, 3); // TR Corner
                            else
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 6 + frame, 0); // top stub
                        }
                    }
                    else
                    {
                        if (leftSame)
                        {
                            if (rightSame)
                            {
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 6 + frame, 4); // floor
                            }
                            else
                            {
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 12, 0 + frame); // left protruding stub
                            }
                        }
                        else
                        {
                            if (rightSame)
                            {
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 9, 0 + frame); // right protruding stub
                            }
                            else
                            {
                                RenderTextureTile(wallBatch, pixelX, pixelY, matInfo.TextureOffset, color, 9 + frame, 3); // lone block
                            }
                        }
                    }
                }
            }
            else
            {
                wallBatch.DrawRectangle(new Rectangle((matX * Map.BlockWidth) - x, (matY * Map.BlockHeight) - y, Map.BlockWidth, Map.BlockHeight), matInfo.DrawColor);
                return false;
            }

            return true;
        }

        static void RenderTextureTile(SpriteBatch spriteBatch, int x, int y, Point textureOffset, Color color, int tileX, int tileY)
        {
            spriteBatch.Draw
            (
                MaterialInfo.MaterialsTexture,
                new Rectangle(x, y, Map.BlockWidth, Map.BlockHeight),
                new Rectangle(textureOffset.X + (tileX * 9), textureOffset.Y + (tileY * 9), 8, 8),
                color
            );
        }
    }
}