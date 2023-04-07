namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class AstComment : AstNode
    {
        public string Comment { get; }

        public AstComment(string comment)
        {
            Comment = comment;
        }
    }
}