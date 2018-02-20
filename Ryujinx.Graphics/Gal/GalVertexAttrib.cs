namespace Ryujinx.Graphics.Gal
{
    public struct GalVertexAttrib
    {
        public int  Index   { get; private set; }
        public int  Buffer  { get; private set; }
        public bool IsConst { get; private set; }
        public int  Offset  { get; private set; }

        public GalVertexAttribSize Size { get; private set; }
        public GalVertexAttribType Type { get; private set; }

        public bool IsBgra { get; private set; }

        public GalVertexAttrib(
            int                 Index,
            int                 Buffer,
            bool                IsConst,
            int                 Offset,
            GalVertexAttribSize Size,
            GalVertexAttribType Type,
            bool                IsBgra)
        {
            this.Index   = Index;
            this.Buffer  = Buffer;
            this.IsConst = IsConst;
            this.Offset  = Offset;
            this.Size    = Size;
            this.Type    = Type;
            this.IsBgra  = IsBgra;
        }
    }
}