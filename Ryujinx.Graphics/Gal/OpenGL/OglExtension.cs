using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Logging;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    static class OglExtension
    {
        // Private lazy backing variables
        private static Lazy<bool> _enhancedLayouts    = new Lazy<bool>(() => HasExtension("GL_ARB_enhanced_layouts"));
        private static Lazy<bool> _textureMirrorClamp = new Lazy<bool>(() => HasExtension("GL_EXT_texture_mirror_clamp"));
        private static Lazy<bool> _viewportArray      = new Lazy<bool>(() => HasExtension("GL_ARB_viewport_array"));

        private static Lazy<bool> _nvidiaDriver      = new Lazy<bool>(() => IsNvidiaDriver());

        // Public accessors
        public static bool EnhancedLayouts    => _enhancedLayouts.Value;
        public static bool TextureMirrorClamp => _textureMirrorClamp.Value;
        public static bool ViewportArray      => _viewportArray.Value;

        public static bool NvidiaDriver       => _nvidiaDriver.Value;

        private static bool HasExtension(string name)
        {
            int numExtensions = GL.GetInteger(GetPName.NumExtensions);

            for (int extension = 0; extension < numExtensions; extension++)
            {
                if (GL.GetString(StringNameIndexed.Extensions, extension) == name)
                {
                    return true;
                }
            }

            Logger.PrintInfo(LogClass.Gpu, $"OpenGL extension {name} unavailable. You may experience some performance degradation");

            return false;
        }

        private static bool IsNvidiaDriver()
        {
            return GL.GetString(StringName.Vendor).Equals("NVIDIA Corporation");
        }

        public static class Required
        {
            // Public accessors
            public static bool EnhancedLayouts    => _enhancedLayoutsRequired.Value;
            public static bool TextureMirrorClamp => _textureMirrorClampRequired.Value;
            public static bool ViewportArray      => _viewportArrayRequired.Value;

            // Private lazy backing variables
            private static Lazy<bool> _enhancedLayoutsRequired    = new Lazy<bool>(() => HasExtensionRequired(OglExtension.EnhancedLayouts,    "GL_ARB_enhanced_layouts"));
            private static Lazy<bool> _textureMirrorClampRequired = new Lazy<bool>(() => HasExtensionRequired(OglExtension.TextureMirrorClamp, "GL_EXT_texture_mirror_clamp"));
            private static Lazy<bool> _viewportArrayRequired      = new Lazy<bool>(() => HasExtensionRequired(OglExtension.ViewportArray,      "GL_ARB_viewport_array"));

            private static bool HasExtensionRequired(bool value, string name)
            {
                if (value)
                {
                    return true;
                }

                Logger.PrintWarning(LogClass.Gpu, $"Required OpenGL extension {name} unavailable. You may experience some rendering issues");

                return false;
            }
        }
    }
}
