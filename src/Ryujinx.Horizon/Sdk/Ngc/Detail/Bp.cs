namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    class Bp
    {
        private readonly BpNode _firstNode = new();
        private readonly SbvSelect _sbvSelect = new();

        public bool Import(ref BinaryReader reader)
        {
            return _firstNode.Import(ref reader) && _sbvSelect.Import(ref reader);
        }

        public int ToPos(int index)
        {
            return _sbvSelect.Select(_firstNode.Set, index);
        }

        public int Enclose(int index)
        {
            if ((uint)index < (uint)_firstNode.Set.BitVector.BitLength)
            {
                if (!_firstNode.Set.Has(index))
                {
                    index = _firstNode.FindOpen(index);
                }

                if (index > 0)
                {
                    return _firstNode.Enclose(index);
                }
            }

            return -1;
        }

        public int ToNodeId(int index)
        {
            if ((uint)index < (uint)_firstNode.Set.BitVector.BitLength)
            {
                if (!_firstNode.Set.Has(index))
                {
                    index = _firstNode.FindOpen(index);
                }

                if (index >= 0)
                {
                    return _firstNode.Set.Rank1(index) - 1;
                }
            }

            return -1;
        }
    }
}
