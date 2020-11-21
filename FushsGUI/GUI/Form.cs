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
    /// A Form control that can contain other controls including child forms,supports events : onClick, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave, onClose
    /// </summary>
    public class Form : Control
    {

        #region Fields

        // Children
        protected List<Control> controls = new List<Control>();

        // Controls deletion ( mainly used for forms ) is not immediate
        protected List<Control> controlsToDelete = new List<Control>();

        // A form can be closed which sends a message to its parent form to delete it ASAP
        public EHandler onClose;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new Form
        /// </summary>
        /// <param name="name">Control name</param>
        /// <param name="text">Text displayed on the control</param>
        /// <param name="PositionWidthHeight">A rectangle that specifies the position, width , height relative to the control parent</param>
        /// <param name="texture">Texture to be drawn on the control, pass null if not needed</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public Form(String name, String text, Rectangle PositionWidthHeight, Texture2D texture, SpriteFont font, Color foreColor, Viewport? viewport = null)
            : base(name, text, PositionWidthHeight, texture, font, foreColor, viewport)
        {
            sourceRectangle = null; // The form will its whole spritesheet
            TextPosition = TextPosition.TopCenter;

            // When the form is clicked we want to remove focus from its children
            onClick += new EHandler(HandlerMeClicked);
        }

        #endregion

        #region Update & Draw

        /// <summary>
        /// Updates the gui
        /// </summary>
        /// <param name="mouseState">Current mouse state</param>
        /// <param name="keyboardState">Current keyboard state</param>
        public override void Update(MouseState mouseState, KeyboardState keyboardState)
        {
            if (!Enabled || !visible) return;

            mouseMoved = false;

            // Update children
            foreach (Control c in controls) {
                c.Update(mouseState, keyboardState);

                // If a child triggered these events the form wont trigger them again
                this.clicked = this.clicked || c.Clicked;
                this.mouseDown = this.mouseDown || c.MouseDown;
                this.mouseMoved = this.mouseMoved || c.MouseMoved;
            }

            // Only give focus to one child and remove it from the others
            foreach (Control c in controls) {
                if (c.Clicked) {
                    if (c is RadioButton) {
                        foreach (Control r in controls) {
                            if (r is RadioButton && c != r)
                                ((RadioButton)r).Checked = false;
                        }
                    }

                    HandlerRemoveFocusFromChildren(c);
                    break;
                }
            }

            // Delete controls in the deletion list
            for (int i = 0; i <= controlsToDelete.Count - 1; i++) {
                for (int t = 0; t <= controls.Count - 1; t++) {
                    if (controls[t] == controlsToDelete[i]) {
                        controls.RemoveAt(t);
                        break;
                    }
                }
            }

            controlsToDelete.Clear();

            // Set permissions for some events
            enableOnClick = !this.clicked;
            enableOnMouseDown = !this.mouseDown;
            enableOnMouseMove = !this.mouseMoved;

            base.Update(mouseState, keyboardState);
        }

        /// <summary>
        /// Draws the control
        /// Must be called inside a spriteBatch.Begin(),spriteBatch.End() block
        /// </summary>
        /// <param name="spriteBatch">Spritebatch you're using to render 2D elements</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!visible) return;

            // Draw form first
            base.Draw(spriteBatch);

            // Draw children
            foreach (Control c in controls)
                c.Draw(spriteBatch);
        }

        #endregion

        #region Misc. Functions

        /// <summary>
        /// Removes the focus from all children except for the one which sent the request
        /// </summary>
        /// <param name="sender">The control that sent the request</param>
        protected virtual void HandlerRemoveFocusFromChildren(Control sender)
        {
            hasFocus = true;
            foreach (Control c in controls)
                if (c != sender) c.Focus = false;
        }

        /// <summary>
        /// Occurrs when a child form Close is called, which tells the parent form to mark it for deletion
        /// </summary>
        /// <param name="sender">Form triggered the event</param>
        protected virtual void HandlerChidlFormClosed(Control sender)
        {
            // cannot delete here since we got here from sender.Update which was called by the loop in this.Update
            // modifying controls here might twho an exception
            controlsToDelete.Add(sender);
        }

        /// <summary>
        /// Removes focus from children only if the form was focused and the user clicked it again
        /// </summary>
        /// <param name="sender">Form that triggered the event</param>
        protected virtual void HandlerMeClicked(Control sender)
        {
            if (!this.gotFocus) {
                foreach (Control c in controls)
                    if (!c.Clicked) c.Focus = false;
            }

        }

        /// <summary>
        /// Removes check from radio buttons except for the one which sent the request
        /// </summary>
        /// <param name="sender"></param>
        protected virtual void HandlerRadioButtonClicked(Control sender)
        {
            foreach (Control c in controls) {
                if (c is RadioButton) {
                    RadioButton radio = (RadioButton)c;
                    if (radio.CheckedThisFrame) // Incase of "CheckBox.Checked = true;" was used
                    {
                        radio.CheckedThisFrame = false;

                        foreach (Control r in controls) {
                            if (r is RadioButton && c != r)
                                ((RadioButton)r).Checked = false;
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a control to the form
        /// Control position will become relative to the form
        /// Control viewport will be changed to match the viewport of the form
        /// </summary>
        /// <param name="control">Control to add</param>
        public virtual void AddControl(Control control)
        {
            if (control == null) return;

            control.Viewport = viewport;

            // If control is a from then register onClose event
            if (control is Form) ((Form)control).onClose += new EHandler(HandlerChidlFormClosed);

            // If RadioButton.Checked = true was called we have to uncheck the other RadioButtons
            if (control is RadioButton) ((RadioButton)control).onToggle += new EHandler(HandlerRadioButtonClicked);

            //control.PositionWidthHeight = new Rectangle(control.PositionWidthHeight.X + this.positionWidthHeight.X, control.PositionWidthHeight.Y + this.positionWidthHeight.Y, control.PositionWidthHeight.Width, control.PositionWidthHeight.Height);
            control.Position += this.Position;
            control.parent = this;
            controls.Add(control);
        }

        /// <summary>
        /// Closes this form, must have a parent to work
        /// </summary>
        public virtual void Close(Control sender)
        {
            if (onClose != null) onClose(this);
        }

        /// <summary>
        /// Enables\Disables the brothers of a control..this control is not affected
        /// </summary>
        /// <param name="child">Control to maintain state</param>
        /// <param name="enabled">The state to set for the brothers</param>
        public virtual void ToggleBrothersOfChildEnabled(Control child, bool enabled)
        {
            foreach (Control c in controls) {
                if (c != child) c.Enabled = enabled;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns a list of the controls contained by this form
        /// </summary>
        public virtual List<Control> Controls
        {
            get
            {
                return controls;
            }
        }

        /// <summary>
        /// Gets, sets the position of the form...When set, child controls will be moved automatically
        /// </summary>
        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                // Changing position should affect children to

                Vector2 diff = value - Position;

                base.Position = value;

                foreach (Control c in controls)
                    c.Position += diff;
            }
        }

        /// <summary>
        /// Gets, sets the view port of the form.. When set, viewports of the child controls will be changed to.
        /// </summary>
        public override Viewport? Viewport
        {
            get
            {
                return base.Viewport;
            }
            set
            {
                base.Viewport = value;

                // Change viewports for child controls
                foreach (Control c in controls)
                    c.Viewport = value;
            }
        }

        #endregion
    }
}
