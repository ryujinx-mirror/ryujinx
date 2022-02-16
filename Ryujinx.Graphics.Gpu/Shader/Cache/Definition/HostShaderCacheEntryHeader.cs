using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Flags indicating if the shader accesses certain built-ins, such as the instance ID.
    /// </summary>
    enum UseFlags : byte
    {
        /// <summary>
        /// None of the built-ins are used.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates whenever the vertex shader reads the gl_InstanceID built-in.
        /// </summary>
        InstanceId = 1 << 0,

        /// <summary>
        /// Indicates whenever any of the VTG stages writes to the gl_Layer built-in.
        /// </summary>
        RtLayer = 1 << 1
    }

    /// <summary>
    /// Host shader entry header used for binding information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x18)]
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
        /// Flags indicating if the shader accesses certain built-ins, such as the instance ID.
        /// </summary>
        public UseFlags UseFlags;

        /// <summary>
        /// Set to true if this entry is in use.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool InUse;

        /// <summary>
        /// Mask of clip distances that are written to on the shader.
        /// </summary>
        public byte ClipDistancesWritten;

        /// <summary>
        /// Reserved / unused.
        /// </summary>
        public byte Reserved;

        /// <summary>
        /// Mask of components written by the fragment shader stage.
        /// </summary>
        public int FragmentOutputMap;

        /// <summary>
        /// Create a new host shader cache entry header.
        /// </summary>
        /// <param name="cBuffersCount">Count of constant buffer descriptors</param>
        /// <param name="sBuffersCount">Count of storage buffer descriptors</param>
        /// <param name="texturesCount">Count of texture descriptors</param>
        /// <param name="imagesCount">Count of image descriptors</param>
        /// <param name="usesInstanceId">Set to true if the shader uses instance id</param>
        /// <param name="clipDistancesWritten">Mask of clip distances that are written to on the shader</param>
        /// <param name="fragmentOutputMap">Mask of components written by the fragment shader stage</param>
        public HostShaderCacheEntryHeader(
            int cBuffersCount,
            int sBuffersCount,
            int texturesCount,
            int imagesCount,
            bool usesInstanceId,
            bool usesRtLayer,
            byte clipDistancesWritten,
            int fragmentOutputMap) : this()
        {
            CBuffersCount        = cBuffersCount;
            SBuffersCount        = sBuffersCount;
            TexturesCount        = texturesCount;
            ImagesCount          = imagesCount;
            ClipDistancesWritten = clipDistancesWritten;
            FragmentOutputMap    = fragmentOutputMap;
            InUse                = true;

            UseFlags = usesInstanceId ? UseFlags.InstanceId : UseFlags.None;

            if (usesRtLayer)
            {
                UseFlags |= UseFlags.RtLayer;
            }
        }
    }
}
