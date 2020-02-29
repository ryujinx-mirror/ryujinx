using ARMeilleure.Instructions;

namespace ARMeilleure.Decoders
{
    struct InstDescriptor
    {
        public static InstDescriptor Undefined => new InstDescriptor(InstName.Und, InstEmit.Und);

        public InstName    Name    { get; }
        public InstEmitter Emitter { get; }

        public InstDescriptor(InstName name, InstEmitter emitter)
        {
            Name    = name;
            Emitter = emitter;
        }
    }
}