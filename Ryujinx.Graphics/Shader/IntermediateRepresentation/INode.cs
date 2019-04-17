namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    interface INode
    {
        Operand Dest { get; set; }

        int SourcesCount { get; }

        Operand GetSource(int index);

        void SetSource(int index, Operand operand);
    }
}