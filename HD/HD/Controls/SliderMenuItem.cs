using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HD
{
    public class SliderMenuItem : MenuItem
    {
        const int width = 300;
        public int SliderPosition;
        public Action<float> OnChange;

        public float Value
        {
            get { return SliderPosition / (float)width; }
            set { SliderPosition = (int)((float)width * value); }
        }

        internal override void Draw(SpriteBatch spriteBatch, int yPosition)
        {
            var left = (Main.ResolutionWidth - width) / 2;
            var shape = new Rectangle(left, yPosition, width, 20);
            IsHover = shape.Contains(new Point(Main.CurrentMouse.X, Main.CurrentMouse.Y));
            if (IsHover) {
                Main.IsMouseOverControl = true;
            }

            spriteBatch.DrawRectangle(new Rectangle(shape.Left, shape.Top + 9, width, 2), IsHover ? Main.HoverColor : Main.DisabledColor);

            Button.Draw(spriteBatch, "  ", new Vector2(left + SliderPosition - 9, yPosition), null);

            spriteBatch.DrawString(Utility.MediumFont, Value.ToString("N2"), new Vector2(shape.Right + 15, shape.Top - 5), Color.White);
        }

        internal override void Update(MouseState mouse, KeyboardState keyboard, KeyboardState lastKeyboard, Menu menu, int menuPosition)
        {
            base.Update(mouse, keyboard, lastKeyboard, menu, menuPosition);

            if (mouse.LeftButton == ButtonState.Pressed && IsHover) {
                var left = (Main.ResolutionWidth - width) / 2;
                SliderPosition = mouse.X - left;
                if (SliderPosition < 0)
                    SliderPosition = 0;
                if (SliderPosition > width)
                    SliderPosition = width;

                if (OnChange != null)
                    OnChange(Value);
            }
        }
    }
}
