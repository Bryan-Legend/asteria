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
    public class TextureMenuItem : MenuItem
    {
        public Texture2D Texture;

        public override int Height
        {
            get
            {
                if (Texture != null)
                    return Texture.Height + 20;
                return base.Height;
            }
        }

        internal override void Draw(SpriteBatch spriteBatch, int yPosition)
        {
            if (Texture != null) {
                spriteBatch.Draw(Texture, new Vector2(((Main.ResolutionWidth - Texture.Width) / 2), yPosition), Color.White);
            }
        }
    }
}
