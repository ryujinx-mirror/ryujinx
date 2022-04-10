using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;
using static Ryujinx.Graphics.Shader.Translation.Translator;

namespace Ryujinx.Graphics.Shader.Translation
{
    public class TranslatorContext
    {
        private readonly DecodedProgram _program;
        private ShaderConfig _config;

        public ulong Address { get; }

        public ShaderStage Stage => _config.Stage;
        public int Size => _config.Size;
        public int Cb1DataSize => _config.Cb1DataSize;

        public IGpuAccessor GpuAccessor => _config.GpuAccessor;

        internal TranslatorContext(ulong address, DecodedProgram program, ShaderConfig config)
        {
            Address = address;
            _program = program;
            _config = config;
        }

        private static bool IsUserAttribute(Operand operand)
        {
            if (operand != null && operand.Type.IsAttribute())
            {
                int value = operand.Value & AttributeConsts.Mask;
                return value >= AttributeConsts.UserAttributeBase && value < AttributeConsts.UserAttributeEnd;
            }

            return false;
        }

        private static FunctionCode[] Combine(FunctionCode[] a, FunctionCode[] b, int aStart)
        {
            // Here we combine two shaders.
            // For shader A:
            // - All user attribute stores on shader A are turned into copies to a
            // temporary variable. It's assumed that shader B will consume them.
            // - All return instructions are turned into branch instructions, the
            // branch target being the start of the shader B code.
            // For shader B:
            // - All user attribute loads on shader B are turned into copies from a
            // temporary variable, as long that attribute is written by shader A.
            FunctionCode[] output = new FunctionCode[a.Length + b.Length - 1];

            List<Operation> ops = new List<Operation>(a.Length + b.Length);

            Operand[] temps = new Operand[AttributeConsts.UserAttributesCount * 4];

            Operand lblB = Label();

            for (int index = aStart; index < a[0].Code.Length; index++)
            {
                Operation operation = a[0].Code[index];

                if (IsUserAttribute(operation.Dest))
                {
                    int tIndex = (operation.Dest.Value - AttributeConsts.UserAttributeBase) / 4;

                    Operand temp = temps[tIndex];

                    if (temp == null)
                    {
                        temp = Local();

                        temps[tIndex] = temp;
                    }

                    operation.Dest = temp;
                }

                if (operation.Inst == Instruction.Return)
                {
                    ops.Add(new Operation(Instruction.Branch, lblB));
                }
                else
                {
                    ops.Add(operation);
                }
            }

            ops.Add(new Operation(Instruction.MarkLabel, lblB));

            for (int index = 0; index < b[0].Code.Length; index++)
            {
                Operation operation = b[0].Code[index];

                for (int srcIndex = 0; srcIndex < operation.SourcesCount; srcIndex++)
                {
                    Operand src = operation.GetSource(srcIndex);

                    if (IsUserAttribute(src))
                    {
                        Operand temp = temps[(src.Value - AttributeConsts.UserAttributeBase) / 4];

                        if (temp != null)
                        {
                            operation.SetSource(srcIndex, temp);
                        }
                    }
                }

                ops.Add(operation);
            }

            output[0] = new FunctionCode(ops.ToArray());

            for (int i = 1; i < a.Length; i++)
            {
                output[i] = a[i];
            }

            for (int i = 1; i < b.Length; i++)
            {
                output[a.Length + i - 1] = b[i];
            }

            return output;
        }

        public void SetNextStage(TranslatorContext nextStage)
        {
            _config.MergeFromtNextStage(nextStage._config);
        }

        public ShaderProgram Translate(TranslatorContext other = null)
        {
            FunctionCode[] code = EmitShader(_program, _config, initializeOutputs: other == null, out _);

            if (other != null)
            {
                other._config.MergeOutputUserAttributes(_config.UsedOutputAttributes, 0);

                FunctionCode[] otherCode = EmitShader(other._program, other._config, initializeOutputs: true, out int aStart);

                code = Combine(otherCode, code, aStart);

                _config.InheritFrom(other._config);
            }

            return Translator.Translate(code, _config);
        }
    }
}
