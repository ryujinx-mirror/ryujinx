using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Ryujinx.Graphics.Gal;
using Ryujinx.HLE;
using Ryujinx.HLE.Input;
using Ryujinx.UI.Input;
using System;
using System.Threading;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Ryujinx
{
    public class GLScreen : GameWindow
    {
        private const int TouchScreenWidth  = 1280;
        private const int TouchScreenHeight = 720;

        private const int TargetFPS = 60;

        private Switch Ns;

        private IGalRenderer Renderer;

        private KeyboardState? Keyboard = null;

        private MouseState? Mouse = null;

        private Thread RenderThread;

        private bool ResizeEvent;

        private bool TitleEvent;

        private string NewTitle;

        public GLScreen(Switch Ns, IGalRenderer Renderer)
            : base(1280, 720,
            new GraphicsMode(), "Ryujinx", 0,
            DisplayDevice.Default, 3, 3,
            GraphicsContextFlags.ForwardCompatible)
        {
            this.Ns       = Ns;
            this.Renderer = Renderer;

            Location = new Point(
                (DisplayDevice.Default.Width  / 2) - (Width  / 2),
                (DisplayDevice.Default.Height / 2) - (Height / 2));
        }

        private void RenderLoop()
        {
            MakeCurrent();

            Stopwatch Chrono = new Stopwatch();

            Chrono.Start();

            long TicksPerFrame = Stopwatch.Frequency / TargetFPS;

            long Ticks = 0;

            while (Exists && !IsExiting)
            {
                if (Ns.WaitFifo())
                {
                    Ns.ProcessFrame();
                }

                Renderer.RunActions();

                if (ResizeEvent)
                {
                    ResizeEvent = false;

                    Renderer.FrameBuffer.SetWindowSize(Width, Height);
                }

                Ticks += Chrono.ElapsedTicks;

                Chrono.Restart();

                if (Ticks >= TicksPerFrame)
                {
                    RenderFrame();

                    //Queue max. 1 vsync
                    Ticks = Math.Min(Ticks - TicksPerFrame, TicksPerFrame);
                }
            }
        }

        public void MainLoop()
        {
            VSync = VSyncMode.Off;

            Visible = true;

            Renderer.FrameBuffer.SetWindowSize(Width, Height);

            Context.MakeCurrent(null);

            //OpenTK doesn't like sleeps in its thread, to avoid this a renderer thread is created
            RenderThread = new Thread(RenderLoop);

            RenderThread.Start();

            while (Exists && !IsExiting)
            {
                ProcessEvents();

                if (!IsExiting)
                {
                    UpdateFrame();

                    if (TitleEvent)
                    {
                        TitleEvent = false;

                        Title = NewTitle;
                    }
                }

                //Polling becomes expensive if it's not slept
                Thread.Sleep(1);
            }
        }

        private new void UpdateFrame()
        {
            HidControllerButtons CurrentButton = 0;
            HidJoystickPosition  LeftJoystick;
            HidJoystickPosition  RightJoystick;

            int LeftJoystickDX  = 0;
            int LeftJoystickDY  = 0;
            int RightJoystickDX = 0;
            int RightJoystickDY = 0;

            //Keyboard Input
            if (Keyboard.HasValue)
            {
                KeyboardState Keyboard = this.Keyboard.Value;

                CurrentButton = Config.JoyConKeyboard.GetButtons(Keyboard);

                (LeftJoystickDX, LeftJoystickDY) = Config.JoyConKeyboard.GetLeftStick(Keyboard);

                (RightJoystickDX, RightJoystickDY) = Config.JoyConKeyboard.GetRightStick(Keyboard);
            }

            //Controller Input
            CurrentButton |= Config.JoyConController.GetButtons();
                
            //Keyboard has priority stick-wise
            if (LeftJoystickDX == 0 && LeftJoystickDY == 0)
            {
                (LeftJoystickDX, LeftJoystickDY) = Config.JoyConController.GetLeftStick();
            }

            if (RightJoystickDX == 0 && RightJoystickDY == 0)
            {
                (RightJoystickDX, RightJoystickDY) = Config.JoyConController.GetRightStick();
            }
            
            LeftJoystick = new HidJoystickPosition
            {
                DX = LeftJoystickDX,
                DY = LeftJoystickDY
            };

            RightJoystick = new HidJoystickPosition
            {
                DX = RightJoystickDX,
                DY = RightJoystickDY
            };

            bool HasTouch = false;

            //Get screen touch position from left mouse click
            //OpenTK always captures mouse events, even if out of focus, so check if window is focused.
            if (Focused && Mouse?.LeftButton == ButtonState.Pressed)
            {
                MouseState Mouse = this.Mouse.Value;

                int ScrnWidth  = Width;
                int ScrnHeight = Height;

                if (Width > (Height * TouchScreenWidth) / TouchScreenHeight)
                {
                    ScrnWidth = (Height * TouchScreenWidth) / TouchScreenHeight;
                }
                else
                {
                    ScrnHeight = (Width * TouchScreenHeight) / TouchScreenWidth;
                }

                int StartX = (Width  - ScrnWidth)  >> 1;
                int StartY = (Height - ScrnHeight) >> 1;

                int EndX = StartX + ScrnWidth;
                int EndY = StartY + ScrnHeight;

                if (Mouse.X >= StartX &&
                    Mouse.Y >= StartY &&
                    Mouse.X <  EndX   &&
                    Mouse.Y <  EndY)
                {
                    int ScrnMouseX = Mouse.X - StartX;
                    int ScrnMouseY = Mouse.Y - StartY;

                    int MX = (ScrnMouseX * TouchScreenWidth)  / ScrnWidth;
                    int MY = (ScrnMouseY * TouchScreenHeight) / ScrnHeight;

                    HidTouchPoint CurrentPoint = new HidTouchPoint
                    {
                        X = MX,
                        Y = MY,

                        //Placeholder values till more data is acquired
                        DiameterX = 10,
                        DiameterY = 10,
                        Angle     = 90
                    };

                    HasTouch = true;

                    Ns.Hid.SetTouchPoints(CurrentPoint);
                }
            }

            if (!HasTouch)
            {
                Ns.Hid.SetTouchPoints();
            }

            Ns.Hid.SetJoyconButton(
                HidControllerId.CONTROLLER_HANDHELD,
                HidControllerLayouts.Handheld_Joined,
                CurrentButton,
                LeftJoystick,
                RightJoystick);

            Ns.Hid.SetJoyconButton(
                HidControllerId.CONTROLLER_HANDHELD,
                HidControllerLayouts.Main,
                CurrentButton,
                LeftJoystick,
                RightJoystick);
        }

        private new void RenderFrame()
        {
            Renderer.FrameBuffer.Render();

            Ns.Statistics.RecordSystemFrameTime();

            double HostFps = Ns.Statistics.GetSystemFrameRate();
            double GameFps = Ns.Statistics.GetGameFrameRate();

            NewTitle = $"Ryujinx | Host FPS: {HostFps:0.0} | Game FPS: {GameFps:0.0}";

            TitleEvent = true;

            SwapBuffers();

            Ns.Os.SignalVsync();
        }

        protected override void OnUnload(EventArgs e)
        {
            RenderThread.Join();

            base.OnUnload(e);
        }

        protected override void OnResize(EventArgs e)
        {
            ResizeEvent = true;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            bool ToggleFullscreen = e.Key == Key.F11 ||
                (e.Modifiers.HasFlag(KeyModifiers.Alt) && e.Key == Key.Enter);

            if (WindowState == WindowState.Fullscreen)
            {
                if (e.Key == Key.Escape || ToggleFullscreen)
                {
                    WindowState = WindowState.Normal;
                }
            }
            else
            {
                if (e.Key == Key.Escape)
                {
                    Exit();
                }

                if (ToggleFullscreen)
                {
                    WindowState = WindowState.Fullscreen;
                }
            }

            Keyboard = e.Keyboard;
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Keyboard = e.Keyboard;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Mouse = e.Mouse;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            Mouse = e.Mouse;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            Mouse = e.Mouse;
        }
    }
}