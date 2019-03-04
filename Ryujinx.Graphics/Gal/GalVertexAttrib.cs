namespace Ryujinx.Graphics.Gal
{
    public struct GalVertexAttrib
    {
        public int    Index   { get; private set; }
        public bool   IsConst { get; private set; }
        public int    Offset  { get; private set; }
        public byte[] Data    { get; private set; }

        public GalVertexAttribSize Size { get; private set; }
        public GalVertexAttribType Type { get; private set; }

        public bool IsBgra { get; private set; }

        public GalVertexAttrib(
            int                 index,
            bool                isConst,
            int                 offset,
            byte[]              data,
            GalVertexAttribSize size,
            GalVertexAttribType type,
            bool                isBgra)
        {
            Index   = index;
            IsConst = isConst;
            Data    = data;
            Offset  = offset;
            Size    = size;
            Type    = type;
            IsBgra  = isBgra;
        }
    }
}