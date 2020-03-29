using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.OpenGL
{
    public static class Debugger
    {
        private static DebugProc _debugCallback;

        public static void Initialize()
        {
            GL.Enable(EnableCap.DebugOutputSynchronous);

            GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DontCare, DebugSeverityControl.DontCare, 0, (int[])null, true);

            _debugCallback = GLDebugHandler;

            GL.DebugMessageCallback(_debugCallback, IntPtr.Zero);
        }

        private static void GLDebugHandler(
            DebugSource source,
            DebugType type,
            int id,
            DebugSeverity severity,
            int length,
            IntPtr message,
            IntPtr userParam)
        {
            string fullMessage = $"{type} {severity} {source} {Marshal.PtrToStringAnsi(message)}";

            switch (type)
            {
                case DebugType.DebugTypeError:
                    Logger.PrintError(LogClass.Gpu, fullMessage);
                    break;
                case DebugType.DebugTypePerformance:
                    Logger.PrintWarning(LogClass.Gpu, fullMessage);
                    break;
                default:
                    Logger.PrintDebug(LogClass.Gpu, fullMessage);
                    break;
            }
        }
    }
}
