using OpenTK.Graphics.OpenGL;
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

            int[] array = null;

            GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DontCare, DebugSeverityControl.DontCare, 0, array, true);

            _debugCallback = PrintDbg;

            GL.DebugMessageCallback(_debugCallback, IntPtr.Zero);
        }

        private static void PrintDbg(
            DebugSource source,
            DebugType type,
            int id,
            DebugSeverity severity,
            int length,
            IntPtr message,
            IntPtr userParam)
        {
            string msg = Marshal.PtrToStringAnsi(message);

            if (type == DebugType.DebugTypeError && !msg.Contains("link"))
            {
                throw new Exception(msg);
            }

            System.Console.WriteLine("GL message: " + source + " " + type + " " + severity + " " + msg);
        }
    }
}
