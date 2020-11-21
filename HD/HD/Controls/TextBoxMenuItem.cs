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
    public class TextBoxMenuItem : MenuItem
    {
        public string TypedText
        {
            get
            {
                if (textBox != null)
                    return textBox.Text;
                return "";
            }
        }

        bool wasFocused;
        bool isFocused;
        public bool IsFocused
        {
            get { return isFocused; }
            set
            {
                isFocused = value;
                if (textBox != null)
                    textBox.Focus = value;
            }
        }

        public bool IsPassword;

        TextBox textBox;

        public TextBoxMenuItem()
        {
            isFocused = true;
        }

        public void SetText(string value)
        {
            Text = value;
            if (textBox != null)
                textBox.Text = value;
        }

        internal override void Draw(SpriteBatch spriteBatch, int yPosition)
        {
            yPosition -= 12;
            if (textBox == null)
            {
                textBox = new TextBox("", base.Text, 30, new Rectangle((int)((Main.ResolutionWidth - Main.MenuTextBoxTexture.Width) / 2), yPosition, Main.MenuTextBoxTexture.Width, Utility.Font.LineSpacing), null, Utility.Font, Color.White) { Focus = IsFocused, TextPosition = TextPosition.TopCenter, IsPassword = IsPassword };
                textBox.onClick += (sender) => { UnfocusOtherTextboxes(); };
            }

            textBox.Position = new Vector2(((Main.ResolutionWidth - Main.MenuTextBoxTexture.Width) / 2), yPosition);
            spriteBatch.Draw(Main.MenuTextBoxTexture, textBox.Position + new Vector2(0, -3), Color.White);
            textBox.Draw(spriteBatch);
        }

        void UnfocusOtherTextboxes()
        {
            foreach (var menuItem in Main.CurrentMenu.Items)
            {
                var textBoxMenuItem = menuItem as TextBoxMenuItem;
                if (menuItem != this && textBoxMenuItem != null)
                {
                    textBoxMenuItem.IsFocused = false;
                }
            }
        }

        internal override void Update(MouseState mouse, KeyboardState keyboard, KeyboardState lastKeyboard, Menu menu, int menuPosition)
        {
            // check for tab & shift-tab here to do menu navigation
            if (wasFocused && keyboard.IsPressed(lastKeyboard, Keys.Tab))
            {
                TextBoxMenuItem textBoxItem;
                do
                {
                    menuPosition = (menuPosition + (keyboard.IsShift() ? -1 : 1)) % menu.Items.Count;
                    if (menuPosition < 0)
                        menuPosition = menu.Items.Count - 1;
                    textBoxItem = menu.Items[menuPosition] as TextBoxMenuItem;
                }
                while (textBoxItem == null);

                //set focus to next text box from index
                IsFocused = false;
                textBoxItem.IsFocused = true;
            }

            if (textBox != null)
                textBox.Update(mouse, keyboard);

            wasFocused = IsFocused;
        }
    }
}
