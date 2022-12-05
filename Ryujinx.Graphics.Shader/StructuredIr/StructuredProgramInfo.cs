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

        public TransformFeedbackOutput GetTransformFeedbackOutput(int attr)
        {
            int index = attr / 4;
            return TransformFeedbackOutputs[index];
        }

        public int GetTransformFeedbackOutputComponents(int attr)
        {
            int index = attr / 4;
            int baseIndex = index & ~3;

            int count = 1;

            for (; count < 4; count++)
            {
                ref var prev = ref TransformFeedbackOutputs[baseIndex + count - 1];
                ref var curr = ref TransformFeedbackOutputs[baseIndex + count];

                int prevOffset = prev.Offset;
                int currOffset = curr.Offset;

                if (!prev.Valid || !curr.Valid || prevOffset + 4 != currOffset)
                {
                    break;
                }
            }

            if (baseIndex + count <= index)
            {
                return 1;
            }

            return count;
        }
    }
}