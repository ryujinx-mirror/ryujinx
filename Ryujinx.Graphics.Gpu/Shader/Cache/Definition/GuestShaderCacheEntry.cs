using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Represent a cached shader entry in a guest shader program.
    /// </summary>
    class GuestShaderCacheEntry
    {
        /// <summary>
        /// The header of the cached shader entry.
        /// </summary>
        public GuestShaderCacheEntryHeader Header { get; }

        /// <summary>
        /// The code of this shader.
        /// </summary>
        /// <remarks>If a Vertex A is present, this also contains the code 2 section.</remarks>
        public byte[] Code { get; }

        /// <summary>
        /// The textures descriptors used for this shader.
        /// </summary>
        public Dictionary<int, GuestTextureDescriptor> TextureDescriptors { get; }

        /// <summary>
        /// Create a new instance of <see cref="GuestShaderCacheEntry"/>.
        /// </summary>
        /// <param name="header">The header of the cached shader entry</param>
        /// <param name="code">The code of this shader</param>
        public GuestShaderCacheEntry(GuestShaderCacheEntryHeader header, byte[] code)
        {
            Header = header;
            Code = code;
            TextureDescriptors = new Dictionary<int, GuestTextureDescriptor>();
        }

        /// <summary>
        /// Parse a raw cached user shader program into an array of shader cache entry.
        /// </summary>
        /// <param name="data">The raw cached user shader program</param>
        /// <param name="fileHeader">The user shader program header</param>
        /// <returns>An array of shader cache entry</returns>
        public static GuestShaderCacheEntry[] Parse(ref ReadOnlySpan<byte> data, out GuestShaderCacheHeader fileHeader)
        {
            fileHeader = MemoryMarshal.Read<GuestShaderCacheHeader>(data);

            data = data.Slice(Unsafe.SizeOf<GuestShaderCacheHeader>());

            ReadOnlySpan<GuestShaderCacheEntryHeader> entryHeaders = MemoryMarshal.Cast<byte, GuestShaderCacheEntryHeader>(data.Slice(0, fileHeader.Count * Unsafe.SizeOf<GuestShaderCacheEntryHeader>()));

            data = data.Slice(fileHeader.Count * Unsafe.SizeOf<GuestShaderCacheEntryHeader>());

            GuestShaderCacheEntry[] result = new GuestShaderCacheEntry[fileHeader.Count];

            for (int i = 0; i < result.Length; i++)
            {
                GuestShaderCacheEntryHeader header = entryHeaders[i];

                // Ignore empty entries
                if (header.Size == 0 && header.SizeA == 0)
                {
                    continue;
                }

                byte[] code = data.Slice(0, header.Size + header.SizeA).ToArray();

                data = data.Slice(header.Size + header.SizeA);

                result[i] = new GuestShaderCacheEntry(header, code);

                ReadOnlySpan<GuestTextureDescriptor> textureDescriptors = MemoryMarshal.Cast<byte, GuestTextureDescriptor>(data.Slice(0, header.GpuAccessorHeader.TextureDescriptorCount * Unsafe.SizeOf<GuestTextureDescriptor>()));

                foreach (GuestTextureDescriptor textureDescriptor in textureDescriptors)
                {
                    result[i].TextureDescriptors.Add((int)textureDescriptor.Handle, textureDescriptor);
                }

                data = data.Slice(header.GpuAccessorHeader.TextureDescriptorCount * Unsafe.SizeOf<GuestTextureDescriptor>());
            }

            return result;
        }
    }
}
