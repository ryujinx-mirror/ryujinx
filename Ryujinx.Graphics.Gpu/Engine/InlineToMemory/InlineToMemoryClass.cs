using Ryujinx.Common;
using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.Texture;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine.InlineToMemory
{
    /// <summary>
    /// Represents a Inline-to-Memory engine class.
    /// </summary>
    class InlineToMemoryClass : IDeviceState
    {
        private readonly GpuContext _context;
        private readonly GpuChannel _channel;
        private readonly DeviceState<InlineToMemoryClassState> _state;

        private bool _isLinear;

        private int _offset;
        private int _size;

        private ulong _dstGpuVa;
        private int _dstX;
        private int _dstY;
        private int _dstWidth;
        private int _dstHeight;
        private int _dstStride;
        private int _dstGobBlocksInY;
        private int _lineLengthIn;
        private int _lineCount;

        private bool _finished;

        private int[] _buffer;

        /// <summary>
        /// Creates a new instance of the Inline-to-Memory engine class.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="initializeState">Indicates if the internal state should be initialized. Set to false if part of another engine</param>
        public InlineToMemoryClass(GpuContext context, GpuChannel channel, bool initializeState)
        {
            _context = context;
            _channel = channel;

            if (initializeState)
            {
                _state = new DeviceState<InlineToMemoryClassState>(new Dictionary<string, RwCallback>
                {
                    { nameof(InlineToMemoryClassState.LaunchDma), new RwCallback(LaunchDma, null) },
                    { nameof(InlineToMemoryClassState.LoadInlineData), new RwCallback(LoadInlineData, null) }
                });
            }
        }

        /// <summary>
        /// Creates a new instance of the inline-to-memory engine class.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        public InlineToMemoryClass(GpuContext context, GpuChannel channel) : this(context, channel, true)
        {
        }

        /// <summary>
        /// Reads data from the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <returns>Data at the specified offset</returns>
        public int Read(int offset) => _state.Read(offset);

        /// <summary>
        /// Writes data to the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <param name="data">Data to be written</param>
        public void Write(int offset, int data) => _state.Write(offset, data);

        /// <summary>
        /// Launches Inline-to-Memory engine DMA copy.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void LaunchDma(int argument)
        {
            LaunchDma(ref _state.State, argument);
        }

        /// <summary>
        /// Launches Inline-to-Memory engine DMA copy.
        /// </summary>
        /// <param name="state">Current class state</param>
        /// <param name="argument">Method call argument</param>
        public void LaunchDma(ref InlineToMemoryClassState state, int argument)
        {
            _isLinear = (argument & 1) != 0;

            _offset = 0;
            _size = (int)(state.LineLengthIn * state.LineCount);

            int count = BitUtils.DivRoundUp(_size, 4);

            if (_buffer == null || _buffer.Length < count)
            {
                _buffer = new int[count];
            }

            ulong dstGpuVa = ((ulong)state.OffsetOutUpperValue << 32) | state.OffsetOut;

            ulong dstBaseAddress = _channel.MemoryManager.Translate(dstGpuVa);

            // Trigger read tracking, to flush any managed resources in the destination region.
            _channel.MemoryManager.Physical.GetSpan(dstBaseAddress, _size, true);

            _dstGpuVa = dstGpuVa;
            _dstX = state.SetDstOriginBytesXV;
            _dstY = state.SetDstOriginSamplesYV;
            _dstWidth = (int)state.SetDstWidth;
            _dstHeight = (int)state.SetDstHeight;
            _dstStride = (int)state.PitchOut;
            _dstGobBlocksInY = 1 << (int)state.SetDstBlockSizeHeight;
            _lineLengthIn = (int)state.LineLengthIn;
            _lineCount = (int)state.LineCount;

            _finished = false;
        }

        /// <summary>
        /// Pushes a block of data to the Inline-to-Memory engine.
        /// </summary>
        /// <param name="data">Data to push</param>
        public void LoadInlineData(ReadOnlySpan<int> data)
        {
            if (!_finished)
            {
                int copySize = Math.Min(data.Length, _buffer.Length - _offset);
                data.Slice(0, copySize).CopyTo(new Span<int>(_buffer).Slice(_offset, copySize));

                _offset += copySize;

                if (_offset * 4 >= _size)
                {
                    FinishTransfer();
                }
            }
        }

        /// <summary>
        /// Pushes a word of data to the Inline-to-Memory engine.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        public void LoadInlineData(int argument)
        {
            if (!_finished)
            {
                _buffer[_offset++] = argument;

                if (_offset * 4 >= _size)
                {
                    FinishTransfer();
                }
            }
        }

        /// <summary>
        /// Performs actual copy of the inline data after the transfer is finished.
        /// </summary>
        private void FinishTransfer()
        {
            Span<byte> data = MemoryMarshal.Cast<int, byte>(_buffer).Slice(0, _size);

            if (_isLinear && _lineCount == 1)
            {
                ulong address = _channel.MemoryManager.Translate(_dstGpuVa);

                _channel.MemoryManager.Physical.Write(address, data);
            }
            else
            {
                var dstCalculator = new OffsetCalculator(
                    _dstWidth,
                    _dstHeight,
                    _dstStride,
                    _isLinear,
                    _dstGobBlocksInY,
                    1);

                int srcOffset = 0;

                ulong dstBaseAddress = _channel.MemoryManager.Translate(_dstGpuVa);

                for (int y = _dstY; y < _dstY + _lineCount; y++)
                {
                    int x1 = _dstX;
                    int x2 = _dstX + _lineLengthIn;
                    int x2Trunc = _dstX + BitUtils.AlignDown(_lineLengthIn, 16);

                    int x;

                    for (x = x1; x < x2Trunc; x += 16, srcOffset += 16)
                    {
                        int dstOffset = dstCalculator.GetOffset(x, y);

                        ulong dstAddress = dstBaseAddress + (ulong)dstOffset;

                        Span<byte> pixel = data.Slice(srcOffset, 16);

                        _channel.MemoryManager.Physical.Write(dstAddress, pixel);
                    }

                    for (; x < x2; x++, srcOffset++)
                    {
                        int dstOffset = dstCalculator.GetOffset(x, y);

                        ulong dstAddress = dstBaseAddress + (ulong)dstOffset;

                        Span<byte> pixel = data.Slice(srcOffset, 1);

                        _channel.MemoryManager.Physical.Write(dstAddress, pixel);
                    }
                }
            }

            _finished = true;

            _context.AdvanceSequence();
        }
    }
}
