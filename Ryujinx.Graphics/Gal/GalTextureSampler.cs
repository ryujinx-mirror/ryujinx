namespace Ryujinx.Graphics.Gal
{
    public struct GalTextureSampler
    {
        public GalTextureWrap AddressU { get; private set; }
        public GalTextureWrap AddressV { get; private set; }
        public GalTextureWrap AddressP { get; private set; }

        public GalTextureFilter    MinFilter { get; private set; }
        public GalTextureFilter    MagFilter { get; private set; }
        public GalTextureMipFilter MipFilter { get; private set; }

        public GalColorF BorderColor { get; private set; }

        public bool             DepthCompare     { get; private set; }
        public DepthCompareFunc DepthCompareFunc { get; private set; }

        public GalTextureSampler(
            GalTextureWrap      AddressU,
            GalTextureWrap      AddressV,
            GalTextureWrap      AddressP,
            GalTextureFilter    MinFilter,
            GalTextureFilter    MagFilter,
            GalTextureMipFilter MipFilter,
            GalColorF           BorderColor,
            bool                DepthCompare,
            DepthCompareFunc    DepthCompareFunc)
        {
            this.AddressU    = AddressU;
            this.AddressV    = AddressV;
            this.AddressP    = AddressP;
            this.MinFilter   = MinFilter;
            this.MagFilter   = MagFilter;
            this.MipFilter   = MipFilter;
            this.BorderColor = BorderColor;

            this.DepthCompare     = DepthCompare;
            this.DepthCompareFunc = DepthCompareFunc;
        }
    }
}