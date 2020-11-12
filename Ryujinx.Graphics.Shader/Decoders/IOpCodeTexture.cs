namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeTexture : IOpCode
    {
        Register Rd { get; }
        Register Ra { get; }
        Register Rb { get; }

        bool IsArray { get; }

        TextureDimensions Dimensions { get; }

        int ComponentMask { get; }

        int HandleOffset { get; }

        TextureLodMode LodMode { get; }

        bool HasOffset       { get; }
        bool HasDepthCompare { get; }
        bool IsMultisample   { get; }
    }
}