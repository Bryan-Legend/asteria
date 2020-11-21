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
    public enum SizeMode { Normal, StretchImage, CenterImage };

    /// <summary>
    /// Basic picture box class, supports events : onClick, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave    
    /// </summary>
    public class PictureBox : Control
    {

        #region Fields

        protected SizeMode sizeMode = SizeMode.Normal;
        protected Rectangle destinationRectangle; // In case image size is not equal to positionWidthHeight

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new PictureBox
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="PositionWidthHeight">A rectangle that specifies the position, width , height relative to the control parent</param>
        /// <param name="image">Image to display</param>
        /// <param name="sizeMode">Optional,Sets image layout : Normal : the picture is displayed without resizing, StrechImage : the image is resized to fill the picture box, CenterImage : the image is centered in the PictureBox without resizing, The defualt is Normal</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public PictureBox(String name, Rectangle PositionWidthHeight, Texture2D image, SizeMode sizeMode = SizeMode.Normal, Viewport? viewport = null)
            : base(name, "", PositionWidthHeight, image, null, Color.Transparent, viewport)
        {
            // Set sizeMode & calculate source rectangle
            SizeMode = sizeMode;
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the control
        /// Must be called inside a spriteBatch.Begin(),spriteBatch.End() block
        /// </summary>
        /// <param name="spriteBatch">Spritebatch you're using to render 2D elements</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!visible) return;

            if (texture != null) {
                Color col = Enabled ? Color.White : Color.Gray;

                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White);
            }
        }

        #endregion

        #region Misc. Functions

        /// <summary>
        /// Calculates rectangles required for rendering
        /// </summary>
        protected void CalcRectangles()
        {
            if (texture == null) return;

            destinationRectangle = positionWidthHeight;

            switch (sizeMode) {
                case FuchsGUI.SizeMode.Normal:
                    if (texture.Width >= positionWidthHeight.Width && texture.Height >= positionWidthHeight.Height) {
                        sourceRectangle = new Rectangle(0, 0, positionWidthHeight.Width, positionWidthHeight.Height);
                    } else {
                        sourceRectangle = null;
                        destinationRectangle = new Rectangle(positionWidthHeight.X, positionWidthHeight.Y, texture.Width, texture.Height);
                    }
                    break;
                case FuchsGUI.SizeMode.StretchImage:
                    sourceRectangle = null;
                    break;
                case FuchsGUI.SizeMode.CenterImage:
                    if (texture != null) {
                        if (texture.Width >= positionWidthHeight.Width && texture.Height >= positionWidthHeight.Height) {
                            sourceRectangle = new Rectangle(texture.Width / 2 - positionWidthHeight.Width / 2, texture.Height / 2 - positionWidthHeight.Height / 2, positionWidthHeight.Width, positionWidthHeight.Height);
                        } else {
                            sourceRectangle = null;
                            destinationRectangle = new Rectangle(positionWidthHeight.Width / 2 + positionWidthHeight.X - texture.Width / 2, positionWidthHeight.Height / 2 + positionWidthHeight.Y - texture.Width / 2, texture.Width, texture.Height);
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets, sets the image to display
        /// </summary>
        public Texture2D Image
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;

                if (value != null && sizeMode == FuchsGUI.SizeMode.CenterImage) CalcRectangles();
            }
        }

        /// <summary>
        /// The image control wont render any text, but it can hold string values...
        /// </summary>
        public override string Text
        {
            get
            {
                return "";
            }
            set
            {
                base.Text = "";
            }
        }

        // Sets,gets image layout : Normal : the picture is displayed without resizing, StrechImage : the image is resized to fill the picture box, CenterImage : the image is centered in the PictureBox without resizing
        public SizeMode SizeMode
        {
            get
            {
                return sizeMode;
            }
            set
            {
                sizeMode = value;
                CalcRectangles();
            }
        }

        /// <summary>
        /// Gets or sets the position of the control relative to its parent, or to the game window if there's no parent
        /// </summary>
        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                CalcRectangles();
            }
        }

        #endregion

    }
}
