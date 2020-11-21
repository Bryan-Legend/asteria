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
    /// Basic TextBox class supports char sets,supports events : onClick, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave, onChange
    /// </summary>
    public class TextBox : Control
    {
        /// <summary>
        /// Sets, gets the ReadOnly state of the text box, if true the user won't be able to type inside the textbox
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the text displayed on the control
        /// </summary>
        public override string Text
        {
            set
            {
                var prevText = base.Text;
                base.Text = value;
                if (prevText != value) {
                    lines = (Text ?? "").Split('\n');
                    if (onChange != null)
                        onChange(this);
                }
            }
        }

        public static Color CursorColor = Color.White;

        public int Rows = 1;

        public EHandler onChange;

        // Max chars the control can contain
        protected int maxLength = 9;
        protected KeyboardState prevKeyboardState;
        protected string charSet;

        DateTime lastCursorOn;
        string[] lines;
        int cursorPosition;
        int currentLine;
        string currentLineText { get { return lines[currentLine]; } }
        int actualCursorPosition { get { return (cursorPosition > currentLineText.Length) ? currentLineText.Length : cursorPosition; } }


        /// <summary>
        /// Creates a new TextBox
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="text">Text displayed on the control</param>
        /// <param name="maxLength">Max text length the control can hold</param>
        /// <param name="PositionWidthHeight">A rectangle that specifies the position, width , height relative to the control parent</param>
        /// <param name="texture">Texture to be drawn on the control, pass null if not needed</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public TextBox(String name, string text, int maxLength, Rectangle PositionWidthHeight, Texture2D texture, SpriteFont font, Color foreColor, Viewport? viewport = null)
            : this(name, text, maxLength, "", PositionWidthHeight, texture, font, foreColor, viewport)
        {
            Text = null;
            Text = text; // so that it initalizes lines
            currentLine = lines.Length - 1;
            cursorPosition = currentLineText.Length;
            TextPosition = TextPosition.Left;
            if (texture != null) sourceRectangle = new Rectangle(0, texture.Height / 2, texture.Width, texture.Height / 2);
        }

        /// <summary>
        /// Creates a new TextBox
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="text">Text displayed on the control</param>
        /// <param name="maxLength">Max text length the control can hold</param>
        /// <param name="charSet">A string containing all allowed chars, other chars will be omitted when typed</param>
        /// <param name="PositionWidthHeight">A rectangle that specifies the position, width , height relative to the control parent</param>
        /// <param name="texture">Texture to be drawn on the control, pass null if not needed</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public TextBox(String name, String text, int maxLength, string charSet, Rectangle PositionWidthHeight, Texture2D texture, SpriteFont font, Color foreColor, Viewport? viewport = null)
            : base(name, text, PositionWidthHeight, texture, font, foreColor, viewport)
        {
            Text = null;
            Text = text; // so that it initalizes lines
            currentLine = lines.Length - 1;
            cursorPosition = currentLineText.Length;
            TextPosition = TextPosition.Left;
            this.charSet = charSet;
            this.maxLength = maxLength;
        }

        Keys heldkey = Keys.Back;
        DateTime heldSince;

        /// <summary>
        /// Updates the gui
        /// </summary>
        /// <param name="mouseState">Current mouse state</param>
        /// <param name="keyboardState">Current keyboard state</param>
        public override void Update(MouseState mouseState, KeyboardState keyboardState)
        {
            if (!enabled || !visible)
                return;

            // if both control and its parent has focus process keyboard commands
            if (!ReadOnly && Focus && ParentHasFocus) {
                var pressed = keyboardState.GetPressedKeys();
                var shiftKeyPressed = false;

                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                    shiftKeyPressed = true;

                var keyPressed = false;
                foreach (var key in pressed) {
                    keyPressed = true;
                    if (prevKeyboardState.IsKeyUp(key) || heldSince < DateTime.UtcNow.AddMilliseconds(-300)) {
                        if (key == Keys.Back || key == Keys.Delete) {
                            var index = GetCurrentCursorIndex() - (key == Keys.Back ? 1 : 0);
                            if (index >= 0 && index < Text.Length) {
                                // cover the case where you arrow up or down from a longer line then delete
                                if (cursorPosition > currentLineText.Length)
                                    cursorPosition = currentLineText.Length;

                                // note previous line length so you know where to place cursor if backspacing a line return
                                var previousLineLength = 0;
                                if (currentLine > 0)
                                    previousLineLength = lines[currentLine - 1].Length;

                                var removed = Text[index];
                                Text = Text.Remove(index, 1);

                                if (key == Keys.Back) {
                                    cursorPosition--;
                                    if (cursorPosition < 0)
                                        cursorPosition = 0;

                                    if (removed == '\n') {
                                        currentLine--;
                                        cursorPosition = previousLineLength;
                                    }
                                }
                            }
                        } else if (key == Keys.Home) {
                            Home();
                        } else if (key == Keys.End) {
                            End();
                        } else if (key == Keys.Left) {
                            cursorPosition--;
                            if (cursorPosition < 0)
                                cursorPosition = 0;
                        } else if (key == Keys.Right) {
                            cursorPosition++;
                            if (cursorPosition > currentLineText.Length)
                                cursorPosition = currentLineText.Length;
                        } else if (key == Keys.Up) {
                            currentLine--;
                            if (currentLine < 0)
                                currentLine = 0;
                        } else if (key == Keys.Down) {
                            currentLine++;
                            if (currentLine >= lines.Length)
                                currentLine = lines.Length - 1;
                        } else if (key == Keys.Enter) {
                            if (lines.Length < Rows) {
                                Text = Text.Insert(GetCurrentCursorIndex(), "\n");
                                currentLine++;
                                cursorPosition = 0;
                            }
                        } else if (currentLineText.Length < maxLength) {
                            char ch;
                            var charFound = KeyboardUtils.KeyToString(key, shiftKeyPressed, out ch);

                            if (charSet != null & charSet != "") {
                                if (!charSet.Contains(ch))
                                    charFound = false;
                            }

                            if (charFound) {
                                Text = Text.Insert(GetCurrentCursorIndex(), ch.ToString());
                                cursorPosition++;
                            }
                        }

                        if (heldkey != key) {
                            heldkey = key;
                            heldSince = DateTime.UtcNow;
                        } else
                            heldSince = heldSince.AddMilliseconds(50);
                    }
                }

                if (!keyPressed)
                    heldkey = Keys.None;
            }

            prevKeyboardState = keyboardState;

            // TextBox has two sprites, one when it has focus, the other when it doesn't
            if (texture != null)
                sourceRectangle = new Rectangle(0, (hasFocus ? texture.Height / 2 : 0), texture.Width, texture.Height / 2);

            base.Update(mouseState, keyboardState);
        }

        int GetCurrentCursorIndex()
        {
            var result = 0;
            for (int i = 0; i < currentLine; i++) {
                //if (result != 0)
                result++;
                result += lines[i].Length;
            }
            result += actualCursorPosition;
            return result;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            var cursorTimespan = DateTime.UtcNow - lastCursorOn;

            if (Focus) {
                if (cursorTimespan.TotalSeconds < 0.5) {
                    var textPosition = CalculateTextPosition();
                    Vector2 pixelCursorPosition;
                    if (Rows == 1)
                        pixelCursorPosition = new Vector2((int)(textPosition.X + font.MeasureString(RenderText.Substring(0, actualCursorPosition)).X - 2), (int)textPosition.Y);
                    else
                        pixelCursorPosition = new Vector2((int)(textPosition.X + font.MeasureString(currentLineText.Substring(0, actualCursorPosition)).X - 2), (int)textPosition.Y + currentLine * font.LineSpacing);
                    spriteBatch.DrawString(font, "|", pixelCursorPosition, CursorColor);
                } else {
                    if (cursorTimespan.TotalSeconds > 1) {
                        lastCursorOn = DateTime.UtcNow;
                    }
                }
            }
        }

        public void End()
        {
            if (currentLineText == null)
                cursorPosition = 0;
            else
                cursorPosition = currentLineText.Length;
        }

        public void Home()
        {
            cursorPosition = 0;
        }
    }
}
