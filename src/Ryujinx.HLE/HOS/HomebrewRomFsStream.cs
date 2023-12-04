using System;
using System.IO;

namespace Ryujinx.HLE.HOS
{
    class HomebrewRomFsStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _positionOffset;

        public HomebrewRomFsStream(Stream baseStream, long positionOffset)
        {
            _baseStream = baseStream;
            _positionOffset = positionOffset;

            _baseStream.Position = _positionOffset;
        }

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _baseStream.Length - _positionOffset;

        public override long Position
        {
            get
            {
                return _baseStream.Position - _positionOffset;
            }
            set
            {
                _baseStream.Position = value + _positionOffset;
            }
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                offset += _positionOffset;
            }

            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _baseStream.Dispose();
            }
        }
    }
}
