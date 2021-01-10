using Ryujinx.Common;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Texture;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private Inline2MemoryParams _params;

        private bool _isLinear;

        private int _offset;
        private int _size;

        private bool _finished;

        private int[] _buffer;

        /// <summary>
        /// Launches Inline-to-Memory engine DMA copy.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        public void LaunchDma(GpuState state, int argument)
        {
            _params = state.Get<Inline2MemoryParams>(MethodOffset.I2mParams);

            _isLinear = (argument & 1) != 0;

            _offset = 0;
            _size   = _params.LineLengthIn * _params.LineCount;

            int count = BitUtils.DivRoundUp(_size, 4);

            if (_buffer == null || _buffer.Length < count)
            {
                _buffer = new int[count];
            }

            ulong dstBaseAddress = _context.MemoryManager.Translate(_params.DstAddress.Pack());

            // Trigger read tracking, to flush any managed resources in the destination region.
            _context.PhysicalMemory.GetSpan(dstBaseAddress, _size, true);

            _finished = false;
        }

        /// <summary>
        /// Pushes a word of data to the Inline-to-Memory engine.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        public void LoadInlineData(GpuState state, int argument)
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

            if (_isLinear && _params.LineCount == 1)
            {
                ulong address = _context.MemoryManager.Translate(_params.DstAddress.Pack());

                _context.PhysicalMemory.Write(address, data);
            }
            else
            {
                var dstCalculator = new OffsetCalculator(
                    _params.DstWidth,
                    _params.DstHeight,
                    _params.DstStride,
                    _isLinear,
                    _params.DstMemoryLayout.UnpackGobBlocksInY(),
                    1);

                int srcOffset = 0;

                ulong dstBaseAddress = _context.MemoryManager.Translate(_params.DstAddress.Pack());

                for (int y = _params.DstY; y < _params.DstY + _params.LineCount; y++)
                {
                    int x1      = _params.DstX;
                    int x2      = _params.DstX + _params.LineLengthIn;
                    int x2Trunc = _params.DstX + BitUtils.AlignDown(_params.LineLengthIn, 16);

                    int x;

                    for (x = x1; x < x2Trunc; x += 16, srcOffset += 16)
                    {
                        int dstOffset = dstCalculator.GetOffset(x, y);

                        ulong dstAddress = dstBaseAddress + (ulong)dstOffset;

                        Span<byte> pixel = data.Slice(srcOffset, 16);

                        _context.PhysicalMemory.Write(dstAddress, pixel);
                    }

                    for (; x < x2; x++, srcOffset++)
                    {
                        int dstOffset = dstCalculator.GetOffset(x, y);

                        ulong dstAddress = dstBaseAddress + (ulong)dstOffset;

                        Span<byte> pixel = data.Slice(srcOffset, 1);

                        _context.PhysicalMemory.Write(dstAddress, pixel);
                    }
                }
            }

            _finished = true;

            _context.AdvanceSequence();
        }
    }
}