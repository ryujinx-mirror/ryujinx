using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Shader
{
    public enum TextureHandleType
    {
        CombinedSampler = 0, // Must be 0.
        SeparateSamplerHandle = 1,
        SeparateSamplerId = 2
    }

    public static class TextureHandle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PackSlots(int cbufSlot0, int cbufSlot1)
        {
            return cbufSlot0 | ((cbufSlot1 + 1) << 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int, int) UnpackSlots(int slots, int defaultTextureBufferIndex)
        {
            int textureBufferIndex;
            int samplerBufferIndex;

            if (slots < 0)
            {
                textureBufferIndex = defaultTextureBufferIndex;
                samplerBufferIndex = textureBufferIndex;
            }
            else
            {
                uint high = (uint)slots >> 16;

                textureBufferIndex = (ushort)slots;
                samplerBufferIndex = high != 0 ? (int)high - 1 : textureBufferIndex;
            }

            return (textureBufferIndex, samplerBufferIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PackOffsets(int cbufOffset0, int cbufOffset1, TextureHandleType type)
        {
            return cbufOffset0 | (cbufOffset1 << 14) | ((int)type << 28);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int, int, TextureHandleType) UnpackOffsets(int handle)
        {
            return (handle & 0x3fff, (handle >> 14) & 0x3fff, (TextureHandleType)((uint)handle >> 28));
        }
    }
}
