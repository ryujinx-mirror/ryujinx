namespace Ryujinx.Graphics.GAL
{
    public struct ShaderInfo
    {
        public int FragmentOutputMap { get; }

        public ShaderInfo(int fragmentOutputMap)
        {
            FragmentOutputMap = fragmentOutputMap;
        }
    }
}