using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation;
using static Spv.Specification;

namespace Ryujinx.Graphics.Shader.CodeGen.Spirv
{
    using SpvInstruction = Spv.Generator.Instruction;

    static class ScalingHelpers
    {
        public static SpvInstruction ApplyScaling(
            CodeGenContext context,
            AstTextureOperation texOp,
            SpvInstruction vector,
            bool intCoords,
            bool isBindless,
            bool isIndexed,
            bool isArray,
            int pCount)
        {
            if (intCoords)
            {
                if (context.Config.Stage.SupportsRenderScale() &&
                    !isBindless &&
                    !isIndexed)
                {
                    int index = texOp.Inst == Instruction.ImageLoad
                        ? context.Config.GetTextureDescriptors().Length + context.Config.FindImageDescriptorIndex(texOp)
                        : context.Config.FindTextureDescriptorIndex(texOp);

                    if (pCount == 3 && isArray)
                    {
                        return ApplyScaling2DArray(context, vector, index);
                    }
                    else if (pCount == 2 && !isArray)
                    {
                        return ApplyScaling2D(context, vector, index);
                    }
                }
            }

            return vector;
        }

        private static SpvInstruction ApplyScaling2DArray(CodeGenContext context, SpvInstruction vector, int index)
        {
            // The array index is not scaled, just x and y.
            var vectorXY = context.VectorShuffle(context.TypeVector(context.TypeS32(), 2), vector, vector, 0, 1);
            var vectorZ = context.CompositeExtract(context.TypeS32(), vector, 2);
            var vectorXYScaled = ApplyScaling2D(context, vectorXY, index);
            var vectorScaled = context.CompositeConstruct(context.TypeVector(context.TypeS32(), 3), vectorXYScaled, vectorZ);

            return vectorScaled;
        }

        private static SpvInstruction ApplyScaling2D(CodeGenContext context, SpvInstruction vector, int index)
        {
            var pointerType = context.TypePointer(StorageClass.Uniform, context.TypeFP32());
            var fieldIndex = context.Constant(context.TypeU32(), 4);
            var scaleIndex = context.Constant(context.TypeU32(), index);

            if (context.Config.Stage == ShaderStage.Vertex)
            {
                var scaleCountPointerType = context.TypePointer(StorageClass.Uniform, context.TypeS32());
                var scaleCountElemPointer = context.AccessChain(scaleCountPointerType, context.ConstantBuffers[0], context.Constant(context.TypeU32(), 3));
                var scaleCount = context.Load(context.TypeS32(), scaleCountElemPointer);

                scaleIndex = context.IAdd(context.TypeU32(), scaleIndex, scaleCount);
            }

            scaleIndex = context.IAdd(context.TypeU32(), scaleIndex, context.Constant(context.TypeU32(), 1));

            var scaleElemPointer = context.AccessChain(pointerType, context.ConstantBuffers[0], fieldIndex, scaleIndex);
            var scale = context.Load(context.TypeFP32(), scaleElemPointer);

            var ivector2Type = context.TypeVector(context.TypeS32(), 2);
            var localVector = context.CoordTemp;

            var passthrough = context.FOrdEqual(context.TypeBool(), scale, context.Constant(context.TypeFP32(), 1f));

            var mergeLabel = context.Label();

            if (context.Config.Stage == ShaderStage.Fragment)
            {
                var scaledInterpolatedLabel = context.Label();
                var scaledNoInterpolationLabel = context.Label();

                var needsInterpolation = context.FOrdLessThan(context.TypeBool(), scale, context.Constant(context.TypeFP32(), 0f));

                context.SelectionMerge(mergeLabel, SelectionControlMask.MaskNone);
                context.BranchConditional(needsInterpolation, scaledInterpolatedLabel, scaledNoInterpolationLabel);

                // scale < 0.0
                context.AddLabel(scaledInterpolatedLabel);

                ApplyScalingInterpolated(context, localVector, vector, scale);
                context.Branch(mergeLabel);

                // scale >= 0.0
                context.AddLabel(scaledNoInterpolationLabel);

                ApplyScalingNoInterpolation(context, localVector, vector, scale);
                context.Branch(mergeLabel);

                context.AddLabel(mergeLabel);

                var passthroughLabel = context.Label();
                var finalMergeLabel = context.Label();

                context.SelectionMerge(finalMergeLabel, SelectionControlMask.MaskNone);
                context.BranchConditional(passthrough, passthroughLabel, finalMergeLabel);

                context.AddLabel(passthroughLabel);

                context.Store(localVector, vector);
                context.Branch(finalMergeLabel);

                context.AddLabel(finalMergeLabel);

                return context.Load(ivector2Type, localVector);
            }
            else
            {
                var passthroughLabel = context.Label();
                var scaledLabel = context.Label();

                context.SelectionMerge(mergeLabel, SelectionControlMask.MaskNone);
                context.BranchConditional(passthrough, passthroughLabel, scaledLabel);

                // scale == 1.0
                context.AddLabel(passthroughLabel);

                context.Store(localVector, vector);
                context.Branch(mergeLabel);

                // scale != 1.0
                context.AddLabel(scaledLabel);

                ApplyScalingNoInterpolation(context, localVector, vector, scale);
                context.Branch(mergeLabel);

                context.AddLabel(mergeLabel);

                return context.Load(ivector2Type, localVector);
            }
        }

        private static void ApplyScalingInterpolated(CodeGenContext context, SpvInstruction output, SpvInstruction vector, SpvInstruction scale)
        {
            var vector2Type = context.TypeVector(context.TypeFP32(), 2);

            var scaleNegated = context.FNegate(context.TypeFP32(), scale);
            var scaleVector = context.CompositeConstruct(vector2Type, scaleNegated, scaleNegated);

            var vectorFloat = context.ConvertSToF(vector2Type, vector);
            var vectorScaled = context.VectorTimesScalar(vector2Type, vectorFloat, scaleNegated);

            var fragCoordPointer = context.Inputs[new IoDefinition(StorageKind.Input, IoVariable.FragmentCoord)];
            var fragCoord = context.Load(context.TypeVector(context.TypeFP32(), 4), fragCoordPointer);
            var fragCoordXY = context.VectorShuffle(vector2Type, fragCoord, fragCoord, 0, 1);

            var scaleMod = context.FMod(vector2Type, fragCoordXY, scaleVector);
            var vectorInterpolated = context.FAdd(vector2Type, vectorScaled, scaleMod);

            context.Store(output, context.ConvertFToS(context.TypeVector(context.TypeS32(), 2), vectorInterpolated));
        }

        private static void ApplyScalingNoInterpolation(CodeGenContext context, SpvInstruction output, SpvInstruction vector, SpvInstruction scale)
        {
            if (context.Config.Stage == ShaderStage.Vertex)
            {
                scale = context.GlslFAbs(context.TypeFP32(), scale);
            }

            var vector2Type = context.TypeVector(context.TypeFP32(), 2);

            var vectorFloat = context.ConvertSToF(vector2Type, vector);
            var vectorScaled = context.VectorTimesScalar(vector2Type, vectorFloat, scale);

            context.Store(output, context.ConvertFToS(context.TypeVector(context.TypeS32(), 2), vectorScaled));
        }

        public static SpvInstruction ApplyUnscaling(
            CodeGenContext context,
            AstTextureOperation texOp,
            SpvInstruction size,
            bool isBindless,
            bool isIndexed)
        {
            if (context.Config.Stage.SupportsRenderScale() &&
                !isBindless &&
                !isIndexed)
            {
                int index = context.Config.FindTextureDescriptorIndex(texOp);

                var pointerType = context.TypePointer(StorageClass.Uniform, context.TypeFP32());
                var fieldIndex = context.Constant(context.TypeU32(), 4);
                var scaleIndex = context.Constant(context.TypeU32(), index);

                if (context.Config.Stage == ShaderStage.Vertex)
                {
                    var scaleCountPointerType = context.TypePointer(StorageClass.Uniform, context.TypeS32());
                    var scaleCountElemPointer = context.AccessChain(scaleCountPointerType, context.ConstantBuffers[0], context.Constant(context.TypeU32(), 3));
                    var scaleCount = context.Load(context.TypeS32(), scaleCountElemPointer);

                    scaleIndex = context.IAdd(context.TypeU32(), scaleIndex, scaleCount);
                }

                scaleIndex = context.IAdd(context.TypeU32(), scaleIndex, context.Constant(context.TypeU32(), 1));

                var scaleElemPointer = context.AccessChain(pointerType, context.ConstantBuffers[0], fieldIndex, scaleIndex);
                var scale = context.GlslFAbs(context.TypeFP32(), context.Load(context.TypeFP32(), scaleElemPointer));

                var passthrough = context.FOrdEqual(context.TypeBool(), scale, context.Constant(context.TypeFP32(), 1f));

                var sizeFloat = context.ConvertSToF(context.TypeFP32(), size);
                var sizeUnscaled = context.FDiv(context.TypeFP32(), sizeFloat, scale);
                var sizeUnscaledInt = context.ConvertFToS(context.TypeS32(), sizeUnscaled);

                return context.Select(context.TypeS32(), passthrough, size, sizeUnscaledInt);
            }

            return size;
        }
    }
}