using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.State
{
    static class GpuStateTable
    {
        public struct TableItem
        {
            public MethodOffset Offset { get; }

            public int Size  { get; }
            public int Count { get; }

            public TableItem(MethodOffset offset, Type type, int count)
            {
                int sizeInBytes = Marshal.SizeOf(type);

                Debug.Assert((sizeInBytes & 3) == 0);

                Offset = offset;
                Size   = sizeInBytes / 4;
                Count  = count;
            }
        }

        public static TableItem[] Table = new TableItem[]
        {
            new TableItem(MethodOffset.RtColorState,           typeof(RtColorState),          8),
            new TableItem(MethodOffset.ViewportTransform,      typeof(ViewportTransform),     8),
            new TableItem(MethodOffset.ViewportExtents,        typeof(ViewportExtents),       8),
            new TableItem(MethodOffset.VertexBufferDrawState,  typeof(VertexBufferDrawState), 1),
            new TableItem(MethodOffset.DepthBiasState,         typeof(DepthBiasState),        1),
            new TableItem(MethodOffset.StencilBackMasks,       typeof(StencilBackMasks),      1),
            new TableItem(MethodOffset.RtDepthStencilState,    typeof(RtDepthStencilState),   1),
            new TableItem(MethodOffset.VertexAttribState,      typeof(VertexAttribState),     16),
            new TableItem(MethodOffset.RtDepthStencilSize,     typeof(Size3D),                1),
            new TableItem(MethodOffset.BlendEnable,            typeof(Boolean32),             8),
            new TableItem(MethodOffset.StencilTestState,       typeof(StencilTestState),      1),
            new TableItem(MethodOffset.SamplerPoolState,       typeof(PoolState),             1),
            new TableItem(MethodOffset.TexturePoolState,       typeof(PoolState),             1),
            new TableItem(MethodOffset.StencilBackTestState,   typeof(StencilBackTestState),  1),
            new TableItem(MethodOffset.ShaderBaseAddress,      typeof(GpuVa),                 1),
            new TableItem(MethodOffset.PrimitiveRestartState,  typeof(PrimitiveRestartState), 1),
            new TableItem(MethodOffset.IndexBufferState,       typeof(IndexBufferState),      1),
            new TableItem(MethodOffset.VertexBufferInstanced,  typeof(Boolean32),             16),
            new TableItem(MethodOffset.FaceState,              typeof(FaceState),             1),
            new TableItem(MethodOffset.RtColorMask,            typeof(RtColorMask),           8),
            new TableItem(MethodOffset.VertexBufferState,      typeof(VertexBufferState),     16),
            new TableItem(MethodOffset.BlendState,             typeof(BlendState),            8),
            new TableItem(MethodOffset.VertexBufferEndAddress, typeof(GpuVa),                 16),
            new TableItem(MethodOffset.ShaderState,            typeof(ShaderState),           6),
        };
    }
}