using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Shader;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine.Threed.ComputeDraw
{
    /// <summary>
    /// Vertex info buffer data updater.
    /// </summary>
    class VertexInfoBufferUpdater : BufferUpdater
    {
        private VertexInfoBuffer _data;

        /// <summary>
        /// Creates a new instance of the vertex info buffer updater.
        /// </summary>
        /// <param name="renderer">Renderer that the vertex info buffer will be used with</param>
        public VertexInfoBufferUpdater(IRenderer renderer) : base(renderer)
        {
        }

        /// <summary>
        /// Sets vertex data related counts.
        /// </summary>
        /// <param name="vertexCount">Number of vertices used on the draw</param>
        /// <param name="instanceCount">Number of draw instances</param>
        /// <param name="firstVertex">Index of the first vertex on the vertex buffer</param>
        /// <param name="firstInstance">Index of the first instanced vertex on the vertex buffer</param>
        public void SetVertexCounts(int vertexCount, int instanceCount, int firstVertex, int firstInstance)
        {
            if (_data.VertexCounts.X != vertexCount)
            {
                _data.VertexCounts.X = vertexCount;
                MarkDirty(VertexInfoBuffer.VertexCountsOffset, sizeof(int));
            }

            if (_data.VertexCounts.Y != instanceCount)
            {
                _data.VertexCounts.Y = instanceCount;
                MarkDirty(VertexInfoBuffer.VertexCountsOffset + sizeof(int), sizeof(int));
            }

            if (_data.VertexCounts.Z != firstVertex)
            {
                _data.VertexCounts.Z = firstVertex;
                MarkDirty(VertexInfoBuffer.VertexCountsOffset + sizeof(int) * 2, sizeof(int));
            }

            if (_data.VertexCounts.W != firstInstance)
            {
                _data.VertexCounts.W = firstInstance;
                MarkDirty(VertexInfoBuffer.VertexCountsOffset + sizeof(int) * 3, sizeof(int));
            }
        }

        /// <summary>
        /// Sets vertex data related counts.
        /// </summary>
        /// <param name="primitivesCount">Number of primitives consumed by the geometry shader</param>
        public void SetGeometryCounts(int primitivesCount)
        {
            if (_data.GeometryCounts.X != primitivesCount)
            {
                _data.GeometryCounts.X = primitivesCount;
                MarkDirty(VertexInfoBuffer.GeometryCountsOffset, sizeof(int));
            }
        }

        /// <summary>
        /// Sets a vertex stride and related data.
        /// </summary>
        /// <param name="index">Index of the vertex stride to be updated</param>
        /// <param name="stride">Stride divided by the component or format size</param>
        /// <param name="componentCount">Number of components that the format has</param>
        public void SetVertexStride(int index, int stride, int componentCount)
        {
            if (_data.VertexStrides[index].X != stride)
            {
                _data.VertexStrides[index].X = stride;
                MarkDirty(VertexInfoBuffer.VertexStridesOffset + index * Unsafe.SizeOf<Vector4<int>>(), sizeof(int));
            }

            for (int c = 1; c < 4; c++)
            {
                int value = c < componentCount ? 1 : 0;

                ref int currentValue = ref GetElementRef(ref _data.VertexStrides[index], c);

                if (currentValue != value)
                {
                    currentValue = value;
                    MarkDirty(VertexInfoBuffer.VertexStridesOffset + index * Unsafe.SizeOf<Vector4<int>>() + c * sizeof(int), sizeof(int));
                }
            }
        }

        /// <summary>
        /// Sets a vertex offset and related data.
        /// </summary>
        /// <param name="index">Index of the vertex offset to be updated</param>
        /// <param name="offset">Offset divided by the component or format size</param>
        /// <param name="divisor">If the draw is instanced, should have the vertex divisor value, otherwise should be zero</param>
        public void SetVertexOffset(int index, int offset, int divisor)
        {
            if (_data.VertexOffsets[index].X != offset)
            {
                _data.VertexOffsets[index].X = offset;
                MarkDirty(VertexInfoBuffer.VertexOffsetsOffset + index * Unsafe.SizeOf<Vector4<int>>(), sizeof(int));
            }

            if (_data.VertexOffsets[index].Y != divisor)
            {
                _data.VertexOffsets[index].Y = divisor;
                MarkDirty(VertexInfoBuffer.VertexOffsetsOffset + index * Unsafe.SizeOf<Vector4<int>>() + sizeof(int), sizeof(int));
            }
        }

        /// <summary>
        /// Sets the offset of the index buffer.
        /// </summary>
        /// <param name="offset">Offset divided by the component size</param>
        public void SetIndexBufferOffset(int offset)
        {
            if (_data.GeometryCounts.W != offset)
            {
                _data.GeometryCounts.W = offset;
                MarkDirty(VertexInfoBuffer.GeometryCountsOffset + sizeof(int) * 3, sizeof(int));
            }
        }

        /// <summary>
        /// Submits all pending buffer updates to the GPU.
        /// </summary>
        public void Commit()
        {
            Commit(MemoryMarshal.Cast<VertexInfoBuffer, byte>(MemoryMarshal.CreateSpan(ref _data, 1)));
        }
    }
}
