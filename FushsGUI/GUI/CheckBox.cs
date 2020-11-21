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
    /// Checkbox class supports events : onClick, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave, onToggle
    /// </summary>
    public class CheckBox : Control
    {
        #region Fields

        protected bool bChecked = false;
        public EHandler onToggle;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new CheckBox
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="text">Text displayed on the control</param>
        /// <param name="PositionWidthHeight">A rectangle that specifies the position, width , height relative to the control parent</param>
        /// <param name="texture">Texture to be drawn on the control, pass null if not needed</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public CheckBox(String name, String text, Rectangle PositionWidthHeight, Texture2D texture, SpriteFont font, Color foreColor, Viewport? viewport = null)
            : base(name, text, PositionWidthHeight, texture, font, foreColor, viewport)
        {
            onClick += new EHandler(HandlerToggle);
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
            // A checkBox has only two sprites
            if (texture != null) sourceRectangle = new Rectangle(0, (bChecked ? texture.Height / 2 : 0), texture.Width, texture.Height / 2);

            base.Update(mouseState, keyboardState);
        }

        #endregion

        #region Misc. functions

        /// <summary>
        /// Toggles the check box on/off
        /// </summary>
        /// <returns>Returns the state of the check box after toggling</returns>
        public virtual bool Toggle()
        {
            HandlerToggle(this);
            return Checked;
        }

        protected virtual void HandlerToggle(Control sender)
        {
            bChecked = !bChecked;
            if (onToggle != null) onToggle(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the state of the checkbox
        /// </summary>
        public virtual bool Checked
        {
            get
            {
                return bChecked;
            }
            set
            {
                if (bChecked != value && onToggle != null) onToggle(this);
                bChecked = value;
            }
        }

        #endregion
    }
}
