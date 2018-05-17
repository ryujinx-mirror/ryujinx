namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrCmnt : ShaderIrNode
    {
        public string Comment { get; private set; }

        public ShaderIrCmnt(string Comment)
        {
            this.Comment = Comment;
        }
    }
}