using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HD
{
    public class Menu
    {
        public static Color HoverColor = Main.DefaultColor;
        public List<MenuItem> Items = new List<MenuItem>();
        public Menu PreviousMenu;
        public Action<Menu> OnShow;
        public bool CloseOnEscape = true;
        public Action OnEscapeClose;

        internal void Draw(SpriteBatch spriteBatch)
        {
            var yPosition = Main.ResolutionHeight;
            foreach (var item in Items)
                yPosition -= item.Height + 2;
            yPosition /= 2;

            foreach (var item in Items) {
                item.Draw(spriteBatch, yPosition);
                yPosition += item.Height + 2;
            }
        }

        internal void Update(MouseState mouse, MouseState lastMouse, KeyboardState keyboard, KeyboardState lastKeyboard)
        {
            if (CloseOnEscape && (keyboard.IsPressed(lastKeyboard, Keys.Escape) || Main.CurrentGamePad.IsPressed(Main.LastGamePad, Buttons.B))) {
                if (OnEscapeClose != null)
                    OnEscapeClose();

                Main.ReturnToPreviousMenu();
            }

            if (mouse.LeftButton == ButtonState.Pressed && lastMouse.LeftButton == ButtonState.Released)
                Click();

            for (int count = 0; count < Items.Count; count++) {
                Items[count].Update(mouse, keyboard, lastKeyboard, this, count);
            }
        }

        internal void Click()
        {
            foreach (var item in Items) {
                if (item.IsHover && item.OnClick != null) {
                    Audio.PlaySound(Sound.Tick);
                    item.OnClick(item.Tag);
                }
            }
        }

        public void Show(bool setPreviousMenu = true)
        {
            if (setPreviousMenu)
                PreviousMenu = Main.CurrentMenu;
            else
                PreviousMenu = Main.CurrentMenu.PreviousMenu;

            Main.CurrentMenu = this;
            if (OnShow != null)
                OnShow(this);
        }
    }
}
