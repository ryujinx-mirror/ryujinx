using Ryujinx.Common;
using Ryujinx.Graphics.Shader;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Host shader entry used for binding information.
    /// </summary>
    class HostShaderCacheEntry
    {
        /// <summary>
        /// The header of the cached shader entry.
        /// </summary>
        public HostShaderCacheEntryHeader Header { get; }

        /// <summary>
        /// Cached constant buffers.
        /// </summary>
        public BufferDescriptor[] CBuffers { get; }

        /// <summary>
        /// Cached storage buffers.
        /// </summary>
        public BufferDescriptor[] SBuffers { get; }

        /// <summary>
        /// Cached texture descriptors.
        /// </summary>
        public TextureDescriptor[] Textures { get; }

        /// <summary>
        /// Cached image descriptors.
        /// </summary>
        public TextureDescriptor[] Images { get; }

        /// <summary>
        /// Create a new instance of <see cref="HostShaderCacheEntry"/>.
        /// </summary>
        /// <param name="header">The header of the cached shader entry</param>
        /// <param name="cBuffers">Cached constant buffers</param>
        /// <param name="sBuffers">Cached storage buffers</param>
        /// <param name="textures">Cached texture descriptors</param>
        /// <param name="images">Cached image descriptors</param>
        private HostShaderCacheEntry(
            HostShaderCacheEntryHeader header,
            BufferDescriptor[] cBuffers,
            BufferDescriptor[] sBuffers,
            TextureDescriptor[] textures,
            TextureDescriptor[] images)
        {
            Header = header;
            CBuffers = cBuffers;
            SBuffers = sBuffers;
            Textures = textures;
            Images = images;
        }

        private HostShaderCacheEntry()
        {
            Header = new HostShaderCacheEntryHeader();
            CBuffers = new BufferDescriptor[0];
            SBuffers = new BufferDescriptor[0];
            Textures = new TextureDescriptor[0];
            Images = new TextureDescriptor[0];
        }

        private HostShaderCacheEntry(ShaderProgramInfo programInfo)
        {
            Header = new HostShaderCacheEntryHeader(programInfo.CBuffers.Count,
                                                    programInfo.SBuffers.Count,
                                                    programInfo.Textures.Count,
                                                    programInfo.Images.Count,
                                                    programInfo.UsesInstanceId,
                                                    programInfo.UsesRtLayer,
                                                    programInfo.ClipDistancesWritten,
                                                    programInfo.FragmentOutputMap);
            CBuffers = programInfo.CBuffers.ToArray();
            SBuffers = programInfo.SBuffers.ToArray();
            Textures = programInfo.Textures.ToArray();
            Images = programInfo.Images.ToArray();
        }

        /// <summary>
        /// Convert the host shader entry to a <see cref="ShaderProgramInfo"/>.
        /// </summary>
        /// <returns>A new <see cref="ShaderProgramInfo"/> from this instance</returns>
        internal ShaderProgramInfo ToShaderProgramInfo()
        {
            return new ShaderProgramInfo(
                CBuffers,
                SBuffers,
                Textures,
                Images,
                default,
                Header.UseFlags.HasFlag(UseFlags.InstanceId),
                Header.UseFlags.HasFlag(UseFlags.RtLayer),
                Header.ClipDistancesWritten,
                Header.FragmentOutputMap);
        }

        /// <summary>
        /// Parse a raw cached user shader program into an array of shader cache entry.
        /// </summary>
        /// <param name="data">The raw cached host shader</param>
        /// <param name="programCode">The host shader program</param>
        /// <returns>An array of shader cache entry</returns>
        internal static HostShaderCacheEntry[] Parse(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> programCode)
        {
            HostShaderCacheHeader fileHeader = MemoryMarshal.Read<HostShaderCacheHeader>(data);

            data = data.Slice(Unsafe.SizeOf<HostShaderCacheHeader>());

            ReadOnlySpan<HostShaderCacheEntryHeader> entryHeaders = MemoryMarshal.Cast<byte, HostShaderCacheEntryHeader>(data.Slice(0, fileHeader.Count * Unsafe.SizeOf<HostShaderCacheEntryHeader>()));

            data = data.Slice(fileHeader.Count * Unsafe.SizeOf<HostShaderCacheEntryHeader>());

            HostShaderCacheEntry[] result = new HostShaderCacheEntry[fileHeader.Count];

            for (int i = 0; i < result.Length; i++)
            {
                HostShaderCacheEntryHeader header = entryHeaders[i];

                if (!header.InUse)
                {
                    continue;
                }

                int cBufferDescriptorsSize = header.CBuffersCount * Unsafe.SizeOf<BufferDescriptor>();
                int sBufferDescriptorsSize = header.SBuffersCount * Unsafe.SizeOf<BufferDescriptor>();
                int textureDescriptorsSize = header.TexturesCount * Unsafe.SizeOf<TextureDescriptor>();
                int imageDescriptorsSize   = header.ImagesCount * Unsafe.SizeOf<TextureDescriptor>();

                ReadOnlySpan<BufferDescriptor> cBuffers = MemoryMarshal.Cast<byte, BufferDescriptor>(data.Slice(0, cBufferDescriptorsSize));
                data = data.Slice(cBufferDescriptorsSize);

                ReadOnlySpan<BufferDescriptor> sBuffers = MemoryMarshal.Cast<byte, BufferDescriptor>(data.Slice(0, sBufferDescriptorsSize));
                data = data.Slice(sBufferDescriptorsSize);

                ReadOnlySpan<TextureDescriptor> textureDescriptors = MemoryMarshal.Cast<byte, TextureDescriptor>(data.Slice(0, textureDescriptorsSize));
                data = data.Slice(textureDescriptorsSize);

                ReadOnlySpan<TextureDescriptor> imageDescriptors = MemoryMarshal.Cast<byte, TextureDescriptor>(data.Slice(0, imageDescriptorsSize));
                data = data.Slice(imageDescriptorsSize);

                result[i] = new HostShaderCacheEntry(header, cBuffers.ToArray(), sBuffers.ToArray(), textureDescriptors.ToArray(), imageDescriptors.ToArray());
            }

            programCode = data.Slice(0, fileHeader.CodeSize);

            return result;
        }

        /// <summary>
        /// Create a new host shader cache file.
        /// </summary>
        /// <param name="programCode">The host shader program</param>
        /// <param name="codeHolders">The shaders code holder</param>
        /// <returns>Raw data of a new host shader cache file</returns>
        internal static byte[] Create(ReadOnlySpan<byte> programCode, CachedShaderStage[] codeHolders)
        {
            HostShaderCacheHeader header = new HostShaderCacheHeader((byte)codeHolders.Length, programCode.Length);

            HostShaderCacheEntry[] entries = new HostShaderCacheEntry[codeHolders.Length];

            for (int i = 0; i < codeHolders.Length; i++)
            {
                if (codeHolders[i] == null)
                {
                    entries[i] = new HostShaderCacheEntry();
                }
                else
                {
                    entries[i] = new HostShaderCacheEntry(codeHolders[i].Info);
                }
            }

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);

                writer.WriteStruct(header);

                foreach (HostShaderCacheEntry entry in entries)
                {
                    writer.WriteStruct(entry.Header);
                }

                foreach (HostShaderCacheEntry entry in entries)
                {
                    foreach (BufferDescriptor cBuffer in entry.CBuffers)
                    {
                        writer.WriteStruct(cBuffer);
                    }

                    foreach (BufferDescriptor sBuffer in entry.SBuffers)
                    {
                        writer.WriteStruct(sBuffer);
                    }

                    foreach (TextureDescriptor texture in entry.Textures)
                    {
                        writer.WriteStruct(texture);
                    }

                    foreach (TextureDescriptor image in entry.Images)
                    {
                        writer.WriteStruct(image);
                    }
                }

                writer.Write(programCode);

                return stream.ToArray();
            }
        }
    }
}
