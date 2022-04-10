using System;

namespace Ryujinx.Graphics.Shader
{
    /// <summary>
    /// GPU state access interface.
    /// </summary>
    public interface IGpuAccessor
    {
        /// <summary>
        /// Prints a log message.
        /// </summary>
        /// <param name="message">Message to print</param>
        void Log(string message)
        {
            // No default log output.
        }

        /// <summary>
        /// Reads data from the constant buffer 1.
        /// </summary>
        /// <param name="offset">Offset in bytes to read from</param>
        /// <returns>Value at the given offset</returns>
        uint ConstantBuffer1Read(int offset)
        {
            return 0;
        }

        /// <summary>
        /// Gets a span of the specified memory location, containing shader code.
        /// </summary>
        /// <param name="address">GPU virtual address of the data</param>
        /// <param name="minimumSize">Minimum size that the returned span may have</param>
        /// <returns>Span of the memory location</returns>
        ReadOnlySpan<ulong> GetCode(ulong address, int minimumSize);

        /// <summary>
        /// Queries the binding number of a constant buffer.
        /// </summary>
        /// <param name="index">Constant buffer index</param>
        /// <returns>Binding number</returns>
        int QueryBindingConstantBuffer(int index)
        {
            return index;
        }

        /// <summary>
        /// Queries the binding number of a storage buffer.
        /// </summary>
        /// <param name="index">Storage buffer index</param>
        /// <returns>Binding number</returns>
        int QueryBindingStorageBuffer(int index)
        {
            return index;
        }

        /// <summary>
        /// Queries the binding number of a texture.
        /// </summary>
        /// <param name="index">Texture index</param>
        /// <returns>Binding number</returns>
        int QueryBindingTexture(int index)
        {
            return index;
        }

        /// <summary>
        /// Queries the binding number of an image.
        /// </summary>
        /// <param name="index">Image index</param>
        /// <returns>Binding number</returns>
        int QueryBindingImage(int index)
        {
            return index;
        }

        /// <summary>
        /// Queries Local Size X for compute shaders.
        /// </summary>
        /// <returns>Local Size X</returns>
        int QueryComputeLocalSizeX()
        {
            return 1;
        }

        /// <summary>
        /// Queries Local Size Y for compute shaders.
        /// </summary>
        /// <returns>Local Size Y</returns>
        int QueryComputeLocalSizeY()
        {
            return 1;
        }

        /// <summary>
        /// Queries Local Size Z for compute shaders.
        /// </summary>
        /// <returns>Local Size Z</returns>
        int QueryComputeLocalSizeZ()
        {
            return 1;
        }

        /// <summary>
        /// Queries Local Memory size in bytes for compute shaders.
        /// </summary>
        /// <returns>Local Memory size in bytes</returns>
        int QueryComputeLocalMemorySize()
        {
            return 0x1000;
        }

        /// <summary>
        /// Queries Shared Memory size in bytes for compute shaders.
        /// </summary>
        /// <returns>Shared Memory size in bytes</returns>
        int QueryComputeSharedMemorySize()
        {
            return 0xc000;
        }

        /// <summary>
        /// Queries Constant Buffer usage information.
        /// </summary>
        /// <returns>A mask where each bit set indicates a bound constant buffer</returns>
        uint QueryConstantBufferUse()
        {
            return 0;
        }

        /// <summary>
        /// Queries host about the presence of the FrontFacing built-in variable bug.
        /// </summary>
        /// <returns>True if the bug is present on the host device used, false otherwise</returns>
        bool QueryHostHasFrontFacingBug()
        {
            return false;
        }

        /// <summary>
        /// Queries host about the presence of the vector indexing bug.
        /// </summary>
        /// <returns>True if the bug is present on the host device used, false otherwise</returns>
        bool QueryHostHasVectorIndexingBug()
        {
            return false;
        }

        /// <summary>
        /// Queries host storage buffer alignment required.
        /// </summary>
        /// <returns>Host storage buffer alignment in bytes</returns>
        int QueryHostStorageBufferOffsetAlignment()
        {
            return 16;
        }

        /// <summary>
        /// Queries host support for texture formats with BGRA component order (such as BGRA8).
        /// </summary>
        /// <returns>True if BGRA formats are supported, false otherwise</returns>
        bool QueryHostSupportsBgraFormat()
        {
            return true;
        }

        /// <summary>
        /// Queries host support for fragment shader ordering critical sections on the shader code.
        /// </summary>
        /// <returns>True if fragment shader interlock is supported, false otherwise</returns>
        bool QueryHostSupportsFragmentShaderInterlock()
        {
            return true;
        }

        /// <summary>
        /// Queries host support for fragment shader ordering scoped critical sections on the shader code.
        /// </summary>
        /// <returns>True if fragment shader ordering is supported, false otherwise</returns>
        bool QueryHostSupportsFragmentShaderOrderingIntel()
        {
            return false;
        }

        /// <summary>
        /// Queries host support for readable images without a explicit format declaration on the shader.
        /// </summary>
        /// <returns>True if formatted image load is supported, false otherwise</returns>
        bool QueryHostSupportsImageLoadFormatted()
        {
            return true;
        }

        /// <summary>
        /// Queries host GPU non-constant texture offset support.
        /// </summary>
        /// <returns>True if the GPU and driver supports non-constant texture offsets, false otherwise</returns>
        bool QueryHostSupportsNonConstantTextureOffset()
        {
            return true;
        }

        /// <summary>
        /// Queries host GPU shader ballot support.
        /// </summary>
        /// <returns>True if the GPU and driver supports shader ballot, false otherwise</returns>
        bool QueryHostSupportsShaderBallot()
        {
            return true;
        }

        /// <summary>
        /// Queries host GPU texture shadow LOD support.
        /// </summary>
        /// <returns>True if the GPU and driver supports texture shadow LOD, false otherwise</returns>
        bool QueryHostSupportsTextureShadowLod()
        {
            return true;
        }

        /// <summary>
        /// Queries sampler type information.
        /// </summary>
        /// <param name="handle">Texture handle</param>
        /// <param name="cbufSlot">Constant buffer slot for the texture handle</param>
        /// <returns>The sampler type value for the given handle</returns>
        SamplerType QuerySamplerType(int handle, int cbufSlot = -1)
        {
            return SamplerType.Texture2D;
        }

        /// <summary>
        /// Queries texture coordinate normalization information.
        /// </summary>
        /// <param name="handle">Texture handle</param>
        /// <param name="cbufSlot">Constant buffer slot for the texture handle</param>
        /// <returns>True if the coordinates are normalized, false otherwise</returns>
        bool QueryTextureCoordNormalized(int handle, int cbufSlot = -1)
        {
            return false;
        }

        /// <summary>
        /// Queries current primitive topology for geometry shaders.
        /// </summary>
        /// <returns>Current primitive topology</returns>
        InputTopology QueryPrimitiveTopology()
        {
            return InputTopology.Points;
        }

        /// <summary>
        /// Queries the tessellation evaluation shader primitive winding order.
        /// </summary>
        /// <returns>True if the primitive winding order is clockwise, false if counter-clockwise</returns>
        bool QueryTessCw()
        {
            return false;
        }

        /// <summary>
        /// Queries the tessellation evaluation shader abstract patch type.
        /// </summary>
        /// <returns>Abstract patch type</returns>
        TessPatchType QueryTessPatchType()
        {
            return TessPatchType.Triangles;
        }

        /// <summary>
        /// Queries the tessellation evaluation shader spacing between tessellated vertices of the patch.
        /// </summary>
        /// <returns>Spacing between tessellated vertices of the patch</returns>
        TessSpacing QueryTessSpacing()
        {
            return TessSpacing.EqualSpacing;
        }

        /// <summary>
        /// Queries texture format information, for shaders using image load or store.
        /// </summary>
        /// <remarks>
        /// This only returns non-compressed color formats.
        /// If the format of the texture is a compressed, depth or unsupported format, then a default value is returned.
        /// </remarks>
        /// <param name="handle">Texture handle</param>
        /// <param name="cbufSlot">Constant buffer slot for the texture handle</param>
        /// <returns>Color format of the non-compressed texture</returns>
        TextureFormat QueryTextureFormat(int handle, int cbufSlot = -1)
        {
            return TextureFormat.R8G8B8A8Unorm;
        }

        /// <summary>
        /// Queries transform feedback enable state.
        /// </summary>
        /// <returns>True if the shader uses transform feedback, false otherwise</returns>
        bool QueryTransformFeedbackEnabled()
        {
            return false;
        }

        /// <summary>
        /// Queries the varying locations that should be written to the transform feedback buffer.
        /// </summary>
        /// <param name="bufferIndex">Index of the transform feedback buffer</param>
        /// <returns>Varying locations for the specified buffer</returns>
        ReadOnlySpan<byte> QueryTransformFeedbackVaryingLocations(int bufferIndex)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        /// <summary>
        /// Queries the stride (in bytes) of the per vertex data written into the transform feedback buffer.
        /// </summary>
        /// <param name="bufferIndex">Index of the transform feedback buffer</param>
        /// <returns>Stride for the specified buffer</returns>
        int QueryTransformFeedbackStride(int bufferIndex)
        {
            return 0;
        }

        /// <summary>
        /// Queries if host state forces early depth testing.
        /// </summary>
        /// <returns>True if early depth testing is forced</returns>
        bool QueryEarlyZForce()
        {
            return false;
        }

        /// <summary>
        /// Registers a texture used by the shader.
        /// </summary>
        /// <param name="handle">Texture handle word offset</param>
        /// <param name="cbufSlot">Constant buffer slot where the texture handle is located</param>
        void RegisterTexture(int handle, int cbufSlot)
        {
            // Only useful when recording information for a disk shader cache.
        }
    }
}
