using ARMeilleure.Instructions;

namespace ARMeilleure.Decoders
{
    readonly struct InstDescriptor
    {
        public static InstDescriptor Undefined => new(InstName.Und, InstEmit.Und);

        public InstName Name { get; }
        public InstEmitter Emitter { get; }

        public InstDescriptor(InstName name, InstEmitter emitter)
        {
            Name = name;
            Emitter = emitter;
        }
    }
}
