using System;

namespace Ryujinx.Cpu.LightningJit.Arm32
{
    readonly struct InstMeta
    {
        public readonly InstName Name;
        public readonly Action<CodeGenContext, uint> EmitFunc;
        public readonly IsaVersion Version;
        public readonly IsaFeature Feature;
        public readonly InstFlags Flags;

        public InstMeta(InstName name, Action<CodeGenContext, uint> emitFunc, IsaVersion isaVersion, IsaFeature isaFeature, InstFlags flags)
        {
            Name = name;
            EmitFunc = emitFunc;
            Version = isaVersion;
            Feature = isaFeature;
            Flags = flags;
        }
    }
}
