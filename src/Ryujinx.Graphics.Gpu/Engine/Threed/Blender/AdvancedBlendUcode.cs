using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Engine.Threed.Blender
{
    /// <summary>
    /// Fixed function alpha state used for a advanced blend function.
    /// </summary>
    readonly struct FixedFunctionAlpha
    {
        /// <summary>
        /// Fixed function alpha state with alpha blending disabled.
        /// </summary>
        public static FixedFunctionAlpha Disabled => new(BlendUcodeEnable.EnableRGBA, default, default, default);

        /// <summary>
        /// Individual enable bits for the RGB and alpha components.
        /// </summary>
        public BlendUcodeEnable Enable { get; }

        /// <summary>
        /// Alpha blend operation.
        /// </summary>
        public BlendOp AlphaOp { get; }

        /// <summary>
        /// Value multiplied with the blend source operand.
        /// </summary>
        public BlendFactor AlphaSrcFactor { get; }

        /// <summary>
        /// Value multiplied with the blend destination operand.
        /// </summary>
        public BlendFactor AlphaDstFactor { get; }

        /// <summary>
        /// Creates a new blend fixed function alpha state.
        /// </summary>
        /// <param name="enable">Individual enable bits for the RGB and alpha components</param>
        /// <param name="alphaOp">Alpha blend operation</param>
        /// <param name="alphaSrc">Value multiplied with the blend source operand</param>
        /// <param name="alphaDst">Value multiplied with the blend destination operand</param>
        public FixedFunctionAlpha(BlendUcodeEnable enable, BlendOp alphaOp, BlendFactor alphaSrc, BlendFactor alphaDst)
        {
            Enable = enable;
            AlphaOp = alphaOp;
            AlphaSrcFactor = alphaSrc;
            AlphaDstFactor = alphaDst;
        }

        /// <summary>
        /// Creates a new blend fixed function alpha state.
        /// </summary>
        /// <param name="alphaOp">Alpha blend operation</param>
        /// <param name="alphaSrc">Value multiplied with the blend source operand</param>
        /// <param name="alphaDst">Value multiplied with the blend destination operand</param>
        public FixedFunctionAlpha(BlendOp alphaOp, BlendFactor alphaSrc, BlendFactor alphaDst) : this(BlendUcodeEnable.EnableRGB, alphaOp, alphaSrc, alphaDst)
        {
        }
    }

    /// <summary>
    /// Blend microcode assembly function delegate.
    /// </summary>
    /// <param name="asm">Assembler</param>
    /// <returns>Fixed function alpha state for the microcode</returns>
    delegate FixedFunctionAlpha GenUcodeFunc(ref UcodeAssembler asm);

    /// <summary>
    /// Advanced blend microcode state.
    /// </summary>
    readonly struct AdvancedBlendUcode
    {
        /// <summary>
        /// Advanced blend operation.
        /// </summary>
        public AdvancedBlendOp Op { get; }

        /// <summary>
        /// Advanced blend overlap mode.
        /// </summary>
        public AdvancedBlendOverlap Overlap { get; }

        /// <summary>
        /// Whenever the source input is pre-multiplied.
        /// </summary>
        public bool SrcPreMultiplied { get; }

        /// <summary>
        /// Fixed function alpha state.
        /// </summary>
        public FixedFunctionAlpha Alpha { get; }

        /// <summary>
        /// Microcode.
        /// </summary>
        public uint[] Code { get; }

        /// <summary>
        /// Constants used by the microcode.
        /// </summary>
        public RgbFloat[] Constants { get; }

        /// <summary>
        /// Creates a new advanced blend state.
        /// </summary>
        /// <param name="op">Advanced blend operation</param>
        /// <param name="overlap">Advanced blend overlap mode</param>
        /// <param name="srcPreMultiplied">Whenever the source input is pre-multiplied</param>
        /// <param name="genFunc">Function that will generate the advanced blend microcode</param>
        public AdvancedBlendUcode(
            AdvancedBlendOp op,
            AdvancedBlendOverlap overlap,
            bool srcPreMultiplied,
            GenUcodeFunc genFunc)
        {
            Op = op;
            Overlap = overlap;
            SrcPreMultiplied = srcPreMultiplied;

            UcodeAssembler asm = new();
            Alpha = genFunc(ref asm);
            Code = asm.GetCode();
            Constants = asm.GetConstants();
        }
    }
}
