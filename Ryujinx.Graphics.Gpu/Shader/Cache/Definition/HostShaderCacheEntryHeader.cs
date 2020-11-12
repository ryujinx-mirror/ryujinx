using System.Runtime.InteropServices;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Host shader entry header used for binding information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x14)]
    struct HostShaderCacheEntryHeader
    {
        /// <summary>
        /// Count of constant buffer descriptors.
        /// </summary>
        public int CBuffersCount;

        /// <summary>
        /// Count of storage buffer descriptors.
        /// </summary>
        public int SBuffersCount;

        /// <summary>
        /// Count of texture descriptors.
        /// </summary>
        public int TexturesCount;

        /// <summary>
        /// Count of image descriptors.
        /// </summary>
        public int ImagesCount;

        /// <summary>
        /// Set to true if the shader uses instance id.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool UsesInstanceId;

        /// <summary>
        /// Set to true if this entry is in use.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool InUse;

        /// <summary>
        /// Reserved / unused.
        /// </summary>
        public short Reserved;

        /// <summary>
        /// Create a new host shader cache entry header.
        /// </summary>
        /// <param name="cBuffersCount">Count of constant buffer descriptors</param>
        /// <param name="sBuffersCount">Count of storage buffer descriptors</param>
        /// <param name="texturesCount">Count of texture descriptors</param>
        /// <param name="imagesCount">Count of image descriptors</param>
        /// <param name="usesInstanceId">Set to true if the shader uses instance id</param>
        public HostShaderCacheEntryHeader(int cBuffersCount, int sBuffersCount, int texturesCount, int imagesCount, bool usesInstanceId) : this()
        {
            CBuffersCount  = cBuffersCount;
            SBuffersCount  = sBuffersCount;
            TexturesCount  = texturesCount;
            ImagesCount    = imagesCount;
            UsesInstanceId = usesInstanceId;
            InUse          = true;
        }
    }
}
