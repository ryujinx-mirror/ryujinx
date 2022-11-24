namespace Ryujinx.Graphics.GAL
{
    public struct BufferAssignment
    {
        public readonly int Binding;
        public readonly BufferRange Range;

        public BufferAssignment(int binding, BufferRange range)
        {
            Binding = binding;
            Range = range;
        }
    }
}