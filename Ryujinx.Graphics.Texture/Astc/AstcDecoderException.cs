using System;

namespace Ryujinx.Graphics.Texture.Astc
{
    public class AstcDecoderException : Exception
    {
        public AstcDecoderException(string exMsg) : base(exMsg) { }
    }
}