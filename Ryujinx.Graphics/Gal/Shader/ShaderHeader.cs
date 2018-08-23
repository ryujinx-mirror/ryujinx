using System;

namespace Ryujinx.Graphics.Gal.Shader
{
    struct OmapTarget
    {
        public bool Red;
        public bool Green;
        public bool Blue;
        public bool Alpha;

        public bool Enabled => Red || Green || Blue || Alpha;

        public bool ComponentEnabled(int Component)
        {
            switch (Component)
            {
                case 0: return Red;
                case 1: return Green;
                case 2: return Blue;
                case 3: return Alpha;
            }

            throw new ArgumentException(nameof(Component));
        }
    }

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

        public OmapTarget[] OmapTargets    { get; private set; }
        public bool         OmapSampleMask { get; private set; }
        public bool         OmapDepth      { get; private set; }

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

            //Type 2 (fragment?) reading
            uint Type2OmapTarget = (uint)Memory.ReadInt32(Position + 72);
            uint Type2Omap       = (uint)Memory.ReadInt32(Position + 76);

            OmapTargets = new OmapTarget[8];

            for (int i = 0; i < OmapTargets.Length; i++)
            {
                int Offset = i * 4;

                OmapTargets[i] = new OmapTarget
                {
                    Red   = ReadBits(Type2OmapTarget, Offset + 0, 1) != 0,
                    Green = ReadBits(Type2OmapTarget, Offset + 1, 1) != 0,
                    Blue  = ReadBits(Type2OmapTarget, Offset + 2, 1) != 0,
                    Alpha = ReadBits(Type2OmapTarget, Offset + 3, 1) != 0
                };
            }

            OmapSampleMask = ReadBits(Type2Omap, 0, 1) != 0;
            OmapDepth      = ReadBits(Type2Omap, 1, 1) != 0;
        }

        public int DepthRegister
        {
            get
            {
                int Count = 0;

                for (int Index = 0; Index < OmapTargets.Length; Index++)
                {
                    for (int Component = 0; Component < 4; Component++)
                    {
                        if (OmapTargets[Index].ComponentEnabled(Component))
                        {
                            Count++;
                        }
                    }
                }

                // Depth register is always two registers after the last color output
                return Count + 1;
            }
        }

        private static int ReadBits(uint Word, int Offset, int BitWidth)
        {
            uint Mask = (1u << BitWidth) - 1u;

            return (int)((Word >> Offset) & Mask);
        }
    }
}