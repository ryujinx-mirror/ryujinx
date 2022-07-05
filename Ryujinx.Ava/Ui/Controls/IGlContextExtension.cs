using Avalonia.OpenGL;
using SPB.Graphics.OpenGL;
using System;

namespace Ryujinx.Ava.Ui.Controls
{
    internal static class IGlContextExtension
    {
        public static OpenGLContextBase AsOpenGLContextBase(this IGlContext context)
        {
            var handle = (IntPtr)context.GetType().GetProperty("Handle").GetValue(context);

            if (OperatingSystem.IsWindows())
            {
                return new AvaloniaWglContext(handle);
            }
            else if (OperatingSystem.IsLinux())
            {
                return new AvaloniaGlxContext(handle);
            }

            return null;
        }
    }
}