using SPB.Graphics;
using System;
using System.Runtime.Versioning;

namespace Ryujinx.Ava.Ui.Controls
{
    [SupportedOSPlatform("linux")]
    internal class AvaloniaGlxContext : SPB.Platform.GLX.GLXOpenGLContext
    {
        public AvaloniaGlxContext(IntPtr handle)
            : base(FramebufferFormat.Default, 0, 0, 0, false, null)
        {
            ContextHandle = handle;
        }
    }
}
