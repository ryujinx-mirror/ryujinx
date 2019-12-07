namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeTld4 : IOpCodeTexture
    {
        TextureGatherOffset Offset { get; }

        int GatherCompIndex { get; }

        bool Bindless { get; }
    }
}