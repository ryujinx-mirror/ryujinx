using System;

namespace Ryujinx.Graphics.Shader.Translation
{
    struct ShaderConfig
    {
        public ShaderStage Stage { get; }

        public OutputTopology OutputTopology { get; }

        public int MaxOutputVertices { get; }

        public int LocalMemorySize { get; }

        public ImapPixelType[] ImapTypes { get; }

        public OmapTarget[] OmapTargets    { get; }
        public bool         OmapSampleMask { get; }
        public bool         OmapDepth      { get; }

        public TranslationFlags Flags { get; }

        private TranslatorCallbacks _callbacks;

        public ShaderConfig(TranslationFlags flags, TranslatorCallbacks callbacks)
        {
            Stage             = ShaderStage.Compute;
            OutputTopology    = OutputTopology.PointList;
            MaxOutputVertices = 0;
            LocalMemorySize   = 0;
            ImapTypes         = null;
            OmapTargets       = null;
            OmapSampleMask    = false;
            OmapDepth         = false;
            Flags             = flags;
            _callbacks        = callbacks;
        }

        public ShaderConfig(ShaderHeader header, TranslationFlags flags, TranslatorCallbacks callbacks)
        {
            Stage             = header.Stage;
            OutputTopology    = header.OutputTopology;
            MaxOutputVertices = header.MaxOutputVertexCount;
            LocalMemorySize   = header.ShaderLocalMemoryLowSize + header.ShaderLocalMemoryHighSize;
            ImapTypes         = header.ImapTypes;
            OmapTargets       = header.OmapTargets;
            OmapSampleMask    = header.OmapSampleMask;
            OmapDepth         = header.OmapDepth;
            Flags             = flags;
            _callbacks        = callbacks;
        }

        public int GetDepthRegister()
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

            // The depth register is always two registers after the last color output.
            return count + 1;
        }

        public bool QueryInfoBool(QueryInfoName info, int index = 0)
        {
            return Convert.ToBoolean(QueryInfo(info, index));
        }

        public int QueryInfo(QueryInfoName info, int index = 0)
        {
            if (_callbacks.QueryInfo != null)
            {
                return _callbacks.QueryInfo(info, index);
            }
            else
            {
                switch (info)
                {
                    case QueryInfoName.ComputeLocalSizeX:
                    case QueryInfoName.ComputeLocalSizeY:
                    case QueryInfoName.ComputeLocalSizeZ:
                        return 1;
                    case QueryInfoName.ComputeLocalMemorySize:
                        return 0x1000;
                    case QueryInfoName.ComputeSharedMemorySize:
                        return 0xc000;
                    case QueryInfoName.IsTextureBuffer:
                        return Convert.ToInt32(false);
                    case QueryInfoName.IsTextureRectangle:
                        return Convert.ToInt32(false);
                    case QueryInfoName.PrimitiveTopology:
                        return (int)InputTopology.Points;
                    case QueryInfoName.StorageBufferOffsetAlignment:
                        return 16;
                    case QueryInfoName.SupportsNonConstantTextureOffset:
                        return Convert.ToInt32(true);
                }
            }

            return 0;
        }

        public void PrintLog(string message)
        {
            _callbacks.PrintLog?.Invoke(message);
        }
    }
}