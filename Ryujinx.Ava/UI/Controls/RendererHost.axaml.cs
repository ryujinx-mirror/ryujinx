using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Common.Configuration;
using Silk.NET.Vulkan;
using SPB.Graphics.OpenGL;
using SPB.Windowing;
using System;

namespace Ryujinx.Ava.UI.Controls
{
    public partial class RendererHost : UserControl, IDisposable
    {
        private readonly GraphicsDebugLevel _graphicsDebugLevel;
        private EmbeddedWindow _currentWindow;

        public bool IsVulkan { get; private set; }

        public RendererHost(GraphicsDebugLevel graphicsDebugLevel)
        {
            _graphicsDebugLevel = graphicsDebugLevel;
            InitializeComponent();
        }

        public RendererHost()
        {
            InitializeComponent();
        }

        public void CreateOpenGL()
        {
            Dispose();

            _currentWindow = new OpenGLEmbeddedWindow(3, 3, _graphicsDebugLevel);
            Initialize();

            IsVulkan = false;
        }

        private void Initialize()
        {
            _currentWindow.WindowCreated += CurrentWindow_WindowCreated;
            _currentWindow.SizeChanged += CurrentWindow_SizeChanged;
            Content = _currentWindow;
        }

        public void CreateVulkan()
        {
            Dispose();

            _currentWindow = new VulkanEmbeddedWindow();
            Initialize();

            IsVulkan = true;
        }

        public OpenGLContextBase GetContext()
        {
            if (_currentWindow is OpenGLEmbeddedWindow openGlEmbeddedWindow)
            {
                return openGlEmbeddedWindow.Context;
            }

            return null;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            Dispose();
        }

        private void CurrentWindow_SizeChanged(object sender, Size e)
        {
            SizeChanged?.Invoke(sender, e);
        }

        private void CurrentWindow_WindowCreated(object sender, IntPtr e)
        {
            RendererInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void MakeCurrent()
        {
            if (_currentWindow is OpenGLEmbeddedWindow openGlEmbeddedWindow)
            {
                openGlEmbeddedWindow.MakeCurrent();
            }
        }

        public void MakeCurrent(SwappableNativeWindowBase window)
        {
            if (_currentWindow is OpenGLEmbeddedWindow openGlEmbeddedWindow)
            {
                openGlEmbeddedWindow.MakeCurrent(window);
            }
        }

        public void SwapBuffers()
        {
            if (_currentWindow is OpenGLEmbeddedWindow openGlEmbeddedWindow)
            {
                openGlEmbeddedWindow.SwapBuffers();
            }
        }

        public event EventHandler<EventArgs> RendererInitialized;
        public event Action<object, Size> SizeChanged;
        public void Dispose()
        {
            if (_currentWindow != null)
            {
                _currentWindow.WindowCreated -= CurrentWindow_WindowCreated;
                _currentWindow.SizeChanged -= CurrentWindow_SizeChanged;
            }
        }

        public SurfaceKHR CreateVulkanSurface(Instance instance, Vk api)
        {
            return (_currentWindow is VulkanEmbeddedWindow vulkanEmbeddedWindow)
                ? vulkanEmbeddedWindow.CreateSurface(instance)
                : default;
        }
    }
}