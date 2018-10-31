using System;

namespace ChocolArm64.Instructions
{
    struct Inst
    {
        public InstInterpreter Interpreter { get; private set; }
        public InstEmitter     Emitter     { get; private set; }
        public Type             Type        { get; private set; }

        public static Inst Undefined => new Inst(null, InstEmit.Und, null);

        public Inst(InstInterpreter interpreter, InstEmitter emitter, Type type)
        {
            Interpreter = interpreter;
            Emitter     = emitter;
            Type        = type;
        }
    }
}