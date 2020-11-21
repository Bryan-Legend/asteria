using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FuchsGUI;
using Microsoft.Xna.Framework.Input;

namespace HD
{
    public class ListMenuItem<T> : MenuItem
    {
        public int ShowCount = 8;

        List<string> itemLabels = new List<string>();
        List<T> itemValues = new List<T>();
        int offset;
        int hoverIndex = -1;

        public override int Height { get { return (Utility.Font.LineSpacing + 3) * (ShowCount + 2); } }

        public void AddItem(string label, T value)
        {
            itemLabels.Add(label);
            itemValues.Add(value);
        }

        internal override void Draw(SpriteBatch spriteBatch, int yPosition)
        {
            var anyIsHover = false;
            for (int i = offset; i < offset + ShowCount; i++) {
                if (i >= itemValues.Count)
                    break;

                var text = itemLabels[i];
                var textSize = Utility.Font.MeasureString(text);

                var topLeft = new Vector2((int)((Main.ResolutionWidth - textSize.X) / 2), yPosition);
                var bottomRight = topLeft + textSize;

                var isHover = false;
                if (Main.CurrentMouse.X > topLeft.X && Main.CurrentMouse.Y > topLeft.Y && Main.CurrentMouse.X <= bottomRight.X && Main.CurrentMouse.Y <= bottomRight.Y) {
                    anyIsHover = true;
                    isHover = true;
                    Main.IsMouseOverControl = true;
                    if (i != hoverIndex) {
                        hoverIndex = i;
                        Tag = itemValues[i];
                        IsHover = false;
                        IsHover = true;
                    }
                }

                spriteBatch.DrawString(Utility.Font, text, topLeft, isHover ? Menu.HoverColor : Color.White);

                yPosition += (int)textSize.Y + 3;
            }
            if (!anyIsHover) {
                hoverIndex = -1;
                IsHover = false;
                Tag = null;
            }

            if (itemValues.Count > ShowCount) {
                Button.Draw(spriteBatch, "Prev", new Vector2((Main.ResolutionWidth - 130) / 2, yPosition), () => { offset = Math.Max(0, offset - ShowCount); }, offset > 0);
                Button.Draw(spriteBatch, "Next", new Vector2((Main.ResolutionWidth + 5) / 2, yPosition), () => { offset += ShowCount; }, offset + ShowCount < itemValues.Count);
            }
        }
    }
}
