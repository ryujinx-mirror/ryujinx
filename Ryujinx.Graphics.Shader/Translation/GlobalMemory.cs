using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class GlobalMemory
    {
        private const int StorageDescsBaseOffset = 0x44; // In words.

        public const int StorageDescSize = 4; // In words.
        public const int StorageMaxCount = 16;

        public const int StorageDescsSize = StorageDescSize * StorageMaxCount;

        public static bool UsesGlobalMemory(Instruction inst)
        {
            return (inst.IsAtomic() && IsGlobalMr(inst)) ||
                    inst == Instruction.LoadGlobal ||
                    inst == Instruction.StoreGlobal;
        }

        private static bool IsGlobalMr(Instruction inst)
        {
            return (inst & Instruction.MrMask) == Instruction.MrGlobal;
        }

        public static int GetStorageCbOffset(ShaderStage stage, int slot)
        {
            return GetStorageBaseCbOffset(stage) + slot * StorageDescSize;
        }

        public static int GetStorageBaseCbOffset(ShaderStage stage)
        {
            switch (stage)
            {
                case ShaderStage.Compute:                return StorageDescsBaseOffset + 2 * StorageDescsSize;
                case ShaderStage.Vertex:                 return StorageDescsBaseOffset;
                case ShaderStage.TessellationControl:    return StorageDescsBaseOffset + 1 * StorageDescsSize;
                case ShaderStage.TessellationEvaluation: return StorageDescsBaseOffset + 2 * StorageDescsSize;
                case ShaderStage.Geometry:               return StorageDescsBaseOffset + 3 * StorageDescsSize;
                case ShaderStage.Fragment:               return StorageDescsBaseOffset + 4 * StorageDescsSize;
            }

            return 0;
        }
    }
}