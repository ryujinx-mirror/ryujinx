using System;

namespace ChocolArm64.Instruction
{
    struct AInst
    {
        public AInstEmitter Emitter { get; private set; }
        public Type         Type    { get; private set; }

        public static AInst Undefined => new AInst(AInstEmit.Und, null);

        public AInst(AInstEmitter Emitter, Type Type)
        {
            this.Emitter = Emitter;
            this.Type    = Type;
        }
    }
}