using System;

namespace Ryujinx.Cpu.LightningJit.Arm32
{
    readonly struct InstInfo
    {
        public readonly uint Encoding;
        public readonly InstName Name;
        public readonly Action<CodeGenContext, uint> EmitFunc;
        public readonly InstFlags Flags;

        public InstInfo(uint encoding, InstName name, Action<CodeGenContext, uint> emitFunc, InstFlags flags)
        {
            Encoding = encoding;
            Name = name;
            EmitFunc = emitFunc;
            Flags = flags;
        }
    }
}
