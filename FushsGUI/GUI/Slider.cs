using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace FuchsGUI
{
    public class Slider : Control
    {
        public static Texture2D PixelTexture;

        public bool IsHover;

        //public int Width = 300;
        public Color HoverColor = Color.LightCyan;
        public Color DisabledColor = Color.LightGray;

        public int SliderPosition;
        public Action<float> OnChange;

        public bool IsAmountSelector;
        public int MaxAmount = 1;

        public float Value
        {
            get { return SliderPosition / (float)Width; }
            set { SliderPosition = (int)((float)Width * value); }
        }

        public int Amount
        {
            get
            {
                var value = Value;
                if (value > 0.95)
                    return MaxAmount;
                return (int)(MaxAmount * Value) + 1;
            }
        }

        public Slider(String name, string text, Rectangle positionWidthHeight, Texture2D texture, SpriteFont font, Color foreColor, Viewport? viewport = null)
            : base(name, text, positionWidthHeight, texture, font, foreColor, viewport)
        {
        }

        /// <summary>
        /// Updates the gui
        /// </summary>
        /// <param name="mouseState">Current mouse state</param>
        /// <param name="keyboardState">Current keyboard state</param>
        public override void Update(MouseState mouseState, KeyboardState keyboardState)
        {
            if (!enabled || !visible)
                return;

            var shape = PositionWidthHeight;
            IsHover = shape.Contains(new Point(mouseState.X, mouseState.Y));

            if (mouseState.LeftButton == ButtonState.Pressed && IsHover)
            {
                var left = shape.Left;
                SliderPosition = mouseState.X - left;
                if (SliderPosition < 0)
                    SliderPosition = 0;
                if (SliderPosition > Width)
                    SliderPosition = Width;

                if (OnChange != null)
                    OnChange(Value);
            }

            base.Update(mouseState, keyboardState);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var shape = PositionWidthHeight;
            DrawRectangle(spriteBatch, new Rectangle(shape.Left, shape.Top + 9, Width, 2), IsHover ? HoverColor : DisabledColor);
            DrawButton(spriteBatch, "  ", new Vector2(shape.Left + SliderPosition - 9, shape.Top), null);
            spriteBatch.DrawString(this.font, Amount.ToString(), new Vector2(shape.Right + 15, shape.Top + 3), Color.White);
        }

        void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(PixelTexture, rect, color);
        }

        void DrawExpandButtonTexture(SpriteBatch batch, Texture2D texture, Vector2 position, int width, Color color = default(Color), int margin = 8)
        {
            if (color == default(Color))
                color = Color.White;
            batch.Draw(texture, position, new Rectangle(0, 0, margin, texture.Height), color);
            batch.Draw(texture, new Rectangle((int)position.X + margin, (int)position.Y, width - margin - margin, texture.Height), new Rectangle(margin, 0, 1, texture.Height), color);
            batch.Draw(texture, position + new Vector2(width - margin, 0), new Rectangle(texture.Width - margin, 0, margin, texture.Height), color);
        }

        Vector2 DrawButton(SpriteBatch spriteBatch, string text, Vector2 position, Action onClick, bool isEnabled = true)
        {
            var topLeft = position;

            var textSize = font.MeasureString(text);
            var bottomRight = topLeft + textSize;

            if (texture != null)
            {
                bottomRight = topLeft + new Vector2(textSize.X + 10, 21);
            }

            var color = IsHover ? HoverColor : DisabledColor;

            if (texture != null)
            {
                DrawExpandButtonTexture(spriteBatch, texture, position, (int)textSize.X + 10, color);
            }

            spriteBatch.DrawString(font, text, topLeft + new Vector2(5, 3), color);
            position.Y += font.LineSpacing + 10;

            return position;
        }
    }
}