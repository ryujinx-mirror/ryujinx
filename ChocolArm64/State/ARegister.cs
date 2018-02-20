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
                case APState.VBit: return GetField(nameof(AThreadState.Overflow));
                case APState.CBit: return GetField(nameof(AThreadState.Carry));
                case APState.ZBit: return GetField(nameof(AThreadState.Zero));
                case APState.NBit: return GetField(nameof(AThreadState.Negative));
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetFieldInt()
        {
            switch (Index)
            {
                case 0:  return GetField(nameof(AThreadState.X0));
                case 1:  return GetField(nameof(AThreadState.X1));
                case 2:  return GetField(nameof(AThreadState.X2));
                case 3:  return GetField(nameof(AThreadState.X3));
                case 4:  return GetField(nameof(AThreadState.X4));
                case 5:  return GetField(nameof(AThreadState.X5));
                case 6:  return GetField(nameof(AThreadState.X6));
                case 7:  return GetField(nameof(AThreadState.X7));
                case 8:  return GetField(nameof(AThreadState.X8));
                case 9:  return GetField(nameof(AThreadState.X9));
                case 10: return GetField(nameof(AThreadState.X10));
                case 11: return GetField(nameof(AThreadState.X11));
                case 12: return GetField(nameof(AThreadState.X12));
                case 13: return GetField(nameof(AThreadState.X13));
                case 14: return GetField(nameof(AThreadState.X14));
                case 15: return GetField(nameof(AThreadState.X15));
                case 16: return GetField(nameof(AThreadState.X16));
                case 17: return GetField(nameof(AThreadState.X17));
                case 18: return GetField(nameof(AThreadState.X18));
                case 19: return GetField(nameof(AThreadState.X19));
                case 20: return GetField(nameof(AThreadState.X20));
                case 21: return GetField(nameof(AThreadState.X21));
                case 22: return GetField(nameof(AThreadState.X22));
                case 23: return GetField(nameof(AThreadState.X23));
                case 24: return GetField(nameof(AThreadState.X24));
                case 25: return GetField(nameof(AThreadState.X25));
                case 26: return GetField(nameof(AThreadState.X26));
                case 27: return GetField(nameof(AThreadState.X27));
                case 28: return GetField(nameof(AThreadState.X28));
                case 29: return GetField(nameof(AThreadState.X29));
                case 30: return GetField(nameof(AThreadState.X30));
                case 31: return GetField(nameof(AThreadState.X31));
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetFieldVector()
        {
            switch (Index)
            {
                case 0:  return GetField(nameof(AThreadState.V0));
                case 1:  return GetField(nameof(AThreadState.V1));
                case 2:  return GetField(nameof(AThreadState.V2));
                case 3:  return GetField(nameof(AThreadState.V3));
                case 4:  return GetField(nameof(AThreadState.V4));
                case 5:  return GetField(nameof(AThreadState.V5));
                case 6:  return GetField(nameof(AThreadState.V6));
                case 7:  return GetField(nameof(AThreadState.V7));
                case 8:  return GetField(nameof(AThreadState.V8));
                case 9:  return GetField(nameof(AThreadState.V9));
                case 10: return GetField(nameof(AThreadState.V10));
                case 11: return GetField(nameof(AThreadState.V11));
                case 12: return GetField(nameof(AThreadState.V12));
                case 13: return GetField(nameof(AThreadState.V13));
                case 14: return GetField(nameof(AThreadState.V14));
                case 15: return GetField(nameof(AThreadState.V15));
                case 16: return GetField(nameof(AThreadState.V16));
                case 17: return GetField(nameof(AThreadState.V17));
                case 18: return GetField(nameof(AThreadState.V18));
                case 19: return GetField(nameof(AThreadState.V19));
                case 20: return GetField(nameof(AThreadState.V20));
                case 21: return GetField(nameof(AThreadState.V21));
                case 22: return GetField(nameof(AThreadState.V22));
                case 23: return GetField(nameof(AThreadState.V23));
                case 24: return GetField(nameof(AThreadState.V24));
                case 25: return GetField(nameof(AThreadState.V25));
                case 26: return GetField(nameof(AThreadState.V26));
                case 27: return GetField(nameof(AThreadState.V27));
                case 28: return GetField(nameof(AThreadState.V28));
                case 29: return GetField(nameof(AThreadState.V29));
                case 30: return GetField(nameof(AThreadState.V30));
                case 31: return GetField(nameof(AThreadState.V31));
            }

            throw new InvalidOperationException();
        }

        private FieldInfo GetField(string Name)
        {
            return typeof(AThreadState).GetField(Name);
        }
    }
}