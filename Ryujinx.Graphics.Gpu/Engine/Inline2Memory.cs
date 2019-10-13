using Ryujinx.Graphics.Gpu.State;
using System;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private Inline2MemoryParams _params;

        private bool _isLinear;

        private int _offset;
        private int _size;

        public void Execute(int argument)
        {
            _params = _context.State.Get<Inline2MemoryParams>(MethodOffset.Inline2MemoryParams);

            _isLinear = (argument & 1) != 0;

            _offset = 0;
            _size   = _params.LineLengthIn * _params.LineCount;
        }

        public void PushData(int argument)
        {
            if (_isLinear)
            {
                for (int shift = 0; shift < 32 && _offset < _size; shift += 8, _offset++)
                {
                    ulong gpuVa = _params.DstAddress.Pack() + (ulong)_offset;

                    _context.MemoryAccessor.Write(gpuVa, new byte[] { (byte)(argument >> shift) });
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}