using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Image
{
    struct TextureBindingInfo
    {
        public Target Target { get; }

        public int Handle { get; }

        public bool IsBindless { get; }

        public int CbufSlot   { get; }
        public int CbufOffset { get; }

        public TextureBindingInfo(Target target, int handle)
        {
            Target = target;
            Handle = handle;

            IsBindless = false;

            CbufSlot   = 0;
            CbufOffset = 0;
        }

        public TextureBindingInfo(Target target, int cbufSlot, int cbufOffset)
        {
            Target = target;
            Handle = 0;

            IsBindless = true;

            CbufSlot   = cbufSlot;
            CbufOffset = cbufOffset;
        }
    }
}