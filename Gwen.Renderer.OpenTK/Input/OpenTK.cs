using System;
using Gwen.Control;
using Gwen.Input;
using OpenTK.Input;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;

namespace Gwen.Renderer.OpenTK.Input
{
    public class OpenTK
    {
        #region Properties

        private Canvas m_Canvas = null;

        private int m_MouseX = 0;
        private int m_MouseY = 0;

        bool m_AltGr = false;

        #endregion

        #region Constructors
        public OpenTK(GameWindow window)
        {
            window.KeyDown += KeyDown;
            window.KeyUp += KeyUp;
        }

        private void KeyDown(KeyboardKeyEventArgs obj)
        {
            throw new NotImplementedException();
        }

        private void KeyUp(KeyboardKeyEventArgs obj)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods
        public void Initialize(Canvas c)
        {
            m_Canvas = c;
        }

        /// <summary>
        /// Translates control key's OpenTK key code to GWEN's code.
        /// </summary>
        /// <param name="key">OpenTK key code.</param>
        /// <returns>GWEN key code.</returns>
        private Key TranslateKeyCode(Keys key)
        {
            switch (key)
            {
                case Keys.Backspace: return Key.Backspace;
                case Keys.Enter: return Key.Return;
                case Keys.KeyPadEnter: return Key.Return;
                case Keys.Escape: return Key.Escape;
                case Keys.Tab: return Key.Tab;
                case Keys.Space: return Key.Space;
                case Keys.Up: return Key.Up;
                case Keys.Down: return Key.Down;
                case Keys.Left: return Key.Left;
                case Keys.Right: return Key.Right;
                case Keys.Home: return Key.Home;
                case Keys.End: return Key.End;
                case Keys.Delete: return Key.Delete;
                case Keys.LeftControl:
                    this.m_AltGr = true;
                    return Key.Control;
                case Keys.LeftAlt: return Key.Alt;
                case Keys.LeftShift: return Key.Shift;
                case Keys.RightControl: return Key.Control;
                case Keys.RightAlt:
                    if (this.m_AltGr)
                    {
                        this.m_Canvas.Input_Key(Key.Control, false);
                    }
                    return Key.Alt;
                case Keys.RightShift: return Key.Shift;

            }
            return Key.Invalid;
        }

        /// <summary>
        /// Translates alphanumeric OpenTK key code to character value.
        /// </summary>
        /// <param name="key">OpenTK key code.</param>
        /// <returns>Translated character.</returns>
        private static char TranslateChar(Keys key)
        {
            if (key >= Keys.A && key <= Keys.Z)
                return (char)('a' + ((int)key - (int)Keys.A));
            return ' ';
        }

       

        public bool ProcessMouseWheel(MouseWheelEventArgs ev)
        {
            return m_Canvas.Input_MouseWheel((int)ev.OffsetY * 60);
        }

        public bool ProcessMouseMove(MouseMoveEventArgs ev)
        {
            m_MouseX = (int)ev.X;
            m_MouseY = (int)ev.Y;
            //we have deltaX now for that
            return m_Canvas.Input_MouseMoved(m_MouseX, m_MouseY, (int)ev.DeltaX, (int)ev.DeltaY);
        }

        public bool ProcessMouseUp(MouseButtonEventArgs ev)
        {
             /* We can not simply cast ev.Button to an int, as 1 is middle click, not right click. */
                int ButtonID = -1; //Do not trigger event.

                if (ev.Button == MouseButton.Left)
                    ButtonID = 0;
                else if (ev.Button == MouseButton.Right)
                    ButtonID = 1;

                if (ButtonID != -1) //We only care about left and right click for now
                    return m_Canvas.Input_MouseButton(ButtonID, false);
                return false;
        }

        public bool ProcessMouseDown(MouseButtonEventArgs ev)
        {
            /* We can not simply cast ev.Button to an int, as 1 is middle click, not right click. */
                int ButtonID = -1; //Do not trigger event.

                if (ev.Button == MouseButton.Left)
                    ButtonID = 0;
                else if (ev.Button == MouseButton.Right)
                    ButtonID = 1;

                if (ButtonID != -1) //We only care about left and right click for now
                    return m_Canvas.Input_MouseButton(ButtonID, true);
                return false;
        }

        public bool ProcessKeyDown(KeyboardKeyEventArgs ev)
        {
            char ch = TranslateChar(ev.Key);

            if (InputHandler.DoSpecialKeys(m_Canvas, ch))
                return false;
            /*
            if (ch != ' ')
            {
                m_Canvas.Input_Character(ch);
            }
            */
            Key iKey = TranslateKeyCode(ev.Key);

            return m_Canvas.Input_Key(iKey, true);
        }

        public bool ProcessKeyUp(KeyboardKeyEventArgs ev)
        {

            Key iKey = TranslateKeyCode(ev.Key);

            return m_Canvas.Input_Key(iKey, false);
        }


        #endregion
    }
}
