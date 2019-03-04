using System;

namespace Ryujinx.Graphics.Gal
{
    class ShaderException : Exception
    {
        public ShaderException() : base() { }

        public ShaderException(string message) : base(message) { }
    }
}