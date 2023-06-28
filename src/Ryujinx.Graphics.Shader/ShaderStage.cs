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

        Count,
    }

    public static class ShaderStageExtensions
    {
        /// <summary>
        /// Checks if the shader stage supports render scale.
        /// </summary>
        /// <param name="stage">Shader stage</param>
        /// <returns>True if the shader stage supports render scale, false otherwise</returns>
        public static bool SupportsRenderScale(this ShaderStage stage)
        {
            return stage == ShaderStage.Vertex || stage == ShaderStage.Fragment || stage == ShaderStage.Compute;
        }
    }
}
