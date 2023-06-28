using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    readonly struct TransformFeedbackOutput
    {
        public readonly bool Valid;
        public readonly int Buffer;
        public readonly int Offset;
        public readonly int Stride;

        public TransformFeedbackOutput(int buffer, int offset, int stride)
        {
            Valid = true;
            Buffer = buffer;
            Offset = offset;
            Stride = stride;
        }
    }

    class StructuredProgramInfo
    {
        public List<StructuredFunction> Functions { get; }

        public HashSet<IoDefinition> IoDefinitions { get; }

        public HelperFunctionsMask HelperFunctionsMask { get; set; }

        public StructuredProgramInfo()
        {
            Functions = new List<StructuredFunction>();

            IoDefinitions = new HashSet<IoDefinition>();
        }
    }
}
