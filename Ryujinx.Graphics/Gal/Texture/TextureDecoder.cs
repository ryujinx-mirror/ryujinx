using System;

namespace Ryujinx.Graphics.Gal.Texture
{
    static class TextureDecoder
    {
        public static byte[] Decode(GalTexture Texture)
        {
            switch (Texture.Format)
            {
                case GalTextureFormat.BC1: return BCn.DecodeBC1(Texture, 0);
                case GalTextureFormat.BC2: return BCn.DecodeBC2(Texture, 0);
                case GalTextureFormat.BC3: return BCn.DecodeBC3(Texture, 0);
            }

            throw new NotImplementedException(Texture.Format.ToString());
        }
    }
}