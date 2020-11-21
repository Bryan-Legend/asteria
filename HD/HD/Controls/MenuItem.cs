using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HD
{
    public class MenuItem
    {
        public string Text = "";
        public Action<Object> OnClick;
        public object Tag;

        bool isHover;
        public bool IsHover
        {
            get { return isHover; }
            set
            {
                if (!isHover && value)
                    Audio.PlaySound(Sound.HoverTick);
                isHover = value;
            }
        }

        int height;
        public virtual int Height { get { return height; } }

        public MenuItem()
        {
            height = Utility.Font.LineSpacing + 3;
        }

        internal virtual void Draw(SpriteBatch spriteBatch, int yPosition)
        {
            if (Text == null)
                return;

            var textSize = Utility.Font.MeasureString(Text);
            height = (int)textSize.Y + 3;

            var topLeft = new Vector2((int)((Main.ResolutionWidth - textSize.X) / 2), yPosition);
            var bottomRight = topLeft + textSize;

            if (OnClick != null && Main.CurrentMouse.X > topLeft.X && Main.CurrentMouse.Y > topLeft.Y && Main.CurrentMouse.X <= bottomRight.X && Main.CurrentMouse.Y <= bottomRight.Y) {
                IsHover = true;
                Main.IsMouseOverControl = true;
            } else
                IsHover = false;

            spriteBatch.DrawString(Utility.Font, Text, topLeft, IsHover ? Menu.HoverColor : Color.White);
        }

        internal virtual void Update(MouseState mouse, KeyboardState keyboard, KeyboardState lastKeyboard, Menu menu, int menuPosition)
        {
        }
    }
}
