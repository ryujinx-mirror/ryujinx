using Ryujinx.Graphics.Shader.CodeGen.Glsl;
using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.Instructions;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation.Optimizations;
using System;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    public static class Translator
    {
        private const int HeaderSize = 0x50;

        public static Span<byte> ExtractCode(Span<byte> code, bool compute, out int headerSize)
        {
            if (compute)
            {
                headerSize = 0;
            }
            else
            {
                headerSize = HeaderSize;
            }

            Block[] cfg = Decoder.Decode(code, (ulong)headerSize);

            ulong endAddress = 0;

            foreach (Block block in cfg)
            {
                if (endAddress < block.EndAddress)
                {
                    endAddress = block.EndAddress;
                }
            }

            return code.Slice(0, headerSize + (int)endAddress);
        }

        public static ShaderProgram Translate(Span<byte> code, TranslationConfig translationConfig)
        {
            bool compute   = (translationConfig.Flags & TranslationFlags.Compute)   != 0;
            bool debugMode = (translationConfig.Flags & TranslationFlags.DebugMode) != 0;

            Operation[] ops = DecodeShader(code, compute, debugMode, out ShaderHeader header);

            ShaderStage stage;

            if (compute)
            {
                stage = ShaderStage.Compute;
            }
            else
            {
                stage = header.Stage;
            }

            int maxOutputVertexCount = 0;

            OutputTopology outputTopology = OutputTopology.LineStrip;

            if (!compute)
            {
                maxOutputVertexCount = header.MaxOutputVertexCount;
                outputTopology       = header.OutputTopology;
            }

            ShaderConfig config = new ShaderConfig(
                stage,
                translationConfig.Flags,
                translationConfig.MaxCBufferSize,
                maxOutputVertexCount,
                outputTopology);

            return Translate(ops, config);
        }

        public static ShaderProgram Translate(Span<byte> vpACode, Span<byte> vpBCode, TranslationConfig translationConfig)
        {
            bool debugMode = (translationConfig.Flags & TranslationFlags.DebugMode) != 0;

            Operation[] vpAOps = DecodeShader(vpACode, compute: false, debugMode, out _);
            Operation[] vpBOps = DecodeShader(vpBCode, compute: false, debugMode, out ShaderHeader header);

            ShaderConfig config = new ShaderConfig(
                header.Stage,
                translationConfig.Flags,
                translationConfig.MaxCBufferSize,
                header.MaxOutputVertexCount,
                header.OutputTopology);

            return Translate(Combine(vpAOps, vpBOps), config);
        }

        private static ShaderProgram Translate(Operation[] ops, ShaderConfig config)
        {
            BasicBlock[] irBlocks = ControlFlowGraph.MakeCfg(ops);

            Dominance.FindDominators(irBlocks[0], irBlocks.Length);

            Dominance.FindDominanceFrontiers(irBlocks);

            Ssa.Rename(irBlocks);

            Optimizer.Optimize(irBlocks, config.Stage);

            StructuredProgramInfo sInfo = StructuredProgram.MakeStructuredProgram(irBlocks, config);

            GlslProgram program = GlslGenerator.Generate(sInfo, config);

            ShaderProgramInfo spInfo = new ShaderProgramInfo(
                program.CBufferDescriptors,
                program.SBufferDescriptors,
                program.TextureDescriptors,
                program.ImageDescriptors,
                sInfo.InterpolationQualifiers,
                sInfo.UsesInstanceId);

            string glslCode = program.Code;

            return new ShaderProgram(spInfo, config.Stage, glslCode);
        }

        private static Operation[] DecodeShader(Span<byte> code, bool compute, bool debugMode, out ShaderHeader header)
        {
            Block[] cfg;

            EmitterContext context;

            ulong headerSize;

            if (compute)
            {
                header = null;

                cfg = Decoder.Decode(code, 0);

                context = new EmitterContext(ShaderStage.Compute, header);

                headerSize = 0;
            }
            else
            {
                header = new ShaderHeader(code);

                cfg = Decoder.Decode(code, HeaderSize);

                context = new EmitterContext(header.Stage, header);

                headerSize = HeaderSize;
            }

            for (int blkIndex = 0; blkIndex < cfg.Length; blkIndex++)
            {
                Block block = cfg[blkIndex];

                context.CurrBlock = block;

                context.MarkLabel(context.GetLabel(block.Address));

                for (int opIndex = 0; opIndex < block.OpCodes.Count; opIndex++)
                {
                    OpCode op = block.OpCodes[opIndex];

                    if (debugMode)
                    {
                        string instName;

                        if (op.Emitter != null)
                        {
                            instName = op.Emitter.Method.Name;
                        }
                        else
                        {
                            instName = "???";
                        }

                        string dbgComment = $"0x{(op.Address - headerSize):X6}: 0x{op.RawOpCode:X16} {instName}";

                        context.Add(new CommentNode(dbgComment));
                    }

                    if (op.NeverExecute)
                    {
                        continue;
                    }

                    Operand predSkipLbl = null;

                    bool skipPredicateCheck = op.Emitter == InstEmit.Bra;

                    if (op is OpCodeSync opSync)
                    {
                        // If the instruction is a SYNC instruction with only one
                        // possible target address, then the instruction is basically
                        // just a simple branch, we can generate code similar to branch
                        // instructions, with the condition check on the branch itself.
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

                    if (op.Emitter != null)
                    {
                        op.Emitter(context);
                    }

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
            // Here we combine two shaders.
            // For shader A:
            // - All user attribute stores on shader A are turned into copies to a
            // temporary variable. It's assumed that shader B will consume them.
            // - All return instructions are turned into branch instructions, the
            // branch target being the start of the shader B code.
            // For shader B:
            // - All user attribute loads on shader B are turned into copies from a
            // temporary variable, as long that attribute is written by shader A.
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