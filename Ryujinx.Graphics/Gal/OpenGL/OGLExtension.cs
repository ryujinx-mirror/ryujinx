using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OGLExtension
    {
        // Private lazy backing variables
        private static Lazy<bool> s_EnhancedLayouts    = new Lazy<bool>(() => HasExtension("GL_ARB_enhanced_layouts"));
        private static Lazy<bool> s_TextureMirrorClamp = new Lazy<bool>(() => HasExtension("GL_EXT_texture_mirror_clamp"));
        private static Lazy<bool> s_ViewportArray      = new Lazy<bool>(() => HasExtension("GL_ARB_viewport_array"));

        private static Lazy<bool> s_NvidiaDriver      = new Lazy<bool>(() => IsNvidiaDriver());

        // Public accessors
        public static bool EnhancedLayouts    => s_EnhancedLayouts.Value;
        public static bool TextureMirrorClamp => s_TextureMirrorClamp.Value;
        public static bool ViewportArray      => s_ViewportArray.Value;

        public static bool NvidiaDrvier       => s_NvidiaDriver.Value;

        private static bool HasExtension(string Name)
        {
            int NumExtensions = GL.GetInteger(GetPName.NumExtensions);

            for (int Extension = 0; Extension < NumExtensions; Extension++)
            {
                if (GL.GetString(StringNameIndexed.Extensions, Extension) == Name)
                {
                    return true;
                }
            }

            Logger.PrintInfo(LogClass.Gpu, $"OpenGL extension {Name} unavailable. You may experience some performance degredation");

            return false;
        }

        private static bool IsNvidiaDriver()
        {
            return GL.GetString(StringName.Vendor).Equals("NVIDIA Corporation");
        }

        public static class Required
        {
            // Public accessors
            public static bool EnhancedLayouts    => s_EnhancedLayoutsRequired.Value;
            public static bool TextureMirrorClamp => s_TextureMirrorClampRequired.Value;
            public static bool ViewportArray      => s_ViewportArrayRequired.Value;

            // Private lazy backing variables
            private static Lazy<bool> s_EnhancedLayoutsRequired    = new Lazy<bool>(() => HasExtensionRequired(OGLExtension.EnhancedLayouts,    "GL_ARB_enhanced_layouts"));
            private static Lazy<bool> s_TextureMirrorClampRequired = new Lazy<bool>(() => HasExtensionRequired(OGLExtension.TextureMirrorClamp, "GL_EXT_texture_mirror_clamp"));
            private static Lazy<bool> s_ViewportArrayRequired      = new Lazy<bool>(() => HasExtensionRequired(OGLExtension.ViewportArray,      "GL_ARB_viewport_array"));

            private static bool HasExtensionRequired(bool Value, string Name)
            {
                if (Value)
                {
                    return true;
                }

                Logger.PrintWarning(LogClass.Gpu, $"Required OpenGL extension {Name} unavailable. You may experience some rendering issues");

                return false;
            }
        }
    }
}
