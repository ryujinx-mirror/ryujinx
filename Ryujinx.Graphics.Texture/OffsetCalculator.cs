using Ryujinx.Common;

using static Ryujinx.Graphics.Texture.BlockLinearConstants;

namespace Ryujinx.Graphics.Texture
{
    public class OffsetCalculator
    {
        private int  _stride;
        private bool _isLinear;
        private int  _bytesPerPixel;

        private BlockLinearLayout _layoutConverter;

        public OffsetCalculator(
            int  width,
            int  height,
            int  stride,
            bool isLinear,
            int  gobBlocksInY,
            int  bytesPerPixel)
        {
            _stride        = stride;
            _isLinear      = isLinear;
            _bytesPerPixel = bytesPerPixel;

            int wAlignment = GobStride / bytesPerPixel;

            int wAligned = BitUtils.AlignUp(width, wAlignment);

            if (!isLinear)
            {
                _layoutConverter = new BlockLinearLayout(
                    wAligned,
                    height,
                    1,
                    gobBlocksInY,
                    1,
                    bytesPerPixel);
            }
        }

        public int GetOffset(int x, int y)
        {
            if (_isLinear)
            {
                return x * _bytesPerPixel + y * _stride;
            }
            else
            {
                return _layoutConverter.GetOffset(x, y, 0);
            }
        }
    }
}