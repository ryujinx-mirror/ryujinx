using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Input.HLE;
using System;
using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace Ryujinx.Headless.SDL2.Vulkan
{
    class VulkanWindow : WindowBase
    {
        private GraphicsDebugLevel _glLogLevel;

        public VulkanWindow(InputManager inputManager, GraphicsDebugLevel glLogLevel, AspectRatio aspectRatio, bool enableMouse) : base(inputManager, glLogLevel, aspectRatio, enableMouse)
        {
            _glLogLevel = glLogLevel;
        }

        public override SDL_WindowFlags GetWindowFlags() => SDL_WindowFlags.SDL_WINDOW_VULKAN;

        protected override void InitializeWindowRenderer() { }

        protected override void InitializeRenderer()
        {
            Renderer?.Window.SetSize(DefaultWidth, DefaultHeight);
            MouseDriver.SetClientSize(DefaultWidth, DefaultHeight);
        }

        public unsafe IntPtr CreateWindowSurface(IntPtr instance)
        {
            if (SDL_Vulkan_CreateSurface(WindowHandle, instance, out ulong surfaceHandle) == SDL_bool.SDL_FALSE)
            {
                string errorMessage = $"SDL_Vulkan_CreateSurface failed with error \"{SDL_GetError()}\"";

                Logger.Error?.Print(LogClass.Application, errorMessage);

                throw new Exception(errorMessage);
            }

            return (IntPtr)surfaceHandle;
        }

        // TODO: Fix this in SDL2-CS.
        [DllImport("SDL2", EntryPoint = "SDL_Vulkan_GetInstanceExtensions", CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_bool SDL_Vulkan_GetInstanceExtensions_Workaround(IntPtr window, out uint count, IntPtr names);

        public unsafe string[] GetRequiredInstanceExtensions()
        {
            if (SDL_Vulkan_GetInstanceExtensions_Workaround(WindowHandle, out uint extensionsCount, IntPtr.Zero) == SDL_bool.SDL_TRUE)
            {
                IntPtr[] rawExtensions = new IntPtr[(int)extensionsCount];
                string[] extensions = new string[(int)extensionsCount];

                fixed (IntPtr* rawExtensionsPtr = rawExtensions)
                {
                    if (SDL_Vulkan_GetInstanceExtensions_Workaround(WindowHandle, out extensionsCount, (IntPtr)rawExtensionsPtr) == SDL_bool.SDL_TRUE)
                    {
                        for (int i = 0; i < extensions.Length; i++)
                        {
                            extensions[i] = Marshal.PtrToStringUTF8(rawExtensions[i]);
                        }

                        return extensions;
                    }
                }
            }

            string errorMessage = $"SDL_Vulkan_GetInstanceExtensions failed with error \"{SDL_GetError()}\"";

            Logger.Error?.Print(LogClass.Application, errorMessage);

            throw new Exception(errorMessage);
        }

        protected override void FinalizeWindowRenderer()
        {
            Device.DisposeGpu();
        }

        protected override void SwapBuffers(object texture) { }
    }
}
