/*
 * FuchsGUI by Hisham Ghosheh
 * www.ghoshehsoft.wordpress.com
 * ghoshehsoft@live.com
 */


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
    /// <summary>
    /// Circular Button supports events : onClick, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave
    /// </summary>
    public class CircleButton : Button
    {
        #region Initialization

        /// <summary>
        /// Creates a new circular button
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="text">Text displayed on the control</param>
        /// <param name="CenterRadius">A Vector3 that specifies the center of the button and its radius</param>
        /// <param name="texture">Texture to be drawn on the control, pass null if not needed</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public CircleButton(String name, String text, Vector3 CenterRadius, Texture2D texture, SpriteFont font, Color foreColor, Viewport? viewport = null)
            : base(name, text, new Rectangle((int)(CenterRadius.X - CenterRadius.Z), (int)(CenterRadius.Y - CenterRadius.Z), (int)(2f * CenterRadius.Z), (int)(2f * CenterRadius.Z)), texture, font, foreColor, viewport)
        {
        }

        #endregion

        #region Misc. Functions

        /// <summary>
        /// Performs poitn-circle collision check
        /// </summary>
        /// <param name="mouseX">Mouse x relative to game window</param>
        /// <param name="mouseY">Mouse y relative to game window</param>
        /// <returns>Returns true if mouse is inside the button</returns>
        protected override bool CheckMouseInside(int mouseX, int mouseY)
        {
            Point mousePos = GetMouseRelativeToViewPort(mouseX, mouseY);

            Vector2 center = new Vector2((positionWidthHeight.X + positionWidthHeight.Width / 2f), (positionWidthHeight.Y + positionWidthHeight.Height / 2f));
            return Vector2.Distance(new Vector2(mousePos.X, mousePos.Y), center) < positionWidthHeight.Width / 2f;
        }

        #endregion
    }
}
