using Gdk;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;
using Ryujinx.Configuration;
using Ryujinx.Graphics.OpenGL;
using Ryujinx.HLE;
using Ryujinx.HLE.Input;
using Ryujinx.Ui;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Ryujinx.Ui
{
    public class GLRenderer : GLWidget
    {
        private const int TouchScreenWidth = 1280;
        private const int TouchScreenHeight = 720;
        private const int TargetFps = 60;

        public ManualResetEvent WaitEvent { get; set; }

        public bool IsActive   { get; set; }
        public bool IsStopped  { get; set; }
        public bool IsFocused  { get; set; }

        private double _mouseX;
        private double _mouseY;
        private bool _mousePressed;

        private bool _titleEvent;

        private bool _toggleFullscreen;

        private string _newTitle;

        private readonly long _ticksPerFrame;

        private long _ticks = 0;

        private System.Diagnostics.Stopwatch _chrono;

        private Switch _device;

        private Renderer _renderer;

        private HotkeyButtons _prevHotkeyButtons = 0;

        private Input.NpadController _primaryController;

        public GLRenderer(Switch device) 
            : base (new GraphicsMode(new ColorFormat(24)), 
            3, 3, 
            GraphicsContextFlags.ForwardCompatible)
        {
            WaitEvent = new ManualResetEvent(false);

            _device = device;

            this.Initialized += GLRenderer_Initialized;
            this.Destroyed += GLRenderer_Destroyed;

            Initialize();

            _chrono = new System.Diagnostics.Stopwatch();

            _ticksPerFrame = System.Diagnostics.Stopwatch.Frequency / TargetFps;

            _primaryController = new Input.NpadController(ConfigurationState.Instance.Hid.JoystickControls);

            AddEvents((int)(Gdk.EventMask.ButtonPressMask 
                          | Gdk.EventMask.ButtonReleaseMask 
                          | Gdk.EventMask.PointerMotionMask 
                          | Gdk.EventMask.KeyPressMask
                          | Gdk.EventMask.KeyReleaseMask));

            this.Shown += Renderer_Shown;
        }

        private void Parent_FocusOutEvent(object o, Gtk.FocusOutEventArgs args)
        {
            IsFocused = false;
        }

        private void Parent_FocusInEvent(object o, Gtk.FocusInEventArgs args)
        {
            IsFocused = true;
        }

        private void GLRenderer_Destroyed(object sender, EventArgs e)
        {
            Exit();

            this.Dispose();
        }

        protected void Renderer_Shown(object sender, EventArgs e)
        {
            IsFocused = this.ParentWindow.State.HasFlag(Gdk.WindowState.Focused);
        }

        public void HandleScreenState(KeyboardState keyboard)
        {
            bool toggleFullscreen = keyboard.IsKeyDown(OpenTK.Input.Key.F11) 
                               || ((keyboard.IsKeyDown(OpenTK.Input.Key.AltLeft) 
                               ||   keyboard.IsKeyDown(OpenTK.Input.Key.AltRight)) 
                               &&   keyboard.IsKeyDown(OpenTK.Input.Key.Enter));

            if (toggleFullscreen == _toggleFullscreen)
            {
                return;
            }

            _toggleFullscreen = toggleFullscreen;

            Gtk.Application.Invoke(delegate
            {
                if (this.ParentWindow.State.HasFlag(Gdk.WindowState.Fullscreen))
                {
                    if (keyboard.IsKeyDown(OpenTK.Input.Key.Escape) || _toggleFullscreen)
                    {
                        this.ParentWindow.Unfullscreen();
                        (this.Toplevel as MainWindow)?.ToggleExtraWidgets(true);
                    }
                }
                else
                {
                    if (keyboard.IsKeyDown(OpenTK.Input.Key.Escape))
                    {
                        Exit();
                    }

                    if (_toggleFullscreen)
                    {
                        this.ParentWindow.Fullscreen();
                        (this.Toplevel as MainWindow)?.ToggleExtraWidgets(false);
                    }
                }
            });
        }

        private void GLRenderer_Initialized(object sender, EventArgs e)
        {
            // Release the GL exclusivity that OpenTK gave us.
            GraphicsContext.MakeCurrent(null);

            WaitEvent.Set();
        }

        protected override bool OnConfigureEvent(EventConfigure evnt)
        {
            var result = base.OnConfigureEvent(evnt);

            _renderer.Window.SetSize(AllocatedWidth, AllocatedHeight);

            return result;
        }

        public void Start()
        {
            IsRenderHandler = true;

            _chrono.Restart();

            IsActive = true;

            Gtk.Window parent = this.Toplevel as Gtk.Window;

            parent.FocusInEvent += Parent_FocusInEvent;
            parent.FocusOutEvent += Parent_FocusOutEvent;

            Gtk.Application.Invoke(delegate
            {
                parent.Present();
            });

            Thread renderLoopThread = new Thread(Render)
            {
                Name = "GUI.RenderLoop"
            };
            renderLoopThread.Start();

            MainLoop();

            renderLoopThread.Join();

            Exit();
        }

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            _mouseX = evnt.X;
            _mouseY = evnt.Y;

            if (evnt.Button == 1)
            {
                _mousePressed = true;
            }

            return false;
        }

        protected override bool OnButtonReleaseEvent(EventButton evnt)
        {
            if (evnt.Button == 1)
            {
                _mousePressed = false;
            }

            return false;
        }

        protected override bool OnMotionNotifyEvent(EventMotion evnt)
        {
            if (evnt.Device.InputSource == InputSource.Mouse)
            {
                _mouseX = evnt.X;
                _mouseY = evnt.Y;
            }

            return false;
        }

        public void Exit()
        {
            if (IsStopped)
            {
                return;
            }

            IsStopped = true;
            IsActive = false;

            using (ScopedGlContext scopedGLContext = new ScopedGlContext(WindowInfo, GraphicsContext))
            {
                _device.DisposeGpu();
            }

            WaitEvent.Set();
        }

        public void Initialize()
        {
            if (!(_device.Gpu.Renderer is Renderer))
            {
                throw new NotSupportedException($"GPU renderer must be an OpenGL renderer when using GLRenderer!");
            }

            _renderer = (Renderer)_device.Gpu.Renderer;
        }

        public void Render()
        {
            using (ScopedGlContext scopedGLContext = new ScopedGlContext(WindowInfo, GraphicsContext))
            {
                _renderer.Initialize();

                SwapBuffers();
            }

            while (IsActive)
            {
                if (IsStopped)
                {
                    return;
                }

                using (ScopedGlContext scopedGLContext = new ScopedGlContext(WindowInfo, GraphicsContext))
                {
                    _ticks += _chrono.ElapsedTicks;

                    _chrono.Restart();

                    if (_device.WaitFifo())
                    {
                        _device.ProcessFrame();
                    }

                    if (_ticks >= _ticksPerFrame)
                    {
                        _device.PresentFrame(SwapBuffers);

                        _device.Statistics.RecordSystemFrameTime();

                        double hostFps = _device.Statistics.GetSystemFrameRate();
                        double gameFps = _device.Statistics.GetGameFrameRate();

                        string titleNameSection = string.IsNullOrWhiteSpace(_device.System.TitleName) ? string.Empty
                            : " | " + _device.System.TitleName;

                        string titleIdSection = string.IsNullOrWhiteSpace(_device.System.TitleIdText) ? string.Empty
                            : " | " + _device.System.TitleIdText.ToUpper();

                        _newTitle = $"Ryujinx {Program.Version}{titleNameSection}{titleIdSection} | Host FPS: {hostFps:0.0} | Game FPS: {gameFps:0.0} | " +
                            $"Game Vsync: {(_device.EnableDeviceVsync ? "On" : "Off")}";

                        _titleEvent = true;

                        _device.System.SignalVsync();

                        _device.VsyncEvent.Set();

                        _ticks = Math.Min(_ticks - _ticksPerFrame, _ticksPerFrame);
                    }
                }
            }
        }

        public void SwapBuffers()
        {
            OpenTK.Graphics.GraphicsContext.CurrentContext.SwapBuffers();
        }

        public void MainLoop()
        {
            while (IsActive)
            {
                if (_titleEvent)
                {
                    _titleEvent = false;

                    Gtk.Application.Invoke(delegate
                    {
                        this.ParentWindow.Title = _newTitle;
                    });
                }

                if (IsFocused)
                {
                    UpdateFrame();
                }

                // Polling becomes expensive if it's not slept
                Thread.Sleep(1);
            }
        }

        private bool UpdateFrame()
        {
            if (!IsActive)
            {
                return true;
            }

            if (IsStopped)
            {
                return false;
            }

            HotkeyButtons currentHotkeyButtons = 0;
            ControllerButtons currentButton = 0;
            JoystickPosition leftJoystick;
            JoystickPosition rightJoystick;
            HLE.Input.Keyboard? hidKeyboard = null;

            KeyboardState keyboard = OpenTK.Input.Keyboard.GetState();

            Gtk.Application.Invoke(delegate
            {
                HandleScreenState(keyboard);
            });

            int leftJoystickDx = 0;
            int leftJoystickDy = 0;
            int rightJoystickDx = 0;
            int rightJoystickDy = 0;

            // Normal Input
            currentHotkeyButtons = KeyboardControls.GetHotkeyButtons(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);
            currentButton = KeyboardControls.GetButtons(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);

            if (ConfigurationState.Instance.Hid.EnableKeyboard)
            {
                hidKeyboard = KeyboardControls.GetKeysDown(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);
            }

            (leftJoystickDx, leftJoystickDy) = KeyboardControls.GetLeftStick(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);
            (rightJoystickDx, rightJoystickDy) = KeyboardControls.GetRightStick(ConfigurationState.Instance.Hid.KeyboardControls, keyboard);

            if (!hidKeyboard.HasValue)
            {
                hidKeyboard = new HLE.Input.Keyboard
                {
                    Modifier = 0,
                    Keys = new int[0x8]
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
            if (IsFocused && _mousePressed)
            {
                int screenWidth = AllocatedWidth;
                int screenHeight = AllocatedHeight;

                if (AllocatedWidth > (AllocatedHeight * TouchScreenWidth) / TouchScreenHeight)
                {
                    screenWidth = (AllocatedHeight * TouchScreenWidth) / TouchScreenHeight;
                }
                else
                {
                    screenHeight = (AllocatedWidth * TouchScreenHeight) / TouchScreenWidth;
                }

                int startX = (AllocatedWidth - screenWidth) >> 1;
                int startY = (AllocatedHeight - screenHeight) >> 1;

                int endX = startX + screenWidth;
                int endY = startY + screenHeight;


                if (_mouseX >= startX &&
                    _mouseY >= startY &&
                    _mouseX < endX &&
                    _mouseY < endY)
                {
                    int screenMouseX = (int)_mouseX - startX;
                    int screenMouseY = (int)_mouseY - startY;

                    int mX = (screenMouseX * TouchScreenWidth) / screenWidth;
                    int mY = (screenMouseY * TouchScreenHeight) / screenHeight;

                    TouchPoint currentPoint = new TouchPoint
                    {
                        X = mX,
                        Y = mY,

                        // Placeholder values till more data is acquired
                        DiameterX = 10,
                        DiameterY = 10,
                        Angle = 90
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

            return true;
        }
    }
}
