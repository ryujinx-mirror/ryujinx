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

        public bool ComponentEnabled(int component)
        {
            switch (component)
            {
                case 0: return Red;
                case 1: return Green;
                case 2: return Blue;
                case 3: return Alpha;
            }

            throw new ArgumentException(nameof(component));
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

        public ShaderHeader(IGalMemory memory, long position)
        {
            uint commonWord0 = (uint)memory.ReadInt32(position + 0);
            uint commonWord1 = (uint)memory.ReadInt32(position + 4);
            uint commonWord2 = (uint)memory.ReadInt32(position + 8);
            uint commonWord3 = (uint)memory.ReadInt32(position + 12);
            uint commonWord4 = (uint)memory.ReadInt32(position + 16);

            SphType         = ReadBits(commonWord0,  0, 5);
            Version         = ReadBits(commonWord0,  5, 5);
            ShaderType      = ReadBits(commonWord0, 10, 4);
            MrtEnable       = ReadBits(commonWord0, 14, 1) != 0;
            KillsPixels     = ReadBits(commonWord0, 15, 1) != 0;
            DoesGlobalStore = ReadBits(commonWord0, 16, 1) != 0;
            SassVersion     = ReadBits(commonWord0, 17, 4);
            DoesLoadOrStore = ReadBits(commonWord0, 26, 1) != 0;
            DoesFp64        = ReadBits(commonWord0, 27, 1) != 0;
            StreamOutMask   = ReadBits(commonWord0, 28, 4);

            ShaderLocalMemoryLowSize = ReadBits(commonWord1,  0, 24);
            PerPatchAttributeCount   = ReadBits(commonWord1, 24,  8);

            ShaderLocalMemoryHighSize = ReadBits(commonWord2,  0, 24);
            ThreadsPerInputPrimitive  = ReadBits(commonWord2, 24,  8);

            ShaderLocalMemoryCrsSize = ReadBits(commonWord3,  0, 24);
            OutputTopology           = ReadBits(commonWord3, 24,  4);

            MaxOutputVertexCount = ReadBits(commonWord4,  0, 12);
            StoreReqStart        = ReadBits(commonWord4, 12,  8);
            StoreReqEnd          = ReadBits(commonWord4, 24,  8);

            //Type 2 (fragment?) reading
            uint type2OmapTarget = (uint)memory.ReadInt32(position + 72);
            uint type2Omap       = (uint)memory.ReadInt32(position + 76);

            OmapTargets = new OmapTarget[8];

            for (int i = 0; i < OmapTargets.Length; i++)
            {
                int offset = i * 4;

                OmapTargets[i] = new OmapTarget
                {
                    Red   = ReadBits(type2OmapTarget, offset + 0, 1) != 0,
                    Green = ReadBits(type2OmapTarget, offset + 1, 1) != 0,
                    Blue  = ReadBits(type2OmapTarget, offset + 2, 1) != 0,
                    Alpha = ReadBits(type2OmapTarget, offset + 3, 1) != 0
                };
            }

            OmapSampleMask = ReadBits(type2Omap, 0, 1) != 0;
            OmapDepth      = ReadBits(type2Omap, 1, 1) != 0;
        }

        public int DepthRegister
        {
            get
            {
                int count = 0;

                for (int index = 0; index < OmapTargets.Length; index++)
                {
                    for (int component = 0; component < 4; component++)
                    {
                        if (OmapTargets[index].ComponentEnabled(component))
                        {
                            count++;
                        }
                    }
                }

                // Depth register is always two registers after the last color output
                return count + 1;
            }
        }

        private static int ReadBits(uint word, int offset, int bitWidth)
        {
            uint mask = (1u << bitWidth) - 1u;

            return (int)((word >> offset) & mask);
        }
    }
}