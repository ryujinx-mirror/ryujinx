namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    class Sbv
    {
        private readonly SbvSelect _sbvSelect;
        private readonly Set _set;

        public SbvSelect SbvSelect => _sbvSelect;
        public Set Set => _set;

        public Sbv()
        {
            _sbvSelect = new();
            _set = new();
        }

        public Sbv(int length)
        {
            _sbvSelect = new();
            _set = new(length);
        }

        public void Build()
        {
            _set.Build();
            _sbvSelect.Build(_set.BitVector.Array, _set.BitVector.BitLength);
        }

        public bool Import(ref BinaryReader reader)
        {
            return _set.Import(ref reader) && _sbvSelect.Import(ref reader);
        }
    }
}
