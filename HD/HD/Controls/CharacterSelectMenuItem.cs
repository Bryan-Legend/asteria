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
    public class CharacterSelectMenuItem : MenuItem
    {
        const int playerStyles = 8;
        static int hover;

        Player[] players;
        Map map;

        public override int Height
        {
            get
            {
                return 64 + 20;
            }
        }

        internal override void Draw(SpriteBatch spriteBatch, int yPosition)
        {
            if (players == null) {
                map = new Map();
                players = new Player[playerStyles];
                for (int i = 0; i < playerStyles; i++) {
                    players[i] = new Player() { Map = map, Position = new Vector2(Main.ResolutionWidth / 2 + (int)((i - 3.5) * 80), yPosition + 64), Name = "", Skin = i };
                }
            }

            hover = -1;
            map.Now = DateTime.UtcNow;
            foreach (var player in players) {
                if (player.Skin == GameSettings.Default.SelectedSkin)
                    spriteBatch.Draw(Utility.EmptySlotTexture, new Rectangle(player.OffsetBoundingBox.Left - 20, player.OffsetBoundingBox.Top - 20, player.OffsetBoundingBox.Width + 40, player.OffsetBoundingBox.Height + 40), Main.SelectedColor);

                player.MousePosition = new Point(Main.CurrentMouse.X, Main.CurrentMouse.Y);
                if (player.OffsetBoundingBox.Contains(player.MousePosition)) {
                    hover = player.Skin;
                    spriteBatch.Draw(Utility.EmptySlotTexture, new Rectangle(player.OffsetBoundingBox.Left - 20, player.OffsetBoundingBox.Top - 20, player.OffsetBoundingBox.Width + 40, player.OffsetBoundingBox.Height + 40), Main.HoverColor);
                }

                player.IsFacingLeft = Main.CurrentMouse.X < player.Position.X;
                player.Draw(spriteBatch, Point.Zero);
            }
        }

        internal override void Update(MouseState mouse, KeyboardState keyboard, KeyboardState lastKeyboard, Menu menu, int menuPosition)
        {
            if (mouse.LeftButton == ButtonState.Pressed && hover >= 0) {
                GameSettings.Default.SelectedSkin = hover;
                if (players != null && players[GameSettings.Default.SelectedSkin] != null)
                    players[GameSettings.Default.SelectedSkin].SetAnimation(Animation.Spawn);
            }
        }
    }
}
