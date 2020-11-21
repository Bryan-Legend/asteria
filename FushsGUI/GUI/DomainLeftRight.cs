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
    /// A one line list with two buttons for items selection supports events : onClick, onChange, onMouseDown, onMouseMove, onMouseEnter, onMouseLeave
    /// </summary>
    public class DomainLeftRight : Form
    {
        #region Fields

        protected Button btnLeft;
        protected Button btnRight;
        protected TextBox txtText;

        protected List<Object> items = new List<Object>();
        protected int index = 0;

        public EHandler onChange;

        Object prevSelectedObject = null;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new DomainLeftRight
        /// </summary>
        /// <param name="name"></param>
        /// <param name="PositionWidthHeight">A rectangle that specifies the position, width , height relative to the control parent</param>
        /// <param name="texture">Texture for the TextBox in the middle</param>
        /// <param name="leftBtnTex">Texture for the left button</param>
        /// <param name="rightBtnTex">Texture for the right button</param>
        /// <param name="font">Font used for displaying text, pass null if there's no text</param>
        /// <param name="foreColor">Color of the displayed text</param>
        /// <param name="viewport">Optional : Viewport used to render the gui, if your game contains only one viewport pass null or don't pass anything at all.</param>
        public DomainLeftRight(String name, Rectangle PositionWidthHeight, Texture2D texture, Texture2D leftBtnTex, Texture2D rightBtnTex, SpriteFont font, Color foreColor, Viewport? viewport = null)
            : base(name, "", PositionWidthHeight, texture, font, foreColor, viewport)
        {
            btnLeft = new Button("btnLeft", "", new Rectangle(0, 0, 10, positionWidthHeight.Height), leftBtnTex, font, foreColor, viewport);
            btnRight = new Button("btnRight", "", new Rectangle(positionWidthHeight.Width - 10, 0, 10, positionWidthHeight.Height), rightBtnTex, font, foreColor, viewport);
            txtText = new TextBox("txtText", "", 0, new Rectangle(10, 0, positionWidthHeight.Width - 20, positionWidthHeight.Height), null, font, foreColor, viewport);
            txtText.ReadOnly = true;

            this.AddControl(btnLeft);
            this.AddControl(btnRight);
            this.AddControl(txtText);

            btnLeft.onClick += new EHandler(BtnLeft_Click);
            btnRight.onClick += new EHandler(BtnRight_Click);

            # region Child event should trigger parent event

            btnLeft.onClick += new EHandler(ChildClicked);
            btnRight.onClick += new EHandler(ChildClicked);
            txtText.onClick += new EHandler(ChildClicked);

            btnLeft.onMouseDown += new EHandler(ChildMouseDown);
            btnRight.onMouseDown += new EHandler(ChildMouseDown);
            txtText.onMouseDown += new EHandler(ChildMouseDown);

            btnLeft.onMouseMove += new EHandler(ChildMouseMove);
            btnRight.onMouseMove += new EHandler(ChildMouseMove);
            txtText.onMouseMove += new EHandler(ChildMouseMove);

            #endregion
        }

        #endregion

        #region Handlers

        protected virtual void ChildClicked(Control sender)
        {
            if (onClick != null) onClick(this);
        }

        protected virtual void ChildMouseDown(Control sender)
        {
            if (onMouseDown != null) onMouseDown(this);
        }

        protected virtual void ChildMouseMove(Control sender)
        {
            if (onMouseMove != null) onMouseMove(this);
        }

        protected virtual void BtnLeft_Click(Control sender)
        {
            index = (int)MathHelper.Clamp(index - 1, 0, items.Count - 1);

            if (items.Count > 0) txtText.Text = items[index].ToString();

            CheckChanges();
        }

        protected virtual void BtnRight_Click(Control sender)
        {
            index = (int)MathHelper.Clamp(index + 1, 0, items.Count - 1);

            if (items.Count > 0) txtText.Text = items[index].ToString();

            CheckChanges();
        }

        #endregion

        #region Misc. Functions

        protected void CheckChanges()
        {
            if (onChange != null) {
                Object selectedObject = SelectedObject;
                if (prevSelectedObject != selectedObject) {
                    prevSelectedObject = selectedObject;
                    onChange(this);
                }
            }
        }

        /// <summary>
        /// Adds a new item to the list
        /// </summary>
        /// <param name="item">item to add</param>
        public virtual void AddItem(Object item)
        {
            items.Add(item);

            if (items.Count > 0) txtText.Text = items[index].ToString();

            CheckChanges();
        }

        /// <summary>
        /// Removes an existing item from the list
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>Returns true on success, false otherwise</returns>
        public virtual bool Remove(Object item)
        {
            bool result = items.Remove(item);
            index = (int)MathHelper.Clamp(index + 1, 0, items.Count - 1);

            CheckChanges();

            return result;
        }

        /// <summary>
        /// Clears the list
        /// </summary>
        public virtual void Clear()
        {
            items.Clear();
            index = 0;

            txtText.Text = "";

            CheckChanges();
        }

        /// <summary>
        /// Simulates a click on the right button of the control
        /// </summary>
        public virtual void Next()
        {
            BtnRight_Click(null);
        }

        /// <summary>
        /// Simulates a click on the left button of the control
        /// </summary>
        public virtual void Previous()
        {
            BtnLeft_Click(null);
        }

        /// <summary>
        /// Makes the given object the selected one (if exists)
        /// </summary>
        /// <param name="item">Item to select</param>
        /// <returns>Returns true if selection was successful</returns>
        public virtual bool Select(object item)
        {
            int i = items.IndexOf(item);
            if (i >= 0) {
                index = i;
                txtText.Text = items[index].ToString();

                return true;
            } else {
                return false;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the selected object or null if list is empty
        /// </summary>
        public Object SelectedObject
        {
            get
            {
                if (items.Count <= 0) {
                    return null;
                } else {
                    index = (int)MathHelper.Clamp(index, 0, items.Count - 1);
                    return items[index];
                }

            }
        }

        #endregion
    }


}
