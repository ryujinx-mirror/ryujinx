using LibRyujinx.Sample;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;

namespace LibRyujinx.NativeSample
{
    internal class NativeWindow : OpenTK.Windowing.Desktop.NativeWindow
    {
        private nint del;
        public delegate void SwapBuffersCallback();
        public delegate IntPtr GetProcAddress(string name);
        public delegate IntPtr CreateSurface(IntPtr instance);

        private bool _run;
        private bool _isVulkan;
        private Vector2 _lastPosition;
        private bool _mousePressed;
        private nint _gamepadIdPtr;
        private string? _gamepadId;

        public NativeWindow(NativeWindowSettings nativeWindowSettings) : base(nativeWindowSettings)
        {
            _isVulkan = true;
        }

        internal unsafe void Start(string gamePath)
        {
            if (!_isVulkan)
            {
                MakeCurrent();
            }

            var getProcAddress = Marshal.GetFunctionPointerForDelegate<GetProcAddress>(x => GLFW.GetProcAddress(x));
            var createSurface = Marshal.GetFunctionPointerForDelegate<CreateSurface>( x =>
            {
                VkHandle surface;
                GLFW.CreateWindowSurface(new VkHandle(x) ,this.WindowPtr, null, out surface);

                return surface.Handle;
            });
            var vkExtensions = GLFW.GetRequiredInstanceExtensions();


            var pointers = new IntPtr[vkExtensions.Length];
            for (int i = 0; i < vkExtensions.Length; i++)
            {
                pointers[i] = Marshal.StringToHGlobalAnsi(vkExtensions[i]);
            }

            fixed (IntPtr* ptr = pointers)
            {
                var nativeGraphicsInterop = new NativeGraphicsInterop()
                {
                    GlGetProcAddress = getProcAddress,
                    VkRequiredExtensions = (nint)ptr,
                    VkRequiredExtensionsCount = pointers.Length,
                    VkCreateSurface = createSurface
                };
                var success = LibRyujinxInterop.InitializeGraphicsRenderer(_isVulkan ? GraphicsBackend.Vulkan : GraphicsBackend.OpenGl, nativeGraphicsInterop);
                var timeZone = Marshal.StringToHGlobalAnsi("UTC");
                success = LibRyujinxInterop.InitializeDevice(true,
                    false,
                    SystemLanguage.AmericanEnglish,
                    RegionCode.USA,
                    true,
                    true,
                    true,
                    false,
                    timeZone,
                    false);
                LibRyujinxInterop.InitializeInput(ClientSize.X, ClientSize.Y);
                Marshal.FreeHGlobal(timeZone);

                var path = Marshal.StringToHGlobalAnsi(gamePath);
                var loaded = LibRyujinxInterop.LoadApplication(path);
                LibRyujinxInterop.SetRendererSize(Size.X, Size.Y);
                Marshal.FreeHGlobal(path);
            }

            _gamepadIdPtr = LibRyujinxInterop.ConnectGamepad(0);
            _gamepadId = Marshal.PtrToStringAnsi(_gamepadIdPtr);

            if (!_isVulkan)
            {
                Context.MakeNoneCurrent();
            }

            _run = true;
            var thread = new Thread(new ThreadStart(RunLoop));
            thread.Start();

            UpdateLoop();

            thread.Join();

            foreach(var ptr in pointers)
            {
                Marshal.FreeHGlobal(ptr);
            }

            Marshal.FreeHGlobal(_gamepadIdPtr);
        }

        public void RunLoop()
        {
            del = Marshal.GetFunctionPointerForDelegate<SwapBuffersCallback>(SwapBuffers);
            LibRyujinxInterop.SetSwapBuffersCallback(del);

            if (!_isVulkan)
            {
                MakeCurrent();

                Context.SwapInterval = 0;
            }

           /* Task.Run(async () =>
            {
                await Task.Delay(1000);

                LibRyujinxInterop.SetVsyncState(true);
            });*/

            LibRyujinxInterop.RunLoop();

            _run = false;

            if (!_isVulkan)
            {
                Context.MakeNoneCurrent();
            }
        }

        private void SwapBuffers()
        {
            if (!_isVulkan)
            {
                this.Context.SwapBuffers();
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            _lastPosition = e.Position;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if(e.Button == MouseButton.Left)
            {
                _mousePressed = true;
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            if (_run)
            {
                LibRyujinxInterop.SetRendererSize(e.Width, e.Height);
                LibRyujinxInterop.SetClientSize(e.Width, e.Height);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButton.Left)
            {
                _mousePressed = false;
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (_gamepadIdPtr != IntPtr.Zero)
            {
                var key = GetKeyMapping(e.Key);

                LibRyujinxInterop.SetButtonReleased(key, _gamepadIdPtr);
            }
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_gamepadIdPtr != IntPtr.Zero)
            {
                var key = GetKeyMapping(e.Key);

                LibRyujinxInterop.SetButtonPressed(key, _gamepadIdPtr);
            }
        }

        public void UpdateLoop()
        {
            while(_run)
            {
                ProcessWindowEvents(true);
                NewInputFrame();
                ProcessWindowEvents(IsEventDriven);
                if (_mousePressed)
                {
                    LibRyujinxInterop.SetTouchPoint((int)_lastPosition.X, (int)_lastPosition.Y);
                }
                else
                {
                    LibRyujinxInterop.ReleaseTouchPoint();
                }

                LibRyujinxInterop.UpdateInput();

                Thread.Sleep(1);
            }
        }

        public GamepadButtonInputId GetKeyMapping(Keys key)
        {
            if(_keyMapping.TryGetValue(key, out var mapping))
            {
                return mapping;
            }

            return GamepadButtonInputId.Unbound;
        }

        private Dictionary<Keys, GamepadButtonInputId> _keyMapping = new Dictionary<Keys, GamepadButtonInputId>()
        {
            {Keys.A, GamepadButtonInputId.A },
            {Keys.S, GamepadButtonInputId.B },
            {Keys.Z, GamepadButtonInputId.X },
            {Keys.X, GamepadButtonInputId.Y },
            {Keys.Equal, GamepadButtonInputId.Plus },
            {Keys.Minus, GamepadButtonInputId.Minus },
            {Keys.Q, GamepadButtonInputId.LeftShoulder },
            {Keys.D1, GamepadButtonInputId.LeftTrigger },
            {Keys.W, GamepadButtonInputId.RightShoulder },
            {Keys.D2, GamepadButtonInputId.RightTrigger },
            {Keys.E, GamepadButtonInputId.LeftStick },
            {Keys.R, GamepadButtonInputId.RightStick },
            {Keys.Up, GamepadButtonInputId.DpadUp },
            {Keys.Down, GamepadButtonInputId.DpadDown },
            {Keys.Left, GamepadButtonInputId.DpadLeft },
            {Keys.Right, GamepadButtonInputId.DpadRight },
            {Keys.U, GamepadButtonInputId.SingleLeftTrigger0 },
            {Keys.D7, GamepadButtonInputId.SingleLeftTrigger1 },
            {Keys.O, GamepadButtonInputId.SingleRightTrigger0 },
            {Keys.D9, GamepadButtonInputId.SingleRightTrigger1 }
        };
    }
}
