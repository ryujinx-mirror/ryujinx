using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Ryujinx.Graphics.Gal;
using Ryujinx.HLE;
using Ryujinx.HLE.Input;
using System;
using System.Threading;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Ryujinx
{
    public class GlScreen : GameWindow
    {
        private const int TouchScreenWidth  = 1280;
        private const int TouchScreenHeight = 720;

        private const int TargetFps = 60;

        private Switch _device;

        private IGalRenderer _renderer;

        private KeyboardState? _keyboard = null;

        private MouseState? _mouse = null;

        private Thread _renderThread;

        private bool _resizeEvent;

        private bool _titleEvent;

        private string _newTitle;

        public GlScreen(Switch device, IGalRenderer renderer)
            : base(1280, 720,
            new GraphicsMode(), "Ryujinx", 0,
            DisplayDevice.Default, 3, 3,
            GraphicsContextFlags.ForwardCompatible)
        {
            _device   = device;
            _renderer = renderer;

            Location = new Point(
                (DisplayDevice.Default.Width  / 2) - (Width  / 2),
                (DisplayDevice.Default.Height / 2) - (Height / 2));
        }

        private void RenderLoop()
        {
            MakeCurrent();

            Stopwatch chrono = new Stopwatch();

            chrono.Start();

            long ticksPerFrame = Stopwatch.Frequency / TargetFps;

            long ticks = 0;

            while (Exists && !IsExiting)
            {
                if (_device.WaitFifo())
                {
                    _device.ProcessFrame();
                }

                _renderer.RunActions();

                if (_resizeEvent)
                {
                    _resizeEvent = false;

                    _renderer.RenderTarget.SetWindowSize(Width, Height);
                }

                ticks += chrono.ElapsedTicks;

                chrono.Restart();

                if (ticks >= ticksPerFrame)
                {
                    RenderFrame();

                    //Queue max. 1 vsync
                    ticks = Math.Min(ticks - ticksPerFrame, ticksPerFrame);
                }
            }
        }

        public void MainLoop()
        {
            VSync = VSyncMode.Off;

            Visible = true;

            _renderer.RenderTarget.SetWindowSize(Width, Height);

            Context.MakeCurrent(null);

            //OpenTK doesn't like sleeps in its thread, to avoid this a renderer thread is created
            _renderThread = new Thread(RenderLoop);

            _renderThread.Start();

            while (Exists && !IsExiting)
            {
                ProcessEvents();

                if (!IsExiting)
                {
                    UpdateFrame();

                    if (_titleEvent)
                    {
                        _titleEvent = false;

                        Title = _newTitle;
                    }
                }

                //Polling becomes expensive if it's not slept
                Thread.Sleep(1);
            }
        }

        private new void UpdateFrame()
        {
            HidControllerButtons currentButton = 0;
            HidJoystickPosition  leftJoystick;
            HidJoystickPosition  rightJoystick;

            int leftJoystickDx  = 0;
            int leftJoystickDy  = 0;
            int rightJoystickDx = 0;
            int rightJoystickDy = 0;

            //Keyboard Input
            if (_keyboard.HasValue)
            {
                KeyboardState keyboard = _keyboard.Value;

                currentButton = Configuration.Instance.KeyboardControls.GetButtons(keyboard);

                (leftJoystickDx, leftJoystickDy) = Configuration.Instance.KeyboardControls.GetLeftStick(keyboard);

                (rightJoystickDx, rightJoystickDy) = Configuration.Instance.KeyboardControls.GetRightStick(keyboard);
            }
            
            currentButton |= Configuration.Instance.GamepadControls.GetButtons();

            //Keyboard has priority stick-wise
            if (leftJoystickDx == 0 && leftJoystickDy == 0)
            {
                (leftJoystickDx, leftJoystickDy) = Configuration.Instance.GamepadControls.GetLeftStick();
            }

            if (rightJoystickDx == 0 && rightJoystickDy == 0)
            {
                (rightJoystickDx, rightJoystickDy) = Configuration.Instance.GamepadControls.GetRightStick();
            }

            leftJoystick = new HidJoystickPosition
            {
                Dx = leftJoystickDx,
                Dy = leftJoystickDy
            };

            rightJoystick = new HidJoystickPosition
            {
                Dx = rightJoystickDx,
                Dy = rightJoystickDy
            };

            currentButton |= _device.Hid.UpdateStickButtons(leftJoystick, rightJoystick);

            bool hasTouch = false;

            //Get screen touch position from left mouse click
            //OpenTK always captures mouse events, even if out of focus, so check if window is focused.
            if (Focused && _mouse?.LeftButton == ButtonState.Pressed)
            {
                MouseState mouse = _mouse.Value;

                int scrnWidth  = Width;
                int scrnHeight = Height;

                if (Width > (Height * TouchScreenWidth) / TouchScreenHeight)
                {
                    scrnWidth = (Height * TouchScreenWidth) / TouchScreenHeight;
                }
                else
                {
                    scrnHeight = (Width * TouchScreenHeight) / TouchScreenWidth;
                }

                int startX = (Width  - scrnWidth)  >> 1;
                int startY = (Height - scrnHeight) >> 1;

                int endX = startX + scrnWidth;
                int endY = startY + scrnHeight;

                if (mouse.X >= startX &&
                    mouse.Y >= startY &&
                    mouse.X <  endX   &&
                    mouse.Y <  endY)
                {
                    int scrnMouseX = mouse.X - startX;
                    int scrnMouseY = mouse.Y - startY;

                    int mX = (scrnMouseX * TouchScreenWidth)  / scrnWidth;
                    int mY = (scrnMouseY * TouchScreenHeight) / scrnHeight;

                    HidTouchPoint currentPoint = new HidTouchPoint
                    {
                        X = mX,
                        Y = mY,

                        //Placeholder values till more data is acquired
                        DiameterX = 10,
                        DiameterY = 10,
                        Angle     = 90
                    };

                    hasTouch = true;

                    _device.Hid.SetTouchPoints(currentPoint);
                }
            }

            if (!hasTouch)
            {
                _device.Hid.SetTouchPoints();
            }

            HidControllerBase controller = _device.Hid.PrimaryController;

            controller.SendInput(currentButton, leftJoystick, rightJoystick);
        }

        private new void RenderFrame()
        {
            _renderer.RenderTarget.Render();

            _device.Statistics.RecordSystemFrameTime();

            double hostFps = _device.Statistics.GetSystemFrameRate();
            double gameFps = _device.Statistics.GetGameFrameRate();

            string titleSection = string.IsNullOrWhiteSpace(_device.System.CurrentTitle) ? string.Empty
                : " | " + _device.System.CurrentTitle;

            _newTitle = $"Ryujinx{titleSection} | Host FPS: {hostFps:0.0} | Game FPS: {gameFps:0.0} | " +
                $"Game Vsync: {(_device.EnableDeviceVsync ? "On" : "Off")}";

            _titleEvent = true;

            SwapBuffers();

            _device.System.SignalVsync();

            _device.VsyncEvent.Set();
        }

        protected override void OnUnload(EventArgs e)
        {
            _renderThread.Join();

            base.OnUnload(e);
        }

        protected override void OnResize(EventArgs e)
        {
            _resizeEvent = true;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            bool toggleFullscreen = e.Key == Key.F11 ||
                (e.Modifiers.HasFlag(KeyModifiers.Alt) && e.Key == Key.Enter);

            if (WindowState == WindowState.Fullscreen)
            {
                if (e.Key == Key.Escape || toggleFullscreen)
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

                if (toggleFullscreen)
                {
                    WindowState = WindowState.Fullscreen;
                }
            }

            _keyboard = e.Keyboard;
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            _keyboard = e.Keyboard;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _mouse = e.Mouse;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            _mouse = e.Mouse;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            _mouse = e.Mouse;
        }
    }
}