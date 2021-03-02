using ARMeilleure.Translation;
using ARMeilleure.Translation.PTC;
using Gdk;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Configuration;
using Ryujinx.Graphics.OpenGL;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.Modules.Motion;
using Ryujinx.Ui.Widgets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Ryujinx.Ui
{
    using Switch = HLE.Switch;

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

        private bool _isActive;
        private bool _isStopped;
        private bool _isFocused;

        private double _mouseX;
        private double _mouseY;
        private bool   _mousePressed;

        private bool _toggleFullscreen;
        private bool _toggleDockedMode;

        private readonly long _ticksPerFrame;

        private long _ticks = 0;

        private readonly Stopwatch _chrono;

        private readonly Switch _device;

        private Renderer _renderer;

        private HotkeyButtons _prevHotkeyButtons;

        private Client _dsuClient;

        private GraphicsDebugLevel _glLogLevel;

        private readonly ManualResetEvent _exitEvent;
        
        // Hide Cursor
        const int CursorHideIdleTime = 8; // seconds
        private static readonly Cursor _invisibleCursor = new Cursor(Display.Default, CursorType.BlankCursor);
        private long _lastCursorMoveTime;
        private bool _hideCursorOnIdle;

        public GlRenderer(Switch device, GraphicsDebugLevel glLogLevel)
            : base (GetGraphicsMode(),
            3, 3,
            glLogLevel == GraphicsDebugLevel.None
            ? GraphicsContextFlags.ForwardCompatible
            : GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            WaitEvent = new ManualResetEvent(false);

            _device = device;

            Initialized  += GLRenderer_Initialized;
            Destroyed    += GLRenderer_Destroyed;
            ShuttingDown += GLRenderer_ShuttingDown;

            Initialize();

            _chrono = new Stopwatch();

            _ticksPerFrame = Stopwatch.Frequency / TargetFps;

            AddEvents((int)(EventMask.ButtonPressMask
                          | EventMask.ButtonReleaseMask
                          | EventMask.PointerMotionMask
                          | EventMask.KeyPressMask
                          | EventMask.KeyReleaseMask));

            Shown += Renderer_Shown;

            _dsuClient = new Client();

            _glLogLevel = glLogLevel;

            _exitEvent = new ManualResetEvent(false);

            _hideCursorOnIdle = ConfigurationState.Instance.HideCursorOnIdle;
            _lastCursorMoveTime = Stopwatch.GetTimestamp();

            ConfigurationState.Instance.HideCursorOnIdle.Event += HideCursorStateChanged;
        }

        private void HideCursorStateChanged(object sender, ReactiveEventArgs<bool> state)
        {
            Gtk.Application.Invoke(delegate
            {
                _hideCursorOnIdle = state.NewValue;

                if (_hideCursorOnIdle)
                {
                    _lastCursorMoveTime = Stopwatch.GetTimestamp();
                }
                else
                {
                    Window.Cursor = null;
                }
            });
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
            _isFocused = false;
        }

        private void Parent_FocusInEvent(object o, Gtk.FocusInEventArgs args)
        {
            _isFocused = true;
        }

        private void GLRenderer_Destroyed(object sender, EventArgs e)
        {
            ConfigurationState.Instance.HideCursorOnIdle.Event -= HideCursorStateChanged;

            _dsuClient?.Dispose();
            Dispose();
        }

        protected void Renderer_Shown(object sender, EventArgs e)
        {
            _isFocused = this.ParentWindow.State.HasFlag(Gdk.WindowState.Focused);
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
                            if (!ConfigurationState.Instance.ShowConfirmExit || GtkDialog.CreateExitDialog())
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

            bool toggleDockedMode = keyboard.IsKeyDown(OpenTK.Input.Key.F9);

            if (toggleDockedMode != _toggleDockedMode)
            {
                if (toggleDockedMode)
                {
                    ConfigurationState.Instance.System.EnableDockedMode.Value =
                        !ConfigurationState.Instance.System.EnableDockedMode.Value;
                }
            }

            _toggleDockedMode = toggleDockedMode;

            if (_hideCursorOnIdle)
            {
                long cursorMoveDelta = Stopwatch.GetTimestamp() - _lastCursorMoveTime;
                Window.Cursor = (cursorMoveDelta >= CursorHideIdleTime * Stopwatch.Frequency) ? _invisibleCursor : null;
            }
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

            _isActive = true;

            Gtk.Window parent = this.Toplevel as Gtk.Window;

            parent.FocusInEvent  += Parent_FocusInEvent;
            parent.FocusOutEvent += Parent_FocusOutEvent;

            Gtk.Application.Invoke(delegate
            {
                parent.Present();

                string titleNameSection = string.IsNullOrWhiteSpace(_device.Application.TitleName) ? string.Empty
                    : $" - {_device.Application.TitleName}";

                string titleVersionSection = string.IsNullOrWhiteSpace(_device.Application.DisplayVersion) ? string.Empty
                    : $" v{_device.Application.DisplayVersion}";

                string titleIdSection = string.IsNullOrWhiteSpace(_device.Application.TitleIdText) ? string.Empty
                    : $" ({_device.Application.TitleIdText.ToUpper()})";

                string titleArchSection = _device.Application.TitleIs64Bit ? " (64-bit)" : " (32-bit)";

                parent.Title = $"Ryujinx {Program.Version}{titleNameSection}{titleVersionSection}{titleIdSection}{titleArchSection}";
            });

            Thread renderLoopThread = new Thread(Render)
            {
                Name = "GUI.RenderLoop"
            };
            renderLoopThread.Start();

            Thread nvStutterWorkaround = new Thread(NVStutterWorkaround)
            {
                Name = "GUI.NVStutterWorkaround"
            };
            nvStutterWorkaround.Start();

            MainLoop();

            renderLoopThread.Join();
            nvStutterWorkaround.Join();

            Exit();
        }

        private void NVStutterWorkaround()
        {
            while (_isActive)
            {
                // When NVIDIA Threaded Optimization is on, the driver will snapshot all threads in the system whenever the application creates any new ones.
                // The ThreadPool has something called a "GateThread" which terminates itself after some inactivity.
                // However, it immediately starts up again, since the rules regarding when to terminate and when to start differ.
                // This creates a new thread every second or so.
                // The main problem with this is that the thread snapshot can take 70ms, is on the OpenGL thread and will delay rendering any graphics.
                // This is a little over budget on a frame time of 16ms, so creates a large stutter.
                // The solution is to keep the ThreadPool active so that it never has a reason to terminate the GateThread.

                // TODO: This should be removed when the issue with the GateThread is resolved.

                ThreadPool.QueueUserWorkItem((state) => { });
                Thread.Sleep(300);
            }
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

            if (_hideCursorOnIdle)
            {
                _lastCursorMoveTime = Stopwatch.GetTimestamp();
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

            if (_isStopped)
            {
                return;
            }

            _isStopped = true;
            _isActive  = false;

            _exitEvent.WaitOne();
            _exitEvent.Dispose();
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

            _device.Gpu.Renderer.Initialize(_glLogLevel);

            // Make sure the first frame is not transparent.
            GL.ClearColor(OpenTK.Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            SwapBuffers();

            _device.Gpu.InitializeShaderCache();
            Translator.IsReadyForTranslation.Set();

            while (_isActive)
            {
                if (_isStopped)
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

                while (_device.ConsumeFrameAvailable())
                {
                    _device.PresentFrame(SwapBuffers);
                }

                if (_ticks >= _ticksPerFrame)
                {
                    string dockedMode = ConfigurationState.Instance.System.EnableDockedMode ? "Docked" : "Handheld";
                    float scale = Graphics.Gpu.GraphicsConfig.ResScale;
                    if (scale != 1)
                    {
                        dockedMode += $" ({scale}x)";
                    }

                    StatusUpdatedEvent?.Invoke(this, new StatusUpdatedEventArgs(
                        _device.EnableDeviceVsync,
                        dockedMode,
                        ConfigurationState.Instance.Graphics.AspectRatio.Value.ToText(),
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
            while (_isActive)
            {
                UpdateFrame();

                // Polling becomes expensive if it's not slept
                Thread.Sleep(1);
            }

            _exitEvent.Set();
        }

        private bool UpdateFrame()
        {
            if (!_isActive)
            {
                return true;
            }

            if (_isStopped)
            {
                return false;
            }

            if (_isFocused)
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
                    if (_isFocused)
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

            if(_isFocused)
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
            if (_isFocused && _mousePressed)
            {
                float aspectWidth = SwitchPanelHeight * ConfigurationState.Instance.Graphics.AspectRatio.Value.ToFloat();

                int screenWidth  = AllocatedWidth;
                int screenHeight = AllocatedHeight;

                if (AllocatedWidth > AllocatedHeight * aspectWidth / SwitchPanelHeight)
                {
                    screenWidth = (int)(AllocatedHeight * aspectWidth) / SwitchPanelHeight;
                }
                else
                {
                    screenHeight = (AllocatedWidth * SwitchPanelHeight) / (int)aspectWidth;
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

                    int mX = (screenMouseX * (int)aspectWidth)  / screenWidth;
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
