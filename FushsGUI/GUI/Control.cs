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
    public enum TextPosition { Left, Center, TopCenter };
    public delegate void EHandler(Control sender);

    /// <summary>
    /// Abstract control class, supports events : onClick, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave
    /// </summary>
    public abstract class Control
    {

        #region Fields

        // Color to draw controls when in active
        protected static Color inactiveColor = new Color(240, 240, 240);

        public Control parent; // parent contiaing this control, set from out side

        protected String name;
        String text;
        protected SpriteFont font;
        protected Color foreColor;

        protected Rectangle positionWidthHeight;

        public TextPosition TextPosition = TextPosition.Center;

        protected Texture2D texture;
        protected Rectangle? sourceRectangle; // Which part of the spritesheet to draw

        protected bool hasFocus = false;
        protected bool enabled = true;
        protected bool visible = true;

        // Events (actually delegates) supported by this control
        public EHandler onClick;
        public EHandler onMouseDown;
        public EHandler onMouseMove;
        public EHandler onMouseEnter;
        public EHandler onMouseLeave;

        protected bool enableOnClick = true;
        protected bool enableOnMouseEnter = true;
        protected bool enableOnMouseLeave = true;
        protected bool enableOnMouseMove = true;
        protected bool enableOnMouseDown = true;

        protected bool readyForMouseDown = false; // Mosue is over & button is realeased
        protected bool mouseDown = false; // half click
        protected bool mouseMoved = false;
        protected bool clicked = false; // mouse was down and then up
        protected bool gotFocus = false; // Control has just got focus, wasn't focused in the previouse frame
        protected bool wasReleased = false; // To make sure that unintentional clicking doesn't happen
        protected bool mouseOver = false;
        protected bool mouseLastIn = false; // Was mouse inside the control last frame?

        protected Viewport? viewport; // Viewport used to render the control

        protected MouseState prevMouseState;

        #endregion

        #region Initialization

        /// <summary>
        /// Main constructor for the abstract class
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="text">Text displayed on the control</param>
        /// <param name="PositionWidthHeight">A rectangle that specifies the position, width , height relative to the control parent</param>
        /// <param name="texture">Texture to be drawn on the control, pass null if not needed</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public Control(String name, String text, Rectangle PositionWidthHeight, Texture2D texture, SpriteFont font, Color foreColor, Viewport? viewport = null)
        {
            this.positionWidthHeight = PositionWidthHeight;
            this.name = name;
            this.text = text;
            this.font = font;
            this.foreColor = foreColor;
            this.texture = texture;

            this.viewport = viewport.HasValue ? viewport : null;
        }

        #endregion

        #region Update & Draw

        /// <summary>
        /// Updates the gui
        /// </summary>
        /// <param name="mouseState">Current mouse state</param>
        /// <param name="keyboardState">Current keyboard state</param>
        public virtual void Update(MouseState mouseState, KeyboardState keyboardState)
        {
            if (!enabled || !visible) return;

            gotFocus = false;

            clicked = false;
            mouseOver = false;
            mouseMoved = false;

            if (CheckMouseInside(mouseState.X, mouseState.Y) && CheckMouseInsideParent(mouseState.X, mouseState.Y)) {
                // MouseEntered event
                if (!mouseLastIn && enableOnMouseEnter && onMouseEnter != null) onMouseEnter(this);

                mouseOver = true;

                // if mouse moved since last frame
                if (mouseState.X != prevMouseState.X || mouseState.Y != prevMouseState.Y) {
                    mouseMoved = true;
                    if (enableOnMouseMove && onMouseMove != null) onMouseMove(this);
                }
            } else {
                // MouseLeave event
                if (mouseLastIn && enableOnMouseLeave && onMouseLeave != null) onMouseLeave(this);

                if (mouseState.LeftButton == ButtonState.Released) mouseDown = false;
            }

            if (mouseOver && mouseDown && mouseState.LeftButton == ButtonState.Released) {
                mouseDown = false;
                wasReleased = false;
                clicked = true;
                gotFocus = !hasFocus;
                hasFocus = true;

                // OnClick event
                if (enableOnClick && onClick != null) onClick(this);
            }

            if (readyForMouseDown && wasReleased && mouseOver && mouseState.LeftButton == ButtonState.Pressed) {
                mouseDown = true;
                wasReleased = false;

                // OnMouseDown event
                if (enableOnMouseDown && onMouseDown != null) onMouseDown(this);
            }

            if (mouseOver && mouseState.LeftButton == ButtonState.Released)
                readyForMouseDown = true;
            else
                readyForMouseDown = false;

            if (mouseState.LeftButton == ButtonState.Released) wasReleased = true;

            mouseLastIn = mouseOver;
            prevMouseState = mouseState;
        }

        /// <summary>
        /// Draws the control
        /// Must be called inside a spriteBatch.Begin(),spriteBatch.End() block
        /// </summary>
        /// <param name="spriteBatch">Spritebatch you're using to render 2D elements</param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!visible) return;

            Color col = hasFocus ? Color.White : inactiveColor;

            if (!Enabled) col = Color.Gray;

            // Only draw texture if provided during creation
            if (texture != null) {
                spriteBatch.Draw(
                    texture,
                    positionWidthHeight,
                    sourceRectangle,
                    col);
            }

            // Only draw text if there's some text & a font was supplied
            if (text != "" && font != null) {
                var textPos = CalculateTextPosition();
                spriteBatch.DrawString(font, RenderText, textPos, foreColor);
            }
        }

        protected string RenderText { get { return IsPassword ? new String('*', text.Length) : text; } }

        protected Vector2 CalculateTextPosition()
        {
            Vector2 textPos;
            if (TextPosition == TextPosition.Center)
                textPos = AbsPosition + new Vector2(positionWidthHeight.Width, positionWidthHeight.Height) / 2f - font.MeasureString(RenderText) / 2f;
            else if (TextPosition == TextPosition.TopCenter)
                textPos = AbsPosition + new Vector2((positionWidthHeight.Width - font.MeasureString(RenderText).X) / 2f, 0);
            else
                textPos = new Vector2(positionWidthHeight.X, positionWidthHeight.Y);

            textPos.X = (int)textPos.X;
            textPos.Y = (int)textPos.Y;
            return textPos;
        }

        #endregion

        #region Misc. functions

        /// <summary>
        /// Returns a System.String that represents the current control.
        /// </summary>
        /// <returns>A System.String that represents the current control.</returns>
        public override string ToString()
        {
            return "FuchsGUI:" + ParentName + "." + name;
        }

        /// <summary>
        /// Performs simple rectangle point collision check.
        /// Can be overriden for custom shape controls..
        /// </summary>
        /// <param name="mouseX">Mouse x relative to game window</param>
        /// <param name="mouseY">Mouse y relative to game window</param>
        /// <returns>returns true if mouse is inside the control</returns>
        protected virtual bool CheckMouseInside(int mouseX, int mouseY)
        {
            Point mousePos = GetMouseRelativeToViewPort(mouseX, mouseY);

            if (mousePos.X < 0 || mousePos.Y < 0) return false;

            return positionWidthHeight.Contains(mousePos);
        }

        /// <summary>
        /// Returns mouse position relative to the viewport if exists
        /// </summary>
        /// <param name="mouseX">Mouse x relative to game window</param>
        /// <param name="mouseY">Mouse y relative to game window</param>
        /// <returns>Returns mouse position relative to the viewport if exists</returns>
        protected virtual Point GetMouseRelativeToViewPort(int mouseX, int mouseY)
        {
            Point viewportPosition = (viewport.HasValue ? new Point(viewport.Value.X, viewport.Value.Y) : Point.Zero);

            int x = mouseX - viewportPosition.X;
            int y = mouseY - viewportPosition.Y;

            return new Point(x, y);
        }

        // Used to only allow points inside the parent to be clicked
        protected bool CheckMouseInsideParent(int mouseX, int mouseY)
        {

            if (parent == null)
                return true;
            else
                return parent.CheckMouseInside(mouseX, mouseY);
        }

        /// <summary>
        /// Simulates a click on this control
        /// </summary>
        public void Click()
        {
            if (onClick != null) onClick(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the control
        /// </summary>
        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets or sets the text displayed on the control
        /// </summary>
        public virtual String Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        /// <summary>
        /// Gets the name of the control parent, if none "Screen" is returned
        /// </summary>
        public String ParentName
        {
            get
            {
                if (parent == null)
                    return "Screen";
                else
                    return parent.name;
            }
        }

        #region Events related properties

        /// <summary>
        /// Gets or sets the focus of the control
        /// </summary>
        public bool Focus
        {
            get
            {
                if (parent == null)
                    return hasFocus;
                else
                    return hasFocus && parent.ParentHasFocus;
            }
            set
            {
                hasFocus = value;
            }
        }

        /// <summary>
        /// Returns true if the OnMouseDown event is triggered
        /// </summary>
        public bool MouseDown
        {
            get
            {
                return mouseDown;
            }
        }

        /// <summary>
        /// Returns true if the controls got the focus this frame
        /// </summary>
        public bool GotFocus
        {
            get
            {
                return gotFocus;
            }
        }

        /// <summary>
        /// Returns true if the OnClick event is triggered
        /// </summary>
        public bool Clicked
        {
            get
            {
                return clicked;
            }
        }

        /// <summary>
        /// Returns true if the OnMouseMove event is triggered
        /// </summary>
        public bool MouseMoved
        {
            get
            {
                return mouseMoved;
            }
        }

        /// <summary>
        /// Returns true if the mouse is over the control or one of its children
        /// </summary>
        public bool MouseOver
        {
            get
            {
                return mouseOver && visible;
            }
        }

        #endregion

        /// <summary>
        /// Returns true if the parent has focus or there's no parent
        /// </summary>
        public bool ParentHasFocus
        {
            get
            {
                bool focus = true;
                if (parent != null) focus = parent.Focus;
                return focus;
            }
        }

        /// <summary>
        /// Enables/Disables the control, disabled controls won't raise events until enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (parent == null)
                    return enabled;
                else
                    return enabled && parent.Enabled;
            }
            set
            {
                enabled = value;
            }
        }

        /// <summary>
        /// Shows/Hides the control, hidden controls wont receive any input & won't raise events, and ofcourse not drawn
        /// </summary>
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
            }
        }

        /// <summary>
        /// Gets the rectangle that specifies the layout of the control
        /// </summary>
        public Rectangle PositionWidthHeight
        {
            get
            {
                return positionWidthHeight;
            }
        }

        /// <summary>
        /// Gets or sets the position of the control relative to its parent, or to the game window if there's no parent
        /// </summary>
        public virtual Vector2 Position
        {
            get
            {
                Vector2 parentPos = (parent == null ? Vector2.Zero : parent.AbsPosition);

                return new Vector2(positionWidthHeight.X, positionWidthHeight.Y) - parentPos;
            }
            set
            {
                Vector2 parentPos = (parent == null ? Vector2.Zero : parent.AbsPosition);

                positionWidthHeight.X = (int)(value + parentPos).X;
                positionWidthHeight.Y = (int)(value + parentPos).Y;
            }
        }

        /// <summary>
        /// Gets , sets the viewport of this control, setting it manully might give unwanted results.
        /// </summary>
        public virtual Viewport? Viewport
        {
            get
            {
                return viewport;
            }
            set
            {
                viewport = value;
            }
        }

        /// <summary>
        /// Returns the position of the control relative to the game window
        /// </summary>
        public Vector2 AbsPosition
        {
            get
            {
                return new Vector2(positionWidthHeight.X, positionWidthHeight.Y);
            }
        }

        /// <summary>
        /// Gets the width of the control
        /// </summary>
        public int Width
        {
            get
            {
                return positionWidthHeight.Width;
            }
        }

        /// <summary>
        /// Gets the height of the control
        /// </summary>
        public int Height
        {
            get
            {
                return positionWidthHeight.Height;
            }
        }

        #endregion

        public bool IsPassword { get; set; }
    }
}
