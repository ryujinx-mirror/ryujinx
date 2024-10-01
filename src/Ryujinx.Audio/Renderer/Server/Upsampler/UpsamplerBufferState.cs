using Ryujinx.Common.Memory;

namespace Ryujinx.Audio.Renderer.Server.Upsampler
{
    public struct UpsamplerBufferState
    {
        public const int HistoryLength = 20;

        public float Scale;
        public Array20<float> History;
        public bool Initialized;
        public int Phase;
    }
}
