/*
 * FuchsGUI by Hisham Ghosheh
 * www.ghoshehsoft.wordpress.com
 * ghoshehsoft@live.com
 */


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FuchsGUI
{
    /// <summary>
    /// Basic button class supports events : onClick, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave
    /// </summary>
    public class Button : Control
    {
        #region Fields

        protected KeyboardState prevKeyboardState; // Needed since buttons are allowed to be clicked with enter key

        protected Rectangle[] sourceRectangles;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new button
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="text">Text displayed on the control</param>
        /// <param name="PositionWidthHeight">A rectangle that specifies the position, width , height relative to the control parent</param>
        /// <param name="texture">Texture to be drawn on the control, pass null if not needed</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public Button(String name, String text, Rectangle PositionWidthHeight, Texture2D texture, SpriteFont font, Color foreColor, Viewport? viewport = null)
            : base(name, text, PositionWidthHeight, texture, font, foreColor, viewport)
        {
            sourceRectangles = new Rectangle[3];
            if (texture != null) {
                sourceRectangles[0] = new Rectangle(0, 0, texture.Width, texture.Height / 3);
                sourceRectangles[1] = new Rectangle(0, texture.Height / 3, texture.Width, texture.Height / 3);
                sourceRectangles[2] = new Rectangle(0, 2 * texture.Height / 3, texture.Width, texture.Height / 3);

                sourceRectangle = sourceRectangles[0];
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the gui
        /// </summary>
        /// <param name="mouseState">Current mouse state</param>
        /// <param name="keyboardState">Current keyboard state</param>
        public override void Update(MouseState mouseState, KeyboardState keyboardState)
        {
            if (!enabled || !visible) return;

            sourceRectangle = sourceRectangles[0];

            if (hasFocus && keyboardState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter)) {
                mouseDown = false;
                wasReleased = false;

                // OnClick event
                if (ParentHasFocus && enableOnClick && onClick != null) onClick(this);
            }

            if (mouseDown)
                sourceRectangle = sourceRectangles[2];
            else if (mouseOver)
                sourceRectangle = sourceRectangles[1];

            prevKeyboardState = keyboardState;

            base.Update(mouseState, keyboardState);
        }

        #endregion
    }
}
