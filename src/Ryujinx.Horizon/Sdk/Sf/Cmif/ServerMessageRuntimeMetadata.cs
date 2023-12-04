namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    readonly struct ServerMessageRuntimeMetadata
    {
        public ushort InDataSize { get; }
        public ushort OutDataSize { get; }
        public byte InHeadersSize { get; }
        public byte OutHeadersSize { get; }
        public byte InObjectsCount { get; }
        public byte OutObjectsCount { get; }

        public int UnfixedOutPointerSizeOffset => InDataSize + InHeadersSize + 0x10;

        public ServerMessageRuntimeMetadata(
            ushort inDataSize,
            ushort outDataSize,
            byte inHeadersSize,
            byte outHeadersSize,
            byte inObjectsCount,
            byte outObjectsCount)
        {
            InDataSize = inDataSize;
            OutDataSize = outDataSize;
            InHeadersSize = inHeadersSize;
            OutHeadersSize = outHeadersSize;
            InObjectsCount = inObjectsCount;
            OutObjectsCount = outObjectsCount;
        }
    }
}
