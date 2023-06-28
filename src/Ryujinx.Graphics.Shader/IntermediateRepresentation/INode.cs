namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    interface INode
    {
        Operand Dest { get; set; }

        int DestsCount { get; }
        int SourcesCount { get; }

        Operand GetDest(int index);
        Operand GetSource(int index);

        void SetSource(int index, Operand operand);
    }
}
