using System;
using System.Reflection;

namespace ChocolArm64.State
{
    struct ARegister
    {
        public int Index;

        public ARegisterType Type;

        public ARegister(int Index, ARegisterType Type)
        {
            this.Index = Index;
            this.Type  = Type;
        }

        public override int GetHashCode()
        {
            return (ushort)Index | ((ushort)Type << 16);
        }

        public override bool Equals(object Obj)
        {
            return Obj is ARegister Reg &&
                   Reg.Index == Index &&
                   Reg.Type  == Type;
        }

        public FieldInfo GetField()
        {
            switch (Type)
            {
                case ARegisterType.Flag:   return GetFieldFlag();
                case ARegisterType.Int:    return GetFieldInt();
                case ARegisterType.Vector: return GetFieldVector();
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetFieldFlag()
        {
            switch ((APState)Index)
            {
                case APState.VBit: return GetField(nameof(ARegisters.Overflow));
                case APState.CBit: return GetField(nameof(ARegisters.Carry));
                case APState.ZBit: return GetField(nameof(ARegisters.Zero));
                case APState.NBit: return GetField(nameof(ARegisters.Negative));
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetFieldInt()
        {
            switch (Index)
            {
                case 0:  return GetField(nameof(ARegisters.X0));
                case 1:  return GetField(nameof(ARegisters.X1));
                case 2:  return GetField(nameof(ARegisters.X2));
                case 3:  return GetField(nameof(ARegisters.X3));
                case 4:  return GetField(nameof(ARegisters.X4));
                case 5:  return GetField(nameof(ARegisters.X5));
                case 6:  return GetField(nameof(ARegisters.X6));
                case 7:  return GetField(nameof(ARegisters.X7));
                case 8:  return GetField(nameof(ARegisters.X8));
                case 9:  return GetField(nameof(ARegisters.X9));
                case 10: return GetField(nameof(ARegisters.X10));
                case 11: return GetField(nameof(ARegisters.X11));
                case 12: return GetField(nameof(ARegisters.X12));
                case 13: return GetField(nameof(ARegisters.X13));
                case 14: return GetField(nameof(ARegisters.X14));
                case 15: return GetField(nameof(ARegisters.X15));
                case 16: return GetField(nameof(ARegisters.X16));
                case 17: return GetField(nameof(ARegisters.X17));
                case 18: return GetField(nameof(ARegisters.X18));
                case 19: return GetField(nameof(ARegisters.X19));
                case 20: return GetField(nameof(ARegisters.X20));
                case 21: return GetField(nameof(ARegisters.X21));
                case 22: return GetField(nameof(ARegisters.X22));
                case 23: return GetField(nameof(ARegisters.X23));
                case 24: return GetField(nameof(ARegisters.X24));
                case 25: return GetField(nameof(ARegisters.X25));
                case 26: return GetField(nameof(ARegisters.X26));
                case 27: return GetField(nameof(ARegisters.X27));
                case 28: return GetField(nameof(ARegisters.X28));
                case 29: return GetField(nameof(ARegisters.X29));
                case 30: return GetField(nameof(ARegisters.X30));
                case 31: return GetField(nameof(ARegisters.X31));
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetFieldVector()
        {
            switch (Index)
            {
                case 0:  return GetField(nameof(ARegisters.V0));
                case 1:  return GetField(nameof(ARegisters.V1));
                case 2:  return GetField(nameof(ARegisters.V2));
                case 3:  return GetField(nameof(ARegisters.V3));
                case 4:  return GetField(nameof(ARegisters.V4));
                case 5:  return GetField(nameof(ARegisters.V5));
                case 6:  return GetField(nameof(ARegisters.V6));
                case 7:  return GetField(nameof(ARegisters.V7));
                case 8:  return GetField(nameof(ARegisters.V8));
                case 9:  return GetField(nameof(ARegisters.V9));
                case 10: return GetField(nameof(ARegisters.V10));
                case 11: return GetField(nameof(ARegisters.V11));
                case 12: return GetField(nameof(ARegisters.V12));
                case 13: return GetField(nameof(ARegisters.V13));
                case 14: return GetField(nameof(ARegisters.V14));
                case 15: return GetField(nameof(ARegisters.V15));
                case 16: return GetField(nameof(ARegisters.V16));
                case 17: return GetField(nameof(ARegisters.V17));
                case 18: return GetField(nameof(ARegisters.V18));
                case 19: return GetField(nameof(ARegisters.V19));
                case 20: return GetField(nameof(ARegisters.V20));
                case 21: return GetField(nameof(ARegisters.V21));
                case 22: return GetField(nameof(ARegisters.V22));
                case 23: return GetField(nameof(ARegisters.V23));
                case 24: return GetField(nameof(ARegisters.V24));
                case 25: return GetField(nameof(ARegisters.V25));
                case 26: return GetField(nameof(ARegisters.V26));
                case 27: return GetField(nameof(ARegisters.V27));
                case 28: return GetField(nameof(ARegisters.V28));
                case 29: return GetField(nameof(ARegisters.V29));
                case 30: return GetField(nameof(ARegisters.V30));
                case 31: return GetField(nameof(ARegisters.V31));
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetField(string Name)
        {
            return typeof(ARegisters).GetField(Name);
        }
    }
}