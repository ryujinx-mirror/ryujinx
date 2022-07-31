using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    struct TransformFeedbackOutput
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

        public HashSet<int> Inputs { get; }
        public HashSet<int> Outputs { get; }
        public HashSet<int> InputsPerPatch { get; }
        public HashSet<int> OutputsPerPatch { get; }

        public HelperFunctionsMask HelperFunctionsMask { get; set; }

        public TransformFeedbackOutput[] TransformFeedbackOutputs { get; }

        public StructuredProgramInfo()
        {
            Functions = new List<StructuredFunction>();

            Inputs = new HashSet<int>();
            Outputs = new HashSet<int>();
            InputsPerPatch = new HashSet<int>();
            OutputsPerPatch = new HashSet<int>();

            TransformFeedbackOutputs = new TransformFeedbackOutput[0xc0];
        }
    }
}