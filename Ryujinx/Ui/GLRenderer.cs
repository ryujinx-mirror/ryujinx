using ARMeilleure.Translation.PTC;
using Gdk;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Ryujinx.Configuration;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Graphics.OpenGL;
using Ryujinx.HLE;
using Ryujinx.HLE.HOS.Services.Hid;
using System;
using System.Collections.Generic;
using System.Threading;
using Ryujinx.Motion;

namespace Ryujinx.Ui
{
    public class GlRenderer : GLWidget
    {
        static GlRenderer()
        {
            OpenTK.Graphics.GraphicsContext.ShareContexts = true;
        }

        private const int SwitchPanelWidth  = 1280;
        private const int SwitchPanelHeight = 720;
        private const int TargetFps         = 60;

        public ManualResetEvent WaitEvent { get; set; }

        public static event EventHandler<StatusUpdatedEventArgs> StatusUpdatedEvent;

        public bool IsActive   { get; set; }
        public bool IsStopped  { get; set; }
        public bool IsFocused  { get; set; }

        private double _mouseX;
        private double _mouseY;
        private bool   _mousePressed;

        private bool _toggleFullscreen;

        private readonly long _ticksPerFrame;

        private long _ticks = 0;

        private System.Diagnostics.Stopwatch _chrono;

        private Switch _device;

        private Renderer _renderer;

        private HotkeyButtons _prevHotkeyButtons;

        private Client _dsuClient;

        private GraphicsDebugLevel _glLogLevel;

        public GlRenderer(Switch device, GraphicsDebugLevel glLogLevel)
            : base (GetGraphicsMode(),
            3, 3,
            glLogLevel == GraphicsDebugLevel.None 
            ? GraphicsContextFlags.ForwardCompatible 
            : GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            WaitEvent = new ManualResetEvent(false);

            _device = device;

            this.Initialized  += GLRenderer_Initialized;
            this.Destroyed    += GLRenderer_Destroyed;
            this.ShuttingDown += GLRenderer_ShuttingDown;

            Initialize();

            _chrono = new System.Diagnostics.Stopwatch();

            _ticksPerFrame = System.Diagnostics.Stopwatch.Frequency / TargetFps;

            AddEvents((int)(EventMask.ButtonPressMask
                          | EventMask.ButtonReleaseMask
                          | EventMask.PointerMotionMask
                          | EventMask.KeyPressMask
                          | EventMask.KeyReleaseMask));

            this.Shown += Renderer_Shown;

            _dsuClient = new Client();

            _glLogLevel = glLogLevel;
        }

        private static GraphicsMode GetGraphicsMode()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix ? new GraphicsMode(new ColorFormat(24)) : new GraphicsMode(new ColorFormat());
        }

        private void GLRenderer_ShuttingDown(object sender, EventArgs args)
        {
            _device.DisposeGpu();
            _dsuClient?.Dispose();
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
            _dsuClient?.Dispose();
            Dispose();
        }

        protected void Renderer_Shown(object sender, EventArgs e)
        {
            IsFocused = this.ParentWindow.State.HasFlag(Gdk.WindowState.Focused);
        }

        public void HandleScreenState(KeyboardState keyboard)
        {
            bool toggleFullscreen =  keyboard.IsKeyDown(OpenTK.Input.Key.F11)
                                || ((keyboard.IsKeyDown(OpenTK.Input.Key.AltLeft)
                                ||   keyboard.IsKeyDown(OpenTK.Input.Key.AltRight))
                                &&   keyboard.IsKeyDown(OpenTK.Input.Key.Enter))
                                ||   keyboard.IsKeyDown(OpenTK.Input.Key.Escape);

            bool fullScreenToggled = ParentWindow.State.HasFlag(Gdk.WindowState.Fullscreen);

            if (toggleFullscreen != _toggleFullscreen)
            {
                if (toggleFullscreen)
                {
                    if (fullScreenToggled)
                    {
                        ParentWindow.Unfullscreen();
                        (Toplevel as MainWindow)?.ToggleExtraWidgets(true);
                    }
                    else
                    {
                        if (keyboard.IsKeyDown(OpenTK.Input.Key.Escape))
                        {
                            if (GtkDialog.CreateExitDialog())
                            {
                                Exit();
                            }
                        }
                        else
                        {
                            ParentWindow.Fullscreen();
                            (Toplevel as MainWindow)?.ToggleExtraWidgets(false);
                        }
                    }
                }
            }

            _toggleFullscreen = toggleFullscreen;
        }

        private void GLRenderer_Initialized(object sender, EventArgs e)
        {
            // Release the GL exclusivity that OpenTK gave us as we aren't going to use it in GTK Thread.
            GraphicsContext.MakeCurrent(null);

            WaitEvent.Set();
        }

        protected override bool OnConfigureEvent(EventConfigure evnt)
        {
            bool result = base.OnConfigureEvent(evnt);

            Gdk.Monitor monitor = Display.GetMonitorAtWindow(Window);

            _renderer.Window.SetSize(evnt.Width * monitor.ScaleFactor, evnt.Height * monitor.ScaleFactor);

            return result;
        }

        public void Start()
        {
            IsRenderHandler = true;

            _chrono.Restart();

            IsActive = true;

            Gtk.Window parent = this.Toplevel as Gtk.Window;

            parent.FocusInEvent  += Parent_FocusInEvent;
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

        protected override void OnGetPreferredHeight(out int minimumHeight, out int naturalHeight)
        {
            Gdk.Monitor monitor = Display.GetMonitorAtWindow(Window);

            // If the monitor is at least 1080p, use the Switch panel size as minimal size.
            if (monitor.Geometry.Height >= 1080)
            {
                minimumHeight = SwitchPanelHeight;
            }
            // Otherwise, we default minimal size to 480p 16:9.
            else
            {
                minimumHeight = 480;
            }

            naturalHeight = minimumHeight;
        }

        protected override void OnGetPreferredWidth(out int minimumWidth, out int naturalWidth)
        {
            Gdk.Monitor monitor = Display.GetMonitorAtWindow(Window);

            // If the monitor is at least 1080p, use the Switch panel size as minimal size.
            if (monitor.Geometry.Height >= 1080)
            {
                minimumWidth = SwitchPanelWidth;
            }
            // Otherwise, we default minimal size to 480p 16:9.
            else
            {
                minimumWidth = 854;
            }

            naturalWidth = minimumWidth;
        }

        public void Exit()
        {
            _dsuClient?.Dispose();
            if (IsStopped)
            {
                return;
            }

            IsStopped = true;
            IsActive  = false;
        }

        public void Initialize()
        {
            if (!(_device.Gpu.Renderer is Renderer))
            {
                throw new NotSupportedException($"GPU renderer must be an OpenGL renderer when using {typeof(Renderer).Name}!");
            }

            _renderer = (Renderer)_device.Gpu.Renderer;
        }

        public void Render()
        {
            // First take exclusivity on the OpenGL context.
            _renderer.InitializeBackgroundContext(GraphicsContext);
            Gtk.Window parent = Toplevel as Gtk.Window;
            parent.Present();
            GraphicsContext.MakeCurrent(WindowInfo);

            _device.Gpu.Initialize(_glLogLevel);

            // Make sure the first frame is not transparent.
            GL.ClearColor(OpenTK.Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            SwapBuffers();

            while (IsActive)
            {
                if (IsStopped)
                {
                    return;
                }

                _ticks += _chrono.ElapsedTicks;

                _chrono.Restart();

                if (_device.WaitFifo())
                {
                    _device.Statistics.RecordFifoStart();
                    _device.ProcessFrame();
                    _device.Statistics.RecordFifoEnd();
                }

                string dockedMode = ConfigurationState.Instance.System.EnableDockedMode ? "Docked" : "Handheld";
                float scale = Graphics.Gpu.GraphicsConfig.ResScale;
                if (scale != 1)
                {
                    dockedMode += $" ({scale}x)";
                }

                if (_ticks >= _ticksPerFrame)
                {
                    _device.PresentFrame(SwapBuffers);

                    StatusUpdatedEvent?.Invoke(this, new StatusUpdatedEventArgs(
                        _device.EnableDeviceVsync,
                        dockedMode,
                        $"Game: {_device.Statistics.GetGameFrameRate():00.00} FPS",
                        $"FIFO: {_device.Statistics.GetFifoPercent():0.00} %",
                        $"GPU:  {_renderer.GpuVendor}"));

                    _ticks = Math.Min(_ticks - _ticksPerFrame, _ticksPerFrame);
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
                UpdateFrame();

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

            if (IsFocused)
            {
                Gtk.Application.Invoke(delegate
                {
                    KeyboardState keyboard = OpenTK.Input.Keyboard.GetState();

                    HandleScreenState(keyboard);

                    if (keyboard.IsKeyDown(OpenTK.Input.Key.Delete))
                    {
                        if (!ParentWindow.State.HasFlag(Gdk.WindowState.Fullscreen))
                        {
                            Ptc.Continue();
                        }
                    }
                });
            }

            List<GamepadInput> gamepadInputs = new List<GamepadInput>(NpadDevices.MaxControllers);
            List<SixAxisInput> motionInputs  = new List<SixAxisInput>(NpadDevices.MaxControllers);

            MotionDevice motionDevice = new MotionDevice(_dsuClient);
            
            foreach (InputConfig inputConfig in ConfigurationState.Instance.Hid.InputConfig.Value)
            {
                ControllerKeys   currentButton = 0;
                JoystickPosition leftJoystick  = new JoystickPosition();
                JoystickPosition rightJoystick = new JoystickPosition();
                KeyboardInput?   hidKeyboard   = null;

                int leftJoystickDx  = 0;
                int leftJoystickDy  = 0;
                int rightJoystickDx = 0;
                int rightJoystickDy = 0;

                if (inputConfig.EnableMotion)
                {
                    motionDevice.RegisterController(inputConfig.PlayerIndex);
                }

                if (inputConfig is KeyboardConfig keyboardConfig)
                {
                    if (IsFocused)
                    {
                        // Keyboard Input
                        KeyboardController keyboardController = new KeyboardController(keyboardConfig);

                        currentButton = keyboardController.GetButtons();

                        (leftJoystickDx,  leftJoystickDy)  = keyboardController.GetLeftStick();
                        (rightJoystickDx, rightJoystickDy) = keyboardController.GetRightStick();

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

                        if (ConfigurationState.Instance.Hid.EnableKeyboard)
                        {
                            hidKeyboard = keyboardController.GetKeysDown();
                        }

                        if (!hidKeyboard.HasValue)
                        {
                            hidKeyboard = new KeyboardInput
                            {
                                Modifier = 0,
                                Keys     = new int[0x8]
                            };
                        }

                        if (ConfigurationState.Instance.Hid.EnableKeyboard)
                        {
                            _device.Hid.Keyboard.Update(hidKeyboard.Value);
                        }
                    }
                }
                else if (inputConfig is Common.Configuration.Hid.ControllerConfig controllerConfig)
                {
                    // Controller Input
                    JoystickController joystickController = new JoystickController(controllerConfig);

                    currentButton |= joystickController.GetButtons();

                    (leftJoystickDx,  leftJoystickDy)  = joystickController.GetLeftStick();
                    (rightJoystickDx, rightJoystickDy) = joystickController.GetRightStick();

                    leftJoystick = new JoystickPosition
                    {
                        Dx = controllerConfig.LeftJoycon.InvertStickX ? -leftJoystickDx : leftJoystickDx,
                        Dy = controllerConfig.LeftJoycon.InvertStickY ? -leftJoystickDy : leftJoystickDy
                    };

                    rightJoystick = new JoystickPosition
                    {
                        Dx = controllerConfig.RightJoycon.InvertStickX ? -rightJoystickDx : rightJoystickDx,
                        Dy = controllerConfig.RightJoycon.InvertStickY ? -rightJoystickDy : rightJoystickDy
                    };
                }

                currentButton |= _device.Hid.UpdateStickButtons(leftJoystick, rightJoystick);

                motionDevice.Poll(inputConfig, inputConfig.Slot);

                SixAxisInput sixAxisInput = new SixAxisInput()
                {
                    PlayerId      = (HLE.HOS.Services.Hid.PlayerIndex)inputConfig.PlayerIndex,
                    Accelerometer = motionDevice.Accelerometer,
                    Gyroscope     = motionDevice.Gyroscope,
                    Rotation      = motionDevice.Rotation,
                    Orientation   = motionDevice.Orientation
                };

                motionInputs.Add(sixAxisInput);

                gamepadInputs.Add(new GamepadInput
                {
                    PlayerId = (HLE.HOS.Services.Hid.PlayerIndex)inputConfig.PlayerIndex,
                    Buttons  = currentButton,
                    LStick   = leftJoystick,
                    RStick   = rightJoystick
                });

                if (inputConfig.ControllerType == Common.Configuration.Hid.ControllerType.JoyconPair)
                {
                    if (!inputConfig.MirrorInput)
                    {
                        motionDevice.Poll(inputConfig, inputConfig.AltSlot);

                        sixAxisInput = new SixAxisInput()
                        {
                            PlayerId      = (HLE.HOS.Services.Hid.PlayerIndex)inputConfig.PlayerIndex,
                            Accelerometer = motionDevice.Accelerometer,
                            Gyroscope     = motionDevice.Gyroscope,
                            Rotation      = motionDevice.Rotation,
                            Orientation   = motionDevice.Orientation
                        };
                    }

                    motionInputs.Add(sixAxisInput);
                }
            }
            
            _device.Hid.Npads.Update(gamepadInputs);
            _device.Hid.Npads.UpdateSixAxis(motionInputs);

            if(IsFocused)
            {
                // Hotkeys
                HotkeyButtons currentHotkeyButtons = KeyboardController.GetHotkeyButtons(OpenTK.Input.Keyboard.GetState());

                if (currentHotkeyButtons.HasFlag(HotkeyButtons.ToggleVSync) &&
                    !_prevHotkeyButtons.HasFlag(HotkeyButtons.ToggleVSync))
                {
                    _device.EnableDeviceVsync = !_device.EnableDeviceVsync;
                }

                _prevHotkeyButtons = currentHotkeyButtons;
            }

            //Touchscreen
            bool hasTouch = false;

            // Get screen touch position from left mouse click
            // OpenTK always captures mouse events, even if out of focus, so check if window is focused.
            if (IsFocused && _mousePressed)
            {
                int screenWidth  = AllocatedWidth;
                int screenHeight = AllocatedHeight;

                if (AllocatedWidth > (AllocatedHeight * SwitchPanelWidth) / SwitchPanelHeight)
                {
                    screenWidth = (AllocatedHeight * SwitchPanelWidth) / SwitchPanelHeight;
                }
                else
                {
                    screenHeight = (AllocatedWidth * SwitchPanelHeight) / SwitchPanelWidth;
                }

                int startX = (AllocatedWidth  - screenWidth)  >> 1;
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

                    int mX = (screenMouseX * SwitchPanelWidth) / screenWidth;
                    int mY = (screenMouseY * SwitchPanelHeight) / screenHeight;

                    TouchPoint currentPoint = new TouchPoint
                    {
                        X = (uint)mX,
                        Y = (uint)mY,

                        // Placeholder values till more data is acquired
                        DiameterX = 10,
                        DiameterY = 10,
                        Angle     = 90
                    };

                    hasTouch = true;

                    _device.Hid.Touchscreen.Update(currentPoint);
                }
            }

            if (!hasTouch)
            {
                _device.Hid.Touchscreen.Update();
            }

            _device.Hid.DebugPad.Update();

            return true;
        }
    }
}
