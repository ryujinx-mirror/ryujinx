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

        public int AddFunction(Function function)
        {
            int functionId = _functionList.Count;
            _functionList.Add(function);

            return functionId;
        }

        public int GetOrCreateFunctionId(HelperFunctionName functionName)
        {
            if (_functionIds.TryGetValue(functionName, out int functionId))
            {
                return functionId;
            }

            Function function = GenerateFunction(functionName);
            functionId = AddFunction(function);
            _functionIds.Add(functionName, functionId);

            return functionId;
        }

        private Function GenerateFunction(HelperFunctionName functionName)
        {
            return functionName switch
            {
                HelperFunctionName.ConvertDoubleToFloat => GenerateConvertDoubleToFloatFunction(),
                HelperFunctionName.ConvertFloatToDouble => GenerateConvertFloatToDoubleFunction(),
                HelperFunctionName.TexelFetchScale => GenerateTexelFetchScaleFunction(),
                HelperFunctionName.TextureSizeUnscale => GenerateTextureSizeUnscaleFunction(),
                _ => throw new ArgumentException($"Invalid function name {functionName}")
            };
        }

        private Function GenerateConvertDoubleToFloatFunction()
        {
            EmitterContext context = new EmitterContext();

            Operand valueLow = Argument(0);
            Operand valueHigh = Argument(1);

            Operand mantissaLow = context.BitwiseAnd(valueLow, Const(((1 << 22) - 1)));
            Operand mantissa = context.ShiftRightU32(valueLow, Const(22));

            mantissa = context.BitwiseOr(mantissa, context.ShiftLeft(context.BitwiseAnd(valueHigh, Const(0xfffff)), Const(10)));
            mantissa = context.BitwiseOr(mantissa, context.ConditionalSelect(mantissaLow, Const(1), Const(0)));

            Operand exp = context.BitwiseAnd(context.ShiftRightU32(valueHigh, Const(20)), Const(0x7ff));
            Operand sign = context.ShiftRightS32(valueHigh, Const(31));

            Operand resultSign = context.ShiftLeft(sign, Const(31));

            Operand notZero = context.BitwiseOr(mantissa, exp);

            Operand lblNotZero = Label();

            context.BranchIfTrue(lblNotZero, notZero);

            context.Return(resultSign);

            context.MarkLabel(lblNotZero);

            Operand notNaNOrInf = context.ICompareNotEqual(exp, Const(0x7ff));

            mantissa = context.BitwiseOr(mantissa, Const(0x40000000));
            exp = context.ISubtract(exp, Const(0x381));

            // Note: Overflow cases are not handled here and might produce incorrect results.

            Operand roundBits = context.BitwiseAnd(mantissa, Const(0x7f));
            Operand roundBitsXor64 = context.BitwiseExclusiveOr(roundBits, Const(0x40));
            mantissa = context.ShiftRightU32(context.IAdd(mantissa, Const(0x40)), Const(7));
            mantissa = context.BitwiseAnd(mantissa, context.ConditionalSelect(roundBitsXor64, Const(~0), Const(~1)));

            exp = context.ConditionalSelect(mantissa, exp, Const(0));
            exp = context.ConditionalSelect(notNaNOrInf, exp, Const(0xff));

            Operand result = context.IAdd(context.IAdd(mantissa, context.ShiftLeft(exp, Const(23))), resultSign);

            context.Return(result);

            return new Function(ControlFlowGraph.Create(context.GetOperations()).Blocks, "ConvertDoubleToFloat", true, 2, 0);
        }

        private Function GenerateConvertFloatToDoubleFunction()
        {
            EmitterContext context = new EmitterContext();

            Operand value = Argument(0);

            Operand mantissa = context.BitwiseAnd(value, Const(0x7fffff));
            Operand exp = context.BitwiseAnd(context.ShiftRightU32(value, Const(23)), Const(0xff));
            Operand sign = context.ShiftRightS32(value, Const(31));

            Operand notNaNOrInf = context.ICompareNotEqual(exp, Const(0xff));
            Operand expNotZero = context.ICompareNotEqual(exp, Const(0));
            Operand notDenorm = context.BitwiseOr(expNotZero, context.ICompareEqual(mantissa, Const(0)));

            exp = context.IAdd(exp, Const(0x380));

            Operand shiftDist = context.ISubtract(Const(32), context.FindMSBU32(mantissa));
            Operand normExp = context.ISubtract(context.ISubtract(Const(1), shiftDist), Const(1));
            Operand normMant = context.ShiftLeft(mantissa, shiftDist);

            exp = context.ConditionalSelect(notNaNOrInf, exp, Const(0x7ff));
            exp = context.ConditionalSelect(notDenorm, exp, normExp);
            mantissa = context.ConditionalSelect(expNotZero, mantissa, normMant);

            Operand resultLow = context.ShiftLeft(mantissa, Const(29));
            Operand resultHigh = context.ShiftRightU32(mantissa, Const(3));

            resultHigh = context.IAdd(resultHigh, context.ShiftLeft(exp, Const(20)));
            resultHigh = context.IAdd(resultHigh, context.ShiftLeft(sign, Const(31)));

            context.Copy(Argument(1), resultLow);
            context.Copy(Argument(2), resultHigh);
            context.Return();

            return new Function(ControlFlowGraph.Create(context.GetOperations()).Blocks, "ConvertFloatToDouble", false, 1, 2);
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