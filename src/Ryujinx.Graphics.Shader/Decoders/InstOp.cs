using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    readonly struct InstOp
    {
        public readonly ulong Address;
        public readonly ulong RawOpCode;
        public readonly InstEmitter Emitter;
        public readonly InstProps Props;
        public readonly InstName Name;

        public InstOp(ulong address, ulong rawOpCode, InstName name, InstEmitter emitter, InstProps props)
        {
            Address = address;
            RawOpCode = rawOpCode;
            Name = name;
            Emitter = emitter;
            Props = props;
        }

        public ulong GetAbsoluteAddress()
        {
            return (ulong)((long)Address + (((int)(RawOpCode >> 20) << 8) >> 8) + 8);
        }
    }
}
