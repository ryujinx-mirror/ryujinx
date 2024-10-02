using LibRyujinx.Sample;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace LibRyujinx.NativeSample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var success = LibRyujinxInterop.Initialize(IntPtr.Zero);
                success = LibRyujinxInterop.InitializeGraphics(new GraphicsConfiguration());
                var nativeWindowSettings = new NativeWindowSettings()
                {
                    ClientSize = new Vector2i(800, 600),
                    Title = "Ryujinx Native",
                    API = ContextAPI.NoAPI,
                    IsEventDriven = false,
                    // This is needed to run on macos
                    Flags = ContextFlags.ForwardCompatible,
                };

                using var window = new NativeWindow(nativeWindowSettings);

                window.IsVisible = true;
                window.Start(args[0]);
            }
        }
    }
}