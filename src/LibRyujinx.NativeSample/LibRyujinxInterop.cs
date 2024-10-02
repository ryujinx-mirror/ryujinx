using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibRyujinx.Sample
{
    internal static class LibRyujinxInterop
    {
        private const string dll = "LibRyujinx.dll";

        [DllImport(dll, EntryPoint = "initialize")]
        public extern static bool Initialize(IntPtr path);


        [DllImport(dll, EntryPoint = "graphics_initialize")]
        public extern static bool InitializeGraphics(GraphicsConfiguration graphicsConfiguration);

        [DllImport(dll, EntryPoint = "device_initialize")]
        internal extern static bool InitializeDevice(bool isHostMapped,
                                                  bool useHypervisor,
                                                  SystemLanguage systemLanguage,
                                                  RegionCode regionCode,
                                                  bool enableVsync,
                                                  bool enableDockedMode,
                                                  bool enablePtc,
                                                  bool enableInternetAccess,
                                                  IntPtr timeZone,
                                                  bool ignoreMissingServices);

        [DllImport(dll, EntryPoint = "graphics_initialize_renderer")]
        internal extern static bool InitializeGraphicsRenderer(GraphicsBackend backend, NativeGraphicsInterop nativeGraphicsInterop);

        [DllImport(dll, EntryPoint = "device_load")]
        internal extern static bool LoadApplication(IntPtr pathPtr);

        [DllImport(dll, EntryPoint = "graphics_renderer_run_loop")]
        internal extern static void RunLoop();

        [DllImport(dll, EntryPoint = "graphics_renderer_set_size")]
        internal extern static void SetRendererSize(int width, int height);

        [DllImport(dll, EntryPoint = "graphics_renderer_set_swap_buffer_callback")]
        internal extern static void SetSwapBuffersCallback(IntPtr swapBuffers);

        [DllImport(dll, EntryPoint = "graphics_renderer_set_vsync")]
        internal extern static void SetVsyncState(bool enabled);

        [DllImport(dll, EntryPoint = "input_initialize")]
        internal extern static void InitializeInput(int width, int height);

        [DllImport(dll, EntryPoint = "input_set_client_size")]
        internal extern static void SetClientSize(int width, int height);

        [DllImport(dll, EntryPoint = "input_set_touch_point")]
        internal extern static void SetTouchPoint(int x, int y);

        [DllImport(dll, EntryPoint = "input_release_touch_point")]
        internal extern static void ReleaseTouchPoint();

        [DllImport(dll, EntryPoint = "input_update")]
        internal extern static void UpdateInput();

        [DllImport(dll, EntryPoint = "input_set_button_pressed")]
        public extern static void SetButtonPressed(GamepadButtonInputId button, IntPtr idPtr);

        [DllImport(dll, EntryPoint = "input_set_button_released")]
        public extern static void SetButtonReleased(GamepadButtonInputId button, IntPtr idPtr);

        [DllImport(dll, EntryPoint = "input_set_stick_axis")]
        public extern static void SetStickAxis(StickInputId stick, Vector2 axes, IntPtr idPtr);

        [DllImport(dll, EntryPoint = "input_connect_gamepad")]
        public extern static IntPtr ConnectGamepad(int index);
    }

    public enum GraphicsBackend
    {
        Vulkan,
        OpenGl
    }

    public enum BackendThreading
    {
        Auto,
        Off,
        On
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GraphicsConfiguration
    {
        public float ResScale = 1f;
        public float MaxAnisotropy = -1;
        public bool FastGpuTime = true;
        public bool Fast2DCopy = true;
        public bool EnableMacroJit = false;
        public bool EnableMacroHLE = true;
        public bool EnableShaderCache = true;
        public bool EnableTextureRecompression = false;
        public BackendThreading BackendThreading = BackendThreading.Auto;
        public AspectRatio AspectRatio = AspectRatio.Fixed16x9;

        public GraphicsConfiguration()
        {
        }
    }
    public enum SystemLanguage
    {
        Japanese,
        AmericanEnglish,
        French,
        German,
        Italian,
        Spanish,
        Chinese,
        Korean,
        Dutch,
        Portuguese,
        Russian,
        Taiwanese,
        BritishEnglish,
        CanadianFrench,
        LatinAmericanSpanish,
        SimplifiedChinese,
        TraditionalChinese,
        BrazilianPortuguese,
    }
    public enum RegionCode
    {
        Japan,
        USA,
        Europe,
        Australia,
        China,
        Korea,
        Taiwan,

        Min = Japan,
        Max = Taiwan,
    }

    public struct NativeGraphicsInterop
    {
        public IntPtr GlGetProcAddress;
        public IntPtr VkNativeContextLoader;
        public IntPtr VkCreateSurface;
        public IntPtr VkRequiredExtensions;
        public int VkRequiredExtensionsCount;
    }

    public enum AspectRatio
    {
        Fixed4x3,
        Fixed16x9,
        Fixed16x10,
        Fixed21x9,
        Fixed32x9,
        Stretched
    }

    /// <summary>
    /// Represent a button from a gamepad.
    /// </summary>
    public enum GamepadButtonInputId : byte
    {
        Unbound,
        A,
        B,
        X,
        Y,
        LeftStick,
        RightStick,
        LeftShoulder,
        RightShoulder,

        // Likely axis
        LeftTrigger,
        // Likely axis
        RightTrigger,

        DpadUp,
        DpadDown,
        DpadLeft,
        DpadRight,

        // Special buttons

        Minus,
        Plus,

        Back = Minus,
        Start = Plus,

        Guide,
        Misc1,

        // Xbox Elite paddle
        Paddle1,
        Paddle2,
        Paddle3,
        Paddle4,

        // PS5 touchpad button
        Touchpad,

        // Virtual buttons for single joycon
        SingleLeftTrigger0,
        SingleRightTrigger0,

        SingleLeftTrigger1,
        SingleRightTrigger1,

        Count
    }

    public enum StickInputId : byte
    {
        Unbound,
        Left,
        Right,

        Count
    }
}
