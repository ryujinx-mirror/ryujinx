using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeBranch : OpCodeConditional
    {
        public int Offset { get; }

        public bool PushTarget { get; protected set; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeBranch(emitter, address, opCode);

        public OpCodeBranch(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Offset = ((int)(opCode >> 20) << 8) >> 8;

            PushTarget = false;
        }

        public ulong GetAbsoluteAddress()
        {
            return (ulong)((long)Address + (long)Offset + 8);
        }
    }
}