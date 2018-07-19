namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderHeader
    {
        public const int PointList     = 1;
        public const int LineStrip     = 6;
        public const int TriangleStrip = 7;

        public int  SphType         { get; private set; }
        public int  Version         { get; private set; }
        public int  ShaderType      { get; private set; }
        public bool MrtEnable       { get; private set; }
        public bool KillsPixels     { get; private set; }
        public bool DoesGlobalStore { get; private set; }
        public int  SassVersion     { get; private set; }
        public bool DoesLoadOrStore { get; private set; }
        public bool DoesFp64        { get; private set; }
        public int  StreamOutMask   { get; private set; }

        public int ShaderLocalMemoryLowSize { get; private set; }
        public int PerPatchAttributeCount   { get; private set; }

        public int ShaderLocalMemoryHighSize { get; private set; }
        public int ThreadsPerInputPrimitive  { get; private set; }

        public int ShaderLocalMemoryCrsSize { get; private set; }
        public int OutputTopology           { get; private set; }

        public int MaxOutputVertexCount { get; private set; }
        public int StoreReqStart        { get; private set; }
        public int StoreReqEnd          { get; private set; }

        public ShaderHeader(IGalMemory Memory, long Position)
        {
            uint CommonWord0 = (uint)Memory.ReadInt32(Position + 0);
            uint CommonWord1 = (uint)Memory.ReadInt32(Position + 4);
            uint CommonWord2 = (uint)Memory.ReadInt32(Position + 8);
            uint CommonWord3 = (uint)Memory.ReadInt32(Position + 12);
            uint CommonWord4 = (uint)Memory.ReadInt32(Position + 16);

            SphType         = ReadBits(CommonWord0,  0, 5);
            Version         = ReadBits(CommonWord0,  5, 5);
            ShaderType      = ReadBits(CommonWord0, 10, 4);
            MrtEnable       = ReadBits(CommonWord0, 14, 1) != 0;
            KillsPixels     = ReadBits(CommonWord0, 15, 1) != 0;
            DoesGlobalStore = ReadBits(CommonWord0, 16, 1) != 0;
            SassVersion     = ReadBits(CommonWord0, 17, 4);
            DoesLoadOrStore = ReadBits(CommonWord0, 26, 1) != 0;
            DoesFp64        = ReadBits(CommonWord0, 27, 1) != 0;
            StreamOutMask   = ReadBits(CommonWord0, 28, 4);

            ShaderLocalMemoryLowSize = ReadBits(CommonWord1,  0, 24);
            PerPatchAttributeCount   = ReadBits(CommonWord1, 24,  8);

            ShaderLocalMemoryHighSize = ReadBits(CommonWord2,  0, 24);
            ThreadsPerInputPrimitive  = ReadBits(CommonWord2, 24,  8);

            ShaderLocalMemoryCrsSize = ReadBits(CommonWord3,  0, 24);
            OutputTopology           = ReadBits(CommonWord3, 24,  4);

            MaxOutputVertexCount = ReadBits(CommonWord4,  0, 12);
            StoreReqStart        = ReadBits(CommonWord4, 12,  8);
            StoreReqEnd          = ReadBits(CommonWord4, 24,  8);
        }

        private static int ReadBits(uint Word, int Offset, int BitWidth)
        {
            uint Mask = (1u << BitWidth) - 1u;

            return (int)((Word >> Offset) & Mask);
        }
    }
}