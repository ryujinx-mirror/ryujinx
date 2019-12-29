using System;

namespace Ryujinx.Graphics.Shader.Translation
{
    struct ShaderConfig
    {
        public ShaderStage Stage { get; }

        public OutputTopology OutputTopology { get; }

        public int MaxOutputVertices { get; }

        public OutputMapTarget[] OmapTargets    { get; }
        public bool              OmapSampleMask { get; }
        public bool              OmapDepth      { get; }

        public TranslationFlags Flags { get; }

        private QueryInfoCallback _queryInfoCallback;

        public ShaderConfig(TranslationFlags flags, QueryInfoCallback queryInfoCallback)
        {
            Stage              = ShaderStage.Compute;
            OutputTopology     = OutputTopology.PointList;
            MaxOutputVertices  = 0;
            OmapTargets        = null;
            OmapSampleMask     = false;
            OmapDepth          = false;
            Flags              = flags;
            _queryInfoCallback = queryInfoCallback;
        }

        public ShaderConfig(ShaderHeader header, TranslationFlags flags, QueryInfoCallback queryInfoCallback)
        {
            Stage              = header.Stage;
            OutputTopology     = header.OutputTopology;
            MaxOutputVertices  = header.MaxOutputVertexCount;
            OmapTargets        = header.OmapTargets;
            OmapSampleMask     = header.OmapSampleMask;
            OmapDepth          = header.OmapDepth;
            Flags              = flags;
            _queryInfoCallback = queryInfoCallback;
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
            if (_queryInfoCallback != null)
            {
                return _queryInfoCallback(info, index);
            }
            else
            {
                switch (info)
                {
                    case QueryInfoName.ComputeLocalSizeX:
                    case QueryInfoName.ComputeLocalSizeY:
                    case QueryInfoName.ComputeLocalSizeZ:
                        return 1;
                    case QueryInfoName.ComputeSharedMemorySize:
                        return 0xc000;
                    case QueryInfoName.IsTextureBuffer:
                        return Convert.ToInt32(false);
                    case QueryInfoName.IsTextureRectangle:
                        return Convert.ToInt32(false);
                    case QueryInfoName.MaximumViewportDimensions:
                        return 0x8000;
                    case QueryInfoName.PrimitiveTopology:
                        return (int)InputTopology.Points;
                    case QueryInfoName.StorageBufferOffsetAlignment:
                        return 16;
                    case QueryInfoName.SupportsNonConstantTextureOffset:
                        return Convert.ToInt32(true);
                    case QueryInfoName.ViewportTransformEnable:
                        return Convert.ToInt32(true);
                }
            }

            return 0;
        }
    }
}