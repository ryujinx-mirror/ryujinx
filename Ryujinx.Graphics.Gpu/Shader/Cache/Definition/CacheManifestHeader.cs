using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Header of the shader cache manifest.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    struct CacheManifestHeader
    {
        /// <summary>
        /// The version of the cache.
        /// </summary>
        public ulong Version;

        /// <summary>
        /// The graphics api used for this cache.
        /// </summary>
        public CacheGraphicsApi GraphicsApi;

        /// <summary>
        /// The hash type used for this cache.
        /// </summary>
        public CacheHashType HashType;

        /// <summary>
        /// CRC-16 checksum over the data in the file.
        /// </summary>
        public ushort TableChecksum;

        /// <summary>
        /// Construct a new cache manifest header.
        /// </summary>
        /// <param name="version">The version of the cache</param>
        /// <param name="graphicsApi">The graphics api used for this cache</param>
        /// <param name="hashType">The hash type used for this cache</param>
        public CacheManifestHeader(ulong version, CacheGraphicsApi graphicsApi, CacheHashType hashType)
        {
            Version = version;
            GraphicsApi = graphicsApi;
            HashType = hashType;
            TableChecksum = 0;
        }

        /// <summary>
        /// Update the checksum in the header.
        /// </summary>
        /// <param name="data">The data to perform the checksum on</param>
        public void UpdateChecksum(ReadOnlySpan<byte> data)
        {
            TableChecksum = CalculateCrc16(data);
        }

        /// <summary>
        /// Calculate a CRC-16 over data.
        /// </summary>
        /// <param name="data">The data to perform the CRC-16 on</param>
        /// <returns>A CRC-16 over data</returns>
        private static ushort CalculateCrc16(ReadOnlySpan<byte> data)
        {
            int crc = 0;

            const ushort poly = 0x1021;

            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i] << 8;

                for (int j = 0; j < 8; j++)
                {
                    crc <<= 1;

                    if ((crc & 0x10000) != 0)
                    {
                        crc = (crc ^ poly) & 0xFFFF;
                    }
                }
            }

            return (ushort)crc;
        }

        /// <summary>
        /// Check the validity of the header.
        /// </summary>
        /// <param name="graphicsApi">The target graphics api in use</param>
        /// <param name="hashType">The target hash type in use</param>
        /// <param name="data">The data after this header</param>
        /// <returns>True if the header is valid</returns>
        /// <remarks>This doesn't check that versions match</remarks>
        public bool IsValid(CacheGraphicsApi graphicsApi, CacheHashType hashType, ReadOnlySpan<byte> data)
        {
            return GraphicsApi == graphicsApi && HashType == hashType && TableChecksum == CalculateCrc16(data);
        }
    }
}
