using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL.Image
{
    class TextureBase
    {
        public int Handle { get; protected set; }

        public TextureCreateInfo Info { get; }

        public int Width { get; }
        public int Height { get; }
        public float ScaleFactor { get; }

        public Target Target => Info.Target;
        public Format Format => Info.Format;

        public TextureBase(TextureCreateInfo info, float scaleFactor = 1f)
        {
            Info = info;
            Width = (int)Math.Ceiling(Info.Width * scaleFactor);
            Height = (int)Math.Ceiling(Info.Height * scaleFactor);
            ScaleFactor = scaleFactor;

            Handle = GL.GenTexture();
        }

        public void Bind(int unit)
        {
            Bind(Target.Convert(), unit);
        }

        protected void Bind(TextureTarget target, int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            GL.BindTexture(target, Handle);
        }
    }
}
