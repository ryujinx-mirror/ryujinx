using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OGLExtension
    {
        private static Lazy<bool> s_EnhancedLayouts    = new Lazy<bool>(() => HasExtension("GL_ARB_enhanced_layouts"));
        private static Lazy<bool> s_TextureMirrorClamp = new Lazy<bool>(() => HasExtension("GL_EXT_texture_mirror_clamp"));
        private static Lazy<bool> s_ViewportArray      = new Lazy<bool>(() => HasExtension("GL_ARB_viewport_array"));

        private static Lazy<bool> s_NvidiaDriver      = new Lazy<bool>(() => IsNvidiaDriver());

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

            return false;
        }

        private static bool IsNvidiaDriver() {
            return GL.GetString(StringName.Vendor).Equals("NVIDIA Corporation");
        }
    }
}