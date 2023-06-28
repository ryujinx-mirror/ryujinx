namespace Ryujinx.Graphics.Shader.CodeGen.Spirv
{
    readonly record struct TextureMeta(int CbufSlot, int Handle, TextureFormat Format);
}
