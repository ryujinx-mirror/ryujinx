namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class CommentNode : Operation
    {
        public string Comment { get; }

        public CommentNode(string comment) : base(Instruction.Comment, null)
        {
            Comment = comment;
        }
    }
}
