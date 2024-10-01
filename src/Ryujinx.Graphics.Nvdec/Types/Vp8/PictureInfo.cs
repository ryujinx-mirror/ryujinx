using Ryujinx.Common.Memory;
using Ryujinx.Graphics.Video;

namespace Ryujinx.Graphics.Nvdec.Types.Vp8
{
    struct PictureInfo
    {
#pragma warning disable CS0649 // Field is never assigned to
        public Array13<uint> Unknown0;
        public uint GpTimerTimeoutValue;
        public ushort FrameWidth;
        public ushort FrameHeight;
        public byte KeyFrame; // 1: key frame - 0: not
        public byte Version;
        public byte Flags0;
        // TileFormat : 2 // 0: TBL; 1: KBL;
        // GobHeight : 3 // Set GOB height, 0: GOB_2, 1: GOB_4, 2: GOB_8, 3: GOB_16, 4: GOB_32 (NVDEC3 onwards)
        // ReserverdSurfaceFormat : 3
        public byte ErrorConcealOn; // 1: error conceal on - 0: off
        public uint FirstPartSize; // the size of first partition (frame header and mb header partition)
        public uint HistBufferSize; // in units of 256
        public uint VLDBufferSize; // in units of 1
        public Array2<uint> FrameStride; // [y_c]
        public uint LumaTopOffset; // offset of luma top field in units of 256
        public uint LumaBotOffset; // offset of luma bottom field in units of 256
        public uint LumaFrameOffset; // offset of luma frame in units of 256
        public uint ChromaTopOffset; // offset of chroma top field in units of 256
        public uint ChromaBotOffset; // offset of chroma bottom field in units of 256
        public uint ChromaFrameOffset; // offset of chroma frame in units of 256
        public uint Flags1;
        // EnableTFOutput : 1; // =1, enable dbfdma to output the display surface; if disable, then the following configure on tf is useless.
        // Remap for VC1
        // VC1MapYFlag : 1
        // MapYValue : 3
        // VC1MapUVFlag : 1
        // MapUVValue : 3
        // TF
        // OutStride : 8
        // TilingFormat : 3;
        // OutputStructure : 1 // 0:frame, 1:field
        // Reserved0 : 11
        public Array2<int> OutputTop; // in units of 256
        public Array2<int> OutputBottom; // in units of 256
        // Histogram
        public uint Flags2;
        // EnableHistogram : 1 // enable histogram info collection
        // HistogramStartX : 12 // start X of Histogram window
        // HistogramStartY : 12 // start Y of Histogram window
        // Reserved1 : 7
        // HistogramEndX : 12 // end X of Histogram window
        // HistogramEndY : 12 // end y of Histogram window
        // Reserved2 : 8
        // Decode picture buffer related
        public sbyte CurrentOutputMemoryLayout;
        public Array3<sbyte> OutputMemoryLayout; // output NV12/NV24 setting. item 0:golden - 1: altref - 2: last
        public byte SegmentationFeatureDataUpdate;
        public Array3<byte> Reserved3;
        public uint ResultValue; // ucode return result
        public Array8<uint> PartitionOffset;
        public Array3<uint> Reserved4;
#pragma warning restore CS0649

        public Vp8PictureInfo Convert()
        {
            return new Vp8PictureInfo()
            {
                KeyFrame = KeyFrame != 0,
                FirstPartSize = FirstPartSize,
                Version = Version,
                FrameWidth = FrameWidth,
                FrameHeight = FrameHeight,
            };
        }
    }
}
