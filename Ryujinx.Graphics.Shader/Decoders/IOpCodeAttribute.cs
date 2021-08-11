namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeAttribute
    {
        int AttributeOffset { get; }
        int Count { get; }
    }
}