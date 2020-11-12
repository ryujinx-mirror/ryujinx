namespace Ryujinx.Graphics.Shader
{
    public enum ShaderStage : byte
    {
        Compute,
        Vertex,
        TessellationControl,
        TessellationEvaluation,
        Geometry,
        Fragment,

        Count
    }
}