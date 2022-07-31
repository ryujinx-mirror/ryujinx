using Ryujinx.Common.Memory;
using Silk.NET.Vulkan;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Ryujinx.Graphics.Vulkan
{
    struct PipelineUid : IRefEquatable<PipelineUid>
    {
        public ulong Id0;
        public ulong Id1;
        public ulong Id2;
        public ulong Id3;

        public ulong Id4;
        public ulong Id5;
        public ulong Id6;
        public ulong Id7;

        public ulong Id8;
        public ulong Id9;
        public ulong Id10;
        public ulong Id11;

        private uint VertexAttributeDescriptionsCount => (byte)((Id8 >> 38) & 0xFF);
        private uint VertexBindingDescriptionsCount => (byte)((Id8 >> 46) & 0xFF);
        private uint ViewportsCount => (byte)((Id8 >> 54) & 0xFF);
        private uint ScissorsCount => (byte)((Id9 >> 0) & 0xFF);
        private uint ColorBlendAttachmentStateCount => (byte)((Id9 >> 8) & 0xFF);
        private bool HasDepthStencil => ((Id9 >> 63) & 0x1) != 0UL;

        public Array32<VertexInputAttributeDescription> VertexAttributeDescriptions;
        public Array33<VertexInputBindingDescription> VertexBindingDescriptions;
        public Array16<Viewport> Viewports;
        public Array16<Rect2D> Scissors;
        public Array8<PipelineColorBlendAttachmentState> ColorBlendAttachmentState;
        public Array9<Format> AttachmentFormats;

        public override bool Equals(object obj)
        {
            return obj is PipelineUid other && Equals(other);
        }

        public bool Equals(ref PipelineUid other)
        {
            if (!Unsafe.As<ulong, Vector256<byte>>(ref Id0).Equals(Unsafe.As<ulong, Vector256<byte>>(ref other.Id0)) ||
                !Unsafe.As<ulong, Vector256<byte>>(ref Id4).Equals(Unsafe.As<ulong, Vector256<byte>>(ref other.Id4)) ||
                !Unsafe.As<ulong, Vector256<byte>>(ref Id8).Equals(Unsafe.As<ulong, Vector256<byte>>(ref other.Id8)))
            {
                return false;
            }

            if (!SequenceEqual<VertexInputAttributeDescription>(VertexAttributeDescriptions.ToSpan(), other.VertexAttributeDescriptions.ToSpan(), VertexAttributeDescriptionsCount))
            {
                return false;
            }

            if (!SequenceEqual<VertexInputBindingDescription>(VertexBindingDescriptions.ToSpan(), other.VertexBindingDescriptions.ToSpan(), VertexBindingDescriptionsCount))
            {
                return false;
            }

            if (!SequenceEqual<PipelineColorBlendAttachmentState>(ColorBlendAttachmentState.ToSpan(), other.ColorBlendAttachmentState.ToSpan(), ColorBlendAttachmentStateCount))
            {
                return false;
            }

            if (!SequenceEqual<Format>(AttachmentFormats.ToSpan(), other.AttachmentFormats.ToSpan(), ColorBlendAttachmentStateCount + (HasDepthStencil ? 1u : 0u)))
            {
                return false;
            }

            return true;
        }

        private static bool SequenceEqual<T>(ReadOnlySpan<T> x, ReadOnlySpan<T> y, uint count) where T : unmanaged
        {
            return MemoryMarshal.Cast<T, byte>(x.Slice(0, (int)count)).SequenceEqual(MemoryMarshal.Cast<T, byte>(y.Slice(0, (int)count)));
        }

        public override int GetHashCode()
        {
            ulong hash64 = Id0 * 23 ^
                           Id1 * 23 ^
                           Id2 * 23 ^
                           Id3 * 23 ^
                           Id4 * 23 ^
                           Id5 * 23 ^
                           Id6 * 23 ^
                           Id7 * 23 ^
                           Id8 * 23 ^
                           Id9 * 23 ^
                           Id10 * 23 ^
                           Id11 * 23;

            for (int i = 0; i < (int)VertexAttributeDescriptionsCount; i++)
            {
                hash64 ^= VertexAttributeDescriptions[i].Binding * 23;
                hash64 ^= (uint)VertexAttributeDescriptions[i].Format * 23;
                hash64 ^= VertexAttributeDescriptions[i].Location * 23;
                hash64 ^= VertexAttributeDescriptions[i].Offset * 23;
            }

            for (int i = 0; i < (int)VertexBindingDescriptionsCount; i++)
            {
                hash64 ^= VertexBindingDescriptions[i].Binding * 23;
                hash64 ^= (uint)VertexBindingDescriptions[i].InputRate * 23;
                hash64 ^= VertexBindingDescriptions[i].Stride * 23;
            }

            for (int i = 0; i < (int)ColorBlendAttachmentStateCount; i++)
            {
                hash64 ^= ColorBlendAttachmentState[i].BlendEnable * 23;
                hash64 ^= (uint)ColorBlendAttachmentState[i].SrcColorBlendFactor * 23;
                hash64 ^= (uint)ColorBlendAttachmentState[i].DstColorBlendFactor * 23;
                hash64 ^= (uint)ColorBlendAttachmentState[i].ColorBlendOp * 23;
                hash64 ^= (uint)ColorBlendAttachmentState[i].SrcAlphaBlendFactor * 23;
                hash64 ^= (uint)ColorBlendAttachmentState[i].DstAlphaBlendFactor * 23;
                hash64 ^= (uint)ColorBlendAttachmentState[i].AlphaBlendOp * 23;
                hash64 ^= (uint)ColorBlendAttachmentState[i].ColorWriteMask * 23;
            }

            for (int i = 0; i < (int)ColorBlendAttachmentStateCount; i++)
            {
                hash64 ^= (uint)AttachmentFormats[i] * 23;
            }

            return (int)hash64 ^ ((int)(hash64 >> 32) * 17);
        }
    }
}
