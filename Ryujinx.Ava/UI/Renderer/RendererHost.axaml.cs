using Avalonia;
using Avalonia.Controls;
using Ryujinx.Common.Configuration;
using Ryujinx.Ui.Common.Configuration;
using Silk.NET.Vulkan;
using System;

namespace Ryujinx.Ava.UI.Renderer
{
    public partial class RendererHost : UserControl, IDisposable
    {
        public EmbeddedWindow EmbeddedWindow;

        public event EventHandler<EventArgs> WindowCreated;
        public event Action<object, Size>    SizeChanged;

        public RendererHost()
        {
            InitializeComponent();

            Dispose();

            if (ConfigurationState.Instance.Graphics.GraphicsBackend.Value == GraphicsBackend.OpenGl)
            {
                EmbeddedWindow = new EmbeddedWindowOpenGL();
            }
            else
            {
                EmbeddedWindow = new EmbeddedWindowVulkan();
            }

            Initialize();
        }

        private void Initialize()
        {
            EmbeddedWindow.WindowCreated += CurrentWindow_WindowCreated;
            EmbeddedWindow.SizeChanged   += CurrentWindow_SizeChanged;

            Content = EmbeddedWindow;
        }

        public void Dispose()
        {
            if (EmbeddedWindow != null)
            {
                EmbeddedWindow.WindowCreated -= CurrentWindow_WindowCreated;
                EmbeddedWindow.SizeChanged   -= CurrentWindow_SizeChanged;
            }
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
            WindowCreated?.Invoke(this, EventArgs.Empty);
        }
    }
}