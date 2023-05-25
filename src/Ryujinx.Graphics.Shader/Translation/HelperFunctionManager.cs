using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    class HelperFunctionManager
    {
        private readonly List<Function> _functionList;
        private readonly Dictionary<HelperFunctionName, int> _functionIds;
        private readonly ShaderStage _stage;

        public HelperFunctionManager(List<Function> functionList, ShaderStage stage)
        {
            _functionList = functionList;
            _functionIds = new Dictionary<HelperFunctionName, int>();
            _stage = stage;
        }

        public int GetOrCreateFunctionId(HelperFunctionName functionName)
        {
            if (_functionIds.TryGetValue(functionName, out int functionId))
            {
                return functionId;
            }

            Function function = GenerateFunction(functionName);
            functionId = _functionList.Count;
            _functionList.Add(function);
            _functionIds.Add(functionName, functionId);

            return functionId;
        }

        private Function GenerateFunction(HelperFunctionName functionName)
        {
            return functionName switch
            {
                HelperFunctionName.TexelFetchScale => GenerateTexelFetchScaleFunction(),
                HelperFunctionName.TextureSizeUnscale => GenerateTextureSizeUnscaleFunction(),
                _ => throw new ArgumentException($"Invalid function name {functionName}")
            };
        }

        private Function GenerateTexelFetchScaleFunction()
        {
            EmitterContext context = new EmitterContext();

            Operand input = Argument(0);
            Operand samplerIndex = Argument(1);
            Operand index = GetScaleIndex(context, samplerIndex);

            Operand scale = context.Load(StorageKind.ConstantBuffer, 0, Const((int)SupportBufferField.RenderScale), index);

            Operand scaleIsOne = context.FPCompareEqual(scale, ConstF(1f));
            Operand lblScaleNotOne = Label();

            context.BranchIfFalse(lblScaleNotOne, scaleIsOne);
            context.Return(input);
            context.MarkLabel(lblScaleNotOne);

            int inArgumentsCount;

            if (_stage == ShaderStage.Fragment)
            {
                Operand scaleIsLessThanZero = context.FPCompareLess(scale, ConstF(0f));
                Operand lblScaleGreaterOrEqualZero = Label();

                context.BranchIfFalse(lblScaleGreaterOrEqualZero, scaleIsLessThanZero);

                Operand negScale = context.FPNegate(scale);
                Operand inputScaled = context.FPMultiply(context.IConvertS32ToFP32(input), negScale);
                Operand fragCoordX = context.Load(StorageKind.Input, IoVariable.FragmentCoord, null, Const(0));
                Operand fragCoordY = context.Load(StorageKind.Input, IoVariable.FragmentCoord, null, Const(1));
                Operand fragCoord = context.ConditionalSelect(Argument(2), fragCoordY, fragCoordX);
                Operand inputBias = context.FPModulo(fragCoord, negScale);
                Operand inputWithBias = context.FPAdd(inputScaled, inputBias);

                context.Return(context.FP32ConvertToS32(inputWithBias));
                context.MarkLabel(lblScaleGreaterOrEqualZero);

                inArgumentsCount = 3;
            }
            else
            {
                inArgumentsCount = 2;
            }

            Operand inputScaled2 = context.FPMultiply(context.IConvertS32ToFP32(input), scale);

            context.Return(context.FP32ConvertToS32(inputScaled2));

            return new Function(ControlFlowGraph.Create(context.GetOperations()).Blocks, "TexelFetchScale", true, inArgumentsCount, 0);
        }

        private Function GenerateTextureSizeUnscaleFunction()
        {
            EmitterContext context = new EmitterContext();

            Operand input = Argument(0);
            Operand samplerIndex = Argument(1);
            Operand index = GetScaleIndex(context, samplerIndex);

            Operand scale = context.FPAbsolute(context.Load(StorageKind.ConstantBuffer, 0, Const((int)SupportBufferField.RenderScale), index));

            Operand scaleIsOne = context.FPCompareEqual(scale, ConstF(1f));
            Operand lblScaleNotOne = Label();

            context.BranchIfFalse(lblScaleNotOne, scaleIsOne);
            context.Return(input);
            context.MarkLabel(lblScaleNotOne);

            Operand inputUnscaled = context.FPDivide(context.IConvertS32ToFP32(input), scale);

            context.Return(context.FP32ConvertToS32(inputUnscaled));

            return new Function(ControlFlowGraph.Create(context.GetOperations()).Blocks, "TextureSizeUnscale", true, 2, 0);
        }

        private Operand GetScaleIndex(EmitterContext context, Operand index)
        {
            switch (_stage)
            {
                case ShaderStage.Vertex:
                    Operand fragScaleCount = context.Load(StorageKind.ConstantBuffer, 0, Const((int)SupportBufferField.FragmentRenderScaleCount));
                    return context.IAdd(Const(1), context.IAdd(index, fragScaleCount));
                default:
                    return context.IAdd(Const(1), index);
            }
        }
    }
}