namespace Ryujinx.Graphics.GAL
{
    public readonly record struct VertexAttribDescriptor(int BufferIndex, int Offset, bool IsZero, Format Format);
}
