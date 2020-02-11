using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class GlobalMemory
    {
        private const int StorageDescsBaseOffset = 0x44; // In words.

        public const int StorageDescSize = 4; // In words.
        public const int StorageMaxCount = 16;

        public const int StorageDescsSize = StorageDescSize * StorageMaxCount;

        public const int UbeBaseOffset = 0x98; // In words.
        public const int UbeMaxCount   = 9;
        public const int UbeDescsSize  = StorageDescSize * UbeMaxCount;
        public const int UbeFirstCbuf  = 8;

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
            return stage switch
            {
                ShaderStage.Compute                => StorageDescsBaseOffset + 2 * StorageDescsSize,
                ShaderStage.Vertex                 => StorageDescsBaseOffset,
                ShaderStage.TessellationControl    => StorageDescsBaseOffset + 1 * StorageDescsSize,
                ShaderStage.TessellationEvaluation => StorageDescsBaseOffset + 2 * StorageDescsSize,
                ShaderStage.Geometry               => StorageDescsBaseOffset + 3 * StorageDescsSize,
                ShaderStage.Fragment               => StorageDescsBaseOffset + 4 * StorageDescsSize,
                _ => 0
            };
        }
    }
}