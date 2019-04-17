using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Shader.Decoders;
using System;

namespace Ryujinx.Graphics.Shader
{
    struct OutputMapTarget
    {
        public bool Red   { get; }
        public bool Green { get; }
        public bool Blue  { get; }
        public bool Alpha { get; }

        public bool Enabled => Red || Green || Blue || Alpha;

        public OutputMapTarget(bool red, bool green, bool blue, bool alpha)
        {
            Red   = red;
            Green = green;
            Blue  = blue;
            Alpha = alpha;
        }

        public bool ComponentEnabled(int component)
        {
            switch (component)
            {
                case 0: return Red;
                case 1: return Green;
                case 2: return Blue;
                case 3: return Alpha;
            }

            throw new ArgumentOutOfRangeException(nameof(component));
        }
    }

    class ShaderHeader
    {
        public int SphType { get; }

        public int Version { get; }

        public int ShaderType { get; }

        public bool MrtEnable { get; }

        public bool KillsPixels { get; }

        public bool DoesGlobalStore { get; }

        public int SassVersion { get; }

        public bool DoesLoadOrStore { get; }

        public bool DoesFp64 { get; }

        public int StreamOutMask{ get; }

        public int ShaderLocalMemoryLowSize { get; }

        public int PerPatchAttributeCount { get; }

        public int ShaderLocalMemoryHighSize { get; }

        public int ThreadsPerInputPrimitive { get; }

        public int ShaderLocalMemoryCrsSize { get; }

        public int OutputTopology { get; }

        public int MaxOutputVertexCount { get; }

        public int StoreReqStart { get; }
        public int StoreReqEnd   { get; }

        public OutputMapTarget[] OmapTargets    { get; }
        public bool              OmapSampleMask { get; }
        public bool              OmapDepth      { get; }

        public ShaderHeader(IGalMemory memory, ulong address)
        {
            int commonWord0 = memory.ReadInt32((long)address + 0);
            int commonWord1 = memory.ReadInt32((long)address + 4);
            int commonWord2 = memory.ReadInt32((long)address + 8);
            int commonWord3 = memory.ReadInt32((long)address + 12);
            int commonWord4 = memory.ReadInt32((long)address + 16);

            SphType = commonWord0.Extract(0, 5);

            Version = commonWord0.Extract(5, 5);

            ShaderType = commonWord0.Extract(10, 4);

            MrtEnable = commonWord0.Extract(14);

            KillsPixels = commonWord0.Extract(15);

            DoesGlobalStore = commonWord0.Extract(16);

            SassVersion = commonWord0.Extract(17, 4);

            DoesLoadOrStore = commonWord0.Extract(26);

            DoesFp64 = commonWord0.Extract(27);

            StreamOutMask = commonWord0.Extract(28, 4);

            ShaderLocalMemoryLowSize = commonWord1.Extract(0, 24);

            PerPatchAttributeCount = commonWord1.Extract(24, 8);

            ShaderLocalMemoryHighSize = commonWord2.Extract(0, 24);

            ThreadsPerInputPrimitive = commonWord2.Extract(24, 8);

            ShaderLocalMemoryCrsSize = commonWord3.Extract(0, 24);

            OutputTopology = commonWord3.Extract(24, 4);

            MaxOutputVertexCount = commonWord4.Extract(0, 12);

            StoreReqStart = commonWord4.Extract(12, 8);
            StoreReqEnd   = commonWord4.Extract(24, 8);

            int type2OmapTarget = memory.ReadInt32((long)address + 72);
            int type2Omap       = memory.ReadInt32((long)address + 76);

            OmapTargets = new OutputMapTarget[8];

            for (int offset = 0; offset < OmapTargets.Length * 4; offset += 4)
            {
                OmapTargets[offset >> 2] = new OutputMapTarget(
                    type2OmapTarget.Extract(offset + 0),
                    type2OmapTarget.Extract(offset + 1),
                    type2OmapTarget.Extract(offset + 2),
                    type2OmapTarget.Extract(offset + 3));
            }

            OmapSampleMask = type2Omap.Extract(0);
            OmapDepth      = type2Omap.Extract(1);
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

                //Depth register is always two registers after the last color output.
                return count + 1;
            }
        }
    }
}