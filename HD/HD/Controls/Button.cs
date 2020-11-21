using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HD
{
    public static class Button
    {
        public static Texture2D ButtonTexture;

        public static Vector2 Draw(SpriteBatch spriteBatch, string text, Vector2 position, Action onClick, bool isEnabled = true)
        {
            var topLeft = position;

            var textSize = Utility.SmallFont.MeasureString(text);
            var bottomRight = topLeft + textSize;

            if (ButtonTexture != null)
            {
                bottomRight = topLeft + new Vector2(textSize.X + 10, 21);
            }

            var isHover = false;

            if (onClick != null && Main.CurrentMouse.X > topLeft.X && Main.CurrentMouse.Y > topLeft.Y && Main.CurrentMouse.X <= bottomRight.X && Main.CurrentMouse.Y <= bottomRight.Y)
            {
                isHover = true;
                Main.IsMouseOverControl = true;
            }

            var color = isHover ? Main.HoverColor : Main.DefaultColor;
            if (!isEnabled)
                color = Main.DisabledColor;

            if (ButtonTexture != null)
            {
                spriteBatch.DrawExpandButtonTexture(ButtonTexture, position, (int)textSize.X + 10, color);
            }

            spriteBatch.DrawString(Utility.SmallFont, text, topLeft + new Vector2(5, 3), color);
            position.Y += Utility.SmallFont.LineSpacing + 10;

            if (isEnabled &&
                onClick != null &&
                isHover &&
                (
                    (Main.CurrentMouse.LeftButton == ButtonState.Released && Main.LastMouse.LeftButton == ButtonState.Pressed) ||
                    (Main.CurrentGamePad.IsButtonUp(Buttons.A) && Main.LastGamePad.IsButtonDown(Buttons.A))
                ))
            {
                Audio.PlaySound(Sound.SmallTick);
                onClick();
            }

            if (isHover && Main.Player != null)
                Main.IsLeftMouseButtonHandled = true;

            return position;
        }
    }
}
