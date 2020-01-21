using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Ryujinx.Configuration;
using Ryujinx.Graphics.OpenGL;
using Ryujinx.HLE;
using Ryujinx.HLE.Input;
using Ryujinx.Profiler.UI;
using Ryujinx.Ui;
using System;
using System.Threading;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Ryujinx.Ui
{
    public class GlScreen : GameWindow
    {
        private const int TouchScreenWidth  = 1280;
        private const int TouchScreenHeight = 720;

        private const int TargetFps = 60;

        private Switch _device;

        private Renderer _renderer;

        private HotkeyButtons _prevHotkeyButtons = 0;

        private KeyboardState? _keyboard = null;

        private MouseState? _mouse = null;

        private Input.NpadController _primaryController;

        private Thread _renderThread;

        private bool _resizeEvent;

        private bool _titleEvent;

        private string _newTitle;

#if USE_PROFILING
        private ProfileWindowManager _profileWindow;
#endif

        public GlScreen(Switch device)
            : base(1280, 720,
            new GraphicsMode(), "Ryujinx", 0,
            DisplayDevice.Default, 3, 3,
            GraphicsContextFlags.ForwardCompatible)
        {
            _device = device;

            if (!(device.Gpu.Renderer is Renderer))
            {
                throw new NotSupportedException($"GPU renderer must be an OpenGL renderer when using GlScreen!");
            }

            _renderer = (Renderer)device.Gpu.Renderer;

            _primaryController = new Input.NpadController(ConfigurationState.Instance.Hid.JoystickControls);

            Location = new Point(
                (DisplayDevice.Default.Width  / 2) - (Width  / 2),
                (DisplayDevice.Default.Height / 2) - (Height / 2));

#if USE_PROFILING
            // Start profile window, it will handle itself from there
            _profileWindow = new ProfileWindowManager();
#endif
        }

        private void RenderLoop()
        {
            MakeCurrent();

            _renderer.Initialize();

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

                if (_resizeEvent)
                {
                    _resizeEvent = false;

                    _renderer.Window.SetSize(Width, Height);
                }

                ticks += chrono.ElapsedTicks;

                chrono.Restart();

                if (ticks >= ticksPerFrame)
                {
                    RenderFrame();

                    // Queue max. 1 vsync
                    ticks = Math.Min(ticks - ticksPerFrame, ticksPerFrame);
                }
            }

            _device.DisposeGpu();
        }

        public void MainLoop()
        {
            VSync = VSyncMode.Off;

            Visible = true;

            Context.MakeCurrent(null);

            // OpenTK doesn't like sleeps in its thread, to avoid this a renderer thread is created
            _renderThread = new Thread(RenderLoop)
            {
                Name = "GUI.RenderThread"
            };

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

                // Polling becomes expensive if it's not slept
                Thread.Sleep(1);
            }
        }

        private new void UpdateFrame()
        {
            HotkeyButtons       currentHotkeyButtons = 0;
            ControllerButtons   currentButton = 0;
            JoystickPosition    leftJoystick;
            JoystickPosition    rightJoystick;
            HLE.Input.Keyboard? hidKeyboard = null;

            int leftJoystickDx  = 0;
            int leftJoystickDy  = 0;
            int rightJoystickDx = 0;
            int rightJoystickDy = 0;

            // Keyboard Input
            if (_keyboard.HasValue)
            {
                KeyboardState keyboard = _keyboard.Value;

#if USE_PROFILING
                // Profiler input, lets the profiler get access to the main windows keyboard state
                _profileWindow.UpdateKeyInput(keyboard);
#endif

                // Normal Input
                currentHotkeyButtons = KeyboardControls.GetHotkeyButtons(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);
                currentButton        = KeyboardControls.GetButtons(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);

                if (ConfigurationState.Instance.Hid.EnableKeyboard)
                {
                    hidKeyboard = KeyboardControls.GetKeysDown(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);
                }

                (leftJoystickDx, leftJoystickDy)   = KeyboardControls.GetLeftStick(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);
                (rightJoystickDx, rightJoystickDy) = KeyboardControls.GetRightStick(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);
            }

            if (!hidKeyboard.HasValue)
            {
                hidKeyboard = new HLE.Input.Keyboard
                {
                    Modifier = 0,
                    Keys     = new int[0x8]
                };
            }

            currentButton |= _primaryController.GetButtons();

            // Keyboard has priority stick-wise
            if (leftJoystickDx == 0 && leftJoystickDy == 0)
            {
                (leftJoystickDx, leftJoystickDy) = _primaryController.GetLeftStick();
            }

            if (rightJoystickDx == 0 && rightJoystickDy == 0)
            {
                (rightJoystickDx, rightJoystickDy) = _primaryController.GetRightStick();
            }

            leftJoystick = new JoystickPosition
            {
                Dx = leftJoystickDx,
                Dy = leftJoystickDy
            };

            rightJoystick = new JoystickPosition
            {
                Dx = rightJoystickDx,
                Dy = rightJoystickDy
            };

            currentButton |= _device.Hid.UpdateStickButtons(leftJoystick, rightJoystick);

            bool hasTouch = false;

            // Get screen touch position from left mouse click
            // OpenTK always captures mouse events, even if out of focus, so check if window is focused.
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

                    TouchPoint currentPoint = new TouchPoint
                    {
                        X = mX,
                        Y = mY,

                        // Placeholder values till more data is acquired
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

            if (ConfigurationState.Instance.Hid.EnableKeyboard && hidKeyboard.HasValue)
            {
                _device.Hid.WriteKeyboard(hidKeyboard.Value);
            }

            BaseController controller = _device.Hid.PrimaryController;

            controller.SendInput(currentButton, leftJoystick, rightJoystick);

            // Toggle vsync
            if (currentHotkeyButtons.HasFlag(HotkeyButtons.ToggleVSync) &&
                !_prevHotkeyButtons.HasFlag(HotkeyButtons.ToggleVSync))
            {
                _device.EnableDeviceVsync = !_device.EnableDeviceVsync;
            }

            _prevHotkeyButtons = currentHotkeyButtons;
        }

        private new void RenderFrame()
        {
            _device.PresentFrame(SwapBuffers);

            _device.Statistics.RecordSystemFrameTime();

            double hostFps = _device.Statistics.GetSystemFrameRate();
            double gameFps = _device.Statistics.GetGameFrameRate();

            string titleNameSection = string.IsNullOrWhiteSpace(_device.System.TitleName) ? string.Empty
                : " | " + _device.System.TitleName;

            string titleIdSection = string.IsNullOrWhiteSpace(_device.System.TitleIdText) ? string.Empty
                : " | " + _device.System.TitleIdText.ToUpper();

            _newTitle = $"Ryujinx{titleNameSection}{titleIdSection} | Host FPS: {hostFps:0.0} | Game FPS: {gameFps:0.0} | " +
                $"Game Vsync: {(_device.EnableDeviceVsync ? "On" : "Off")}";

            _titleEvent = true;

            _device.System.SignalVsync();

            _device.VsyncEvent.Set();
        }

        protected override void OnUnload(EventArgs e)
        {
#if USE_PROFILING
            _profileWindow.Close();
#endif

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
