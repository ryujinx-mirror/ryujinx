using OpenTK.Graphics.OpenGL;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OGLExtension
    {
        private static bool Initialized = false;

        private static bool EnhancedLayouts;

        public static bool HasEnhancedLayouts()
        {
            EnsureInitialized();

            return EnhancedLayouts;
        }

        private static void EnsureInitialized()
        {
            if (Initialized)
            {
                return;
            }

            EnhancedLayouts = HasExtension("GL_ARB_enhanced_layouts");
        }

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
    }
}