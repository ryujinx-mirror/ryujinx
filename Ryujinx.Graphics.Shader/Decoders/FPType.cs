using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.Decoders
{
    enum FPType
    {
        FP16 = 1,
        FP32 = 2,
        FP64 = 3
    }

    static class FPTypeExtensions
    {
        public static Instruction ToInstFPType(this FPType type)
        {
            return type == FPType.FP64 ? Instruction.FP64 : Instruction.FP32;
        }
    }
}