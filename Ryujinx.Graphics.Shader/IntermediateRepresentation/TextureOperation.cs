namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class TextureOperation : Operation
    {
        public const int DefaultCbufSlot = -1;

        public SamplerType Type { get; set; }
        public TextureFormat Format { get; set; }
        public TextureFlags Flags { get; private set; }

        public int CbufSlot { get; private set; }
        public int Handle { get; private set; }

        public TextureOperation(
            Instruction inst,
            SamplerType type,
            TextureFormat format,
            TextureFlags flags,
            int cbufSlot,
            int handle,
            int compIndex,
            Operand dest,
            Operand[] sources) : base(inst, compIndex, dest, sources)
        {
            Type = type;
            Format = format;
            Flags = flags;
            CbufSlot = cbufSlot;
            Handle = handle;
        }

        public TextureOperation(
            Instruction inst,
            SamplerType type,
            TextureFormat format,
            TextureFlags flags,
            int handle,
            int compIndex,
            Operand dest,
            Operand[] sources) : this(inst, type, format, flags, DefaultCbufSlot, handle, compIndex, dest, sources)
        {
        }

        public void TurnIntoIndexed(int handle)
        {
            Type |= SamplerType.Indexed;
            Flags &= ~TextureFlags.Bindless;
            Handle = handle;
        }

        public void SetHandle(int handle, int cbufSlot = DefaultCbufSlot)
        {
            if ((Flags & TextureFlags.Bindless) != 0)
            {
                Flags &= ~TextureFlags.Bindless;

                RemoveSource(0);
            }

            CbufSlot = cbufSlot;
            Handle = handle;
        }
    }
}