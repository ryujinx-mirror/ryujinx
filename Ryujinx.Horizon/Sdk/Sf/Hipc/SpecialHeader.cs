using Ryujinx.Common.Utilities;

namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    struct SpecialHeader
    {
        private uint _word;

        public bool SendPid
        {
            get => _word.Extract(0);
            set => _word = _word.Insert(0, value);
        }

        public int CopyHandlesCount
        {
            get => (int)_word.Extract(1, 4);
            set => _word = _word.Insert(1, 4, (uint)value);
        }

        public int MoveHandlesCount
        {
            get => (int)_word.Extract(5, 4);
            set => _word = _word.Insert(5, 4, (uint)value);
        }
    }
}
