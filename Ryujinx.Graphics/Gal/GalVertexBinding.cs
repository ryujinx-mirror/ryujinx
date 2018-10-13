namespace Ryujinx.Graphics.Gal
{
    public struct GalVertexBinding
    {
        //VboKey shouldn't be here, but ARB_vertex_attrib_binding is core since 4.3

        public bool Enabled;
        public int Stride;
        public long VboKey;
        public bool Instanced;
        public int Divisor;
        public GalVertexAttrib[] Attribs;
    }
}