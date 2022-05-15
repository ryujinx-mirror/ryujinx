using SPB.Graphics;
using System;
using System.Runtime.Versioning;

namespace Ryujinx.Ava.Ui.Controls
{
    [SupportedOSPlatform("windows")]
    public class AvaloniaWglContext : SPB.Platform.WGL.WGLOpenGLContext
    {
        public AvaloniaWglContext(IntPtr handle)
            : base(FramebufferFormat.Default, 0, 0, 0, false, null)
        {
            ContextHandle = handle;
        }
    }
}
