using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Shader.CodeGen.Glsl;
using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.Instructions;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation.Optimizations;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    public static class Translator
    {
        public static ShaderProgram Translate(IGalMemory memory, ulong address, ShaderConfig config)
        {
            return Translate(memory, address, 0, config);
        }

        public static ShaderProgram Translate(
            IGalMemory   memory,
            ulong        address,
            ulong        addressB,
            ShaderConfig config)
        {
            Operation[] shaderOps = DecodeShader(memory, address, config.Type);

            if (addressB != 0)
            {
                //Dual vertex shader.
                Operation[] shaderOpsB = DecodeShader(memory, addressB, config.Type);

                shaderOps = Combine(shaderOps, shaderOpsB);
            }

            BasicBlock[] irBlocks = ControlFlowGraph.MakeCfg(shaderOps);

            Dominance.FindDominators(irBlocks[0], irBlocks.Length);

            Dominance.FindDominanceFrontiers(irBlocks);

            Ssa.Rename(irBlocks);

            Optimizer.Optimize(irBlocks);

            StructuredProgramInfo sInfo = StructuredProgram.MakeStructuredProgram(irBlocks);

            GlslProgram program = GlslGenerator.Generate(sInfo, config);

            ShaderProgramInfo spInfo = new ShaderProgramInfo(
                program.CBufferDescriptors,
                program.TextureDescriptors);

            return new ShaderProgram(spInfo, program.Code);
        }

        private static Operation[] DecodeShader(IGalMemory memory, ulong address, GalShaderType shaderType)
        {
            ShaderHeader header = new ShaderHeader(memory, address);

            Block[] cfg = Decoder.Decode(memory, address);

            EmitterContext context = new EmitterContext(shaderType, header);

            for (int blkIndex = 0; blkIndex < cfg.Length; blkIndex++)
            {
                Block block = cfg[blkIndex];

                context.CurrBlock = block;

                context.MarkLabel(context.GetLabel(block.Address));

                for (int opIndex = 0; opIndex < block.OpCodes.Count; opIndex++)
                {
                    OpCode op = block.OpCodes[opIndex];

                    if (op.NeverExecute)
                    {
                        continue;
                    }

                    Operand predSkipLbl = null;

                    bool skipPredicateCheck = op.Emitter == InstEmit.Bra;

                    if (op is OpCodeSync opSync)
                    {
                        //If the instruction is a SYNC instruction with only one
                        //possible target address, then the instruction is basically
                        //just a simple branch, we can generate code similar to branch
                        //instructions, with the condition check on the branch itself.
                        skipPredicateCheck |= opSync.Targets.Count < 2;
                    }

                    if (!(op.Predicate.IsPT || skipPredicateCheck))
                    {
                        Operand label;

                        if (opIndex == block.OpCodes.Count - 1 && block.Next != null)
                        {
                            label = context.GetLabel(block.Next.Address);
                        }
                        else
                        {
                            label = Label();

                            predSkipLbl = label;
                        }

                        Operand pred = Register(op.Predicate);

                        if (op.InvertPredicate)
                        {
                            context.BranchIfTrue(label, pred);
                        }
                        else
                        {
                            context.BranchIfFalse(label, pred);
                        }
                    }

                    context.CurrOp = op;

                    op.Emitter(context);

                    if (predSkipLbl != null)
                    {
                        context.MarkLabel(predSkipLbl);
                    }
                }
            }

            return context.GetOperations();
        }

        private static Operation[] Combine(Operation[] a, Operation[] b)
        {
            //Here we combine two shaders.
            //For shader A:
            //- All user attribute stores on shader A are turned into copies to a
            //temporary variable. It's assumed that shader B will consume them.
            //- All return instructions are turned into branch instructions, the
            //branch target being the start of the shader B code.
            //For shader B:
            //- All user attribute loads on shader B are turned into copies from a
            //temporary variable, as long that attribute is written by shader A.
            List<Operation> output = new List<Operation>(a.Length + b.Length);

            Operand[] temps = new Operand[AttributeConsts.UserAttributesCount * 4];

            Operand lblB = Label();

            for (int index = 0; index < a.Length; index++)
            {
                Operation operation = a[index];

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
                    output.Add(new Operation(Instruction.Branch, lblB));
                }
                else
                {
                    output.Add(operation);
                }
            }

            output.Add(new Operation(Instruction.MarkLabel, lblB));

            for (int index = 0; index < b.Length; index++)
            {
                Operation operation = b[index];

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

                output.Add(operation);
            }

            return output.ToArray();
        }

        private static bool IsUserAttribute(Operand operand)
        {
            return operand != null &&
                   operand.Type == OperandType.Attribute &&
                   operand.Value >= AttributeConsts.UserAttributeBase &&
                   operand.Value <  AttributeConsts.UserAttributeEnd;
        }
    }
}