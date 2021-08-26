namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeAttribute : IOpCode
    {
        int AttributeOffset { get; }
        int Count { get; }
        bool Indexed { get; }
    }
}