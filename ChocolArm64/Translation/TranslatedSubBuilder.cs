using ChocolArm64.IntermediateRepresentation;
using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;

using static ChocolArm64.State.RegisterConsts;

namespace ChocolArm64.Translation
{
    class TranslatedSubBuilder
    {
        private ExecutionMode _mode;

        private bool _isComplete;

        private Dictionary<Register, int> _locals;

        private RegisterUsage _regUsage;

        public TranslatedSubBuilder(ExecutionMode mode, bool isComplete = false)
        {
            _mode       = mode;
            _isComplete = isComplete;
        }

        public TranslatedSub Build(BasicBlock[] blocks, string name, TranslationTier tier, bool rejit = true)
        {
            _regUsage = new RegisterUsage(blocks[0], blocks.Length);

            DynamicMethod method = new DynamicMethod(name, typeof(long), TranslatedSub.FixedArgTypes);

            TranslatedSub subroutine = new TranslatedSub(method, tier, rejit);

            _locals = new Dictionary<Register, int>();

            Dictionary<ILLabel, Label> labels = new Dictionary<ILLabel, Label>();

            ILGenerator generator = method.GetILGenerator();

            Label GetLabel(ILLabel label)
            {
                if (!labels.TryGetValue(label, out Label ilLabel))
                {
                    ilLabel = generator.DefineLabel();

                    labels.Add(label, ilLabel);
                }

                return ilLabel;
            }

            foreach (BasicBlock block in blocks)
            {
                for (int index = 0; index < block.Count; index++)
                {
                    Operation operation = block.GetOperation(index);

                    switch (operation.Type)
                    {
                        case OperationType.Call:
                            generator.Emit(OpCodes.Call, operation.GetArg<MethodInfo>(0));
                            break;

                        case OperationType.CallVirtual:
                            generator.Emit(OpCodes.Callvirt, operation.GetArg<MethodInfo>(0));
                            break;

                        case OperationType.IL:
                            generator.Emit(operation.GetArg<OpCode>(0));
                            break;

                        case OperationType.ILBranch:
                            generator.Emit(operation.GetArg<OpCode>(0), GetLabel(operation.GetArg<ILLabel>(1)));
                            break;

                        case OperationType.LoadArgument:
                            generator.EmitLdarg(operation.GetArg<int>(0));
                            break;

                        case OperationType.LoadConstant:
                            EmitLoadConstant(generator, operation.GetArg(0));
                            break;

                        case OperationType.LoadContext:
                            EmitLoadContext(generator, operation.Parent);
                            break;

                        case OperationType.LoadField:
                            generator.Emit(OpCodes.Ldfld, operation.GetArg<FieldInfo>(0));
                            break;

                        case OperationType.LoadLocal:
                            EmitLoadLocal(
                                generator,
                                operation.GetArg<int>(0),
                                operation.GetArg<RegisterType>(1),
                                operation.GetArg<RegisterSize>(2));
                            break;

                        case OperationType.MarkLabel:
                            generator.MarkLabel(GetLabel(operation.GetArg<ILLabel>(0)));
                            break;

                        case OperationType.StoreContext:
                            EmitStoreContext(generator, operation.Parent);
                            break;

                        case OperationType.StoreLocal:
                            EmitStoreLocal(
                                generator,
                                operation.GetArg<int>(0),
                                operation.GetArg<RegisterType>(1),
                                operation.GetArg<RegisterSize>(2));
                            break;
                    }
                }
            }

            subroutine.PrepareMethod();

            return subroutine;
        }

        private static void EmitLoadConstant(ILGenerator generator, object value)
        {
            switch (value)
            {
                case int    valI4: generator.EmitLdc_I4(valI4);           break;
                case long   valI8: generator.Emit(OpCodes.Ldc_I8, valI8); break;
                case float  valR4: generator.Emit(OpCodes.Ldc_R4, valR4); break;
                case double valR8: generator.Emit(OpCodes.Ldc_R8, valR8); break;
            }
        }

        private void EmitLoadContext(ILGenerator generator, BasicBlock block)
        {
            RegisterMask inputs = _regUsage.GetInputs(block);

            long intInputs = inputs.IntMask;
            long vecInputs = inputs.VecMask;

            if (Optimizations.AssumeStrictAbiCompliance && _isComplete)
            {
                intInputs = RegisterUsage.ClearCallerSavedIntRegs(intInputs, _mode);
                vecInputs = RegisterUsage.ClearCallerSavedVecRegs(vecInputs, _mode);
            }

            LoadLocals(generator, intInputs, RegisterType.Int);
            LoadLocals(generator, vecInputs, RegisterType.Vector);
        }

        private void LoadLocals(ILGenerator generator, long inputs, RegisterType baseType)
        {
            for (int bit = 0; bit < 64; bit++)
            {
                long mask = 1L << bit;

                if ((inputs & mask) != 0)
                {
                    Register reg = GetRegFromBit(bit, baseType);

                    generator.EmitLdarg(TranslatedSub.StateArgIdx);
                    generator.Emit(OpCodes.Ldfld, reg.GetField());

                    generator.EmitStloc(GetLocalIndex(generator, reg));
                }
            }
        }

        private void EmitStoreContext(ILGenerator generator, BasicBlock block)
        {
            RegisterMask outputs = _regUsage.GetOutputs(block);

            long intOutputs = outputs.IntMask;
            long vecOutputs = outputs.VecMask;

            if (Optimizations.AssumeStrictAbiCompliance && _isComplete)
            {
                intOutputs = RegisterUsage.ClearCallerSavedIntRegs(intOutputs, _mode);
                vecOutputs = RegisterUsage.ClearCallerSavedVecRegs(vecOutputs, _mode);
            }

            StoreLocals(generator, intOutputs, RegisterType.Int);
            StoreLocals(generator, vecOutputs, RegisterType.Vector);
        }

        private void StoreLocals(ILGenerator generator, long outputs, RegisterType baseType)
        {
            for (int bit = 0; bit < 64; bit++)
            {
                long mask = 1L << bit;

                if ((outputs & mask) != 0)
                {
                    Register reg = GetRegFromBit(bit, baseType);

                    generator.EmitLdarg(TranslatedSub.StateArgIdx);
                    generator.EmitLdloc(GetLocalIndex(generator, reg));

                    generator.Emit(OpCodes.Stfld, reg.GetField());
                }
            }
        }

        private void EmitLoadLocal(ILGenerator generator, int index, RegisterType type, RegisterSize size)
        {
            Register reg = new Register(index, type);

            generator.EmitLdloc(GetLocalIndex(generator, reg));

            if (type == RegisterType.Int && size == RegisterSize.Int32)
            {
                generator.Emit(OpCodes.Conv_U4);
            }
        }

        private void EmitStoreLocal(ILGenerator generator, int index, RegisterType type, RegisterSize size)
        {
            Register reg = new Register(index, type);

            if (type == RegisterType.Int && size == RegisterSize.Int32)
            {
                generator.Emit(OpCodes.Conv_U8);
            }

            generator.EmitStloc(GetLocalIndex(generator, reg));
        }

        private int GetLocalIndex(ILGenerator generator, Register reg)
        {
            if (!_locals.TryGetValue(reg, out int index))
            {
                generator.DeclareLocal(GetFieldType(reg.Type));

                index = _locals.Count;

                _locals.Add(reg, index);
            }

            return index;
        }

        private static Type GetFieldType(RegisterType regType)
        {
            switch (regType)
            {
                case RegisterType.Flag:   return typeof(bool);
                case RegisterType.Int:    return typeof(ulong);
                case RegisterType.Vector: return typeof(Vector128<float>);
            }

            throw new ArgumentException(nameof(regType));
        }

        private static Register GetRegFromBit(int bit, RegisterType baseType)
        {
            if (bit < RegsCount)
            {
                return new Register(bit, baseType);
            }
            else if (baseType == RegisterType.Int)
            {
                return new Register(bit & RegsMask, RegisterType.Flag);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(bit));
            }
        }
    }
}