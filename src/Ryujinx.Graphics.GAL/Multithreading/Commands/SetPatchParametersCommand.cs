using Ryujinx.Common.Memory;
using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetPatchParametersCommand : IGALCommand, IGALCommand<SetPatchParametersCommand>
    {
        public readonly CommandType CommandType => CommandType.SetPatchParameters;
        private int _vertices;
        private Array4<float> _defaultOuterLevel;
        private Array2<float> _defaultInnerLevel;

        public void Set(int vertices, ReadOnlySpan<float> defaultOuterLevel, ReadOnlySpan<float> defaultInnerLevel)
        {
            _vertices = vertices;
            defaultOuterLevel.CopyTo(_defaultOuterLevel.AsSpan());
            defaultInnerLevel.CopyTo(_defaultInnerLevel.AsSpan());
        }

        public static void Run(ref SetPatchParametersCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            renderer.Pipeline.SetPatchParameters(command._vertices, command._defaultOuterLevel.AsSpan(), command._defaultInnerLevel.AsSpan());
        }
    }
}
