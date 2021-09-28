using Ryujinx.Graphics.Nvdec.H264;
using System;

namespace Ryujinx.Graphics.Nvdec
{
    class NvdecDecoderContext : IDisposable
    {
        private Decoder _decoder;

        public Decoder GetDecoder()
        {
            return _decoder ??= new Decoder();
        }

        public void Dispose()
        {
            _decoder?.Dispose();
            _decoder = null;
        }
    }
}