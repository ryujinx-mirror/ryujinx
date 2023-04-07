namespace Ryujinx.Graphics.GAL
{
    public struct ShaderInfo
    {
        public int FragmentOutputMap { get; }
        public ProgramPipelineState? State { get; }
        public bool FromCache { get; set; }

        public ShaderInfo(int fragmentOutputMap, ProgramPipelineState state, bool fromCache = false)
        {
            FragmentOutputMap = fragmentOutputMap;
            State = state;
            FromCache = fromCache;
        }

        public ShaderInfo(int fragmentOutputMap, bool fromCache = false)
        {
            FragmentOutputMap = fragmentOutputMap;
            State = null;
            FromCache = fromCache;
        }
    }
}