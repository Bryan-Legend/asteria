﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HD
{
    public class BrushWalker : IEnumerable
    {
        Brush brush;
        public static BitArray BrushData;
        const int brushImageWidth = 240;

        public BrushWalker(Brush brush)
        {
            this.brush = brush;
        }

        public IEnumerator GetEnumerator()
        {
            if (brush == Brush.Mega) {
                for (var maskY = -32; maskY < 32; maskY++) {
                    for (var maskX = -32; maskX < 32; maskX++) {
                        yield return new Point(maskX, maskY);
                    }
                }
            } else {
                var xOffset = (int)brush % 16;
                var yOffset = (int)brush / 16;

                var offset = (yOffset * brushImageWidth) + (xOffset * 16);

                for (var maskY = 0; maskY < 16; maskY++) {
                    for (var maskX = 0; maskX < 16; maskX++) {
                        if (BrushData[(maskY * brushImageWidth) + maskX + offset])
                            yield return new Point(maskX, maskY);
                    }
                }
            }
        }

        static BrushWalker()
        {
            BrushData = new BitArray(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 252, 63, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 15, 254, 127, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 248, 31, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 1, 252, 63, 255, 255, 255, 255, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 224, 1, 224, 7, 254, 127, 255, 255, 255, 255, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 1, 192, 3, 240, 3, 240, 15, 254, 127, 255, 255, 255, 255, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 128, 1, 192, 3, 192, 3, 224, 7, 248, 7, 240, 15, 254, 127, 255, 255, 255, 255, 0, 0, 0, 1, 0, 0, 0, 1, 128, 1, 128, 3, 192, 3, 192, 3, 224, 7, 224, 7, 248, 7, 248, 31, 254, 127, 255, 255, 255, 255, 248, 31, 0, 1, 0, 0, 0, 0, 128, 1, 0, 1, 192, 3, 192, 3, 224, 7, 224, 7, 248, 7, 248, 31, 254, 127, 255, 255, 255, 255, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 128, 1, 192, 3, 192, 3, 224, 7, 248, 7, 240, 15, 254, 127, 255, 255, 255, 255, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 1, 192, 3, 240, 3, 240, 15, 254, 127, 255, 255, 255, 255, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 224, 1, 224, 7, 254, 127, 255, 255, 255, 255, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 1, 252, 63, 255, 255, 255, 255, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 248, 31, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 240, 15, 254, 127, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 252, 63, 255, 255, 0, 0, 0, 0, 0, 0 });
        }

        public static void GenerateDataFromTexture(Texture2D brushTexture)
        {
            /*
             * This code will load the data from an image file.
             * 
             * That can't be done for the server, so to update the data run the code below on the client and then copy and paste the generated byte array literal to the code above.
             * 
             * Also be sure to update brushImageWidth
             * 
             * */

            var temp = new uint[brushTexture.Width * brushTexture.Height];
            brushTexture.GetData<uint>(temp);

            BrushData = new BitArray(temp.Length);
            for (int i = 0; i < BrushData.Length; i++) {
                if (temp[i] != Color.Transparent.PackedValue)
                    BrushData[i] = true;
            }

            byte[] bytes = new byte[BrushData.Length / 8 + (BrushData.Length % 8 == 0 ? 0 : 1)];
            BrushData.CopyTo(bytes, 0);

            var result = new StringBuilder();
            result.Append("{ ");
            foreach (var b in bytes) {
                result.AppendFormat("{0}, ", b);
            }
            result.Remove(result.Length - 2, 2);
            result.Append(" };");

            var resultAsLiteral = result.ToString();
            Utility.LogMessage(resultAsLiteral);
        }
    }
}