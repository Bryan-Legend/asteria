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
    /// Label control that can wrap text, supports events : onClick, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave
    /// </summary>
    public class Label : Control
    {

        #region Fields

        protected int charsPerLine; // This class allows text wrapping
        protected int distanceBetweenLines;

        // A list contains text after being wraped, the original text stays intact
        protected List<string> lines = new List<string>();

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new label control
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="text">Text displayed on the control</param>
        /// <param name="Position">Position of the label</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="charsPerLine">Number of characters allowed per line, pass zero to disable wrapping</param>
        /// <param name="distanceBetweenLines">Distance between lines in pixles, must be greater than zero</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public Label(String name, String text, Vector2 position, SpriteFont font, Color foreColor, int charsPerLine, int distanceBetweenLines, Viewport? viewport = null)
            : base(name, text, new Rectangle((int)position.X, (int)position.Y, 1, 1), null, font, foreColor, viewport)
        {
            this.charsPerLine = charsPerLine;
            if (distanceBetweenLines <= 0) distanceBetweenLines = (int)font.MeasureString("M").Y;

            this.distanceBetweenLines = distanceBetweenLines;

            CalculateLayout();
        }

        protected void CalculateLayout()
        {
            lines.Clear();

            if (charsPerLine == 0) // No wrapping
            {
                lines.Add(Text);
            } else {
                string[] words = Text.Split(' ');
                string line = "";

                for (int i = 0; i <= words.Count() - 1; i++) {
                    if (words[i].Length >= charsPerLine) {
                        if (line != "") lines.Add(line);
                        lines.Add(words[i]);
                    } else {
                        if (line.Length < charsPerLine) {
                            line += words[i] + " ";
                        } else {
                            lines.Add(line);
                            line = "";
                            --i;
                        }
                    }
                }
                if (line != "") lines.Add(line);
            }

            // Calculating width & height
            int width = 0;
            int height = 0;

            // Gettin max line width
            foreach (string s in lines)
                width = (int)Math.Max(width, font.MeasureString(s).X);

            height = (int)(distanceBetweenLines) * lines.Count;

            // Adjusting the layout rectangle
            positionWidthHeight.Width = width;
            positionWidthHeight.Height = height;
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

            Vector2 pen = AbsPosition;
            foreach (string s in lines) {
                spriteBatch.DrawString(font, s, pen, foreColor);
                pen.Y += distanceBetweenLines;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets, sets the text of the label... When set, layout will be calculated again
        /// </summary>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                CalculateLayout();
            }
        }

        #endregion
    }
}
