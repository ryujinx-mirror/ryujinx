using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.DiskCache
{
    /// <summary>
    /// Binary data serializer.
    /// </summary>
    struct BinarySerializer
    {
        private readonly Stream _stream;
        private Stream _activeStream;

        /// <summary>
        /// Creates a new binary serializer.
        /// </summary>
        /// <param name="stream">Stream to read from or write into</param>
        public BinarySerializer(Stream stream)
        {
            _stream = stream;
            _activeStream = stream;
        }

        /// <summary>
        /// Reads data from the stream.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="data">Data read</param>
        public readonly void Read<T>(ref T data) where T : unmanaged
        {
            Span<byte> buffer = MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref data, 1));
            for (int offset = 0; offset < buffer.Length;)
            {
                offset += _activeStream.Read(buffer[offset..]);
            }
        }

        /// <summary>
        /// Tries to read data from the stream.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="data">Data read</param>
        /// <returns>True if the read was successful, false otherwise</returns>
        public readonly bool TryRead<T>(ref T data) where T : unmanaged
        {
            // Length is unknown on compressed streams.
            if (_activeStream == _stream)
            {
                int size = Unsafe.SizeOf<T>();
                if (_activeStream.Length - _activeStream.Position < size)
                {
                    return false;
                }
            }

            Read(ref data);
            return true;
        }

        /// <summary>
        /// Reads data prefixed with a magic and size from the stream.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="data">Data read</param>
        /// <param name="magic">Expected magic value, for validation</param>
        public readonly void ReadWithMagicAndSize<T>(ref T data, uint magic) where T : unmanaged
        {
            uint actualMagic = 0;
            int size = 0;
            Read(ref actualMagic);
            Read(ref size);

            if (actualMagic != magic)
            {
                throw new DiskCacheLoadException(DiskCacheLoadResult.FileCorruptedInvalidMagic);
            }

            // Structs are expected to expand but not shrink between versions.
            if (size > Unsafe.SizeOf<T>())
            {
                throw new DiskCacheLoadException(DiskCacheLoadResult.FileCorruptedInvalidLength);
            }

            Span<byte> buffer = MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref data, 1))[..size];
            for (int offset = 0; offset < buffer.Length;)
            {
                offset += _activeStream.Read(buffer[offset..]);
            }
        }

        /// <summary>
        /// Writes data into the stream.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="data">Data to be written</param>
        public readonly void Write<T>(ref T data) where T : unmanaged
        {
            Span<byte> buffer = MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref data, 1));
            _activeStream.Write(buffer);
        }

        /// <summary>
        /// Writes data prefixed with a magic and size into the stream.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="data">Data to write</param>
        /// <param name="magic">Magic value to write</param>
        public readonly void WriteWithMagicAndSize<T>(ref T data, uint magic) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            Write(ref magic);
            Write(ref size);
            Span<byte> buffer = MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref data, 1));
            _activeStream.Write(buffer);
        }

        /// <summary>
        /// Indicates that all data that will be read from the stream has been compressed.
        /// </summary>
        public void BeginCompression()
        {
            CompressionAlgorithm algorithm = CompressionAlgorithm.None;
            Read(ref algorithm);

            switch (algorithm)
            {
                case CompressionAlgorithm.None:
                    break;
                case CompressionAlgorithm.Deflate:
                    _activeStream = new DeflateStream(_stream, CompressionMode.Decompress, true);
                    break;
                case CompressionAlgorithm.Brotli:
                    _activeStream = new BrotliStream(_stream, CompressionMode.Decompress, true);
                    break;
                default:
                    throw new ArgumentException($"Invalid compression algorithm \"{algorithm}\"");
            }
        }

        /// <summary>
        /// Indicates that all data that will be written into the stream should be compressed.
        /// </summary>
        /// <param name="algorithm">Compression algorithm that should be used</param>
        public void BeginCompression(CompressionAlgorithm algorithm)
        {
            Write(ref algorithm);

            switch (algorithm)
            {
                case CompressionAlgorithm.None:
                    break;
                case CompressionAlgorithm.Deflate:
                    _activeStream = new DeflateStream(_stream, CompressionLevel.Fastest, true);
                    break;
                case CompressionAlgorithm.Brotli:
                    _activeStream = new BrotliStream(_stream, CompressionLevel.Fastest, true);
                    break;
                default:
                    throw new ArgumentException($"Invalid compression algorithm \"{algorithm}\"");
            }
        }

        /// <summary>
        /// Indicates the end of a compressed chunck.
        /// </summary>
        /// <remarks>
        /// Any data written after this will not be compressed unless <see cref="BeginCompression(CompressionAlgorithm)"/> is called again.
        /// Any data read after this will be assumed to be uncompressed unless <see cref="BeginCompression"/> is called again.
        /// </remarks>
        public void EndCompression()
        {
            if (_activeStream != _stream)
            {
                _activeStream.Dispose();
                _activeStream = _stream;
            }
        }

        /// <summary>
        /// Reads compressed data from the stream.
        /// </summary>
        /// <remarks>
        /// <paramref name="data"/> must have the exact length of the uncompressed data,
        /// otherwise decompression will fail.
        /// </remarks>
        /// <param name="stream">Stream to read from</param>
        /// <param name="data">Buffer to write the uncompressed data into</param>
        public static void ReadCompressed(Stream stream, Span<byte> data)
        {
            CompressionAlgorithm algorithm = (CompressionAlgorithm)stream.ReadByte();

            switch (algorithm)
            {
                case CompressionAlgorithm.None:
                    stream.ReadExactly(data);
                    break;
                case CompressionAlgorithm.Deflate:
                    stream = new DeflateStream(stream, CompressionMode.Decompress, true);
                    for (int offset = 0; offset < data.Length;)
                    {
                        offset += stream.Read(data[offset..]);
                    }
                    stream.Dispose();
                    break;
                case CompressionAlgorithm.Brotli:
                    stream = new BrotliStream(stream, CompressionMode.Decompress, true);
                    for (int offset = 0; offset < data.Length;)
                    {
                        offset += stream.Read(data[offset..]);
                    }
                    stream.Dispose();
                    break;
            }
        }

        /// <summary>
        /// Compresses and writes the compressed data into the stream.
        /// </summary>
        /// <param name="stream">Stream to write into</param>
        /// <param name="data">Data to compress</param>
        /// <param name="algorithm">Compression algorithm to be used</param>
        public static void WriteCompressed(Stream stream, ReadOnlySpan<byte> data, CompressionAlgorithm algorithm)
        {
            stream.WriteByte((byte)algorithm);

            switch (algorithm)
            {
                case CompressionAlgorithm.None:
                    stream.Write(data);
                    break;
                case CompressionAlgorithm.Deflate:
                    stream = new DeflateStream(stream, CompressionLevel.Fastest, true);
                    stream.Write(data);
                    stream.Dispose();
                    break;
                case CompressionAlgorithm.Brotli:
                    stream = new BrotliStream(stream, CompressionLevel.Fastest, true);
                    stream.Write(data);
                    stream.Dispose();
                    break;
            }
        }
    }
}
