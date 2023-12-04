namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// Name of the High-level implementation of a Macro function.
    /// </summary>
    enum MacroHLEFunctionName
    {
        None,
        BindShaderProgram,
        ClearColor,
        ClearDepthStencil,
        DrawArraysInstanced,
        DrawElements,
        DrawElementsInstanced,
        DrawElementsIndirect,
        MultiDrawElementsIndirectCount,

        UpdateBlendState,
        UpdateColorMasks,
        UpdateUniformBufferState,
        UpdateUniformBufferStateCbu,
        UpdateUniformBufferStateCbuV2
    }
}
