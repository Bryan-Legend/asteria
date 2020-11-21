                            // render underlay gradients
                            if (right < material)
                            {
                                var altMatInfo = MaterialInfo.MaterialTypes[(byte)right];
                                if (altMatInfo.Texture != null)
                                {
                                    RenderTextureTile(spriteBatch, pixelX, pixelY, altMatInfo.Texture, 3, 0 + frame); // Right side overlay gradient
                                }
                            }
                            if (left < material)
                            {
                                var altMatInfo = MaterialInfo.MaterialTypes[(byte)left];
                                if (altMatInfo.Texture != null)
                                {
                                    RenderTextureTile(spriteBatch, pixelX, pixelY, altMatInfo.Texture, 4, 0 + frame); // left side overlay gradient
                                }
                            }
                            if (top < material)
                            {
                                var altMatInfo = MaterialInfo.MaterialTypes[(byte)top];
                                if (altMatInfo.Texture != null)
                                {
                                    RenderTextureTile(spriteBatch, pixelX, pixelY, altMatInfo.Texture, 6 + frame, 9); // top side overlay gradient
                                }
                            }
                            if (bottom < material)
                            {
                                var altMatInfo = MaterialInfo.MaterialTypes[(byte)bottom];
                                if (altMatInfo.Texture != null)
                                {
                                    RenderTextureTile(spriteBatch, pixelX, pixelY, altMatInfo.Texture, 6 + frame, 8); // bottom side overlay gradient
                                }
                            }

                            if (topSame) // bottom edges and center
                            {
                                if (leftSame)
                                {
                                    if (rightSame)
                                    {
                                        if (bottomSame)
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 7 + frame, 1); // center
                                        else
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 7 + frame, 2); // bottom surface
                                    }
                                    else
                                    {
                                        if (bottomSame)
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 1, 0 + frame); // right wall
                                        else
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 1 + frame * 2, 7); // BR Corner
                                    }
                                }
                                else
                                {
                                    if (rightSame)
                                    {
                                        if (bottomSame)
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 0, 0 + frame); // left wall
                                        else
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 0 + frame * 2, 7); // BL Corner
                                    }
                                    else
                                    {
                                        if (bottomSame)
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 2, 0 + frame); // vertical wall
                                        else
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 7 + frame, 4); // bottom protruding stub
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
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 7 + frame, 0); // top surface
                                        else
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 1 + frame * 2, 6); // TR Corner
                                    }
                                    else
                                    {
                                        if (rightSame)
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 0 + frame * 2, 6); // TL Corner
                                        else
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 7 + frame, 3); // top stub
                                    }
                                }
                                else
                                {
                                    if (leftSame)
                                    {
                                        if (rightSame)
                                        {
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 7 + frame, 5); // floor
                                        }
                                        else
                                        {
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 5, 0 + frame); // right protruding stub
                                        }
                                    }
                                    else
                                    {
                                        if (rightSame)
                                        {
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 6, 0 + frame); // left protruding stub
                                        }
                                        else
                                        {
                                            RenderTextureTile(spriteBatch, pixelX, pixelY, matInfo.Texture, 0 + frame, 8); // lone block
                                        }
                                    }
                                }
                            }
