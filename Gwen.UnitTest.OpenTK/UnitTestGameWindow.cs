﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mime;
using System.Text;
using System.Threading;
using Gwen.Control;
using Gwen.Renderer;
using GwenNet.Platform;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Gwen.UnitTest.OTK
{
    /// <summary>
    /// Demonstrates the GameWindow class.
    /// </summary>
    public class UnitTestGameWindow : GameWindow
    {

        private Renderer.OpenTK.Input.OpenTK m_Input;
        private OpenTKBase m_Renderer;
        private Skin.SkinBase m_Skin;
        private Canvas m_Canvas;
        private UnitTest m_UnitTest;
        private List<string> defaultFonts = new List<string>();
        private int _currentFont = 0;
        const int FpsFrames = 50;
        private readonly List<long> m_Ftime;
        private readonly Stopwatch m_Stopwatch;
        private long m_LastTime;
        private float m_TotalTime = 0f;

        private bool m_AltDown = false;

        public UnitTestGameWindow()
            : base(new GameWindowSettings(), new NativeWindowSettings())
        //: base(1024, 768)
        {
            KeyDown += Keyboard_KeyDown;
            KeyUp += Keyboard_KeyUp;
            MouseDown += Mouse_ButtonDown;
            MouseUp += Mouse_ButtonUp;
            MouseMove += Mouse_Move;
            MouseWheel += Mouse_Wheel;

            m_Ftime = new List<long>(FpsFrames);
            m_Stopwatch = new Stopwatch();
        }

        protected override void OnClosed()
        {
            if (m_Canvas != null)
            {
                m_Canvas.Dispose();
                m_Canvas = null;
            }
            if (m_Skin != null)
            {
                m_Skin.Dispose();
                m_Skin = null;
            }
            if (m_Renderer != null)
            {
                m_Renderer.Dispose();
                m_Renderer = null;
            }
            base.Dispose();
        }

        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        /// <param name="sender">The KeyboardDevice which generated this event.</param>
        /// <param name="e">The key that was pressed.</param>
        void Keyboard_KeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                Close();
            }
            else if (e.Key == Keys.LeftAlt)
            {
                m_AltDown = true;
            }
            else if (m_AltDown && e.Key == Keys.Enter)
            {
                if (WindowState == WindowState.Fullscreen)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowState = WindowState.Fullscreen;
                }
            }


            m_Input.ProcessKeyDown(e);
        }

        void Keyboard_KeyUp(KeyboardKeyEventArgs e)
        {
            m_AltDown = false;
            if (e.Key == Keys.F)
            {
                _currentFont = (_currentFont + 1) % defaultFonts.Count;
                //m_Skin.DefaultFont= new Font(m_Renderer, defaultFonts[_currentFont],12);
                m_Skin.SetDefaultFont(defaultFonts[_currentFont], 12);
                m_Canvas.Redraw();
            }
            else if (e.Key == Keys.C)
            {
                m_Skin.Colors.ModalBackground = new Color(200, 200, 200, 275);
            }
            m_Input.ProcessKeyUp(e);
        }

        void Mouse_ButtonDown(MouseButtonEventArgs args)
        {
            m_Input.ProcessMouseDown(args);
        }

        void Mouse_ButtonUp(MouseButtonEventArgs args)
        {
            m_Input.ProcessMouseUp(args);
        }

        void Mouse_Move(MouseMoveEventArgs args)
        {
            m_Input.ProcessMouseMove(args);
        }

        void Mouse_Wheel(MouseWheelEventArgs args)
        {
            m_Input.ProcessMouseWheel(args);
        }

        private void GetInstalledFontCollection()
        {

            System.Drawing.Text.InstalledFontCollection ifc = new System.Drawing.Text.InstalledFontCollection();
            System.Drawing.FontFamily[] ff = ifc.Families;

            foreach (System.Drawing.FontFamily family in ff)
            {
                string[] nameParts = family.Name.Split(' ');
                if (nameParts.Length > 2)
                    continue;
                if (family.IsStyleAvailable(System.Drawing.FontStyle.Regular))
                {
                    System.Drawing.Font f = new System.Drawing.Font(family.Name, 12);
                    defaultFonts.Add(f.Name);
                    f.Dispose();
                }
            }


        }

        /// <summary>
        /// Setup OpenGL and load resources here.
        /// </summary>
        protected override void OnLoad()
        {
            GL.ClearColor(Color4.MidnightBlue);

            Platform.Platform.Init(new NetCore());

            //m_Renderer = new Gwen.Renderer.OpenTK.OpenTKGL10();
            //m_Renderer = new Gwen.Renderer.OpenTK.OpenTKGL20();
            m_Renderer = new Gwen.Renderer.OpenTK.OpenTKGL40();

            m_Skin = new Gwen.Skin.TexturedBase(m_Renderer, "DefaultSkin2.png");
            m_Skin.Colors.TooltipText = new Color(175, 200, 200, 200);
            GetInstalledFontCollection();
            m_Skin.DefaultFont = new Font(m_Renderer, "MonoSpace", 12);
            m_Canvas = new Canvas(m_Skin);
            m_Input = new Renderer.OpenTK.Input.OpenTK(this);
            m_Input.Initialize(m_Canvas);

            m_Canvas.SetSize(Size.X, Size.Y);
            m_Canvas.ShouldDrawBackground = true;
            m_Canvas.BackgroundColor = m_Skin.Colors.ModalBackground;

            //if (Configuration.RunningOnMacOS)
            //    m_Canvas.Scale = 1.5f;

            m_UnitTest = new Gwen.UnitTest.UnitTest(m_Canvas);

            m_Stopwatch.Restart();
            m_LastTime = 0;
        }

        /// <summary>
        /// Respond to resize events here.
        /// </summary>
        /// <param name="e">Contains information on the new GameWindow size.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnResize(ResizeEventArgs e)
        {
            m_Renderer.Resize(e.Width, e.Height);

            m_Canvas.SetSize(e.Width, e.Height);
        }

        /// <summary>
        /// Add your game logic here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {

            m_UnitTest.Fps = base.RenderFrequency;
            /*m_TotalTime += (float)e.Time;
			if (m_Ftime.Count == FpsFrames)
				m_Ftime.RemoveAt(0);
                */
            /*m_Ftime.Add(m_Stopwatch.ElapsedMilliseconds - m_LastTime);
			m_LastTime = m_Stopwatch.ElapsedMilliseconds;
			*/

            /*if (m_Stopwatch.ElapsedMilliseconds > 1000)
			{*/
            //Debug.WriteLine (String.Format ("String Cache size: {0} Draw Calls: {1} Vertex Count: {2}", renderer.TextCacheSize, renderer.DrawCallCount, renderer.VertexCount));
            /*m_UnitTest.Note = String.Format("String Cache size: {0} Draw Calls: {1} Vertex Count: {2}", m_Renderer.TextCacheSize, m_Renderer.DrawCallCount, m_Renderer.VertexCount);*/
            //1000f * m_Ftime.Count / m_Ftime.Sum();
            /*
            m_Stopwatch.Restart();

            if (m_Renderer.TextCacheSize > 1000) // each cached string is an allocated texture, flush the cache once in a while in your real project
                m_Renderer.FlushTextCache();
        }*/
        }

        /// <summary>
        /// Add your game rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            m_Stopwatch.Start();
            m_Canvas.RenderCanvas();
            m_Stopwatch.Stop();
            int tts = (int)(1000 / 30 - m_Stopwatch.ElapsedMilliseconds); //30 - требуемое кол-во fps
            if (tts > 0)
                Thread.Sleep(tts);
            m_Stopwatch.Restart();
            SwapBuffers();
        }

        /// <summary>
        /// Entry point of this example.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            using (UnitTestGameWindow window = new UnitTestGameWindow())
            {
                window.Title = "Gwen.net OpenTK Unit Test";
                window.VSync = VSyncMode.Off; // to measure performance
                window.Run();
            }

        }
    }
}
