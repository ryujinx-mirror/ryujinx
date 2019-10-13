using Ryujinx.Common;
using System;

namespace Ryujinx.Graphics.Texture
{
    public struct Size
    {
        public int Width  { get; }
        public int Height { get; }
        public int Depth  { get; }

        public Size(int width, int height, int depth)
        {
            Width  = width;
            Height = height;
            Depth  = depth;
        }
    }
}