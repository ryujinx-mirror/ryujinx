namespace Ryujinx.Graphics.Shader
{
    public struct CBufferDescriptor
    {
        public string Name { get; }

        public int Slot { get; }

        public CBufferDescriptor(string name, int slot)
        {
            Name = name;
            Slot = slot;
        }
    }
}