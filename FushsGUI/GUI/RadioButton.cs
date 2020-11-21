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
    /// Basic RadioBox class, supports events : onClick, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave, onToggle 
    /// Must have a parent to works if more than one RadioBox exist..
    /// </summary>
    public class RadioButton : CheckBox
    {
        #region Fields

        // Was the radio button checked this frame??
        protected bool iGotChecked = false;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new RadioButton
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="text">Text displayed on the control</param>
        /// <param name="PositionWidthHeight">A rectangle that specifies the position, width , height relative to the control parent</param>
        /// <param name="texture">Texture to be drawn on the control, pass null if not needed</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public RadioButton(String name, String text, Rectangle PositionWidthHeight, Texture2D texture, SpriteFont font, Color foreColor, Viewport? viewport = null)
            : base(name, text, PositionWidthHeight, texture, font, foreColor, viewport)
        {
        }

        #endregion

        #region Misc. functions

        protected override void HandlerToggle(Control sender)
        {
            bChecked = true;
            if (onToggle != null) onToggle(this);
        }

        public override bool Toggle()
        {
            Checked = true;

            return true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets, sets the checked property of the control..When set, the check propery of other RadioButtons in the form is set to false
        /// </summary>
        public override bool Checked
        {
            get
            {
                return base.Checked;
            }
            set
            {
                base.Checked = value;
                iGotChecked = value;

                if (value && onToggle != null) onToggle(this);
            }
        }

        /// <summary>
        /// Returns true if the RadioButton was checked this frame
        /// </summary>
        public virtual bool CheckedThisFrame
        {
            get
            {
                return iGotChecked;
            }
            set
            {
                iGotChecked = value;
            }
        }

        #endregion
    }
}
