using System;

namespace ChocolArm64.Instructions
{
    struct Inst
    {
        public InstEmitter Emitter { get; }
        public Type        Type    { get; }

        public static Inst Undefined => new Inst(InstEmit.Und, null);

        public Inst(InstEmitter emitter, Type type)
        {
            Emitter = emitter;
            Type    = type;
        }
    }
}